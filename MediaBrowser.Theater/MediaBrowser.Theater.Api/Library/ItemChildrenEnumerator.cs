using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Session;

namespace MediaBrowser.Theater.Api.Library
{
    public class ChildrenQueryParams
    {
        public bool Recursive { get; set; }
        public bool ExpandSingleItems { get; set; }
        public string[] IncludeItemTypes { get; set; }
        public string[] ExcludeItemTypes { get; set; }
        public ItemFields[] Fields { get; set; }
        public ItemFilter[] Filters { get; set; }
        public string[] SortBy { get; set; }
        public SortOrder? SortOrder { get; set; }
        public int? Limit { get; set; }
    }

    public static class ItemChildren
    {
        public static ItemFields[] DefaultQueryFields = {
            ItemFields.ParentId,
            ItemFields.PrimaryImageAspectRatio,
            ItemFields.DateCreated,
            ItemFields.MediaStreams,
            ItemFields.Taglines,
            ItemFields.Genres,
            ItemFields.Overview,
            ItemFields.DisplayPreferencesId,
            ItemFields.MediaSources
        };

        public static async Task<ItemsResult> Get(IApiClient apiClient, ISessionManager sessionManager, BaseItemDto item, ChildrenQueryParams parameters = null)
        {
            parameters = parameters ?? new ChildrenQueryParams();

            var children = await GetChildrenInternal(item, parameters, apiClient, sessionManager);
            if (children.TotalRecordCount == 1 && parameters.ExpandSingleItems && children.Items[0].IsFolder) {
                return await Get(apiClient, sessionManager, children.Items[0], parameters);
            }

            return children;
        }

        private static async Task<ItemsResult> GetChildrenInternal(
            BaseItemDto item, ChildrenQueryParams parameters, IApiClient apiClient, ISessionManager sessionManager)
        {
            if (item.IsType("channel")) {
                var result = await GetChannelItems(apiClient, new ChannelItemQuery {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.Id,
                    Filters = parameters.Filters,
                    Fields = parameters.Fields ?? DefaultQueryFields,
                    SortBy = parameters.SortBy,
                    SortOrder = parameters.SortOrder,
                });

                return FilterResult(parameters, result);
            }

            if (item.IsType("ChannelFolderItem") && !string.IsNullOrEmpty(item.ChannelId)) {
                var result = await GetChannelItems(apiClient, new ChannelItemQuery {
                    UserId = sessionManager.CurrentUser.Id,
                    ChannelId = item.ChannelId,
                    FolderId = item.Id,
                    Filters = parameters.Filters,
                    Fields = parameters.Fields ?? DefaultQueryFields,
                    SortBy = parameters.SortBy,
                    SortOrder = parameters.SortOrder,
                });

                return FilterResult(parameters, result);
            }

            return await apiClient.GetItemsAsync(new ItemQuery {
                ParentId = item.IsType("Person") ? null : item.Id,
                Person = item.IsType("Person") ? item.Name : null,
                UserId = sessionManager.CurrentUser.Id,
                Recursive = parameters.Recursive,
                Filters = parameters.Filters,
                Fields = parameters.Fields ?? DefaultQueryFields,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder,
                IncludeItemTypes = parameters.IncludeItemTypes,
                ExcludeItemTypes = parameters.ExcludeItemTypes,
                Limit = parameters.Limit
            });
        }

        private static ItemsResult FilterResult(ChildrenQueryParams parameters, ItemsResult result)
        {
            IEnumerable<BaseItemDto> items = result.Items;

            if (parameters.IncludeItemTypes != null) {
                items = items.Where(i => parameters.IncludeItemTypes.Any(i.IsType));
            }

            if (parameters.ExcludeItemTypes != null) {
                items = items.Where(i => !parameters.ExcludeItemTypes.Any(i.IsType));
            }

            if (parameters.Limit != null) {
                items = items.Take(parameters.Limit.Value);
            }

            result.Items = items as BaseItemDto[] ?? items.ToArray();
            result.TotalRecordCount = result.Items.Length;
            return result;
        }

        private static async Task<ItemsResult> GetChannelItems(IApiClient apiClient, ChannelItemQuery query, int? maxItems = null)
        {
            const int callLimit = 3;

            int? queryLimit = await GetChannelQueryLimit(apiClient, query.ChannelId, CancellationToken.None);
            if (maxItems != null) {
                queryLimit = queryLimit == null ? maxItems : Math.Min(queryLimit.Value, maxItems.Value);
            }

            int startIndex = 0;
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