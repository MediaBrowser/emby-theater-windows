using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.Pages;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme
{
    /// <summary>
    /// Class Theme
    /// </summary>
    public class DefaultTheme : ITheme
    {
        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;
        /// <summary>
        /// The _api client
        /// </summary>
        private readonly IApiClient _apiClient;
        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;
        /// <summary>
        /// The _nav service
        /// </summary>
        private readonly INavigationService _navService;
        /// <summary>
        /// The _session manager
        /// </summary>
        private readonly ISessionManager _sessionManager;
        /// <summary>
        /// The _app window
        /// </summary>
        private readonly IApplicationWindow _appWindow;
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;
        private readonly IThemeManager _themeManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTheme"/> class.
        /// </summary>
        /// <param name="playbackManager">The playback manager.</param>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="navService">The nav service.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="appWindow">The app window.</param>
        /// <param name="logManager">The log manager.</param>
        public DefaultTheme(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, INavigationService navService, ISessionManager sessionManager, IApplicationWindow appWindow, ILogManager logManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _navService = navService;
            _sessionManager = sessionManager;
            _appWindow = appWindow;
            _logger = logManager.GetLogger(GetType().Name);
        }

        /// <summary>
        /// Gets the global resources.
        /// </summary>
        /// <returns>IEnumerable{ResourceDictionary}.</returns>
        public IEnumerable<ResourceDictionary> GetGlobalResources()
        {
            return new[] { new AppResources(_playbackManager, _imageManager, _apiClient, _appWindow, _navService, _sessionManager, _logger) };
        }

        /// <summary>
        /// Gets the login page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetLoginPage()
        {
            return new LoginPage(_apiClient, _imageManager, _navService, _sessionManager, _appWindow, _themeManager);
        }

        /// <summary>
        /// Gets the internal player page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetInternalPlayerPage()
        {
            return new InternalPlayerPage(_navService);
        }

        /// <summary>
        /// Gets the home page.
        /// </summary>
        /// <param name="rootItem">The root item.</param>
        /// <returns>Page.</returns>
        public Page GetHomePage(BaseItemDto rootItem)
        {
            return new HomePage(rootItem, rootItem.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _appWindow, _navService, _themeManager);
        }

        /// <summary>
        /// Gets the item page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        public Page GetItemPage(BaseItemDto item, string context)
        {
            if (item.IsFolder)
            {
                return new ListPage(item, item.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _appWindow, _navService, _themeManager);
            }

            return new DetailPage(item.Id, _apiClient, _imageManager, _sessionManager, _appWindow, _themeManager);
        }

        /// <summary>
        /// Shows the default error message.
        /// </summary>
        public void ShowDefaultErrorMessage()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>MessageBoxResult.</returns>
        public MessageBoxResult ShowMessage(MessageBoxInfo options)
        {
            throw new NotImplementedException();
        }
    }
}
