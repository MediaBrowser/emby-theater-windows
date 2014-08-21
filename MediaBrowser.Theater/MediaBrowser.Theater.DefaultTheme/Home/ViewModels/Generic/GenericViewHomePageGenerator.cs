using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Generic
{
    public class GenericViewHomePageGenerator
        : IUserViewHomePageGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;

        public GenericViewHomePageGenerator(IImageManager imageManager, INavigator navigator, IApiClient apiClient, ISessionManager sessionManager, IServerEvents serverEvents, IPlaybackManager playbackManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _serverEvents = serverEvents;
            _playbackManager = playbackManager;
            _logManager = logManager;
        }

        public string CollectionType
        {
            get { return null; }
        }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            IEnumerable<IHomePage> pages = new IHomePage[] {
                new GenericFolderSpotlightViewModel(mediaFolder, _imageManager, _navigator, _apiClient, _serverEvents, _sessionManager, _logManager, _playbackManager);
            };

            return Task.FromResult(pages);
        }
    }
}