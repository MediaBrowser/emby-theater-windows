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
    namespace Shiandow.SuperRes
    {
        partial class SuperResConfigDialog
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
            this.StrengthSetter = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.PassesSetter = new System.Windows.Forms.NumericUpDown();
            this.PrescalerBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ConfigButton = new System.Windows.Forms.Button();
            this.HotkeyBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.AntiRingingSetter = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.AntiAliasingSetter = new System.Windows.Forms.NumericUpDown();
            this.SoftnessSetter = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SharpnessSetter = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.StrengthSetter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PassesSetter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AntiRingingSetter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AntiAliasingSetter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SoftnessSetter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SharpnessSetter)).BeginInit();
            this.SuspendLayout();
            // 
            // StrengthSetter
            // 
            this.StrengthSetter.DecimalPlaces = 2;
            this.StrengthSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.StrengthSetter.Location = new System.Drawing.Point(221, 65);
            this.StrengthSetter.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.StrengthSetter.Name = "StrengthSetter";
            this.StrengthSetter.Size = new System.Drawing.Size(44, 20);
            this.StrengthSetter.TabIndex = 3;
            this.StrengthSetter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.StrengthSetter.Value = new decimal(new int[] {
            8,
            0,
            0,
            65536});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(152, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Strength";
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Location = new System.Drawing.Point(139, 149);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(75, 23);
            this.ButtonOK.TabIndex = 8;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(220, 149);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 9;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Passes";
            // 
            // PassesSetter
            // 
            this.PassesSetter.Location = new System.Drawing.Point(81, 65);
            this.PassesSetter.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.PassesSetter.Name = "PassesSetter";
            this.PassesSetter.Size = new System.Drawing.Size(44, 20);
            this.PassesSetter.TabIndex = 2;
            this.PassesSetter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PassesSetter.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // PrescalerBox
            // 
            this.PrescalerBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PrescalerBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PrescalerBox.FormattingEnabled = true;
            this.PrescalerBox.Location = new System.Drawing.Point(81, 12);
            this.PrescalerBox.Name = "PrescalerBox";
            this.PrescalerBox.Size = new System.Drawing.Size(135, 21);
            this.PrescalerBox.TabIndex = 0;
            this.PrescalerBox.SelectedIndexChanged += new System.EventHandler(this.SelectionChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Prescaler";
            // 
            // ConfigButton
            // 
            this.ConfigButton.Location = new System.Drawing.Point(221, 10);
            this.ConfigButton.Name = "ConfigButton";
            this.ConfigButton.Size = new System.Drawing.Size(75, 23);
            this.ConfigButton.TabIndex = 6;
            this.ConfigButton.Text = "Configure";
            this.ConfigButton.UseVisualStyleBackColor = true;
            this.ConfigButton.Click += new System.EventHandler(this.ConfigButton_Click);
            // 
            // HotkeyBox
            // 
            this.HotkeyBox.Location = new System.Drawing.Point(81, 39);
            this.HotkeyBox.Name = "HotkeyBox";
            this.HotkeyBox.Size = new System.Drawing.Size(215, 20);
            this.HotkeyBox.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 42);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Hotkey";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(152, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Anti Ringing";
            // 
            // AntiRingingSetter
            // 
            this.AntiRingingSetter.DecimalPlaces = 2;
            this.AntiRingingSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.AntiRingingSetter.Location = new System.Drawing.Point(221, 117);
            this.AntiRingingSetter.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.AntiRingingSetter.Name = "AntiRingingSetter";
            this.AntiRingingSetter.Size = new System.Drawing.Size(44, 20);
            this.AntiRingingSetter.TabIndex = 7;
            this.AntiRingingSetter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AntiRingingSetter.Value = new decimal(new int[] {
            8,
            0,
            0,
            65536});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(152, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Anti Aliasing";
            // 
            // AntiAliasingSetter
            // 
            this.AntiAliasingSetter.DecimalPlaces = 2;
            this.AntiAliasingSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.AntiAliasingSetter.Location = new System.Drawing.Point(221, 91);
            this.AntiAliasingSetter.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.AntiAliasingSetter.Name = "AntiAliasingSetter";
            this.AntiAliasingSetter.Size = new System.Drawing.Size(44, 20);
            this.AntiAliasingSetter.TabIndex = 5;
            this.AntiAliasingSetter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AntiAliasingSetter.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            // 
            // SoftnessSetter
            // 
            this.SoftnessSetter.DecimalPlaces = 2;
            this.SoftnessSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.SoftnessSetter.Location = new System.Drawing.Point(81, 117);
            this.SoftnessSetter.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.SoftnessSetter.Name = "SoftnessSetter";
            this.SoftnessSetter.Size = new System.Drawing.Size(44, 20);
            this.SoftnessSetter.TabIndex = 6;
            this.SoftnessSetter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SoftnessSetter.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sharpness";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 119);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Softness";
            // 
            // SharpnessSetter
            // 
            this.SharpnessSetter.DecimalPlaces = 2;
            this.SharpnessSetter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.SharpnessSetter.Location = new System.Drawing.Point(81, 91);
            this.SharpnessSetter.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.SharpnessSetter.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            -2147483648});
            this.SharpnessSetter.Name = "SharpnessSetter";
            this.SharpnessSetter.Size = new System.Drawing.Size(44, 20);
            this.SharpnessSetter.TabIndex = 4;
            this.SharpnessSetter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SharpnessSetter.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // SuperResConfigDialog
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(307, 184);
            this.Controls.Add(this.SharpnessSetter);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SoftnessSetter);
            this.Controls.Add(this.HotkeyBox);
            this.Controls.Add(this.AntiAliasingSetter);
            this.Controls.Add(this.ConfigButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.AntiRingingSetter);
            this.Controls.Add(this.PrescalerBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.PassesSetter);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StrengthSetter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SuperResConfigDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SuperRes Settings";
            ((System.ComponentModel.ISupportInitialize)(this.StrengthSetter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PassesSetter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AntiRingingSetter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AntiAliasingSetter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SoftnessSetter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SharpnessSetter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.NumericUpDown StrengthSetter;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.Button ButtonOK;
            private System.Windows.Forms.Button ButtonCancel;
            private System.Windows.Forms.Label label5;
            private System.Windows.Forms.NumericUpDown PassesSetter;
            private System.Windows.Forms.ComboBox PrescalerBox;
            private System.Windows.Forms.Label label6;
            private System.Windows.Forms.Button ConfigButton;
            private System.Windows.Forms.TextBox HotkeyBox;
            private System.Windows.Forms.Label label7;
            private System.Windows.Forms.Label label4;
            private System.Windows.Forms.NumericUpDown AntiRingingSetter;
            private System.Windows.Forms.Label label3;
            private System.Windows.Forms.NumericUpDown AntiAliasingSetter;
            private System.Windows.Forms.NumericUpDown SoftnessSetter;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.Label label8;
            private System.Windows.Forms.NumericUpDown SharpnessSetter;

        }
    }
}
