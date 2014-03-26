/* KeeChallenge--Provides Yubikey challenge-response capability to Keepass
*  Copyright (C) 2014  Ben Rush
*  
*  This program is free software; you can redistribute it and/or
*  modify it under the terms of the GNU General Public License
*  as published by the Free Software Foundation; either version 2
*  of the License, or (at your option) any later version.
*  
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*  
*  You should have received a copy of the GNU General Public License
*  along with this program; if not, write to the Free Software
*  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.UI;

namespace KeeChallenge
{
    public partial class KeyEntry : Form
    {
        private System.Windows.Forms.Timer countdown;
        private byte[] m_challenge;
        private byte[] m_response;
        private IntPtr yk = IntPtr.Zero;
        
        private bool success;

        private BackgroundWorker keyWorker;

        public byte[] Response
        {
            get { return m_response; }
            private set { m_response = value; }
        }

        public byte[] Challenge
        {
            get { return m_challenge; }
            set { m_challenge = value; }
        }


        public KeyEntry()
        {
            InitializeComponent();
            success = false;
            Response = new byte[KeeChallengeProv.responseLenBytes];
            Challenge = null;

            Icon = Icon.FromHandle(Properties.Resources.yubikey.GetHicon());
        }

        public KeyEntry(byte[] challenge)
        {
            InitializeComponent();
            success = false;
            Response = new byte[KeeChallengeProv.responseLenBytes];
            Challenge = challenge;

            Icon = Icon.FromHandle(Properties.Resources.yubikey.GetHicon());
        }
               
        private void YubiChallengeResponse(object sender, DoWorkEventArgs e) //Should terminate in 15seconds worst case
        {
            uint yubiBufferLen = 64;

            //Send the challenge to yubikey and get response
            try
            {
                if (Challenge == null) return;
                byte[] temp = new byte[yubiBufferLen];
                success = YubiWrapper.yk_challenge_response(yk, YubiWrapper.SLOT_CHAL_HMAC2, 1, KeeChallengeProv.challengeLenBytes, m_challenge, yubiBufferLen, temp) == 1;
                Array.Copy(temp, Response, Response.Length);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
            finally
            {
                bool ret = false;
                if (yk != IntPtr.Zero)
                    ret = YubiWrapper.yk_close_key(yk) == 1;
                if (!ret || YubiWrapper.yk_release() != 1)
                {
                    throw new Exception("Error closing Yubikey");
                }
            }

            return;
        }

        private void keyWorkerDone(object sender, EventArgs e) //guaranteed to run after YubiChallengeResponse
        {
            if (success)
                DialogResult = System.Windows.Forms.DialogResult.OK;  //setting this calls Close() IF the form is shown using ShowDialog()
            else DialogResult = System.Windows.Forms.DialogResult.No; 
        }

        private void Countdown(object sender, EventArgs e)
        {
            if (countdown == null) return;
            if (progressBar.Value > 0)
                progressBar.Value--;
            else countdown.Stop(); //we're done, the keyWorker should finish momentarily and close the window for us
        }
        
        private void OnFormLoad(object sender, EventArgs e)
        {
            ControlBox = false;

            progressBar.Maximum = 15;
            progressBar.Minimum = 0;
            progressBar.Value = 15; 

            //spawn background countdown timer
            countdown = new System.Windows.Forms.Timer();
            countdown.Tick += Countdown;
            countdown.Interval = 1000;
            countdown.Enabled = true;

            try
            {
                if (YubiWrapper.yk_init() != 1) return;

                while (yk == IntPtr.Zero)
                {
                    yk = YubiWrapper.yk_open_first_key();
                    if (yk == IntPtr.Zero)
                    {
                        YubiPrompt prompt = new YubiPrompt();
                        if (prompt.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                    }

                }
            }
            catch (Exception)
            {
                Debug.Assert(false);
                MessageBox.Show("Error connecting to yubikey!", "Error", MessageBoxButtons.OK);
                DialogResult = System.Windows.Forms.DialogResult.Abort;
                return;
            }


            keyWorker = new BackgroundWorker();            
            keyWorker.DoWork += YubiChallengeResponse;
            keyWorker.RunWorkerCompleted += keyWorkerDone;
            keyWorker.RunWorkerAsync();

            countdown.Start();            
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            countdown.Enabled = false;
            countdown.Dispose();
            GlobalWindowManager.RemoveWindow(this);
        }
    }
}
