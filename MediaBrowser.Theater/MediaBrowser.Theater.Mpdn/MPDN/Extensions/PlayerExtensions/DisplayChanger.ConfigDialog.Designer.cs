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
        partial class DisplayChangerConfigDialog
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
            this.checkBoxActivate = new System.Windows.Forms.CheckBox();
            this.checkBoxRestore = new System.Windows.Forms.CheckBox();
            this.checkBoxRestricted = new System.Windows.Forms.CheckBox();
            this.textBoxVideoTypes = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxRestoreExit = new System.Windows.Forms.CheckBox();
            this.checkBoxHighestRate = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(279, 234);
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
            this.buttonOk.Location = new System.Drawing.Point(198, 234);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1004;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // checkBoxActivate
            // 
            this.checkBoxActivate.AutoSize = true;
            this.checkBoxActivate.Location = new System.Drawing.Point(22, 22);
            this.checkBoxActivate.Name = "checkBoxActivate";
            this.checkBoxActivate.Size = new System.Drawing.Size(247, 17);
            this.checkBoxActivate.TabIndex = 1006;
            this.checkBoxActivate.Text = "Activate automatic display refresh rate changer";
            this.checkBoxActivate.UseVisualStyleBackColor = true;
            // 
            // checkBoxRestore
            // 
            this.checkBoxRestore.AutoSize = true;
            this.checkBoxRestore.Location = new System.Drawing.Point(40, 48);
            this.checkBoxRestore.Name = "checkBoxRestore";
            this.checkBoxRestore.Size = new System.Drawing.Size(239, 17);
            this.checkBoxRestore.TabIndex = 1007;
            this.checkBoxRestore.Text = "Restore refresh rate when media file is closed";
            this.checkBoxRestore.UseVisualStyleBackColor = true;
            // 
            // checkBoxRestricted
            // 
            this.checkBoxRestricted.AutoSize = true;
            this.checkBoxRestricted.Location = new System.Drawing.Point(40, 127);
            this.checkBoxRestricted.Name = "checkBoxRestricted";
            this.checkBoxRestricted.Size = new System.Drawing.Size(230, 17);
            this.checkBoxRestricted.TabIndex = 1010;
            this.checkBoxRestricted.Text = "Activate only for the following video formats";
            this.checkBoxRestricted.UseVisualStyleBackColor = true;
            // 
            // textBoxVideoTypes
            // 
            this.textBoxVideoTypes.Location = new System.Drawing.Point(57, 151);
            this.textBoxVideoTypes.Name = "textBoxVideoTypes";
            this.textBoxVideoTypes.Size = new System.Drawing.Size(285, 20);
            this.textBoxVideoTypes.TabIndex = 1011;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(54, 174);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 13);
            this.label1.TabIndex = 1010;
            this.label1.Text = "Format: [w (0..9)] [h (0..9)] [p | i (0..9)]";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(72, 191);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 13);
            this.label2.TabIndex = 1011;
            this.label2.Text = "e.g. w1920p24 p30 w320h180";
            // 
            // checkBoxRestoreExit
            // 
            this.checkBoxRestoreExit.AutoSize = true;
            this.checkBoxRestoreExit.Location = new System.Drawing.Point(40, 74);
            this.checkBoxRestoreExit.Name = "checkBoxRestoreExit";
            this.checkBoxRestoreExit.Size = new System.Drawing.Size(203, 17);
            this.checkBoxRestoreExit.TabIndex = 1008;
            this.checkBoxRestoreExit.Text = "Restore refresh rate when player exits";
            this.checkBoxRestoreExit.UseVisualStyleBackColor = true;
            // 
            // checkBoxHighestRate
            // 
            this.checkBoxHighestRate.AutoSize = true;
            this.checkBoxHighestRate.Location = new System.Drawing.Point(40, 101);
            this.checkBoxHighestRate.Name = "checkBoxHighestRate";
            this.checkBoxHighestRate.Size = new System.Drawing.Size(302, 17);
            this.checkBoxHighestRate.TabIndex = 1009;
            this.checkBoxHighestRate.Text = "Use highest refresh rate if the one requested is unavailable";
            this.checkBoxHighestRate.UseVisualStyleBackColor = true;
            // 
            // DisplayChangerConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(366, 269);
            this.Controls.Add(this.checkBoxHighestRate);
            this.Controls.Add(this.checkBoxRestoreExit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxVideoTypes);
            this.Controls.Add(this.checkBoxRestricted);
            this.Controls.Add(this.checkBoxRestore);
            this.Controls.Add(this.checkBoxActivate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DisplayChangerConfigDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Display Changer Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.Button buttonCancel;
            private System.Windows.Forms.Button buttonOk;
            private System.Windows.Forms.CheckBox checkBoxActivate;
            private System.Windows.Forms.CheckBox checkBoxRestore;
            private System.Windows.Forms.CheckBox checkBoxRestricted;
            private System.Windows.Forms.TextBox textBoxVideoTypes;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.CheckBox checkBoxRestoreExit;
            private System.Windows.Forms.CheckBox checkBoxHighestRate;

        }
    }
}
