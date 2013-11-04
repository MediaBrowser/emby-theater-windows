using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Core.Settings
{
    /// <summary>
    /// Interaction logic for SystemInfoPanel.xaml
    /// </summary>
    public partial class SystemInfoPanel : UserControl
    {
        private readonly IApplicationHost _appHost;
        private readonly IInstallationManager _installationManager;

        public SystemInfoPanel(IApplicationHost appHost, IInstallationManager installationManager)
        {
            _appHost = appHost;
            _installationManager = installationManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += SystemInfoPanel_Loaded;
            Unloaded += SystemInfoPanel_Unloaded;
            BtnUpdate.Click += BtnUpdate_Click;
            BtnRestart.Click += BtnRestart_Click;

            TxtVersion.Text = "Version " + _appHost.ApplicationVersion;
            LoadApplicationUpdates();
        }

        void SystemInfoPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _appHost.HasPendingRestartChanged += _appHost_HasPendingRestartChanged;

            _installationManager.PackageInstalling += _installationManager_PackageInstalling;
            _installationManager.PackageInstallationCompleted += _installationManager_PackageInstalling;
            _installationManager.PackageInstallationFailed += _installationManager_PackageInstallationFailed;
            _installationManager.PackageInstallationCancelled += _installationManager_PackageInstalling;

            UpdateInProgressInstallations();
            UpdateRestartPanelVisibility();
        }

        void _installationManager_PackageInstallationFailed(object sender, InstallationFailedEventArgs e)
        {
            Dispatcher.InvokeAsync(UpdateInProgressInstallations);
        }

        void _installationManager_PackageInstalling(object sender, InstallationEventArgs e)
        {
            Dispatcher.InvokeAsync(UpdateInProgressInstallations);
        }

        void SystemInfoPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            _appHost.HasPendingRestartChanged -= _appHost_HasPendingRestartChanged;

            _installationManager.PackageInstalling -= _installationManager_PackageInstalling;
            _installationManager.PackageInstallationCompleted -= _installationManager_PackageInstalling;
            _installationManager.PackageInstallationFailed -= _installationManager_PackageInstallationFailed;
            _installationManager.PackageInstallationCancelled -= _installationManager_PackageInstalling;
        }

        void _appHost_HasPendingRestartChanged(object sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(UpdateRestartPanelVisibility);
        }

        void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            _appHost.Restart();
        }

        async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var update = await _appHost.CheckForApplicationUpdate(CancellationToken.None, new Progress<double>());

            if (update.IsUpdateAvailable)
            {
                _appHost.UpdateApplication(update.Package, CancellationToken.None, new Progress<double>());

                LoadApplicationUpdates();
            }
        }

        private async void LoadApplicationUpdates()
        {
            if (_appHost.HasPendingRestart || _installationManager.CompletedInstallations.Any() || _installationManager.CurrentInstallations.Any())
            {
                PanelNewVersion.Visibility = Visibility.Collapsed;
                PanelUpToDate.Visibility = Visibility.Collapsed;
            }

            try
            {
                var update = await _appHost.CheckForApplicationUpdate(CancellationToken.None, new Progress<double>());

                if (update.IsUpdateAvailable)
                {
                    PanelNewVersion.Visibility = Visibility.Visible;
                    PanelUpToDate.Visibility = Visibility.Collapsed;

                    TxtNewVersion.Text = "Update now to version " + update.AvailableVersion;
                }
                else
                {
                    PanelNewVersion.Visibility = Visibility.Collapsed;
                    PanelUpToDate.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
                // Already logged at lower levels
                PanelUpToDate.Visibility = Visibility.Collapsed;
                PanelNewVersion.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateRestartPanelVisibility()
        {
            PanelRestart.Visibility = _appHost.HasPendingRestart ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateInProgressInstallations()
        {
            var currentInstalls = _installationManager.CurrentInstallations.ToList();

            currentInstalls.AddRange(_installationManager.CompletedInstallations.Select(i => new Tuple<InstallationInfo,CancellationTokenSource>(i, null)));

            InstallationsPanel.Visibility = currentInstalls.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            
            var currentlyDisplayedInstalls =
                InProgressInstallationsPanel.Children.OfType<InProgressInstallation>().ToList();

            // Update
            foreach (var install in currentlyDisplayedInstalls)
            {
                var info = currentInstalls.FirstOrDefault(i => i.Item1.Name == install.InstallationInfo.Name);

                if (info != null)
                {
                    install.UpdateInstallationInfo(info.Item1, info.Item2);
                }
            }
            
            // Add new ones
            foreach (var install in currentInstalls.Where(i => currentlyDisplayedInstalls.All(c => c.InstallationInfo.Name != i.Item1.Name)))
            {
                InProgressInstallationsPanel.Children.Add(new InProgressInstallation(install.Item1, install.Item2));
            }
        }
    }
}
