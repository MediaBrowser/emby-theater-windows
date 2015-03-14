using System.IO;
using System.Windows.Forms;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Mpdn.Lut3D
    {
        public partial class Lut3DConfigDialog : Lut3DConfigDialogBase
        {
            public Lut3DConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                checkBoxActivate.Checked = Settings.Activate;
                textBoxFileName.Text = Settings.FileName;
            }

            protected override void SaveSettings()
            {
                Settings.Activate = checkBoxActivate.Checked;
                Settings.FileName = textBoxFileName.Text;
            }

            private void ButtonOkClick(object sender, System.EventArgs e)
            {
                if (!ValidateTextBox(true))
                {
                    DialogResult = DialogResult.None;
                }
            }

            private void ButtonOpenClick(object sender, System.EventArgs e)
            {
                if (openFileDialog.ShowDialog(this) != DialogResult.OK) 
                    return;

                textBoxFileName.Text = openFileDialog.FileName;
                ValidateTextBox();
            }

            private void TextBoxFileNameValidating(object sender, System.ComponentModel.CancelEventArgs e)
            {
                ValidateTextBox();
            }

            private bool ValidateTextBox(bool showNoFileError = false)
            {
                if (checkBoxActivate.Checked)
                {
                    if (string.IsNullOrWhiteSpace(textBoxFileName.Text))
                    {
                        if (showNoFileError)
                        {
                            errorProvider.SetError(textBoxFileName, "No 3D LUT file has been selected");
                        }
                        return false;
                    }

                    if (!File.Exists(textBoxFileName.Text))
                    {
                        textBoxFileName.Focus();
                        errorProvider.SetError(textBoxFileName, "File not found");
                        return false;
                    }
                }

                errorProvider.SetError(textBoxFileName, string.Empty);
                return true;
            }

            private void CheckBoxActivateCheckedChanged(object sender, System.EventArgs e)
            {
                ValidateTextBox();
            }
        }

        public class Lut3DConfigDialogBase : ScriptConfigDialog<Lut3D>
        {
        }
    }
}