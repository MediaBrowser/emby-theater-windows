using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class MovieHomePageGenerator
        : IHomePageGenerator
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

        public IEnumerable<IViewModel> GetHomePages()
        {
            var cancellationSource = new CancellationTokenSource();
            Task<MoviesView> movieView = _apiClient.GetMovieView(_sessionManager.CurrentUser.Id, cancellationSource.Token);

            yield return new MovieSpotlightViewModel(movieView, _imageManager, _navigator, _apiClient, _serverEvents, _playbackManager, _sessionManager, _logManager);
            yield return new LatestMoviesViewModel(movieView, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager, _playbackManager);
            yield return new LatestTrailersViewModel(movieView, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager, _playbackManager);
        }
    }
}