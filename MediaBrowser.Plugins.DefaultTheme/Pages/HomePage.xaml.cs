using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BaseItemsPage
    {
        /// <summary>
        /// The _favorite items
        /// </summary>
        private ItemCollectionViewModel _favoriteItems;

        public HomePage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager, IThemeManager themeManager)
            : base(parent, displayPreferencesId, apiClient, imageManager, sessionManager, applicationWindow, navigationManager, themeManager)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the favorite items.
        /// </summary>
        /// <value>The favorite items.</value>
        public ItemCollectionViewModel FavoriteItems
        {
            get { return _favoriteItems; }

            set
            {
                _favoriteItems = value;
                OnPropertyChanged("FavoriteItems");
            }
        }

        /// <summary>
        /// The _resumable items
        /// </summary>
        private ItemCollectionViewModel _resumableItems;
        /// <summary>
        /// Gets or sets the resumable items.
        /// </summary>
        /// <value>The resumable items.</value>
        public ItemCollectionViewModel ResumableItems
        {
            get { return _resumableItems; }

            set
            {
                _resumableItems = value;
                OnPropertyChanged("ResumableItems");
            }
        }

        /// <summary>
        /// The _recently added items
        /// </summary>
        private ItemCollectionViewModel _recentlyAddedItems;
        /// <summary>
        /// Gets or sets the recently added items.
        /// </summary>
        /// <value>The recently added items.</value>
        public ItemCollectionViewModel RecentlyAddedItems
        {
            get { return _recentlyAddedItems; }

            set
            {
                _recentlyAddedItems = value;
                OnPropertyChanged("RecentlyAddedItems");
            }
        }

        /// <summary>
        /// The _recently played items
        /// </summary>
        private ItemCollectionViewModel _recentlyPlayedItems;
        /// <summary>
        /// Gets or sets the recently played items.
        /// </summary>
        /// <value>The recently played items.</value>
        public ItemCollectionViewModel RecentlyPlayedItems
        {
            get { return _recentlyPlayedItems; }

            set
            {
                _recentlyPlayedItems = value;
                OnPropertyChanged("RecentlyPlayedItems");
            }
        }

        /// <summary>
        /// The _spotlight items
        /// </summary>
        private ItemCollectionViewModel _spotlightItems;
        /// <summary>
        /// Gets or sets the spotlight items.
        /// </summary>
        /// <value>The spotlight items.</value>
        public ItemCollectionViewModel SpotlightItems
        {
            get { return _spotlightItems; }

            set
            {
                _spotlightItems = value;
                OnPropertyChanged("SpotlightItems");
            }
        }

        /// <summary>
        /// The _top picks
        /// </summary>
        private ItemCollectionViewModel _topPicks;
        /// <summary>
        /// Gets or sets the top picks.
        /// </summary>
        /// <value>The top picks.</value>
        public ItemCollectionViewModel TopPicks
        {
            get { return _topPicks; }

            set
            {
                _topPicks = value;
                OnPropertyChanged("TopPicks");
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;

            base.OnInitialized(e);
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            AppResources.Instance.SetDefaultPageTitle();

            var parent = ParentItem;

            if (parent == null)
            {
                ApplicationWindow.ClearBackdrops();
            }
            else
            {
                ApplicationWindow.SetBackdrops(parent);
            }
        }

        protected override async void OnParentItemChanged()
        {
            base.OnParentItemChanged();

            await RefreshSpecialItems();
        }

        /// <summary>
        /// Refreshes the special items.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshSpecialItems()
        {
            var tasks = new List<Task>
                {
                    RefreshFavoriteItemsAsync(), 
                    RefreshResumableItemsAsync()
                };

            // In-Progress Items

            // Recently Added Items
            if (ParentItem.RecentlyAddedItemCount > 0)
            {
                tasks.Add(RefreshRecentlyAddedItemsAsync());
            }
            else
            {
                SetRecentlyAddedItems(new BaseItemDto[] { });
            }

            // Recently Played Items
            tasks.Add(RefreshRecentlyPlayedItemsAsync());

            tasks.Add(RefreshTopPicksAsync());
            tasks.Add(RefreshSpotlightItemsAsync());

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Refreshes the favorite items async.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshFavoriteItemsAsync()
        {
            var query = new ItemQuery
            {
                Filters = new[] { ItemFilter.IsFavorite },
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                UserId = SessionManager.CurrentUser.Id,
                ParentId = ParentItem.Id,
                Limit = 10,
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true
            };

            try
            {
                var result = await ApiClient.GetItemsAsync(query).ConfigureAwait(false);

                SetFavoriteItems(result.Items);
            }
            catch (HttpException)
            {
                // Already logged in lower levels
                // Don't allow the entire screen to fail
            }
        }

        /// <summary>
        /// Refreshes the resumable items async.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshResumableItemsAsync()
        {
            var query = new ItemQuery
            {
                Filters = new[] { ItemFilter.IsResumable },
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                UserId = SessionManager.CurrentUser.Id,
                ParentId = ParentItem.Id,
                Limit = 10,
                SortBy = new[] { ItemSortBy.DatePlayed },
                SortOrder = SortOrder.Descending,
                Recursive = true
            };

            try
            {
                var result = await ApiClient.GetItemsAsync(query).ConfigureAwait(false);

                SetResumableItems(result.Items);
            }
            catch (HttpException)
            {
                // Already logged in lower levels
                // Don't allow the entire screen to fail
            }
        }

        /// <summary>
        /// Refreshes the recently played items async.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshRecentlyPlayedItemsAsync()
        {
            var query = new ItemQuery
            {
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                UserId = SessionManager.CurrentUser.Id,
                ParentId = ParentItem.Id,
                Limit = 10,
                SortBy = new[] { ItemSortBy.DatePlayed },
                SortOrder = SortOrder.Descending,
                Recursive = true
            };

            try
            {
                var result = await ApiClient.GetItemsAsync(query).ConfigureAwait(false);
                SetRecentlyPlayedItems(result.Items);
            }
            catch (HttpException)
            {
                // Already logged in lower levels
                // Don't allow the entire screen to fail
            }
        }

        /// <summary>
        /// Refreshes the recently added items async.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshRecentlyAddedItemsAsync()
        {
            var query = new ItemQuery
            {
                Filters = new[] { ItemFilter.IsRecentlyAdded, ItemFilter.IsNotFolder },
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                UserId = SessionManager.CurrentUser.Id,
                ParentId = ParentItem.Id,
                Limit = 10,
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                Recursive = true
            };

            try
            {
                var result = await ApiClient.GetItemsAsync(query).ConfigureAwait(false);
                SetRecentlyAddedItems(result.Items);
            }
            catch (HttpException)
            {
                // Already logged in lower levels
                // Don't allow the entire screen to fail
            }
        }

        /// <summary>
        /// Refreshes the top picks async.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshTopPicksAsync()
        {
            var query = new ItemQuery
            {
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                Filters = new[] { ItemFilter.IsRecentlyAdded, ItemFilter.IsNotFolder },
                UserId = SessionManager.CurrentUser.Id,
                ParentId = ParentItem.Id,
                Limit = 10,
                SortBy = new[] { ItemSortBy.Random },
                SortOrder = SortOrder.Descending,
                Recursive = true
            };

            try
            {
                var result = await ApiClient.GetItemsAsync(query).ConfigureAwait(false);

                TopPicks = new ItemCollectionViewModel(ApiClient, ImageManager) { Items = result.Items, Name = "Top Picks" };
            }
            catch (HttpException)
            {
                // Already logged in lower levels
                // Don't allow the entire screen to fail
            }
        }

        /// <summary>
        /// Refreshes the spotlight items async.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task RefreshSpotlightItemsAsync()
        {
            var query = new ItemQuery
            {
                ImageTypes = new[] { ImageType.Backdrop },
                ExcludeItemTypes = new[] { "Season" },
                UserId = SessionManager.CurrentUser.Id,
                ParentId = ParentItem.Id,
                Limit = 10,
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true
            };

            try
            {
                var result = await ApiClient.GetItemsAsync(query).ConfigureAwait(false);

                SpotlightItems = new ItemCollectionViewModel(ApiClient, ImageManager, rotationPeriodMs: 6000, rotationDevaiationMs: 1000) { Items = result.Items };
            }
            catch (HttpException)
            {
                // Already logged in lower levels
                // Don't allow the entire screen to fail
            }
        }

        /// <summary>
        /// Sets the favorite items.
        /// </summary>
        /// <param name="items">The items.</param>
        private void SetFavoriteItems(BaseItemDto[] items)
        {
            FavoriteItems = new ItemCollectionViewModel(ApiClient, ImageManager) { Items = items, Name = "Favorites" };
        }

        /// <summary>
        /// Sets the resumable items.
        /// </summary>
        /// <param name="items">The items.</param>
        private void SetResumableItems(BaseItemDto[] items)
        {
            ResumableItems = new ItemCollectionViewModel(ApiClient, ImageManager) { Items = items, Name = "Resume" };
        }

        /// <summary>
        /// Sets the recently played items.
        /// </summary>
        /// <param name="items">The items.</param>
        private void SetRecentlyPlayedItems(BaseItemDto[] items)
        {
            RecentlyPlayedItems = new ItemCollectionViewModel(ApiClient, ImageManager) { Items = items, Name = "Recently Played" };
        }

        /// <summary>
        /// Sets the recently added items.
        /// </summary>
        /// <param name="items">The items.</param>
        private void SetRecentlyAddedItems(BaseItemDto[] items)
        {
            RecentlyAddedItems = new ItemCollectionViewModel(ApiClient, ImageManager) { Items = items, Name = "Recently Added" };
        }

        protected override ExtendedListBox ItemsList
        {
            get { return lstCollectionFolders; }
        }

        protected override Task<ItemsResult> GetItemsAsync()
        {
            var query = new ItemQuery
            {
                ParentId = ParentItem.Id,

                Fields = new[] {
                                 ItemFields.UserData,
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated,
                                 ItemFields.MediaStreams,
                                 ItemFields.Taglines,
                                 ItemFields.Genres,
                                 ItemFields.SeriesInfo,
                                 ItemFields.Overview,
                                 ItemFields.DisplayPreferencesId
                             },

                UserId = SessionManager.CurrentUser.Id
            };

            query.SortBy = !string.IsNullOrEmpty(DisplayPreferences.SortBy) ? new[] { DisplayPreferences.SortBy } : new[] { ItemSortBy.SortName };

            query.SortOrder = DisplayPreferences.SortOrder;

            return ApiClient.GetItemsAsync(query);
        }
    }
}
