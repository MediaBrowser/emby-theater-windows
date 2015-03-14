using System;
using System.Windows.Forms;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Mpdn.Resizer
    {
        public partial class ResizerConfigDialog : ResizerConfigDialogBase
        {
            public ResizerConfigDialog()
            {
                InitializeComponent();

                var descs = EnumHelpers.GetDescriptions<ResizerOption>();
                foreach (var desc in descs)
                {
                    listBox.Items.Add(desc);
                }

                listBox.SelectedIndex = 0;
            }

            protected override void LoadSettings()
            {
                listBox.SelectedIndex = (int) Settings.ResizerOption;
            }

            protected override void SaveSettings()
            {
                Settings.ResizerOption = (ResizerOption) listBox.SelectedIndex;
            }

            private void ListBoxDoubleClick(object sender, EventArgs e)
            {
                DialogResult = DialogResult.OK;
            }
        }

        public class ResizerConfigDialogBase : ScriptConfigDialog<Resizer>
        {
        }
    }
}