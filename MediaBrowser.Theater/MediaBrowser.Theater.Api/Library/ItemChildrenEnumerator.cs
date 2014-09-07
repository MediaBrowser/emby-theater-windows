using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Session;

namespace MediaBrowser.Theater.Api.Library
{
    public static class ItemChildren
    {
        public static async Task<ItemsResult> GetChildren(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager, bool expandSingleItems = false)
        {
            var children = await GetChildrenInternal(item, apiClient, sessionManager);
            if (children.TotalRecordCount == 1 && expandSingleItems && children.Items[0].IsFolder) {
                return await GetChildren(children.Items[0], apiClient, sessionManager, true);
            }

            return children;
        }

        private static Task<ItemsResult> GetChildrenInternal(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager)
        {
            if (item.IsType("channel")) {
                return GetChannelItems(apiClient, new ChannelItemQuery {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.Id
                }, CancellationToken.None);
            }

            if (item.IsType("ChannelFolderItem") && !string.IsNullOrEmpty(item.ChannelId)) {
                return GetChannelItems(apiClient, new ChannelItemQuery {
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
            int? queryLimit = await GetChannelQueryLimit(apiClient, query.ChannelId, cancellationToken);
            int startIndex = 0;
            const int callLimit = 3;
            int currentCall = 1;

            ItemsResult result = await GetChannelItems(apiClient, query, startIndex, queryLimit, CancellationToken.None);

            while (result.Items.Length < result.TotalRecordCount && currentCall <= callLimit && queryLimit.HasValue) {
                startIndex += queryLimit.Value;

                ItemsResult innerResult = await GetChannelItems(apiClient, query, startIndex, queryLimit, CancellationToken.None);

                List<BaseItemDto> list = result.Items.ToList();
                list.AddRange(innerResult.Items);
                result.Items = list.ToArray();

                currentCall++;
            }

            return new ItemsResult {
                TotalRecordCount = result.TotalRecordCount,
                Items = result.Items
            };
        }

        private static async Task<int?> GetChannelQueryLimit(IApiClient apiClient, string channelId, CancellationToken cancellationToken)
        {
            ChannelFeatures features = await apiClient.GetChannelFeatures(channelId, cancellationToken);

            return features.MaxPageSize;
        }

        private static async Task<ItemsResult> GetChannelItems(IApiClient apiClient, ChannelItemQuery query, int start, int? limit, CancellationToken cancellationToken)
        {
            query.StartIndex = start;
            query.Limit = limit;

            QueryResult<BaseItemDto> result = await apiClient.GetChannelItems(query, CancellationToken.None);

            return new ItemsResult {
                TotalRecordCount = result.TotalRecordCount,
                Items = result.Items
            };
        }
    }
}