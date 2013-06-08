using System.Windows.Controls.Primitives;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.ViewModels;
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

namespace MediaBrowser.UI.Pages
{
    public abstract class BaseItemsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected IApiClient ApiClient { get; private set; }
        protected IImageManager ImageManager { get; private set; }
        protected ISessionManager SessionManager { get; private set; }
        protected IApplicationWindow ApplicationWindow { get; private set; }
        protected INavigationService NavigationManager { get; private set; }

        private IInputElement _lastFocused;

        protected RangeObservableCollection<BaseItemDtoViewModel> ListItems { get; private set; }
        protected ListCollectionView ListCollectionView { get; private set; }

        protected abstract ExtendedListBox ItemsList { get; }
        protected abstract Task<ItemsResult> GetItemsAsync();
        private Timer CurrentSelectionTimer { get; set; }

        protected BaseItemsPage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager)
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
        }

        protected string DisplayPreferencesId { get; private set; }

        private BaseItemDto _parentItem;
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

        public int ItemCount
        {
            get { return ListItems.Count; }
        }

        public Orientation WrapPanelOrientation
        {
            get
            {
                return DisplayPreferences.ScrollDirection == ScrollDirection.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
            }
        }

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

        void BaseItemsPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigating -= NavigationService_Navigating;
            NavigationService.Navigating += NavigationService_Navigating;
        }

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

        void ItemsList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var model = e.Argument as BaseItemDtoViewModel;

            if (model != null)
            {
                var item = model.Item;

                NavigationManager.NavigateToItem(item, string.Empty);
            }
        }

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

        protected async Task ReloadDisplayPreferences()
        {
            try
            {
                DisplayPreferences = await ApiClient.GetDisplayPreferencesAsync(DisplayPreferencesId);

            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
            }
        }

        protected async Task ReloadItems(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = ListCollectionView.CurrentItem as BaseItemDtoViewModel;

            try
            {
                var result = await GetItemsAsync();

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
                App.Instance.ShowDefaultErrorMessage();
                return;
            }

            OnItemsChanged();
        }

        protected virtual void OnItemsChanged()
        {
            OnPropertyChanged("ItemCount");
        }

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

        protected virtual void OnParentItemChanged()
        {
            ApplicationWindow.SetBackdrops(ParentItem);
        }

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
