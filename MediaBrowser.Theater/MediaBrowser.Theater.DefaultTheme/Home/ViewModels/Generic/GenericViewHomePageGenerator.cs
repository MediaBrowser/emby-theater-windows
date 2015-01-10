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
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;
        private readonly INavigator _navigator;
        private readonly IConnectionManager _connectionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly ISessionManager _sessionManager;

        public GenericViewHomePageGenerator(IImageManager imageManager, INavigator navigator, IConnectionManager connectionManager, ISessionManager sessionManager, IPlaybackManager playbackManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
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
                new GenericFolderSpotlightViewModel(mediaFolder, _imageManager, _navigator, _connectionManager, _sessionManager, _logManager, _playbackManager)
            };

            return Task.FromResult(pages);
        }
    }
}