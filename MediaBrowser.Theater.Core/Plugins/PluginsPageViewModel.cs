using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Core.Plugins
{
    public class PluginsPageViewModel : TabbedViewModel
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;

        private readonly IApplicationHost _appHost;

        public PluginsPageViewModel(IApplicationHost appHost, INavigationService nav, IInstallationManager installationManager, IPresentationManager presentationManager)
        {
            _appHost = appHost;
            _nav = nav;
            _installationManager = installationManager;
            _presentationManager = presentationManager;
        }

        protected override Task<IEnumerable<TabItem>> GetSections()
        {
            var list = new List<TabItem>();

            list.Add(new TabItem
            {
                DisplayName = "Installed Plugins",
                Name = "Installed Plugins"
            });

            list.Add(new TabItem
            {
                DisplayName = "Plugin Catalog",
                Name = "Plugin Catalog"
            });
            
            return Task.FromResult<IEnumerable<TabItem>>(list);
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            if (string.Equals(section, "Installed Plugins", System.StringComparison.OrdinalIgnoreCase))
            {
                return new InstalledPluginListViewModel(_appHost, _nav, _installationManager, _presentationManager);
            }

            return new PluginCategoryListViewModel(_presentationManager, _installationManager, _nav, _appHost);
        }
    }
}
