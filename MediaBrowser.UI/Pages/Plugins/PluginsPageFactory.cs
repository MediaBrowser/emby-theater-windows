using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.Plugins
{
    public class PluginsPageFactory : ISettingsPage
    {
        private readonly IPackageManager _packageManager;
        private readonly IPresentationManager _presentation;
        private readonly IApplicationHost _appHost;
        private readonly INavigationService _nav;
        private readonly IInstallationManager _installationManager;

        public PluginsPageFactory(IPackageManager packageManager, IApplicationHost appHost, INavigationService nav, IPresentationManager presentation, IInstallationManager installationManager)
        {
            _packageManager = packageManager;
            _appHost = appHost;
            _nav = nav;
            _presentation = presentation;
            _installationManager = installationManager;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Plugins"; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new PluginsPage(_packageManager, _appHost, _nav, _presentation, _installationManager);
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/weather6.png", UriKind.Relative); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.System; }
        }

        public bool IsVisible(UserDto user)
        {
            return true;
        }
    }
}
