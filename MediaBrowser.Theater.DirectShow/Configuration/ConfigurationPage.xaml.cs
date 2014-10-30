using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Model.IO;
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
        private readonly IZipClient _zipClient;

        public ConfigurationPage(INavigationService nav, ITheaterConfigurationManager config, IPresentationManager presentation, IMediaFilters mediaFilters, IZipClient zipClient)
        {
            _nav = nav;
            _config = config;
            _presentation = presentation;
            _mediaFilters = mediaFilters;
            this._zipClient = zipClient;
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
                 new SelectListItem{ Text = "Nvidia CUVID", Value="1"},
                 new SelectListItem{ Text = "QuickSync", Value="2"},
                 new SelectListItem{ Text = "DXVA2CopyBack", Value="3"}
            };

            SelectAudioBitstreamingMode.Options = new List<SelectListItem>();

            foreach (string bsOption in Enum.GetNames(typeof(BitstreamChoice)))
            {
                SelectAudioBitstreamingMode.Options.Add(new SelectListItem { Text = bsOption, Value = bsOption });
            }

            SelectAudioRenderer.Options = new List<SelectListItem>();

            foreach (string bsOption in Enum.GetNames(typeof(AudioRendererChoice)))
            {
                SelectAudioRenderer.Options.Add(new SelectListItem { Text = bsOption, Value = bsOption });
            }

            SelectMaxStreamingBitrate.Options = new List<SelectListItem>();

            for (var i = 1; i <= 40; i++)
            {
                SelectMaxStreamingBitrate.Options.Add(new SelectListItem { Text = i.ToString(CultureInfo.InvariantCulture) + " Mbps", Value = i.ToString(CultureInfo.InvariantCulture) });
            }

            SelectFilterSet.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "Stable", Value=""},
                 new SelectListItem{ Text = "Edge", Value="edge"}
            };

            SelectVideoRenderer.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "EVR+", Value="EVRCP"},
                 new SelectListItem{ Text = "madVR", Value="madVR"},
                 new SelectListItem{ Text = "EVR", Value="EVR"}
            };
        }

        async void BtnConfigureSubtitles_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    _mediaFilters.LaunchLavSplitterConfiguration();
            //}
            //catch
            //{
            //    _presentation.ShowDefaultErrorMessage();
            //}
            await _nav.Navigate(new SplitterSettingsPage(_nav, _config, _presentation, _mediaFilters));
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

            //ChkEnableReclock.IsChecked = config.EnableReclock;
            //ChkEnableMadvr.IsChecked = config.VideoConfig.EnableMadvr;
            if (config.VideoConfig.EnableMadvr)
                SelectVideoRenderer.SelectedValue = "madVR";
            else if (config.VideoConfig.UseCustomPresenter)
                SelectVideoRenderer.SelectedValue = "EVRCP";
            else
                SelectVideoRenderer.SelectedValue = "EVR";

            ChkEnableXySubFilter.IsChecked = config.SubtitleConfig.EnableXySubFilter;
            SelectAudioBitstreamingMode.SelectedValue = config.AudioConfig.AudioBitstreaming.ToString();
            SelectHwaMode.SelectedValue = config.VideoConfig.HwaMode.ToString();
            SelectAudioRenderer.SelectedValue = config.AudioConfig.Renderer.ToString();
            SelectMaxStreamingBitrate.SelectedValue = (_config.Configuration.MaxStreamingBitrate / 1000000).ToString(CultureInfo.InvariantCulture);
            ChkEnableAutoRes.IsChecked = config.VideoConfig.AutoChangeRefreshRate;
            SelectFilterSet.SelectedValue = config.FilterSet;
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;
            bool redownloadFilters = false;

            //config.EnableReclock = ChkEnableReclock.IsChecked ?? false;
            //config.VideoConfig.EnableMadvr = ChkEnableMadvr.IsChecked ?? false;
            switch (SelectVideoRenderer.SelectedValue)
            {
                case "madVR":
                    config.VideoConfig.EnableMadvr = true;
                    config.VideoConfig.UseCustomPresenter = true;
                    break;
                case "EVRCP":
                    config.VideoConfig.EnableMadvr = false;
                    config.VideoConfig.UseCustomPresenter = true;
                    break;
                case "EVR":
                    config.VideoConfig.EnableMadvr = false;
                    config.VideoConfig.UseCustomPresenter = false;
                    break;
            }
            config.SubtitleConfig.EnableXySubFilter = ChkEnableXySubFilter.IsChecked ?? false;

            config.AudioConfig.AudioBitstreaming = (BitstreamChoice)Enum.Parse(typeof(BitstreamChoice), SelectAudioBitstreamingMode.SelectedValue);
            config.AudioConfig.Renderer = (AudioRendererChoice)Enum.Parse(typeof(AudioRendererChoice), SelectAudioRenderer.SelectedValue);

            config.VideoConfig.HwaMode = int.Parse(SelectHwaMode.SelectedValue);
            config.VideoConfig.AutoChangeRefreshRate = ChkEnableAutoRes.IsChecked ?? false;
            _config.Configuration.MaxStreamingBitrate = int.Parse(SelectMaxStreamingBitrate.SelectedValue) * 1000000;
            if (config.FilterSet != SelectFilterSet.SelectedValue)
            {
                redownloadFilters = true;
                config.FilterSet = SelectFilterSet.SelectedValue;
            }
            _config.SaveConfiguration();
            if (redownloadFilters)
                URCOMLoader.EnsureObjects(_config, _zipClient, false, true);
        }

        async void BtnConfigureMadVr_Click(object sender, RoutedEventArgs e)
        {
            await _nav.Navigate(new MadVrSettingsPage(_nav, _config, _presentation, _mediaFilters));
        }
    }
}
