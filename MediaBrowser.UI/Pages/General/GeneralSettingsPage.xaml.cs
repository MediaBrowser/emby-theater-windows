using MediaBrowser.Common;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace MediaBrowser.UI.Pages.General
{
    /// <summary>
    /// Interaction logic for GeneralSettingsPage.xaml
    /// </summary>
    public partial class GeneralSettingsPage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IApplicationHost _appHost;

        private PackageVersionInfo _availableAppUpdate;

        public GeneralSettingsPage(ITheaterConfigurationManager config, IApplicationHost appHost)
        {
            _config = config;
            _appHost = appHost;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            SelectUpdateLevel.Options = new List<SelectListItem> 
            { 
                new SelectListItem{ Text = "Official Release", Value = PackageVersionClass.Release.ToString()},
                new SelectListItem{ Text = "Beta", Value = PackageVersionClass.Beta.ToString()},
                new SelectListItem{ Text = "Dev", Value = PackageVersionClass.Dev.ToString()}
            };

            Loaded += GeneralSettingsPage_Loaded;
            Unloaded += GeneralSettingsPage_Unloaded;
            BtnUpdate.Click += BtnUpdate_Click;
        }

        void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
        }

        void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtVersion.Text = "Version " + _appHost.ApplicationVersion;
            SelectUpdateLevel.SelectedValue = _config.Configuration.SystemUpdateLevel.ToString();

            ChkAutoRun.IsChecked = _config.Configuration.RunAtStartup;

            LoadApplicationUpdates();
        }

        private async void LoadApplicationUpdates()
        {
            try
            {
                var update = await _appHost.CheckForApplicationUpdate(CancellationToken.None, new Progress<double>());

                if (update.IsUpdateAvailable)
                {
                    PanelNewVersion.Visibility = Visibility.Visible;
                    PanelUpToDate.Visibility = Visibility.Collapsed;

                    TxtNewVersion.Text = "Update now to version " + update.AvailableVersion + ".";

                    _availableAppUpdate = update.Package;
                }
                else
                {
                    PanelNewVersion.Visibility = Visibility.Collapsed;
                    PanelUpToDate.Visibility = Visibility.Visible;
                }
            }
            catch (HttpException)
            {
                // Already logged at lower levels
                PanelUpToDate.Visibility = Visibility.Collapsed;
                PanelNewVersion.Visibility = Visibility.Collapsed;
            }
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            PackageVersionClass updateLevel;

            if (Enum.TryParse(SelectUpdateLevel.SelectedValue, out updateLevel))
            {
                _config.Configuration.SystemUpdateLevel = updateLevel;
            }

            _config.Configuration.RunAtStartup = ChkAutoRun.IsChecked ?? false;

            _config.SaveConfiguration();
        }
    }
}
