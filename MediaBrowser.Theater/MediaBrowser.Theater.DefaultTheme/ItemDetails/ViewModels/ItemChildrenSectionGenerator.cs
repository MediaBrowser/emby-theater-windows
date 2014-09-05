using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Channels;
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
            if (item.IsType("channel")) {
                return GetChannelItems(apiClient, new ChannelItemQuery {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.Id
                }, CancellationToken.None);
            }

            if (item.IsType("ChannelFolderItem") && !string.IsNullOrEmpty(item.ChannelId)) {
                return GetChannelItems(apiClient, new ChannelItemQuery
                {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.ChannelId,
                    FolderId = item.Id
                }, CancellationToken.None);
            }

            var query = new ItemQuery { ParentId = item.Id, UserId = sessionManager.CurrentUser.Id };
            return apiClient.GetItemsAsync(query);
        }

        private static async Task<ItemsResult> GetChannelItems(IApiClient apiClient, ChannelItemQuery query, CancellationToken cancellationToken)
        {
            var queryLimit = await GetChannelQueryLimit(apiClient, query.ChannelId, cancellationToken);
            var startIndex = 0;
            var callLimit = 3;
            var currentCall = 1;

            var result = await GetChannelItems(apiClient, query, startIndex, queryLimit, CancellationToken.None);

            while (result.Items.Length < result.TotalRecordCount && currentCall <= callLimit && queryLimit.HasValue)
            {
                startIndex += queryLimit.Value;

                var innerResult = await GetChannelItems(apiClient, query, startIndex, queryLimit, CancellationToken.None);

                var list = result.Items.ToList();
                list.AddRange(innerResult.Items);
                result.Items = list.ToArray();

                currentCall++;
            }

            return new ItemsResult
            {
                TotalRecordCount = result.TotalRecordCount,
                Items = result.Items
            };
        }

        private static async Task<int?> GetChannelQueryLimit(IApiClient apiClient,  string channelId, CancellationToken cancellationToken)
        {
            var features = await apiClient.GetChannelFeatures(channelId, cancellationToken);

            return features.MaxPageSize;
        }

        private static async Task<ItemsResult> GetChannelItems(IApiClient apiClient,  ChannelItemQuery query, int start, int? limit, CancellationToken cancellationToken)
        {
            query.StartIndex = start;
            query.Limit = limit;

            var result = await apiClient.GetChannelItems(query, CancellationToken.None);

            return new ItemsResult
            {
                TotalRecordCount = result.TotalRecordCount,
                Items = result.Items
            };
        }
    }
}