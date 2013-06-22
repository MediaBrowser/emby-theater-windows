using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for Folders.xaml
    /// </summary>
    public partial class Folders : BaseItemsControl
    {
        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        protected IApplicationWindow ApplicationWindow { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemsPage" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="displayPreferences">The display preferences.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="applicationWindow">The application window.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="themeManager">The theme manager.</param>
        public Folders(BaseItemDto parent, Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager, IThemeManager themeManager)
            : base(parent, displayPreferences, apiClient, imageManager, sessionManager, navigationManager, themeManager)
        {
            ApplicationWindow = applicationWindow;
            
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            ItemsList.ItemInvoked += ItemsList_ItemInvoked;

            base.OnInitialized(e);
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

        /// <summary>
        /// Gets the items list.
        /// </summary>
        /// <value>The items list.</value>
        protected override ExtendedListBox ItemsList
        {
            get { return LstCollectionFolders; }
        }

        /// <summary>
        /// Gets the items async.
        /// </summary>
        /// <returns>Task{ItemsResult}.</returns>
        protected override Task<ItemsResult> GetItemsAsync()
        {
            var query = new ItemQuery
            {
                ParentId = ParentItem.Id,

                Fields = new[]
                        {
                            ItemFields.UserData,
                            ItemFields.PrimaryImageAspectRatio,
                            ItemFields.DateCreated,
                            ItemFields.SeriesInfo,
                            ItemFields.DisplayPreferencesId
                        },

                UserId = SessionManager.CurrentUser.Id,

                SortBy = !string.IsNullOrEmpty(DisplayPreferences.SortBy)
                        ? new[] { DisplayPreferences.SortBy }
                        : new[] { ItemSortBy.SortName },

                SortOrder = DisplayPreferences.SortOrder
            };

            return ApiClient.GetItemsAsync(query);
        }

        /// <summary>
        /// Called when [current item changed].
        /// </summary>
        protected override void OnCurrentItemChanged()
        {
            if (CurrentItem != null)
            {
                ApplicationWindow.SetBackdrops(CurrentItem);
            }
        }
    }
}
