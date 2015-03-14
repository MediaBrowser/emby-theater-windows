namespace Mpdn.RenderScript
{
    namespace Shiandow.Nedi
    {
        partial class NediConfigDialog
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
            this.checkBoxAlwaysEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxCentered = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(245, 90);
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
            this.buttonOk.Location = new System.Drawing.Point(164, 90);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1004;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // checkBoxAlwaysEnabled
            // 
            this.checkBoxAlwaysEnabled.AutoSize = true;
            this.checkBoxAlwaysEnabled.Location = new System.Drawing.Point(21, 25);
            this.checkBoxAlwaysEnabled.Name = "checkBoxAlwaysEnabled";
            this.checkBoxAlwaysEnabled.Size = new System.Drawing.Size(270, 17);
            this.checkBoxAlwaysEnabled.TabIndex = 1006;
            this.checkBoxAlwaysEnabled.Text = "Use NEDI image doubling even when not upscaling";
            this.checkBoxAlwaysEnabled.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBoxCentered.AutoSize = true;
            this.checkBoxCentered.Location = new System.Drawing.Point(21, 48);
            this.checkBoxCentered.Name = "checkBox1";
            this.checkBoxCentered.Size = new System.Drawing.Size(106, 17);
            this.checkBoxCentered.TabIndex = 1007;
            this.checkBoxCentered.Text = "Center the image";
            this.checkBoxCentered.UseVisualStyleBackColor = true;
            // 
            // NediConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(332, 125);
            this.Controls.Add(this.checkBoxCentered);
            this.Controls.Add(this.checkBoxAlwaysEnabled);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NediConfigDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NEDI Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.Button buttonCancel;
            private System.Windows.Forms.Button buttonOk;
            private System.Windows.Forms.CheckBox checkBoxAlwaysEnabled;
            private System.Windows.Forms.CheckBox checkBoxCentered;

        }
    }
}