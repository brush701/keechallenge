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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using KeePass.UI;

namespace KeeChallenge
{
    public partial class KeyCreation : Form
    {
        public byte[] Secret
        {
            get;
            private set;
        }

        private KeeChallengeProv m_parent;

        public KeyCreation(KeeChallengeProv parent)
        {
            InitializeComponent();
            Secret = null;
            Icon = Icon.FromHandle(Properties.Resources.yubikey.GetHicon());
            m_parent = parent;
        }
  
        public void OnClosing(object o, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                m_parent.LT64 = LT64_cb.Checked;

                Secret = new byte[KeeChallengeProv.secretLenBytes];
                secretTextBox.Text = secretTextBox.Text.Replace(" ", string.Empty); //remove spaces
                
                if (secretTextBox.Text.Length == KeeChallengeProv.secretLenBytes * 2)
                {
                    for (int i = 0; i < secretTextBox.Text.Length; i += 2)
                    {
                        string b = secretTextBox.Text.Substring(i, 2);
                        Secret[i / 2] = Convert.ToByte(b,16);
                    }
                }
                else
                {
                    //invalid key
                    MessageBox.Show("Error: secret must be 20 bytes long");
                    e.Cancel = true;
                    return;
                }
                
                //Confirm they have a key whose secret matches this
                byte[] challenge = m_parent.GenerateChallenge();                
                KeyEntry validate = new KeyEntry(m_parent, challenge);               
                
                if ( validate.ShowDialog(this) != DialogResult.OK)
                {
                    MessageBox.Show("Unable to get response from yubikey");
                    e.Cancel = true;
                    Array.Clear(Secret, 0, Secret.Length);
                    return;
                }

                byte[] validResp = m_parent.GenerateResponse(challenge, Secret);
                
                for (int i = 0; i < validate.Response.Length; i++)
                {
                    if (validate.Response[i] != validResp[i])
                    {
                        MessageBox.Show("Error: secret does not match yubikey");
                        e.Cancel = true;
                        Array.Clear(Secret,0,Secret.Length);
                        return; //Error: wrong secret
                    }                   
                }
                
                Array.Clear(validate.Response, 0, validate.Response.Length);
            }
            GlobalWindowManager.RemoveWindow(this);
        }      
    }
}
