using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;

namespace MediaBrowser.Theater.DirectShow.Configuration
{
    /// <summary>
    /// Interaction logic for ConfigurationPage.xaml
    /// </summary>
    public partial class ConfigurationPage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;

        public ConfigurationPage(ITheaterConfigurationManager config)
        {
            _config = config;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += GeneralSettingsPage_Loaded;
            Unloaded += GeneralSettingsPage_Unloaded;
        }

        void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            ChkEnableReclock.IsChecked = config.EnableReclock;
            ChkEnableMadvr.IsChecked = config.EnableMadvr;
            ChkEnableXySubFilter.IsChecked = config.EnableXySubFilter;
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            config.EnableReclock = ChkEnableReclock.IsChecked ?? false;
            config.EnableMadvr = ChkEnableMadvr.IsChecked ?? false;
            config.EnableXySubFilter = ChkEnableXySubFilter.IsChecked ?? false;
            
            _config.SaveConfiguration();
        }
    }
}
