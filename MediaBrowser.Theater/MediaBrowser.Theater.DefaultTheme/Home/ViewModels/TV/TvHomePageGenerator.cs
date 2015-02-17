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
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class TvHomePageGenerator
        : IUserViewHomePageGenerator
    {
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;
        private readonly INavigator _navigator;
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;

        public TvHomePageGenerator(IImageManager imageManager, INavigator navigator, IConnectionManager connectionManager, ISessionManager sessionManager, IPlaybackManager playbackManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;
            _logManager = logManager;
        }

        public string CollectionType { get { return Model.Entities.CollectionType.TvShows; } }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            if (mediaFolder.CollectionType != "tvshows")
            {
                return Task.FromResult(Enumerable.Empty<IHomePage>());
            }

            IEnumerable<IHomePage> pages = new IHomePage[] {
                new TvSpotlightViewModel(mediaFolder, _imageManager, _navigator, _connectionManager, _sessionManager, _logManager, _playbackManager),
                new ResumeEpisodesViewModel(mediaFolder, _connectionManager, _imageManager, _navigator, _sessionManager, _playbackManager),
                new LatestEpisodesViewModel(mediaFolder, _connectionManager, _imageManager, _navigator, _sessionManager, _playbackManager)
            };

            return Task.FromResult(pages);
        }
    }
}