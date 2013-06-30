using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Theming;
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
        private readonly IThemeManager _themeManager;

        public PluginCatalog(IPackageManager packageManager, IThemeManager themeManager)
        {
            _packageManager = packageManager;
            _themeManager = themeManager;
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
                    CateogriesPanel.Children.Add(new PluginCategory(category.Key, category.ToList()));
                }
            }
            catch (HttpException)
            {
                _themeManager.CurrentTheme.ShowDefaultErrorMessage();
            }

            TxtPleaseWait.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
