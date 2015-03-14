using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Shiandow.Nedi
    {
        public partial class NediConfigDialog : NediConfigDialogBase
        {
            public NediConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                checkBoxAlwaysEnabled.Checked = Settings.AlwaysDoubleImage;
                checkBoxCentered.Checked = Settings.Centered;
            }

            protected override void SaveSettings()
            {
                Settings.AlwaysDoubleImage = checkBoxAlwaysEnabled.Checked;
                Settings.Centered = checkBoxCentered.Checked;
            }
        }

        public class NediConfigDialogBase : ScriptConfigDialog<Nedi>
        {
        }
    }
}
