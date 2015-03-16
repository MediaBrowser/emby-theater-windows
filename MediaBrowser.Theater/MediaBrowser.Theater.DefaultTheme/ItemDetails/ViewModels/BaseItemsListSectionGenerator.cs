using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public abstract class BaseItemsListSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;

        protected BaseItemsListSectionGenerator(IConnectionManager connectionManager, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IPlaybackManager playbackManager)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _playbackManager = playbackManager;

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

            var apiClient = _connectionManager.GetApiClient(itemsResult.Items[0]);

            if (itemsResult.Items.Length == 1 && expandSingleItem && itemsResult.Items[0].IsFolder) {
                var query = new ItemQuery { ParentId = itemsResult.Items[0].Id, UserId = _sessionManager.CurrentUser.Id };
                return await GetItemsSection(await apiClient.GetItemsAsync(query), listCondition);
            }

            if (listCondition(itemsResult)) {
                return new ItemsListViewModel(itemsResult, _connectionManager, _imageManager, _navigator, _playbackManager);
            }

            return new ItemsGridViewModel(itemsResult, _connectionManager, _imageManager, _navigator, _playbackManager);
        }
    }
}