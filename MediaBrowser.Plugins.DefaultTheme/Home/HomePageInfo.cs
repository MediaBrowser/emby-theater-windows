using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class HomePageInfo : IHomePage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;
        private readonly IPresentationManager _presentationManager;
        private readonly INavigationService _navigationManager;
        private readonly IPlaybackManager _playbackManager;

        private readonly ILogger _logger;

        public HomePageInfo(IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager presentationManager, INavigationService navigationManager, ILogger logger, IPlaybackManager playbackManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _sessionManager = sessionManager;
            _presentationManager = presentationManager;
            _navigationManager = navigationManager;
            _logger = logger;
            _playbackManager = playbackManager;
        }

        public string Name
        {
            get { return "Default"; }
        }

        public Page GetHomePage(BaseItemDto rootFolder)
        {
            var page = new HomePage(rootFolder, _presentationManager)
            {
                DataContext = new HomePageViewModel(_presentationManager, _apiClient, _sessionManager, _logger, _imageManager, _navigationManager, _playbackManager)
            };

            return page;
        }
    }
}
