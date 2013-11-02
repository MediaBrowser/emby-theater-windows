using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;

namespace MediaBrowser.Theater.Core.Advanced
{
    /// <summary>
    /// Interaction logic for AdvancedPage.xaml
    /// </summary>
    public partial class AdvancedPage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;

        public AdvancedPage(ITheaterConfigurationManager config)
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
            ChkEnableHighQualityImageScaling.IsChecked = _config.Configuration.EnableHighQualityImageScaling;
            ChkEnableDebugLogging.IsChecked = _config.Configuration.EnableDebugLevelLogging;
            ChkEnableBackdrops.IsChecked = _config.Configuration.EnableBackdrops;
            ChkDownloadCompressedImages.IsChecked = _config.Configuration.DownloadCompressedImages;
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _config.Configuration.EnableDebugLevelLogging = ChkEnableDebugLogging.IsChecked ?? false;
            _config.Configuration.EnableHighQualityImageScaling = ChkEnableHighQualityImageScaling.IsChecked ?? false;
            _config.Configuration.EnableBackdrops = ChkEnableBackdrops.IsChecked ?? false;
            _config.Configuration.DownloadCompressedImages = ChkDownloadCompressedImages.IsChecked ?? false;

            _config.SaveConfiguration();
        }
    }
}
