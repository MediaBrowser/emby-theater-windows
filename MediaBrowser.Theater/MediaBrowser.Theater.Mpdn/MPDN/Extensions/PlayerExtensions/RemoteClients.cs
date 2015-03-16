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
