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
namespace Mpdn.PlayerExtensions
{
    partial class RemoteControlConfig
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txbPort = new System.Windows.Forms.TextBox();
            this.cbRequireValidation = new System.Windows.Forms.CheckBox();
            this.cbIsOnline = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(47, 78);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(128, 78);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Remote Port";
            // 
            // txbPort
            // 
            this.txbPort.Location = new System.Drawing.Point(84, 6);
            this.txbPort.Name = "txbPort";
            this.txbPort.Size = new System.Drawing.Size(119, 20);
            this.txbPort.TabIndex = 3;
            this.txbPort.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txbPort_KeyUp);
            // 
            // cbRequireValidation
            // 
            this.cbRequireValidation.AutoSize = true;
            this.cbRequireValidation.Location = new System.Drawing.Point(15, 55);
            this.cbRequireValidation.Name = "cbRequireValidation";
            this.cbRequireValidation.Size = new System.Drawing.Size(98, 17);
            this.cbRequireValidation.TabIndex = 5;
            this.cbRequireValidation.Text = "Validate Clients";
            this.cbRequireValidation.UseVisualStyleBackColor = true;
            // 
            // cbIsOnline
            // 
            this.cbIsOnline.AutoSize = true;
            this.cbIsOnline.Location = new System.Drawing.Point(15, 32);
            this.cbIsOnline.Name = "cbIsOnline";
            this.cbIsOnline.Size = new System.Drawing.Size(137, 17);
            this.cbIsOnline.TabIndex = 6;
            this.cbIsOnline.Text = "Activate Remote Plugin";
            this.cbIsOnline.UseVisualStyleBackColor = true;
            // 
            // RemoteControlConfig
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(215, 114);
            this.Controls.Add(this.cbIsOnline);
            this.Controls.Add(this.cbRequireValidation);
            this.Controls.Add(this.txbPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemoteControlConfig";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RemoteControl Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbPort;
        private System.Windows.Forms.CheckBox cbRequireValidation;
        private System.Windows.Forms.CheckBox cbIsOnline;
    }
}
