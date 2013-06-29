using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.NetworkSettings
{
    /// <summary>
    /// Class NetworkSettingsPageFactory
    /// </summary>
    public class NetworkSettingsPageFactory : ISystemSettingsPage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;
        private readonly IThemeManager _themeManager;
        private readonly ISessionManager _session;

        public NetworkSettingsPageFactory(ITheaterConfigurationManager config, IApiClient apiClient, IThemeManager themeManager, ISessionManager session)
        {
            _config = config;
            _apiClient = apiClient;
            _themeManager = themeManager;
            _session = session;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Network"; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new NetworkSettingsPage(_config, _apiClient, _themeManager, _session);
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/network.jpg", UriKind.Relative); }
        }

        public int? Order
        {
            get { return null; }
        }
    }
}
