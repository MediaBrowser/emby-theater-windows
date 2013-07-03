using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;

namespace MediaBrowser.UI.Pages.Appearance
{
    public class AppearancePageFactory : ISettingsPage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly ISessionManager _session;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly IPresentationManager _presentation;
        private readonly IThemeManager _themeManager;
        private readonly INavigationService _nav;

        public AppearancePageFactory(ITheaterConfigurationManager config, ISessionManager session, IImageManager imageManager, IApiClient apiClient, IPresentationManager presentation, IThemeManager themeManager, INavigationService nav)
        {
            _config = config;
            _session = session;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _presentation = presentation;
            _themeManager = themeManager;
            _nav = nav;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Appearance"; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new AppearancePage(_config, _session, _imageManager, _apiClient, _presentation, _themeManager, _nav);
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
            return user != null;
        }
    }
}
