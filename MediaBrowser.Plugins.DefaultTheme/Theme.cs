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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme
{
    /// <summary>
    /// Class Theme
    /// </summary>
    class Theme : ITheme
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly ISessionManager _sessionManager;
        private readonly IApplicationWindow _appWindow;
        private readonly ILogger _logger;

        public Theme(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, INavigationService navService, ISessionManager sessionManager, IApplicationWindow appWindow, ILogManager logManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _navService = navService;
            _sessionManager = sessionManager;
            _appWindow = appWindow;
            _logger = logManager.GetLogger(GetType().Name);
        }

        public IEnumerable<ResourceDictionary> GetGlobalResources()
        {
            return new[] { new AppResources(_playbackManager, _imageManager, _apiClient, _appWindow, _navService, _sessionManager, _logger) };
        }

        public Page GetLoginPage()
        {
            return new LoginPage(_apiClient, _imageManager, _navService, _sessionManager, _appWindow);
        }

        public Page GetInternalPlayerPage()
        {
            return new InternalPlayerPage();
        }

        public Page GetHomePage(BaseItemDto rootItem)
        {
            return new HomePage(rootItem, rootItem.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _appWindow, _navService);
        }

        public Page GetItemPage(BaseItemDto item, string context)
        {
            if (item.IsFolder)
            {
                return new ListPage(item, item.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _appWindow, _navService);
            }

            return new DetailPage(item.Id, _apiClient, _imageManager, _sessionManager, _appWindow);
        }
    }
}
