using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.DisplayPreferences;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.UI;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.Pages;
using MediaBrowser.UI.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for ListPage.xaml
    /// </summary>
    public partial class ListPage : BaseItemsPage, IHasDisplayPreferences
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListPage" /> class.
        /// </summary>
        public ListPage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager)
            : base(parent, displayPreferencesId, apiClient, imageManager, sessionManager, applicationWindow, navigationManager)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Subclasses must provide the list box that holds the items
        /// </summary>
        /// <value>The items list.</value>
        protected override ExtendedListBox ItemsList
        {
            get
            {
                return lstItems;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += ListPage_Loaded;
            Unloaded += ListPage_Unloaded;
        }

        async void ListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentItem != null)
            {
                ShowViewButton();

                await AppResources.Instance.SetPageTitle(ParentItem);
            }
            else
            {
                HideViewButton();
            }
        }

        void ListPage_Unloaded(object sender, RoutedEventArgs e)
        {
            HideViewButton();
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name.Equals("CurrentItemIndex"))
            {
                UpdateCurrentItemIndex();
            }
        }

        /// <summary>
        /// Updates the index of the current item.
        /// </summary>
        private void UpdateCurrentItemIndex()
        {
            var index = CurrentItemIndex;

            currentItemIndex.Visibility = index == -1 ? Visibility.Collapsed : Visibility.Visible;
            currentItemIndex.Text = (CurrentItemIndex + 1).ToString();

            currentItemIndexDivider.Visibility = index == -1 ? Visibility.Collapsed : Visibility.Visible;
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

        protected override async void OnParentItemChanged()
        {
            base.OnParentItemChanged();

            var pageTitleTask = AppResources.Instance.SetPageTitle(ParentItem);

            ShowViewButton();

            if (ParentItem.IsType("Season"))
            {
                TxtName.Visibility = Visibility.Visible;
                TxtName.Text = ParentItem.Name;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
            }

            UpdateClearArt(ParentItem);

            TxtOverview.Text = ParentItem.Overview ?? string.Empty;

            await pageTitleTask;
        }

        private async void UpdateClearArt(BaseItemDto item)
        {
            if (!item.HasArtImage && item.IsType("season"))
            {
                item = await ApiClient.GetItemAsync(item.SeriesId, SessionManager.CurrentUser.Id);
            }

            if (item.HasArtImage)
            {
                ImgDefaultLogo.Source =
                    await ImageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        MaxHeight = 200,
                        ImageType = ImageType.Art
                    }));
            }
        }

        /// <summary>
        /// Shows the view button.
        /// </summary>
        private void ShowViewButton()
        {
            var viewButton = AppResources.Instance.ViewButton;
            viewButton.Visibility = Visibility.Visible;
            viewButton.Click -= ViewButton_Click;
            viewButton.Click += ViewButton_Click;
        }

        /// <summary>
        /// Hides the view button.
        /// </summary>
        private void HideViewButton()
        {
            var viewButton = AppResources.Instance.ViewButton;
            viewButton.Visibility = Visibility.Collapsed;
            viewButton.Click -= ViewButton_Click;
        }

        /// <summary>
        /// Handles the Click event of the ViewButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = new DisplayPreferencesMenu
            {
                DisplayPreferencesContainer = this
            };

            menu.ShowModal(this.GetWindow());

            try
            {
                await ApiClient.UpdateDisplayPreferencesAsync(DisplayPreferences);
            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Handles current item selection changes
        /// </summary>
        protected override void OnCurrentItemChanged()
        {
            base.OnCurrentItemChanged();

            var item = CurrentItem;

            if (item == null)
            {
                ItemInfoFooter.Visibility = Visibility.Hidden;
            }
            else
            {
                ItemInfoFooter.Visibility = Visibility.Visible;
                ItemInfoFooter.Item = item;
            }

            UpdateLogo(item);
            UpdateTagline(item);
        }

        private void UpdateTagline(BaseItemDto item)
        {
            TxtTagline.Text = item != null && item.Taglines != null && item.Taglines.Count > 0 ? item.Taglines[0] : string.Empty;
            TxtGenres.Text = item != null && item.Genres != null ? string.Join(" / ", item.Genres.Take(3).ToArray()) : string.Empty;
        }

        private async void UpdateLogo(BaseItemDto item)
        {
            if (item != null && item.HasLogo)
            {
                ImgLogo.Source =
                    await ImageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        MaxHeight = 50,
                        ImageType = ImageType.Logo
                    }));

                ImgLogo.Visibility = Visibility.Visible;
                ImgDefaultLogo.Visibility = Visibility.Hidden;
            }
            else
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
                ImgDefaultLogo.Visibility = Visibility.Visible;
            }
        }

        protected override void OnDisplayPreferencesChanged()
        {
            base.OnDisplayPreferencesChanged();

            PnlThumbstripInfo.Visibility = DisplayPreferences != null &&
                                           DisplayPreferences.ViewType == ViewTypes.Thumbstrip
                                               ? Visibility.Visible
                                               : Visibility.Collapsed;
        }

        protected override void OnItemsChanged()
        {
            base.OnItemsChanged();

            TxtItemCount.Text = ListItems.Count.ToString();
        }

        public void NotifyDisplayPreferencesChanged()
        {
            OnPropertyChanged("DisplayPreferences");
        }
    }
}
