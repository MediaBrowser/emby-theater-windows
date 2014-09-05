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
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;

        public ChannelListHomePageGenerator(IImageManager imageManager, INavigator navigator, IApiClient apiClient, ISessionManager sessionManager, IServerEvents serverEvents, IPlaybackManager playbackManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _serverEvents = serverEvents;
            _playbackManager = playbackManager;
        }

        public string CollectionType
        {
            get { return Model.Entities.CollectionType.Channels; }
        }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            IEnumerable<IHomePage> pages = new IHomePage[] {
                new ChannelListViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _sessionManager, _playbackManager)
            };

            return Task.FromResult(pages);
        }
    }
}