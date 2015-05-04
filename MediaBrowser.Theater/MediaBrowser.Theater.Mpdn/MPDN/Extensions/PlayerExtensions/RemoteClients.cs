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
using System.Windows.Forms;
using System.Net;

namespace Mpdn.PlayerExtensions
{
    public partial class RemoteClients : Form
    {
        #region Variables
        public AcmPlug mainRemote;
        #endregion

        #region Constructor
        public RemoteClients(AcmPlug control)
        {
            InitializeComponent();
            mainRemote = control;
            Load += RemoteClients_Load;
        }
        #endregion

        #region Internal Methods
        internal void ForceUpdate()
        {
            PopulateGrid();
        }
        #endregion

        #region Private Methods
        void RemoteClients_Load(object sender, EventArgs e)
        {
            dgMainGrid.Invoke((MethodInvoker) PopulateGrid);
        }

        private void PopulateGrid()
        {
            ClearGrid();
            foreach(var item in mainRemote.GetClients)
            {
                try
                {
                    var remoteIpEndPoint = item.Value.RemoteEndPoint as IPEndPoint;
                    if (remoteIpEndPoint == null) 
                        continue;
                    string[] tmpRow = { item.Key.ToString(), remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port };
                    AddRow(tmpRow);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(PlayerControl.VideoPanel, "Error " + ex);
                }
            } 
        }

        private void ClearGrid()
        {
            dgMainGrid.Rows.Clear();
        }

        private void AddRow(string[] row)
        {
            dgMainGrid.Rows.Add(row);
            dgMainGrid.Refresh();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        private void dgMainGrid_SelectionChanged(object sender, EventArgs e)
        {
            btnDisconnect.Enabled = dgMainGrid.SelectedRows.Count != 0;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if(dgMainGrid.SelectedRows.Count == 1)
            {
                var clientGUID = dgMainGrid.SelectedRows[0].Cells[0].Value.ToString();
                mainRemote.DisconnectClient(clientGUID);
            }
        }
    }
}
