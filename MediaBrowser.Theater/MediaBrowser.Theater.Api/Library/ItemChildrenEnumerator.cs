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
        public static async Task<ItemsResult> GetChildren(
            BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager,
            bool recursive = false, bool expandSingleItems = false, string[] includeItemTypes = null, ItemFields[] fields = null)
        {
            var children = await GetChildrenInternal(item, apiClient, sessionManager, recursive, includeItemTypes, fields);
            if (children.TotalRecordCount == 1 && expandSingleItems && children.Items[0].IsFolder) {
                return await GetChildren(children.Items[0], apiClient, sessionManager, recursive, true, includeItemTypes, fields);
            }

            return children;
        }

        private static async Task<ItemsResult> GetChildrenInternal(
            BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager,
            bool recursive, string[] includeItemTypes, ItemFields[] fields)
        {
            if (item.IsType("channel")) {
                var result = await GetChannelItems(apiClient, new ChannelItemQuery {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.Id,
                    Fields = fields,
                }, CancellationToken.None);

                result.Items = result.Items.Where(i => includeItemTypes.Any(i.IsType)).ToArray();
                result.TotalRecordCount = result.Items.Length;
            }

            if (item.IsType("ChannelFolderItem") && !string.IsNullOrEmpty(item.ChannelId)) {
                var result = await GetChannelItems(apiClient, new ChannelItemQuery {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.ChannelId,
                    FolderId = item.Id,
                    Fields = fields
                }, CancellationToken.None);

                result.Items = result.Items.Where(i => includeItemTypes.Any(i.IsType)).ToArray();
                result.TotalRecordCount = result.Items.Length;
            }

            if (item.IsType("Person")) {
                return await apiClient.GetItemsAsync(new ItemQuery {
                    Person = item.Name,
                    UserId = sessionManager.CurrentUser.Id,
                    Recursive = recursive,
                    Fields = fields,
                    IncludeItemTypes = includeItemTypes
                });
            }

            var query = new ItemQuery {
                ParentId = item.Id,
                UserId = sessionManager.CurrentUser.Id,
                Recursive = recursive,
                Fields = fields,
                IncludeItemTypes = includeItemTypes
            };

            return await apiClient.GetItemsAsync(query);
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