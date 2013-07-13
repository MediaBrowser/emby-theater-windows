using MediaBrowser.Common;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MediaBrowser.Theater.Vlc.Configuration
{
    /// <summary>
    /// Interaction logic for VlcConfigPage.xaml
    /// </summary>
    public partial class VlcConfigPage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IApplicationHost _appHost;

        public VlcConfigPage(ITheaterConfigurationManager config, IApplicationHost appHost)
        {
            _config = config;
            _appHost = appHost;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += GeneralSettingsPage_Loaded;
            Unloaded += GeneralSettingsPage_Unloaded;

            SelectAudioChannelLayout.Options = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Stereo", Value= AudioLayout.Stereo.ToString()},
                new SelectListItem{ Text = "5.1", Value= AudioLayout.Five1.ToString()},
                new SelectListItem{ Text = "6.1", Value= AudioLayout.Six1.ToString()},
                new SelectListItem{ Text = "7.1", Value= AudioLayout.Seven1.ToString()},
                new SelectListItem{ Text = "Mono", Value= AudioLayout.Mono.ToString()},
                new SelectListItem{ Text = "Spdif", Value= AudioLayout.Spdif.ToString()}
            };
        }

        void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ChkEnableGpuAcceleration.IsChecked = _config.Configuration.VlcConfiguration.EnableGpuAcceleration;

            SelectAudioChannelLayout.SelectedValue = _config.Configuration.VlcConfiguration.AudioLayout.ToString();
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var enableGpuAcceleration = ChkEnableGpuAcceleration.IsChecked ?? false;
            var requiresRestart = enableGpuAcceleration != _config.Configuration.VlcConfiguration.EnableGpuAcceleration;

            _config.Configuration.VlcConfiguration.EnableGpuAcceleration = enableGpuAcceleration;

            _config.SaveConfiguration();

            AudioLayout layout;

            if (Enum.TryParse(SelectAudioChannelLayout.SelectedValue, true, out layout))
            {
                _config.Configuration.VlcConfiguration.AudioLayout = layout;
            }

            if (requiresRestart)
            {
                _appHost.NotifyPendingRestart();
            }
        }
    }
}
