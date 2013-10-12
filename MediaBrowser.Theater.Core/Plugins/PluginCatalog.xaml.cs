using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for PluginCatalog.xaml
    /// </summary>
    public partial class PluginCatalog : UserControl
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IApplicationHost _appHost;
        private readonly IInstallationManager _installationManager;

        public PluginCatalog(IPresentationManager presentation, INavigationService nav, IApplicationHost appHost, IInstallationManager installationManager)
        {
            _presentation = presentation;
            _nav = nav;
            _appHost = appHost;
            _installationManager = installationManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LoadPackages();
        }

        private async void LoadPackages()
        {
            try
            {
                var packages = await _installationManager.GetAvailablePackagesWithoutRegistrationInfo(CancellationToken.None);

                packages = packages.Where(i => i.versions != null && i.versions.Count > 0);

                var categories = packages
                    .Where(i => i.type == PackageType.UserInstalled && i.targetSystem == PackageTargetSystem.MBTheater)
                    .OrderBy(i => string.IsNullOrEmpty(i.category) ? "General" : i.category)
                    .GroupBy(i => string.IsNullOrEmpty(i.category) ? "General" : i.category)
                    .ToList();

                foreach (var category in categories)
                {
                    CateogriesPanel.Children.Add(new PluginCategory(category.Key, category.ToList(), _nav, _appHost, _installationManager));
                }
            }
            catch (Exception)
            {
                _presentation.ShowDefaultErrorMessage();
            }

            TxtPleaseWait.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
