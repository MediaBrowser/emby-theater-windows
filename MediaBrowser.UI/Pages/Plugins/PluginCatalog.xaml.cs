using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for PluginCatalog.xaml
    /// </summary>
    public partial class PluginCatalog : UserControl
    {
        private readonly IPackageManager _packageManager;
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IApplicationHost _appHost;

        public PluginCatalog(IPackageManager packageManager, IPresentationManager presentation, INavigationService nav, IApplicationHost appHost)
        {
            _packageManager = packageManager;
            _presentation = presentation;
            _nav = nav;
            _appHost = appHost;
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
                var packages = await _packageManager.GetAvailablePackagesWithoutRegistrationInfo(CancellationToken.None);

                var categories = packages
                    .Where(i => i.type == PackageType.UserInstalled && i.targetSystem == PackageTargetSystem.MBTheater)
                    .OrderBy(i => string.IsNullOrEmpty(i.category) ? "General" : i.category)
                    .GroupBy(i => string.IsNullOrEmpty(i.category) ? "General" : i.category)
                    .ToList();

                foreach (var category in categories)
                {
                    CateogriesPanel.Children.Add(new PluginCategory(category.Key, category.ToList(), _nav, _appHost));
                }
            }
            catch (HttpException)
            {
                _presentation.ShowDefaultErrorMessage();
            }

            TxtPleaseWait.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
