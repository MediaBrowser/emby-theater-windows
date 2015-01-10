using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Channels
{
    public class ChannelListHomePageGenerator
        : IUserViewHomePageGenerator
    {
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IConnectionManager _connectionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly ISessionManager _sessionManager;

        public ChannelListHomePageGenerator(IImageManager imageManager, INavigator navigator, IConnectionManager connectionManager, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;
        }

        public string CollectionType
        {
            get { return Model.Entities.CollectionType.Channels; }
        }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            IEnumerable<IHomePage> pages = new IHomePage[] {
                new ChannelListViewModel(_connectionManager, _imageManager, _navigator, _sessionManager, _playbackManager)
            };

            return Task.FromResult(pages);
        }
    }
}