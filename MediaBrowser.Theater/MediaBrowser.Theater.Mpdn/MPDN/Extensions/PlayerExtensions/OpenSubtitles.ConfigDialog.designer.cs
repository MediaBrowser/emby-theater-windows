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
namespace Mpdn.PlayerExtensions.GitHub
{
    partial class OpenSubtitlesConfigDialog
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
            this.components = new System.ComponentModel.Container();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.checkBoxEnableAutoDownloader = new System.Windows.Forms.CheckBox();
            this.comboBoxPrefLanguage = new System.Windows.Forms.ComboBox();
            this.cultureBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.labelPrefLang = new System.Windows.Forms.Label();
            this.toolTipComboBox = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.cultureBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(206, 117);
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
            this.buttonOk.Location = new System.Drawing.Point(125, 117);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1004;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableAutoDownloader
            // 
            this.checkBoxEnableAutoDownloader.AutoSize = true;
            this.checkBoxEnableAutoDownloader.Location = new System.Drawing.Point(22, 22);
            this.checkBoxEnableAutoDownloader.Name = "checkBoxEnableAutoDownloader";
            this.checkBoxEnableAutoDownloader.Size = new System.Drawing.Size(210, 17);
            this.checkBoxEnableAutoDownloader.TabIndex = 1006;
            this.checkBoxEnableAutoDownloader.Text = "Enable OpenSubtitles auto downloader";
            this.checkBoxEnableAutoDownloader.UseVisualStyleBackColor = true;
            // 
            // comboBoxPrefLanguage
            // 
            this.comboBoxPrefLanguage.DataSource = this.cultureBindingSource;
            this.comboBoxPrefLanguage.DisplayMember = "EnglishName";
            this.comboBoxPrefLanguage.FormattingEnabled = true;
            this.comboBoxPrefLanguage.Location = new System.Drawing.Point(125, 43);
            this.comboBoxPrefLanguage.Name = "comboBoxPrefLanguage";
            this.comboBoxPrefLanguage.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPrefLanguage.TabIndex = 1007;
            this.comboBoxPrefLanguage.ValueMember = "EnglishName";
            this.comboBoxPrefLanguage.MouseEnter += new System.EventHandler(this.comboBoxPrefLanguage_MouseEnter);
            // 
            // cultureBindingSource
            // 
            this.cultureBindingSource.DataSource = typeof(System.Globalization.CultureInfo);
            // 
            // labelPrefLang
            // 
            this.labelPrefLang.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPrefLang.AutoSize = true;
            this.labelPrefLang.Location = new System.Drawing.Point(19, 46);
            this.labelPrefLang.Name = "labelPrefLang";
            this.labelPrefLang.Size = new System.Drawing.Size(104, 13);
            this.labelPrefLang.TabIndex = 1008;
            this.labelPrefLang.Text = "Preferred Language:";
            // 
            // OpenSubtitlesConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(293, 152);
            this.Controls.Add(this.labelPrefLang);
            this.Controls.Add(this.comboBoxPrefLanguage);
            this.Controls.Add(this.checkBoxEnableAutoDownloader);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenSubtitlesConfigDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OpenSubtitles Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.cultureBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.CheckBox checkBoxEnableAutoDownloader;
        private System.Windows.Forms.ComboBox comboBoxPrefLanguage;
        private System.Windows.Forms.BindingSource cultureBindingSource;
        private System.Windows.Forms.Label labelPrefLang;
        private System.Windows.Forms.ToolTip toolTipComboBox;

    }
}
