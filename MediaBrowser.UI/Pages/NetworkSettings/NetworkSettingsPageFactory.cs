using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.NetworkSettings
{
    /// <summary>
    /// Class NetworkSettingsPageFactory
    /// </summary>
    public class NetworkSettingsPageFactory : ISettingsPage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _session;
        private readonly IPresentationManager _presentationManager;

        public NetworkSettingsPageFactory(ITheaterConfigurationManager config, IApiClient apiClient, ISessionManager session, IPresentationManager presentationManager)
        {
            _config = config;
            _apiClient = apiClient;
            _session = session;
            _presentationManager = presentationManager;
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
            return new NetworkSettingsPage(_config, _apiClient, _session, _presentationManager);
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/network.jpg", UriKind.Relative); }
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
