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

using KeePass.Plugins;

namespace KeeChallenge
{
    public sealed class KeeChallengeExt : Plugin
    {
        private IPluginHost m_host = null;
        private KeeChallengeProv m_prov = null;

        public IPluginHost Host
        {
            get { return m_host; }
        }

        public override bool Initialize(IPluginHost host)
        {
            if (m_host != null) { Terminate(); }

            if (host == null) return false;

            m_host = host;

            m_prov = new KeeChallengeProv();
            m_host.KeyProviderPool.Add(m_prov);

            return true;
        }

        public override void Terminate()
        {           
            if (m_host != null)
            {
                m_host.KeyProviderPool.Remove(m_prov);

                m_prov = null;
                m_host = null;
            }
        }
     
    }
}
