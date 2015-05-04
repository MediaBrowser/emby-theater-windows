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
namespace Mpdn.RenderScript
{
    namespace Mpdn.Presets
    {
        partial class PresetGroupDialog
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
            }

            #region Component Designer generated code

            /// <summary> 
            /// Required method for Designer support - do not modify 
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.ScriptBox = new System.Windows.Forms.ComboBox();
            this.ScriptLabel = new System.Windows.Forms.Label();
            this.HotkeyLabel = new System.Windows.Forms.Label();
            this.HotkeyBox = new System.Windows.Forms.TextBox();
            this.buttonConfigure = new System.Windows.Forms.Button();
            this.buttonAdv = new System.Windows.Forms.Button();
            this.DescriptionLabelLabel = new System.Windows.Forms.Label();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(326, 96);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(245, 96);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // ScriptBox
            // 
            this.ScriptBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ScriptBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ScriptBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ScriptBox.FormattingEnabled = true;
            this.ScriptBox.Location = new System.Drawing.Point(99, 14);
            this.ScriptBox.Name = "ScriptBox";
            this.ScriptBox.Size = new System.Drawing.Size(221, 21);
            this.ScriptBox.TabIndex = 9;
            this.ScriptBox.SelectedIndexChanged += new System.EventHandler(this.ScriptBox_SelectedIndexChanged);
            // 
            // ScriptLabel
            // 
            this.ScriptLabel.AutoSize = true;
            this.ScriptLabel.Location = new System.Drawing.Point(59, 17);
            this.ScriptLabel.Name = "ScriptLabel";
            this.ScriptLabel.Size = new System.Drawing.Size(34, 13);
            this.ScriptLabel.TabIndex = 10;
            this.ScriptLabel.Text = "Script";
            // 
            // HotkeyLabel
            // 
            this.HotkeyLabel.AutoSize = true;
            this.HotkeyLabel.Location = new System.Drawing.Point(52, 70);
            this.HotkeyLabel.Name = "HotkeyLabel";
            this.HotkeyLabel.Size = new System.Drawing.Size(41, 13);
            this.HotkeyLabel.TabIndex = 11;
            this.HotkeyLabel.Text = "Hotkey";
            // 
            // HotkeyBox
            // 
            this.HotkeyBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HotkeyBox.Location = new System.Drawing.Point(99, 67);
            this.HotkeyBox.Name = "HotkeyBox";
            this.HotkeyBox.Size = new System.Drawing.Size(302, 20);
            this.HotkeyBox.TabIndex = 12;
            // 
            // buttonConfigure
            // 
            this.buttonConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConfigure.Location = new System.Drawing.Point(326, 12);
            this.buttonConfigure.Name = "buttonConfigure";
            this.buttonConfigure.Size = new System.Drawing.Size(75, 23);
            this.buttonConfigure.TabIndex = 13;
            this.buttonConfigure.Text = "Configure";
            this.buttonConfigure.UseVisualStyleBackColor = true;
            this.buttonConfigure.Click += new System.EventHandler(this.buttonConfigure_Click);
            // 
            // buttonAdv
            // 
            this.buttonAdv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdv.Location = new System.Drawing.Point(18, 96);
            this.buttonAdv.Name = "buttonAdv";
            this.buttonAdv.Size = new System.Drawing.Size(75, 23);
            this.buttonAdv.TabIndex = 14;
            this.buttonAdv.Text = "Modify List";
            this.buttonAdv.UseVisualStyleBackColor = true;
            this.buttonAdv.Click += new System.EventHandler(this.buttonAdv_Click);
            // 
            // DescriptionLabelLabel
            // 
            this.DescriptionLabelLabel.AutoSize = true;
            this.DescriptionLabelLabel.Location = new System.Drawing.Point(33, 44);
            this.DescriptionLabelLabel.Name = "DescriptionLabelLabel";
            this.DescriptionLabelLabel.Size = new System.Drawing.Size(60, 13);
            this.DescriptionLabelLabel.TabIndex = 15;
            this.DescriptionLabelLabel.Text = "Description";
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionLabel.Location = new System.Drawing.Point(99, 40);
            this.DescriptionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(302, 21);
            this.DescriptionLabel.TabIndex = 16;
            this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PresetGroupDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(419, 137);
            this.Controls.Add(this.DescriptionLabel);
            this.Controls.Add(this.DescriptionLabelLabel);
            this.Controls.Add(this.buttonAdv);
            this.Controls.Add(this.buttonConfigure);
            this.Controls.Add(this.HotkeyBox);
            this.Controls.Add(this.HotkeyLabel);
            this.Controls.Add(this.ScriptLabel);
            this.Controls.Add(this.ScriptBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(289, 172);
            this.Name = "PresetGroupDialog";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Script Group";
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.Button buttonCancel;
            private System.Windows.Forms.Button buttonOk;
            private System.Windows.Forms.ComboBox ScriptBox;
            private System.Windows.Forms.Label ScriptLabel;
            private System.Windows.Forms.Label HotkeyLabel;
            private System.Windows.Forms.TextBox HotkeyBox;
            private System.Windows.Forms.Button buttonConfigure;
            private System.Windows.Forms.Button buttonAdv;
            private System.Windows.Forms.Label DescriptionLabelLabel;
            private System.Windows.Forms.Label DescriptionLabel;
        }
    }
}  
