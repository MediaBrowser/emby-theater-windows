using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MediaBrowser.Theater.Presentation.Pages
{
    /// <summary>
    /// Class BaseItemsPage
    /// </summary>
    public abstract class BaseItemsPage : Page, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        protected IApiClient ApiClient { get; private set; }
        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        protected IImageManager ImageManager { get; private set; }
        /// <summary>
        /// Gets the session manager.
        /// </summary>
        /// <value>The session manager.</value>
        protected ISessionManager SessionManager { get; private set; }
        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        protected IApplicationWindow ApplicationWindow { get; private set; }
        /// <summary>
        /// Gets the navigation manager.
        /// </summary>
        /// <value>The navigation manager.</value>
        protected INavigationService NavigationManager { get; private set; }
        /// <summary>
        /// Gets the theme manager.
        /// </summary>
        /// <value>The theme manager.</value>
        protected IThemeManager ThemeManager { get; private set; }

        /// <summary>
        /// The _last focused
        /// </summary>
        private IInputElement _lastFocused;

        /// <summary>
        /// Gets the list items.
        /// </summary>
        /// <value>The list items.</value>
        protected RangeObservableCollection<BaseItemDtoViewModel> ListItems { get; private set; }
        /// <summary>
        /// Gets the list collection view.
        /// </summary>
        /// <value>The list collection view.</value>
        protected ListCollectionView ListCollectionView { get; private set; }

        /// <summary>
        /// Gets the items list.
        /// </summary>
        /// <value>The items list.</value>
        protected abstract ExtendedListBox ItemsList { get; }
        /// <summary>
        /// Gets the items async.
        /// </summary>
        /// <returns>Task{ItemsResult}.</returns>
        protected abstract Task<ItemsResult> GetItemsAsync();
        /// <summary>
        /// Gets or sets the current selection timer.
        /// </summary>
        /// <value>The current selection timer.</value>
        private Timer CurrentSelectionTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemsPage"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="displayPreferencesId">The display preferences id.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="applicationWindow">The application window.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="themeManager">The theme manager.</param>
        /// <exception cref="System.ArgumentNullException">
        /// parent
        /// or
        /// displayPreferencesId
        /// </exception>
        protected BaseItemsPage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager, IThemeManager themeManager)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (string.IsNullOrEmpty(displayPreferencesId))
            {
                throw new ArgumentNullException("displayPreferencesId");
            }

            NavigationManager = navigationManager;
            ApplicationWindow = applicationWindow;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;

            DisplayPreferencesId = displayPreferencesId;
            _parentItem = parent;
            ThemeManager = themeManager;
        }

        /// <summary>
        /// Gets the display preferences id.
        /// </summary>
        /// <value>The display preferences id.</value>
        protected string DisplayPreferencesId { get; private set; }

        /// <summary>
        /// The _parent item
        /// </summary>
        private BaseItemDto _parentItem;
        /// <summary>
        /// Gets or sets the parent item.
        /// </summary>
        /// <value>The parent item.</value>
        public BaseItemDto ParentItem
        {
            get { return _parentItem; }

            set
            {
                _parentItem = value;
                OnPropertyChanged("ParentItem");
            }
        }

        /// <summary>
        /// The _current item
        /// </summary>
        private BaseItemDto _currentItem;
        /// <summary>
        /// Gets or sets the current selected item
        /// </summary>
        /// <value>The current item.</value>
        public BaseItemDto CurrentItem
        {
            get { return _currentItem; }

            set
            {
                _currentItem = value;

                // Update the current item index immediately
                if (value == null)
                {
                    CurrentItemIndex = -1;
                }
                else
                {
                    CurrentItemIndex = ItemsList.SelectedIndex;
                }

                // Fire notification events after a short delay
                // We don't want backdrops and logos reloading while the user is navigating quickly
                if (CurrentSelectionTimer != null)
                {
                    CurrentSelectionTimer.Change(500, Timeout.Infinite);
                }
                else
                {
                    CurrentSelectionTimer = new Timer(CurrentItemChangedTimerCallback, value, 500, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// The _current item index
        /// </summary>
        private int _currentItemIndex;
        /// <summary>
        /// Gets of sets the index of the current item being displayed
        /// </summary>
        /// <value>The index of the current item.</value>
        public int CurrentItemIndex
        {
            get { return _currentItemIndex; }

            set
            {
                _currentItemIndex = value;
                OnPropertyChanged("CurrentItemIndex");
            }
        }

        /// <summary>
        /// The _display preferences
        /// </summary>
        private DisplayPreferences _displayPreferences;
        /// <summary>
        /// Gets of sets the current DisplayPreferences
        /// </summary>
        /// <value>The display preferences.</value>
        public DisplayPreferences DisplayPreferences
        {
            get { return _displayPreferences; }

            private set
            {
                _displayPreferences = value;

                OnPropertyChanged("DisplayPreferences");
            }
        }

        /// <summary>
        /// The _item count
        /// </summary>
        private int _itemCount;
        /// <summary>
        /// Gets or sets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public int ItemCount
        {
            get { return _itemCount; }

            set
            {
                _itemCount = value;
                OnPropertyChanged("ItemCount");
            }
        }

        /// <summary>
        /// Gets the wrap panel orientation.
        /// </summary>
        /// <value>The wrap panel orientation.</value>
        public Orientation WrapPanelOrientation
        {
            get
            {
                return DisplayPreferences.ScrollDirection == ScrollDirection.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = this;

            OnPropertyChanged("ParentItem");
            
            Loaded += BaseItemsPage_Loaded;
            FocusManager.SetIsFocusScope(this, true);

            ListItems = new RangeObservableCollection<BaseItemDtoViewModel>();
            ListCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ListItems);

            ItemsList.ItemsSource = ListCollectionView;
            ItemsList.ItemInvoked += ItemsList_ItemInvoked;
            ItemsList.SelectionChanged += ItemsList_SelectionChanged;

            await ReloadDisplayPreferences();
            await ReloadItems(true);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ItemsList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                CurrentItem = (e.AddedItems[0] as BaseItemDtoViewModel).Item;
            }
            else
            {
                CurrentItem = null;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the BaseItemsPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BaseItemsPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigating -= NavigationService_Navigating;
            NavigationService.Navigating += NavigationService_Navigating;
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (string.Equals(name, "DisplayPreferences"))
            {
                OnDisplayPreferencesChanged();
            }
            else if (string.Equals(name, "ParentItem"))
            {
                OnParentItemChanged();
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

        /// <summary>
        /// Handles the Navigating event of the NavigationService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs"/> instance containing the event data.</param>
        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content.Equals(this))
            {
                FocusManager.SetFocusedElement(this, _lastFocused);
            }
            else
            {
                _lastFocused = FocusManager.GetFocusedElement(this);
            }
        }

        /// <summary>
        /// Reloads the display preferences.
        /// </summary>
        /// <returns>Task.</returns>
        protected async Task ReloadDisplayPreferences()
        {
            try
            {
                DisplayPreferences = await ApiClient.GetDisplayPreferencesAsync(DisplayPreferencesId);

            }
            catch (HttpException)
            {
                ThemeManager.CurrentTheme.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Reloads the items.
        /// </summary>
        /// <param name="isInitialLoad">if set to <c>true</c> [is initial load].</param>
        /// <returns>Task.</returns>
        protected async Task ReloadItems(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = ListCollectionView.CurrentItem as BaseItemDtoViewModel;

            try
            {
                var result = await GetItemsAsync();

                ItemCount = result.Items.Length;

                int? selectedIndex = null;

                if (isInitialLoad)
                {
                    selectedIndex = 0;
                }
                else if (currentItem != null)
                {
                    var index = Array.FindIndex(result.Items, i => string.Equals(i.Id, currentItem.Item.Id));

                    if (index != -1)
                    {
                        selectedIndex = index;
                    }
                }
                
                ListItems.Clear();

                var averagePrimaryImageAspectRatio = BaseItemDtoViewModel.GetAveragePrimaryImageAspectRatio(result.Items);

                ListItems.AddRange(result.Items.Select(i => new BaseItemDtoViewModel(ApiClient, ImageManager)
                {
                    ImageWidth = DisplayPreferences.PrimaryImageWidth,
                    ViewType = DisplayPreferences.ViewType,
                    Item = i,
                    AveragePrimaryImageAspectRatio = averagePrimaryImageAspectRatio
                }));

                if (_lastFocused == null)
                {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }

                if (selectedIndex.HasValue)
                {
                    new ListFocuser(ItemsList).FocusAfterContainersGenerated(selectedIndex.Value);
                }
            }
            catch (HttpException)
            {
                ThemeManager.CurrentTheme.ShowDefaultErrorMessage();
                return;
            }

            OnItemsChanged();
        }

        /// <summary>
        /// Called when [items changed].
        /// </summary>
        protected virtual void OnItemsChanged()
        {
        }

        /// <summary>
        /// Called when [display preferences changed].
        /// </summary>
        protected virtual void OnDisplayPreferencesChanged()
        {
            // Notify all of the child view models
            foreach (var item in ListItems)
            {
                item.ImageWidth = DisplayPreferences.PrimaryImageWidth;
                item.ViewType = DisplayPreferences.ViewType;
            }

            OnPropertyChanged("WrapPanelOrientation");

            if (DisplayPreferences.ScrollDirection == ScrollDirection.Horizontal)
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

        /// <summary>
        /// Called when [parent item changed].
        /// </summary>
        protected virtual void OnParentItemChanged()
        {
            ApplicationWindow.SetBackdrops(ParentItem);
        }

        /// <summary>
        /// Called when [current item changed].
        /// </summary>
        protected virtual void OnCurrentItemChanged()
        {
            if (CurrentItem != null)
            {
                ApplicationWindow.SetBackdrops(CurrentItem);
            }
            else
            {
                ApplicationWindow.ClearBackdrops();
            }
        }

        /// <summary>
        /// Currents the item changed timer callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private async void CurrentItemChangedTimerCallback(object state)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                // Fire notification events for the UI
                OnPropertyChanged("CurrentItem");

                // Alert subclasses
                OnCurrentItemChanged();
            });
        }

    }
}
