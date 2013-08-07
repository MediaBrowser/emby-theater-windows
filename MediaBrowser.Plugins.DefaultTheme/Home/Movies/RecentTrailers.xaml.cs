using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home.Movies
{
    /// <summary>
    /// Interaction logic for ResumableMovies.xaml
    /// </summary>
    public partial class RecentTrailers : BaseItemsControl
    {
        public RecentTrailers(Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow)
            : base(displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            ItemsList.ItemInvoked += ItemsList_ItemInvoked;

            base.OnInitialized(e);
        }

        protected override bool AutoSelectFirstItemOnFirstLoad
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Itemses the list_ item invoked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void ItemsList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var model = e.Argument as BaseItemDtoViewModel;

            if (model != null)
            {
                var item = model.Item;

                NavigationManager.NavigateToItem(item, string.Empty);
            }
        }

        protected override ExtendedListBox ItemsList
        {
            get { return LstItems; }
        }

        protected override Task<ItemsResult> GetItemsAsync()
        {
            var query = new ItemQuery
            {
                Fields = new[]
                        {
                            ItemFields.PrimaryImageAspectRatio,
                            ItemFields.DateCreated,
                            ItemFields.DisplayPreferencesId
                        },

                UserId = SessionManager.CurrentUser.Id,

                SortBy = new[] { ItemSortBy.DateCreated },

                SortOrder = SortOrder.Descending,

                IncludeItemTypes = new[] { "Trailer" },

                Filters = new[] { ItemFilter.IsUnplayed },

                Recursive = true,

                Limit = 8
            };

            return ApiClient.GetItemsAsync(query);
        }
    }
}
