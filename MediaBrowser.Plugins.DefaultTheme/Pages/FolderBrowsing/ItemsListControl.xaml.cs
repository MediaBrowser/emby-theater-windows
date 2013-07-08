using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.DisplayPreferences;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Pages.FolderBrowsing
{
    /// <summary>
    /// Interaction logic for ItemsListControl.xaml
    /// </summary>
    public partial class ItemsListControl : BaseFolderControl, IHasDisplayPreferences
    {
        private double _itemContainerWidth;
        public double ItemContainerWidth
        {
            get { return _itemContainerWidth; }
            set
            {
                _itemContainerWidth = value;
                OnPropertyChanged("ItemContainerWidth");
            }
        }

        private double _itemContainerHeight;
        public double ItemContainerHeight
        {
            get { return _itemContainerHeight; }
            set
            {
                _itemContainerHeight = value;
                OnPropertyChanged("ItemContainerHeight");
            }
        }
        
        public ItemsListControl(BaseItemDto parentItem, Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow)
            : base(parentItem, displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += ItemsListControl_Loaded;
            Unloaded += ItemsListControl_Unloaded;
            ItemsList.ItemInvoked += ItemsList_ItemInvoked;
        }

        void ItemsListControl_Unloaded(object sender, RoutedEventArgs e)
        {
            HideViewButton();
        }

        async void ItemsListControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentItem != null)
            {
                ShowViewButton();

                await PageTitlePanel.Current.SetPageTitle(ParentItem);
            }
            else
            {
                HideViewButton();
            }
        }

        async void ItemsList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var model = e.Argument as BaseItemDtoViewModel;

            if (model != null)
            {
                var item = model.Item;

                await NavigationManager.NavigateToItem(item, string.Empty);
            }
        }

        protected override ExtendedListBox ItemsList
        {
            get { return LstItems; }
        }

        protected override bool AutoSelectFirstItemOnFirstLoad
        {
            get
            {
                return true;
            }
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
                                 ItemFields.MediaStreams,
                                 ItemFields.Taglines,
                                 ItemFields.Genres,
                                 ItemFields.SeriesInfo,
                                 ItemFields.Overview,
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

        private async void UpdateClearArt(BaseItemDto item)
        {
            if (!item.HasArtImage && !string.IsNullOrEmpty(item.SeriesId))
            {
                item = await ApiClient.GetItemAsync(item.SeriesId, SessionManager.CurrentUser.Id);
            }

            if (item.HasArtImage)
            {
                ImgDefaultLogo.Source =
                    await ImageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        MaxHeight = 120,
                        ImageType = ImageType.Art
                    }));
            }
        }

        /// <summary>
        /// Shows the view button.
        /// </summary>
        private void ShowViewButton()
        {
            var viewButton = TopRightPanel.Current.ViewButton;
            viewButton.Visibility = Visibility.Visible;
            viewButton.Click -= ViewButton_Click;
            viewButton.Click += ViewButton_Click;
        }

        /// <summary>
        /// Hides the view button.
        /// </summary>
        private void HideViewButton()
        {
            var viewButton = TopRightPanel.Current.ViewButton;
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
                PresentationManager.ShowDefaultErrorMessage();
            }
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

        protected override async void OnParentItemChanged()
        {
            base.OnParentItemChanged();

            var pageTitleTask = PageTitlePanel.Current.SetPageTitle(ParentItem);

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

        protected override void OnDisplayPreferencesChanged()
        {
            base.OnDisplayPreferencesChanged();

            var displayPreferences = DisplayPreferences;

            ItemContainerWidth = GetItemContainerWidth(displayPreferences, MedianPrimaryImageAspectRatio);
            ItemContainerHeight = GetItemContainerHeight(displayPreferences, MedianPrimaryImageAspectRatio);

            PnlThumbstripInfo.Visibility = displayPreferences != null &&
                                           displayPreferences.ViewType == ViewTypes.Thumbstrip
                                               ? Visibility.Visible
                                               : Visibility.Collapsed;

            if (displayPreferences.ScrollDirection == ScrollDirection.Horizontal)
            {
                ScrollViewer.SetHorizontalScrollBarVisibility(ItemsList, ScrollBarVisibility.Hidden);
                ScrollViewer.SetVerticalScrollBarVisibility(ItemsList, ScrollBarVisibility.Disabled);
            }
            else
            {
                ScrollViewer.SetHorizontalScrollBarVisibility(ItemsList, ScrollBarVisibility.Disabled);
                ScrollViewer.SetVerticalScrollBarVisibility(ItemsList, ScrollBarVisibility.Hidden);
            }
        }

        protected virtual double GetItemContainerWidth(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            // 14 = double the margin between items as defined in the resource file
            return displayPreferences.PrimaryImageWidth + 14;
        }

        protected virtual double GetItemContainerHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            // 14 = double the margin between items as defined in the resource file
            return GetImageDisplayHeight(displayPreferences, medianPrimaryImageAspectRatio) + 14;
        }

        protected override void OnItemsChanged()
        {
            base.OnItemsChanged();

            TxtItemCount.Text = ListItems.Count.ToString(CultureInfo.CurrentCulture);
        }

        public void NotifyDisplayPreferencesChanged()
        {
            OnPropertyChanged("DisplayPreferences");
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
                        MaxHeight = 70,
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

        protected override double GetImageDisplayHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (!medianPrimaryImageAspectRatio.Equals(0))
            {
                if (string.IsNullOrEmpty(displayPreferences.ViewType) || string.Equals(displayPreferences.ViewType, ViewTypes.Poster))
                {
                    double height = displayPreferences.PrimaryImageWidth;
                    height /= medianPrimaryImageAspectRatio;

                    return height;
                }
            }

            return base.GetImageDisplayHeight(displayPreferences, medianPrimaryImageAspectRatio);
        }
    }
}
