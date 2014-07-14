using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class MovieHomePageGenerator
        : IHomePageMediaFolderGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;

        public MovieHomePageGenerator(IImageManager imageManager, INavigator navigator, IApiClient apiClient, ISessionManager sessionManager, IServerEvents serverEvents, IPlaybackManager playbackManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _serverEvents = serverEvents;
            _playbackManager = playbackManager;
            _logManager = logManager;
        }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            if (mediaFolder.CollectionType != "movies") {
                return Task.FromResult(Enumerable.Empty<IHomePage>());
            }

            IEnumerable<IHomePage> pages = new IHomePage[] {
                new MovieSpotlightViewModel(mediaFolder, _imageManager, _navigator, _apiClient, _serverEvents, _playbackManager, _sessionManager, _logManager),
                new LatestItemsViewModel(mediaFolder, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager, _playbackManager)
            };

            return Task.FromResult(pages);
        }
    }
}