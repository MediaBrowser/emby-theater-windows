using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.System;
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
        private readonly IMediaFilters _mediaFilters;
        private readonly IPresentationManager _presentation;

        public ConfigurationPage(ITheaterConfigurationManager config, IPresentationManager presentation, IMediaFilters mediaFilters)
        {
            _config = config;
            _presentation = presentation;
            _mediaFilters = mediaFilters;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += GeneralSettingsPage_Loaded;
            Unloaded += GeneralSettingsPage_Unloaded;

            BtnConfigureAudio.Click += BtnConfigureAudio_Click;
            BtnConfigureSubtitles.Click += BtnConfigureSubtitles_Click;
        }

        void BtnConfigureSubtitles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mediaFilters.LaunchLavSplitterConfiguration();
            }
            catch 
            {
                _presentation.ShowDefaultErrorMessage();
            }
        }

        void BtnConfigureAudio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mediaFilters.LaunchLavAudioConfiguration();
            }
            catch
            {
                _presentation.ShowDefaultErrorMessage();
            }
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
