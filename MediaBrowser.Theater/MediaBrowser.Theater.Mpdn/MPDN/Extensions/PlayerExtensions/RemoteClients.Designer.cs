namespace Mpdn.PlayerExtensions
{
    partial class RemoteClients
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgMainGrid = new System.Windows.Forms.DataGridView();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.colClientGUID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgMainGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // dgMainGrid
            // 
            this.dgMainGrid.AllowUserToAddRows = false;
            this.dgMainGrid.AllowUserToDeleteRows = false;
            this.dgMainGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgMainGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colClientGUID,
            this.colAddr});
            this.dgMainGrid.Location = new System.Drawing.Point(12, 12);
            this.dgMainGrid.Name = "dgMainGrid";
            this.dgMainGrid.RowHeadersVisible = false;
            this.dgMainGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgMainGrid.Size = new System.Drawing.Size(360, 209);
            this.dgMainGrid.TabIndex = 0;
            this.dgMainGrid.SelectionChanged += new System.EventHandler(this.dgMainGrid_SelectionChanged);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(297, 227);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(171, 227);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(120, 23);
            this.btnDisconnect.TabIndex = 2;
            this.btnDisconnect.Text = "Disconnect Remote";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // colClientGUID
            // 
            this.colClientGUID.HeaderText = "Client ID";
            this.colClientGUID.Name = "colClientGUID";
            this.colClientGUID.Width = 225;
            // 
            // colAddr
            // 
            this.colAddr.FillWeight = 160F;
            this.colAddr.HeaderText = "Client Address";
            this.colAddr.Name = "colAddr";
            this.colAddr.Width = 132;
            // 
            // RemoteClients
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 262);
            this.ControlBox = false;
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgMainGrid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "RemoteClients";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connected Remote Clients";
            ((System.ComponentModel.ISupportInitialize)(this.dgMainGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgMainGrid;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClientGUID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddr;
    }
}