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
using System.Runtime.InteropServices;


namespace KeeChallenge
{
    public class YubiWrapper
    {
        public const byte SLOT_CHAL_HMAC1 = 0x30;
        public const byte SLOT_CHAL_HMAC2 = 0x38;

        [DllImport("libykpers-1-1.dll")]
        public static extern int yk_init();

        [DllImport("libykpers-1-1.dll")]
        public static extern int yk_release();

        [DllImport("libykpers-1-1.dll")]
        public static extern int yk_close_key(IntPtr yk);

        [DllImport("libykpers-1-1.dll")]
        public static extern IntPtr yk_open_first_key();

        [DllImport("libykpers-1-1.dll")]
        public static extern int yk_challenge_response(IntPtr yk, byte yk_cmd, int may_block, uint challenge_len, byte[] challenge, uint response_len, byte[] response);
    }
}
