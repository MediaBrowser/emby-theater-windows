using Mpdn.Config;

namespace Mpdn.PlayerExtensions
{
    namespace GitHub
    {
        public partial class MouseControlConfigDialog : MouseControlConfigBase
        {
            public MouseControlConfigDialog()
            {
                InitializeComponent();
            }

            protected override void LoadSettings()
            {
                checkBoxMouseWheelSeek.Checked = Settings.EnableMouseWheelSeek;
                checkBoxFsToggle.Checked = Settings.EnableMiddleClickFsToggle;
            }

            protected override void SaveSettings()
            {
                Settings.EnableMouseWheelSeek = checkBoxMouseWheelSeek.Checked;
                Settings.EnableMiddleClickFsToggle = checkBoxFsToggle.Checked;
            }
        }

        public class MouseControlConfigBase : ScriptConfigDialog<MouseControlSettings>
        {
        }
    }
}
