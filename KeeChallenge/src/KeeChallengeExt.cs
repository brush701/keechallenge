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
using System.Text;
using System.Windows.Forms;
using System.Configuration;

using KeePass.Plugins;

namespace KeeChallenge
{
    public sealed class KeeChallengeExt : Plugin
    {
        private IPluginHost m_host = null;
        private KeeChallengeProv m_prov = null;

        private ToolStripMenuItem m_MenuItem = null;
        private ToolStripMenuItem m_YubiSlot1 = null;
        private ToolStripMenuItem m_YubiSlot2 = null;
        private ToolStripSeparator m_Separator = null;

        public override String UpdateUrl
        {
            get { return "https://sourceforge.net/p/keechallenge/code/ci/master/tree/VERSION?format=raw"; }
        }

        public IPluginHost Host
        {
            get { return m_host; }
        }

        public override bool Initialize(IPluginHost host)
        {
            if (m_host != null) { Terminate(); }

            if (host == null) return false;

            m_host = host;
            
            int slot = Properties.Settings.Default.YubikeySlot - 1;  //Important: for readability, the slot settings are not zero based. We must account for this during read/save
            YubiSlot yubiSlot = YubiSlot.SLOT2;
            if (Enum.IsDefined(typeof(YubiSlot),slot))
                yubiSlot = (YubiSlot)slot;

            ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
            m_Separator = new ToolStripSeparator();
            tsMenu.Add(m_Separator);

            m_YubiSlot1 = new ToolStripMenuItem();
            m_YubiSlot1.Name = "Slot1";
            m_YubiSlot1.Text = "Slot 1";
            m_YubiSlot1.CheckOnClick = true;
            m_YubiSlot1.Checked = yubiSlot == YubiSlot.SLOT1;
            m_YubiSlot1.Click += (s, e) => { m_YubiSlot2.Checked = false; m_prov.YubikeySlot = YubiSlot.SLOT1; };
            
            m_YubiSlot2 = new ToolStripMenuItem();
            m_YubiSlot2.Name = "Slot2";
            m_YubiSlot2.Text = "Slot 2";
            m_YubiSlot2.CheckOnClick = true;
            m_YubiSlot2.Checked = yubiSlot == YubiSlot.SLOT2;
            m_YubiSlot2.Click += (s, e) => { m_YubiSlot1.Checked = false; m_prov.YubikeySlot = YubiSlot.SLOT2; };

            m_MenuItem = new ToolStripMenuItem();
            m_MenuItem.Text = "KeeChallenge Settings";
            m_MenuItem.DropDownItems.AddRange(new ToolStripItem[] { m_YubiSlot1, m_YubiSlot2 });
            
            tsMenu.Add(m_MenuItem);

            m_prov = new KeeChallengeProv();
            m_prov.YubikeySlot = yubiSlot;
            m_host.KeyProviderPool.Add(m_prov);

            return true;
        }

        public override void Terminate()
        {           
            if (m_host != null)
            {
                m_host.KeyProviderPool.Remove(m_prov);
                if (m_YubiSlot1.Checked)
                    Properties.Settings.Default.YubikeySlot = 1;
                else if (m_YubiSlot2.Checked)
                    Properties.Settings.Default.YubikeySlot = 2;
                
                Properties.Settings.Default.Save();

                ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
                tsMenu.Remove(m_MenuItem);
                tsMenu.Remove(m_Separator);

                m_prov = null;
                m_host = null;
            }
        }       
     
    }
}
