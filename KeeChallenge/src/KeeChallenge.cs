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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;

using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib.Cryptography;
using KeePass.UI;
using KeePassLib.Serialization;

namespace KeeChallenge
{
    public sealed class KeeChallengeProv : KeyProvider
    {
        public const string m_name = "Yubikey challenge-response";
        public const int keyLenBytes = 20;
        public const int challengeLenBytes = 64;
        public const int secretLenBytes = 20;
        private bool m_LT64 = false;

        //If variable length challenges are enabled, a 63 byte challenge is sent instead.
        //See GenerateChallenge() and http://forum.yubico.com/viewtopic.php?f=16&t=1078
        public bool LT64
        {
            get { return m_LT64; }
            set { m_LT64 = value; }
        }

        public YubiSlot YubikeySlot
        {
            get;
            set;
        }

        public KeeChallengeProv()
        {
            YubikeySlot = YubiSlot.SLOT2;
        }

        private IOConnectionInfo mInfo;

        public override string Name
        {
            get { return m_name; }
        }

        public override bool SecureDesktopCompatible
        {
            get
            {
                return true;
            }
        }

        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            if (ctx == null)
            {
                Debug.Assert(false);
                return null;
            }

            mInfo = ctx.DatabaseIOInfo.CloneDeep();
            string db = mInfo.Path;
            Regex rgx = new Regex(@"\.kdbx$");
            mInfo.Path = rgx.Replace(db, ".xml");

            if (Object.ReferenceEquals(db,mInfo.Path)) //no terminating .kdbx found-> maybe using keepass 1? should never happen...
            {
                MessageService.ShowWarning("Invalid database. KeeChallenge only works with .kdbx files.");
                return null;
            }


            try
            {
                if (ctx.CreatingNewKey) return Create(ctx);
                return Get(ctx);
            }
            catch (Exception ex) { MessageService.ShowWarning(ex.Message); }

            return null;
        }

        public byte[] GenerateChallenge()
        {
            CryptoRandom rand = CryptoRandom.Instance;
            byte[] chal =  CryptoRandom.Instance.GetRandomBytes(challengeLenBytes);  
            if (LT64)
            {
                chal[challengeLenBytes - 2] = (byte)~chal[challengeLenBytes - 1];
            }

            return chal;
        }

        public byte[] GenerateResponse(byte[] challenge, byte[] key)
        {
            HMACSHA1 hmac = new HMACSHA1(key);

            if (LT64)
                challenge = challenge.Take(challengeLenBytes - 1).ToArray();

            byte[] resp = hmac.ComputeHash(challenge);
            hmac.Clear();
            return resp;
        }

        private bool EncryptAndSave(byte[] secret)
        {
            //generate a random challenge for use next time
            byte[] challenge = GenerateChallenge();

            //generate the expected HMAC-SHA1 response for the challenge based on the secret
            byte[] resp = GenerateResponse(challenge, secret);

            //use the response to encrypt the secret
            SHA256 sha = SHA256Managed.Create();
            byte[] key = sha.ComputeHash(resp); // get a 256 bit key from the 160 bit hmac response
            byte[] secretHash = sha.ComputeHash(secret);

            AesManaged aes = new AesManaged();
            aes.KeySize = key.Length * sizeof(byte) * 8; //pedantic, but foolproof
            aes.Key = key;
            aes.GenerateIV();
            aes.Padding = PaddingMode.PKCS7;
            byte[] iv = aes.IV;

            byte[] encrypted;
            ICryptoTransform enc = aes.CreateEncryptor();
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, enc, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(secret, 0, secret.Length);
                    csEncrypt.FlushFinalBlock();

                    encrypted = msEncrypt.ToArray();
                    csEncrypt.Close();
                    csEncrypt.Clear();
                }
                msEncrypt.Close();
            }

            sha.Clear();
            aes.Clear();

            Stream s = null;
            try
            {
                FileTransactionEx ft = new FileTransactionEx(mInfo,
                    false);
                s = ft.OpenWrite();
               
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.CloseOutput = true;
                settings.Indent = true;
                settings.IndentChars = "\t";
                settings.NewLineOnAttributes = true;

                XmlWriter xml = XmlWriter.Create(s, settings);
                xml.WriteStartDocument();
                xml.WriteStartElement("data");

                xml.WriteStartElement("aes");
                xml.WriteElementString("encrypted", Convert.ToBase64String(encrypted));
                xml.WriteElementString("iv", Convert.ToBase64String(iv));
                xml.WriteEndElement();

                xml.WriteElementString("challenge", Convert.ToBase64String(challenge));
                xml.WriteElementString("verification", Convert.ToBase64String(secretHash));
                xml.WriteElementString("lt64", LT64.ToString());

                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Close();                
  
                ft.CommitWrite();  
            }
            catch (Exception)
            {
                MessageService.ShowWarning(String.Format("Error: unable to write to file {0}", mInfo.Path));
                return false;
            }    
            finally
            {                
                s.Close();
            }

            return true;
        }

        private static bool DecryptSecret(byte[] encryptedSecret, byte[] yubiResp, byte[] iv, byte[] verification, out byte[] secret)
        {
            //use the response to decrypt the secret
            SHA256 sha = SHA256Managed.Create();
            byte[] key = sha.ComputeHash(yubiResp); // get a 256 bit key from the 160 bit hmac response

            AesManaged aes = new AesManaged();
            aes.KeySize = key.Length * sizeof(byte) * 8; //pedantic, but foolproof
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;

            secret = new byte[keyLenBytes];
            ICryptoTransform dec = aes.CreateDecryptor();
            using (MemoryStream msDecrypt = new MemoryStream(encryptedSecret))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, dec, CryptoStreamMode.Read))
                {
                    csDecrypt.Read(secret, 0, secret.Length);
                    csDecrypt.Close();
                    csDecrypt.Clear();
                }
                msDecrypt.Close();
            }

            byte[] secretHash = sha.ComputeHash(secret);
            for (int i = 0; i < secretHash.Length; i++)
            {
                if (secretHash[i] != verification[i])
                {
                    MessageService.ShowWarning("Incorrect response!");
                    Array.Clear(secret, 0, secret.Length);
                    return false;
                }
            }

            //return the secret
            sha.Clear();
            aes.Clear();
            return true;
        }
       
        private bool ReadEncryptedSecret(out byte[] encryptedSecret, out byte[] challenge, out byte[] iv, out byte[] verification)
        {
            encryptedSecret = null;
            iv = null;
            challenge = null;
            verification = null;
            
            LT64 = false; //default to false if not found

            XmlReader xml = null;
            Stream s = null;
            try
            {
                s = IOConnection.OpenRead(mInfo);

                //read file

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CloseInput = true;
                xml = XmlReader.Create(s, settings);
                
                while (xml.Read())
                {
                    if (xml.IsStartElement())
                    {
                        switch (xml.Name)
                        {
                            case "encrypted":
                                xml.Read();
                                encryptedSecret = Convert.FromBase64String(xml.Value.Trim());
                                break;
                            case "iv":
                                xml.Read();
                                iv = Convert.FromBase64String(xml.Value.Trim());
                                break;
                            case "challenge":
                                xml.Read();
                                challenge = Convert.FromBase64String(xml.Value.Trim());
                                break;
                            case "verification":
                                xml.Read();
                                verification = Convert.FromBase64String(xml.Value.Trim());
                                break;
                            case "lt64":
                                xml.Read();
                                if (!bool.TryParse(xml.Value.Trim(), out m_LT64)) throw new Exception("Unable to parse LT64 flag");
                                break;

                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageService.ShowWarning(String.Format("Error: file {0} could not be read correctly. Is the file corrupt? Reverting to recovery mode", mInfo.Path));
                return false;
            }
            finally
            {
                if (xml != null)
                    xml.Close();
                if (s != null)
                    s.Close();
            }

            //if failed, return false
            return true;
        }

        private byte[] Create(KeyProviderQueryContext ctx)
        {
            //show the entry dialog for the secret
            //get the secret
            KeyCreation creator = new KeyCreation(this);

            if (creator.ShowDialog() != System.Windows.Forms.DialogResult.OK) return null;

            byte[] secret = new byte[creator.Secret.Length];
            
            Array.Copy(creator.Secret, secret, creator.Secret.Length); //probably paranoid here, but not a big performance hit
            Array.Clear(creator.Secret, 0, creator.Secret.Length);

            if (!EncryptAndSave(secret))
            {
                return null;
            }

            //store the encrypted secret, the iv, and the challenge to disk           
           
            return secret;
        }

        private byte[] Get(KeyProviderQueryContext ctx)
        {
            //read the challenge, iv, and encrypted secret from disk -- if missing, you must use recovery mode
            byte[] encryptedSecret = null;
            byte[] iv = null;
            byte[] challenge = null;
            byte[] verification = null;
            byte[] secret = null;

            if (!ReadEncryptedSecret(out encryptedSecret, out challenge, out iv, out verification))
            {
                secret = RecoveryMode();
                EncryptAndSave(secret);
                return secret;
            }
                //show the dialog box prompting user to press yubikey button
            byte[] resp = new byte[YubiWrapper.yubiRespLen];
            KeyEntry entryForm = new KeyEntry(this, challenge);
            
            if (entryForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                if (entryForm.RecoveryMode)
                {
                    secret = RecoveryMode();
                    EncryptAndSave(secret);
                    return secret;
                }

                else return null;                
            }               

            entryForm.Response.CopyTo(resp,0);
            Array.Clear(entryForm.Response,0,entryForm.Response.Length);

            if (DecryptSecret(encryptedSecret, resp, iv, verification, out secret))
            {
                if (EncryptAndSave(secret))
                    return secret;
                else return null;
            }
            else
            {
                return null;
            }
        }

        private static byte[] RecoveryMode()
        {
            //prompt user to enter secret
            RecoveryMode recovery = new RecoveryMode();
            if (recovery.ShowDialog() != System.Windows.Forms.DialogResult.OK) return null;
            byte[] secret = new byte[recovery.Secret.Length];

            recovery.Secret.CopyTo(secret, 0);
            Array.Clear(recovery.Secret, 0, recovery.Secret.Length);            
             
            return secret;
       }

    }
}
