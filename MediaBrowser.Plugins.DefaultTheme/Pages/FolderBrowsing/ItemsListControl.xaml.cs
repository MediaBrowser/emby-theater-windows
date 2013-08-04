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
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Extensions;
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

        private Sidebar _sidebarControl;

        private readonly IUserInputManager _userInput;

        public ItemsListControl(BaseItemDto parentItem, Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow, IUserInputManager userInput)
            : base(parentItem, displayPreferences, apiClient, imageManager, sessionManager, navigationManager, appWindow)
        {
            _userInput = userInput;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += ItemsListControl_Loaded;
            Unloaded += ItemsListControl_Unloaded;
            ItemsList.ItemInvoked += ItemsList_ItemInvoked;

            _sidebarControl = new Sidebar(ApiClient, ImageManager);
            Sidebar.Content = _sidebarControl;
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
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated,
                                 ItemFields.MediaStreams,
                                 ItemFields.Taglines,
                                 ItemFields.Genres,
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
            var menu = new DisplayPreferencesMenu(_userInput, NavigationManager)
            {
                DisplayPreferencesContainer = this
            };

            menu.ShowModal(this.GetWindow());

            try
            {
                await ApiClient.UpdateDisplayPreferencesAsync(DisplayPreferences, SessionManager.CurrentUser.Id, "DefaultTheme");
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }

        protected override async void OnParentItemChanged()
        {
            base.OnParentItemChanged();

            var pageTitleTask = PageTitlePanel.Current.SetPageTitle(ParentItem);

            ShowViewButton();

            if (ParentItem.IsType("season") && ParentItem.IndexNumber.HasValue)
            {
                TxtParentName.Text = "Season " + ParentItem.IndexNumber.Value;
                TxtParentName.Visibility = Visibility.Visible;
            }
            else
            {
                TxtParentName.Visibility = Visibility.Collapsed;
            }

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

            if (Sidebar.Visibility == Visibility.Visible)
            {
                _sidebarControl.Item = item;
            }
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

            // Sidebar visibility
            string showSidebar;

            if (!displayPreferences.CustomPrefs.TryGetValue("sidebar", out showSidebar))
            {
                showSidebar = "0";
            }

            Sidebar.Visibility = string.Equals(showSidebar, "1")
                                     ? Visibility.Visible
                                     : Visibility.Collapsed;

            BottomGrid.Visibility = displayPreferences.ViewType == ViewTypes.List
                                               ? Visibility.Collapsed
                                               : Visibility.Visible;
        }

        protected virtual double GetItemContainerWidth(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (string.Equals(displayPreferences.ViewType, ViewTypes.List))
            {
                return 1600;
            }

            // 14 = double the margin between items as defined in the resource file
            return displayPreferences.PrimaryImageWidth + 14;
        }

        protected virtual double GetItemContainerHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            // 14 = double the margin between items as defined in the resource file
            return GetImageDisplayHeight(displayPreferences, medianPrimaryImageAspectRatio) + 14;
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

        private void UpdateLogo(BaseItemDto item)
        {
            const int maxheight = 100;

            if (BottomGrid.Visibility == Visibility.Collapsed || Sidebar.Visibility == Visibility.Visible)
            {
                ImgLogo.Visibility = Visibility.Collapsed;
                TxtBottomName.Visibility = Visibility.Collapsed;
            }

            else if (item != null && item.HasLogo && Sidebar.Visibility == Visibility.Collapsed)
            {
                SetLogo(item, ApiClient.GetLogoImageUrl(item, new ImageOptions
                {
                    Height = maxheight,
                    ImageType = ImageType.Logo
                }));
            }
            else if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
            {
                SetLogo(item, ApiClient.GetArtImageUrl(item, new ImageOptions
                {
                    Height = maxheight,
                    ImageType = ImageType.Art
                }));
            }
            else
            {
                SetDefaultLogo(item);
            }
        }

        private async void SetLogo(BaseItemDto item, string url)
        {
            try
            {
                ImgLogo.Source = await ImageManager.GetRemoteBitmapAsync(url);

                ImgLogo.Visibility = Visibility.Visible;
                TxtBottomName.Visibility = Visibility.Collapsed;
            }
            catch
            {
                SetDefaultLogo(item);
            }
        }

        private void SetDefaultLogo(BaseItemDto item)
        {
            TxtBottomName.Text = item.Name;

            // Just hide it so that it still takes up the same amount of space
            ImgLogo.Visibility = Visibility.Hidden;
            TxtBottomName.Visibility = Visibility.Visible;
        }

        protected override double GetImageDisplayHeight(Model.Entities.DisplayPreferences displayPreferences, double medianPrimaryImageAspectRatio)
        {
            if (string.Equals(displayPreferences.ViewType, ViewTypes.Thumbstrip) || string.Equals(displayPreferences.ViewType, ViewTypes.List))
            {
                double height = displayPreferences.PrimaryImageWidth;
                return height / AspectRatioHelper.GetAspectRatio(ImageType.Backdrop, medianPrimaryImageAspectRatio);
            }

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
