using System.Collections.Generic;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.System;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;
using MediaBrowser.Theater.Interfaces.Navigation;

namespace MediaBrowser.Theater.DirectShow.Configuration
{
    /// <summary>
    /// Interaction logic for ConfigurationPage.xaml
    /// </summary>
    public partial class ConfigurationPage : BasePage
    {
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IMediaFilters _mediaFilters;
        private readonly IPresentationManager _presentation;

        public ConfigurationPage(INavigationService nav, ITheaterConfigurationManager config, IPresentationManager presentation, IMediaFilters mediaFilters)
        {
            _nav = nav;
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

            SelectHwaMode.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "Auto", Value="-1"},
                 new SelectListItem{ Text = "Disabled", Value="0"},
                 new SelectListItem{ Text = "QuickSync", Value="2"},
                 new SelectListItem{ Text = "DXVA2CopyBack", Value="3"}
            };

            SelectudioBitstreamingMode.Options = new List<SelectListItem>();

            foreach (string bsOption in Enum.GetNames(typeof(BitstreamChoice)))
            {
                SelectudioBitstreamingMode.Options.Add(new SelectListItem { Text = bsOption, Value = bsOption });
            }

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

        async void BtnConfigureAudio_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    _mediaFilters.LaunchLavAudioConfiguration();
            //}
            //catch
            //{
            //    _presentation.ShowDefaultErrorMessage();
            //}
            await _nav.Navigate(new LavAudioSettingsPage(_nav, _config, _presentation, _mediaFilters));
        }

        void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            ChkEnableReclock.IsChecked = config.EnableReclock;
            ChkEnableMadvr.IsChecked = config.EnableMadvr;
            ChkEnableXySubFilter.IsChecked = config.EnableXySubFilter;
            SelectudioBitstreamingMode.SelectedValue = config.AudioConfig.AudioBitstreaming.ToString();
            SelectHwaMode.SelectedValue = config.VideoConfig.HwaMode.ToString();
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            config.EnableReclock = ChkEnableReclock.IsChecked ?? false;
            config.EnableMadvr = ChkEnableMadvr.IsChecked ?? false;
            config.EnableXySubFilter = ChkEnableXySubFilter.IsChecked ?? false;

            config.AudioConfig.AudioBitstreaming = (BitstreamChoice)Enum.Parse(typeof(BitstreamChoice), SelectudioBitstreamingMode.SelectedValue);

            config.VideoConfig.HwaMode = int.Parse(SelectHwaMode.SelectedValue);

            _config.SaveConfiguration();
        }

        async void BtnConfigureMadVr_Click(object sender, RoutedEventArgs e)
        {
            await _nav.Navigate(new MadVrSettingsPage(_nav, _config, _presentation, _mediaFilters));
        }
    }
}
