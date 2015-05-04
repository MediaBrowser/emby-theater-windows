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
    namespace Shiandow.Chroma
    {
        partial class ChromaScalerConfigDialog
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
            this.BSetter = new System.Windows.Forms.NumericUpDown();
            this.CSetter = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.PresetBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.BSetter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CSetter)).BeginInit();
            this.SuspendLayout();
            // 
            // BSetter
            // 
            this.BSetter.DecimalPlaces = 2;
            this.BSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.BSetter.Location = new System.Drawing.Point(51, 17);
            this.BSetter.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.BSetter.Name = "BSetter";
            this.BSetter.Size = new System.Drawing.Size(44, 20);
            this.BSetter.TabIndex = 0;
            this.BSetter.Value = new decimal(new int[] {
            35,
            0,
            0,
            131072});
            this.BSetter.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // CSetter
            // 
            this.CSetter.DecimalPlaces = 2;
            this.CSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.CSetter.Location = new System.Drawing.Point(128, 17);
            this.CSetter.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CSetter.Name = "CSetter";
            this.CSetter.Size = new System.Drawing.Size(44, 20);
            this.CSetter.TabIndex = 1;
            this.CSetter.Value = new decimal(new int[] {
            35,
            0,
            0,
            131072});
            this.CSetter.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "B";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(108, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "C";
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Location = new System.Drawing.Point(50, 106);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(75, 23);
            this.ButtonOK.TabIndex = 3;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(131, 106);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 4;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // PresetBox
            // 
            this.PresetBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PresetBox.FormattingEnabled = true;
            this.PresetBox.Location = new System.Drawing.Point(51, 60);
            this.PresetBox.Name = "PresetBox";
            this.PresetBox.Size = new System.Drawing.Size(121, 21);
            this.PresetBox.TabIndex = 2;
            this.PresetBox.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Preset";
            // 
            // ChromaScalerConfigDialog
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(218, 141);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PresetBox);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CSetter);
            this.Controls.Add(this.BSetter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChromaScalerConfigDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ChromaScaler Settings";
            ((System.ComponentModel.ISupportInitialize)(this.BSetter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CSetter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.NumericUpDown BSetter;
            private System.Windows.Forms.NumericUpDown CSetter;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.Button ButtonOK;
            private System.Windows.Forms.Button ButtonCancel;
            private System.Windows.Forms.ComboBox PresetBox;
            private System.Windows.Forms.Label label3;

        }
    }
}
