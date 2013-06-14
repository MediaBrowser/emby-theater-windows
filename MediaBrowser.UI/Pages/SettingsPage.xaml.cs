using MediaBrowser.Theater.Interfaces.Theming;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        /// <summary>
        /// The _theme manager
        /// </summary>
        private readonly IThemeManager _themeManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        /// <param name="themeManager">The theme manager.</param>
        public SettingsPage(IThemeManager themeManager)
        {
            _themeManager = themeManager;
            InitializeComponent();

            Loaded += SettingsPage_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the SettingsPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _themeManager.CurrentTheme.SetPageTitle("Settings");
        }
    }
}
