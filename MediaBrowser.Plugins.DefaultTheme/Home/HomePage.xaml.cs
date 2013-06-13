using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BaseItemsPage
    {
        public HomePage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager, IThemeManager themeManager)
            : base(parent, displayPreferencesId, apiClient, imageManager, sessionManager, applicationWindow, navigationManager, themeManager)
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;
            ItemsList.ItemInvoked += ItemsList_ItemInvoked;

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(new[] { "movies", "tv", "music", "games", "apps", "folders" });

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
