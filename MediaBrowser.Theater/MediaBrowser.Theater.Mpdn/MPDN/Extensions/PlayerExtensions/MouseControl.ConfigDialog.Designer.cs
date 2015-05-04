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
    namespace GitHub
    {
        partial class MouseControlConfigDialog
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.checkBoxMouseWheelSeek = new System.Windows.Forms.CheckBox();
            this.checkBoxFsToggle = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(279, 100);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1005;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(198, 100);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1004;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // checkBoxMouseWheelSeek
            // 
            this.checkBoxMouseWheelSeek.AutoSize = true;
            this.checkBoxMouseWheelSeek.Location = new System.Drawing.Point(22, 22);
            this.checkBoxMouseWheelSeek.Name = "checkBoxMouseWheelSeek";
            this.checkBoxMouseWheelSeek.Size = new System.Drawing.Size(172, 17);
            this.checkBoxMouseWheelSeek.TabIndex = 1006;
            this.checkBoxMouseWheelSeek.Text = "Enable seek with mouse wheel";
            this.checkBoxMouseWheelSeek.UseVisualStyleBackColor = true;
            // 
            // checkBoxFsToggle
            // 
            this.checkBoxFsToggle.AutoSize = true;
            this.checkBoxFsToggle.Location = new System.Drawing.Point(22, 49);
            this.checkBoxFsToggle.Name = "checkBoxFsToggle";
            this.checkBoxFsToggle.Size = new System.Drawing.Size(281, 17);
            this.checkBoxFsToggle.TabIndex = 1007;
            this.checkBoxFsToggle.Text = "Enable windowed / full screen toggle with middle click";
            this.checkBoxFsToggle.UseVisualStyleBackColor = true;
            // 
            // MouseControlConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(366, 135);
            this.Controls.Add(this.checkBoxFsToggle);
            this.Controls.Add(this.checkBoxMouseWheelSeek);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MouseControlConfigDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mouse Control Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.Button buttonCancel;
            private System.Windows.Forms.Button buttonOk;
            private System.Windows.Forms.CheckBox checkBoxMouseWheelSeek;
            private System.Windows.Forms.CheckBox checkBoxFsToggle;

        }
    }
}
