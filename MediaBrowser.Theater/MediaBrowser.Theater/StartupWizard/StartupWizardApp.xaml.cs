using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace MediaBrowser.Theater.StartupWizard
{
    public partial class StartupWizardApp
    {
        public StartupWizardApp()
        {
            InitializeComponent();

            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;
        }
    }
}