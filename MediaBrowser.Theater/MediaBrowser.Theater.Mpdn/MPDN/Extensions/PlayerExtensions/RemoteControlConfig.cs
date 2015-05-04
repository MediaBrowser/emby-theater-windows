// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mpdn.Config;

namespace Mpdn.PlayerExtensions
{
    public partial class RemoteControlConfig : RemoteControlConfigBase
    {
        #region Constructor
        public RemoteControlConfig()
        {
            InitializeComponent();
        }
        #endregion

        #region Protected Methods
        protected override void LoadSettings()
        {
            txbPort.Text = Settings.ConnectionPort.ToString();
            cbRequireValidation.Checked = Settings.ValidateClients;
            cbIsOnline.Checked = Settings.IsActive;
        }

        protected override void SaveSettings()
        {
            int portNum = Settings.ConnectionPort;
            var portString = txbPort.Text;
            int.TryParse(portString, out portNum);
            Settings.ConnectionPort = portNum;
            Settings.ValidateClients = cbRequireValidation.Checked;
            Settings.IsActive = cbIsOnline.Checked;
        }
        #endregion

        #region Private Methods
        private void validateTextBox(TextBox txb)
        {
            int value = 0;
            if (!int.TryParse(txb.Text, out value))
            {
                try
                {
                    int cursorIndex = txb.SelectionStart - 1;
                    txb.Text = txb.Text.Remove(cursorIndex, 1);
                    txb.SelectionStart = cursorIndex;
                    txb.SelectionLength = 0;
                }
                catch (Exception)
                {

                }
            }
        }

        private void validatePortNumber()
        {
            var portString = txbPort.Text;
            int port = 0;
            int.TryParse(portString, out port);
            if(port < 0 || port > 65535)
            {
                MessageBox.Show("Please enter a port between 1 and 65535", "Invalid Port Number", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txbPort.Text = Settings.ConnectionPort.ToString();
            }
        }

        private void txbPort_KeyUp(object sender, KeyEventArgs e)
        {
            validateTextBox(txbPort);
            validatePortNumber();
        }
        #endregion


    }

    public class RemoteControlConfigBase : ScriptConfigDialog<RemoteControlSettings>
    { 
    }
}
