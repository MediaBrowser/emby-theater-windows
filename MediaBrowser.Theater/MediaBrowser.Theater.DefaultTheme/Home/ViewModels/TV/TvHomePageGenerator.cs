using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class TvHomePageGenerator
        : IUserViewHomePageGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly ItemTileFactory _itemFactory;
        private readonly ILogManager _logManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        public TvHomePageGenerator(IImageManager imageManager, INavigator navigator, IConnectionManager connectionManager, ISessionManager sessionManager, ILogManager logManager, ItemTileFactory itemFactory)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _logManager = logManager;
            _itemFactory = itemFactory;
        }

        public string CollectionType
        {
            get { return Model.Entities.CollectionType.TvShows; }
        }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            if (mediaFolder.CollectionType != "tvshows") {
                return Task.FromResult(Enumerable.Empty<IHomePage>());
            }

            IEnumerable<IHomePage> pages = new IHomePage[] {
                new TvSpotlightViewModel(mediaFolder, _imageManager, _navigator, _connectionManager, _sessionManager, _logManager, _itemFactory),
                new ResumeEpisodesViewModel(mediaFolder, _connectionManager, _sessionManager, _itemFactory),
                new LatestEpisodesViewModel(mediaFolder, _connectionManager, _sessionManager, _itemFactory)
            };

            return Task.FromResult(pages);
        }
    }
}