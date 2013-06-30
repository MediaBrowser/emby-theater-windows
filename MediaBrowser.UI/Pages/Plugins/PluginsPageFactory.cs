using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;
using MediaBrowser.Theater.Interfaces.Theming;

namespace MediaBrowser.UI.Pages.Plugins
{
    public class PluginsPageFactory : ISettingsPage
    {
        private readonly IPackageManager _packageManager;
        private readonly IThemeManager _themeManager;
        private readonly IApplicationHost _appHost;

        public PluginsPageFactory(IPackageManager packageManager, IThemeManager themeManager, IApplicationHost appHost)
        {
            _packageManager = packageManager;
            _themeManager = themeManager;
            _appHost = appHost;
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
            return new PluginsPage(_packageManager, _themeManager, _appHost);
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
    }
}
