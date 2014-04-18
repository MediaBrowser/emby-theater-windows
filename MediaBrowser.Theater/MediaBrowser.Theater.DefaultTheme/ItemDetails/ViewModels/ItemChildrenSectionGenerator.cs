using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemChildrenSectionGenerator
        : BaseItemsListSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;

        public ItemChildrenSectionGenerator(IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IServerEvents serverEvents, IPlaybackManager playbackManager)
            : base(apiClient, sessionManager, imageManager, navigator, serverEvents, playbackManager)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
        }

        public override bool HasSection(BaseItemDto item)
        {
            return item != null && item.IsFolder;
        }

        public override async Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            ItemsResult result = await Query(item, _apiClient, _sessionManager);
            return new[] { await GetItemsSection(result) };
        }

        public static Task<ItemsResult> Query(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager)
        {
            var query = new ItemQuery { ParentId = item.Id, UserId = sessionManager.CurrentUser.Id };
            return apiClient.GetItemsAsync(query);
        }
    }
}