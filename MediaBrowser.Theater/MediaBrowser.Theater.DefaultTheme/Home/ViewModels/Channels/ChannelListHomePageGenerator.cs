using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Channels
{
    public class ChannelListHomePageGenerator
        : IUserViewHomePageGenerator
    {
        private readonly ItemTileFactory _itemFactory;
        private readonly ISessionManager _sessionManager;

        public ChannelListHomePageGenerator(ISessionManager sessionManager, ItemTileFactory itemFactory)
        {
            _sessionManager = sessionManager;
            _itemFactory = itemFactory;
        }

        public string CollectionType
        {
            get { return Model.Entities.CollectionType.Channels; }
        }

        public Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder)
        {
            IEnumerable<IHomePage> pages = new IHomePage[] {
                new ChannelListViewModel(_sessionManager, _itemFactory)
            };

            return Task.FromResult(pages);
        }
    }
}