using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemChildrenSectionGenerator
        : BaseItemsListSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;

        public ItemChildrenSectionGenerator(IConnectionManager connectionManager, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IPlaybackManager playbackManager)
            : base(connectionManager, sessionManager, imageManager, navigator, playbackManager)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public override bool HasSection(BaseItemDto item)
        {
            return item != null && item.IsFolder;
        }

        public override async Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            ItemsResult result = await ItemChildren.Get(_connectionManager, _sessionManager, item);
            return new[] { await GetItemsSection(result) };
        }
    }
}