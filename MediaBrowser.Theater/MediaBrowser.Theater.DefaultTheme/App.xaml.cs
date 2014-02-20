using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace MediaBrowser.Theater.DefaultTheme
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        public App()
        {
            InitializeComponent();
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;
        }
    }
}