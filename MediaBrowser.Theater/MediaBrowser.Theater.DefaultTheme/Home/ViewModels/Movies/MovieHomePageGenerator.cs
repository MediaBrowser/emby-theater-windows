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
        : IUserViewHomePageGenerator
    {
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;
        private readonly INavigator _navigator;
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;

        public MovieHomePageGenerator(IImageManager imageManager, INavigator navigator, IConnectionManager connectionManager, ISessionManager sessionManager, IPlaybackManager playbackManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;
            _logManager = logManager;
        }

        public string CollectionType { get { return Model.Entities.CollectionType.Movies; } }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            IEnumerable<IHomePage> pages = new IHomePage[] {
                new MovieSpotlightViewModel(mediaFolder, _imageManager, _navigator, _connectionManager, _playbackManager, _sessionManager, _logManager),
                new LatestMoviesViewModel(mediaFolder, _connectionManager, _imageManager, _navigator, _sessionManager, _playbackManager)
            };

            return Task.FromResult(pages);
        }
    }
}