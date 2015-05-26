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
using System.Security;
using System.Runtime.ConstrainedExecution;
using System.IO;

namespace KeeChallenge
{
    public enum YubiSlot
    {
        SLOT1 = 0,
        SLOT2 = 1
    };

    public class YubiWrapper
    {
        public const uint yubiRespLen = 20;
        private const uint yubiBuffLen = 64;

        private List<string> nativeDLLs =  new List<string>() { "libykpers-1-1.dll", "libyubikey-0.dll", "libjson-0.dll", "libjson-c-2.dll" };

        private static bool is64BitProcess = (IntPtr.Size == 8);

        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string methodName);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [SecurityCritical]
        internal static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }
        
        private static ReadOnlyCollection<byte> slots = new ReadOnlyCollection<byte>(new List<byte>()
        {
            0x30, //SLOT_CHAL_HMAC1
            0x38  //SLOT_CHAL_HMAC2
        });

        private IntPtr yk = IntPtr.Zero;

        public bool Init()
        {
            try
            { 
                if (!IsLinux) //no DLL Hell on Linux!
                {     
                    foreach (string s in nativeDLLs) //support upgrading from installs of versions 1.0.2 and prior
                    {
                        string path = Path.Combine(Environment.CurrentDirectory, s);
                        if (File.Exists(path)) //prompt the user to do it to avoid permissions issues
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch (Exception)
                            {
                                string warn = "Please login as an administrator and delete the following files from " + Environment.CurrentDirectory + ":\n" + string.Join("\n", nativeDLLs.ToArray());
                                MessageBox.Show(warn);
                                return false;
                            }
                        }
                    }


                    if (!DoesWin32MethodExist("kernel32.dll", "SetDllDirectoryW")) throw new PlatformNotSupportedException("KeeChallenge requires Windows XP Service Pack 1 or greater");
                    
                    string _32BitDir = Path.Combine(AssemblyDirectory, "32bit");
                    string _64BitDir = Path.Combine(AssemblyDirectory, "64bit");
                    if (!Directory.Exists(_32BitDir) || !Directory.Exists(_64BitDir))
                    {
                        string err = String.Format("Error: one of the following directories is missing:\n{0}\n{1}\nPlease reinstall KeeChallenge and ensure that these directories are present", _32BitDir, _64BitDir);
                        MessageBox.Show(err);
                        return false;
                    }
                    if (!is64BitProcess) 
                        SetDllDirectory(_32BitDir);
                    else
                        SetDllDirectory(_64BitDir);
                }
                if (yk_init() != 1) return false;
                yk = yk_open_first_key();
                if (yk == IntPtr.Zero) return false;
            }
            catch (Exception e)
            {
                Debug.Assert(false,e.Message);         
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
            int ret = yk_challenge_response(yk, slots[(int)slot], 1, (uint)challenge.Length, challenge, yubiBuffLen, temp);
            if (ret == 1)
            {
                Array.Copy(temp, response, response.Length);
                return true;
            }
            else return false;
        }

        public void Close()
        {
            if (yk != IntPtr.Zero)
            {
                bool ret = YubiWrapper.yk_close_key(yk) == 1;
                if (!ret || YubiWrapper.yk_release() != 1)
                {
                    throw new Exception("Error closing Yubikey");
                }
            }
        }
    }
}
