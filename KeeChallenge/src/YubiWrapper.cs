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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;


namespace KeeChallenge
{
    public enum YubiSlot
    {
        SLOT1 = 0,
        SLOT2 = 1
    };

    public class YubiWrapper
    {
        //public const byte SLOT_CHAL_HMAC1 = 0x30;
        //public const byte SLOT_CHAL_HMAC2 = 0x38;

        private const uint yubiRespLen = 20;
        private const uint yubiBuffLen = 64;
        private const uint yubiChalLen = 64;
        
        private static ReadOnlyCollection<byte> slots = new ReadOnlyCollection<byte>(new List<byte>()
        {
            0x30, 
            0x38 
        });

        private IntPtr yk;

        public bool Init()
        {
           try
           {
            if (yk_init() != 1) return false;
            yk = yk_open_first_key();
            if (yk == IntPtr.Zero) return false;
           }
           catch (Exception)
           {
                Debug.Assert(false);
                MessageBox.Show("Error connecting to yubikey!", "Error", MessageBoxButtons.OK);
                return false;
           }
           return true;
        }

        [DllImport("libykpers-1-1.dll")]
        private static extern int yk_init();

        [DllImport("libykpers-1-1.dll")]
        private static extern int yk_release();

        [DllImport("libykpers-1-1.dll")]
        private static extern int yk_close_key(IntPtr yk);

        [DllImport("libykpers-1-1.dll")]
        private static extern IntPtr yk_open_first_key();

        [DllImport("libykpers-1-1.dll")]
        private static extern int yk_challenge_response(IntPtr yk, byte yk_cmd, int may_block, uint challenge_len, byte[] challenge, uint response_len, byte[] response);

        public bool ChallengeResponse(YubiSlot slot, byte[] challenge, out byte[] response)
        {
            response = new byte[yubiRespLen];
            if (yk == IntPtr.Zero) return false;
            
            byte[] temp = new byte[yubiBuffLen];
            int ret = yk_challenge_response(yk, slots[(int)slot], 1, yubiChalLen, challenge, yubiBuffLen, temp);
            if (ret == 1)
            {
                Array.Copy(temp, response, response.Length);
                return true;
            }
            else return false;
        }

        public void Close()
        {
            bool ret = false;
            if (yk != IntPtr.Zero)
                ret = YubiWrapper.yk_close_key(yk) == 1;
            if (!ret || YubiWrapper.yk_release() != 1)
            {
                throw new Exception("Error closing Yubikey");
            }
        }
    }
}
