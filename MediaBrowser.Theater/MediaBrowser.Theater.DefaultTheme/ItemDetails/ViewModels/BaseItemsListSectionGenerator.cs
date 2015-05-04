using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public abstract class BaseItemsListSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ItemTileFactory _itemFactory;
        private readonly ISessionManager _sessionManager;

        protected BaseItemsListSectionGenerator(IConnectionManager connectionManager, ISessionManager sessionManager, ItemTileFactory itemFactory)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _itemFactory = itemFactory;

            ListThreshold = 8;
        }

        public int ListThreshold { get; set; }

        public virtual bool HasSection(BaseItemDto item)
        {
            return true;
        }

        public virtual Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            return Task.FromResult(Enumerable.Empty<IItemDetailSection>());
        }

        protected async Task<IItemDetailSection> GetItemsSection(ItemsResult itemsResult, bool expandSingleItem = true)
        {
            return await GetItemsSection(itemsResult, result => (result.Items.Length > 0 && result.Items[0].Type == "Episode") || result.Items.Length > ListThreshold, expandSingleItem);
        }

        protected async Task<IItemDetailSection> GetItemsSection(ItemsResult itemsResult, Func<ItemsResult, bool> listCondition, bool expandSingleItem = true)
        {
            if (itemsResult.Items.Length == 0) {
                return null;
            }

            IApiClient apiClient = _connectionManager.GetApiClient(itemsResult.Items[0]);

            if (itemsResult.Items.Length == 1 && expandSingleItem && itemsResult.Items[0].IsFolder) {
                var query = new ItemQuery { ParentId = itemsResult.Items[0].Id, UserId = _sessionManager.CurrentUser.Id };
                return await GetItemsSection(await apiClient.GetItemsAsync(query), listCondition);
            }

            if (listCondition(itemsResult)) {
                return new ItemsListViewModel(itemsResult, _itemFactory);
            }

            return new ItemsGridViewModel(itemsResult, _itemFactory);
        }
    }
}