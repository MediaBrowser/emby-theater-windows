using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Extensions;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public abstract class BaseItemsControl : UserControl
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
        /// Gets the navigation manager.
        /// </summary>
        /// <value>The navigation manager.</value>
        protected INavigationService NavigationManager { get; private set; }
        protected IPresentationManager PresentationManager { get; private set; }

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
        private DispatcherTimer CurrentSelectionTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemsPage" /> class.
        /// </summary>
        /// <param name="displayPreferences">The display preferences.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appWindow">The app window.</param>
        /// <exception cref="System.ArgumentNullException">parent
        /// or
        /// displayPreferencesId</exception>
        protected BaseItemsControl(DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager appWindow)
        {
            if (displayPreferences == null)
            {
                throw new ArgumentNullException("displayPreferences");
            }

            NavigationManager = navigationManager;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            PresentationManager = appWindow;

            _displayPreferences = displayPreferences;
        }

        /// <summary>
        /// Gets a value indicating whether [auto select first item on first load].
        /// </summary>
        /// <value><c>true</c> if [auto select first item on first load]; otherwise, <c>false</c>.</value>
        protected virtual bool AutoSelectFirstItemOnFirstLoad
        {
            get
            {
                return false;
            }
        }

        private readonly object _timerLock = new object();

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
                    CurrentItemIndex = ListCollectionView.CurrentPosition;
                }

                // Fire notification events after a short delay
                // We don't want backdrops and logos reloading while the user is navigating quickly
                lock (_timerLock)
                {
                    if (CurrentSelectionTimer != null)
                    {
                        CurrentSelectionTimer.Stop();
                        CurrentSelectionTimer.Start();
                    }
                    else
                    {
                        CurrentSelectionTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, CurrentItemChangedTimerCallback, Dispatcher);
                    }
                }

                if (SetBackdropsOnCurrentItemChanged && value != null)
                {
                    PresentationManager.SetBackdrops(value);
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
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = this;

            Unloaded += BaseItemsControl_Unloaded;

            OnPropertyChanged("ParentItem");

            ListItems = new RangeObservableCollection<BaseItemDtoViewModel>();
            ListCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ListItems);
            ItemsList.ItemsSource = ListCollectionView;
            ListCollectionView.CurrentChanged += ListCollectionView_CurrentChanged;

            OnPropertyChanged("DisplayPreferences");
            await ReloadItems(true);
        }

        void BaseItemsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeTimer();
        }

        void ListCollectionView_CurrentChanged(object sender, EventArgs e)
        {
            var viewModel = ListCollectionView.CurrentItem as BaseItemDtoViewModel;

            CurrentItem = viewModel == null ? null : viewModel.Item;
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

                if (AutoSelectFirstItemOnFirstLoad && isInitialLoad)
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

                var meanPrimaryImageAspectRatio = result.Items.MedianPrimaryImageAspectRatio();

                ListItems.AddRange(result.Items.Select(i =>
                {
                    var model = CreateViewModel(i);
                    model.MedianPrimaryImageAspectRatio = meanPrimaryImageAspectRatio;
                    return model;
                }));

                if (selectedIndex.HasValue)
                {
                    new ListFocuser(ItemsList).FocusAfterContainersGenerated(selectedIndex.Value);
                }
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
                return;
            }

            OnItemsChanged();
        }

        protected virtual BaseItemDtoViewModel CreateViewModel(BaseItemDto item)
        {
            return new BaseItemDtoViewModel(ApiClient, ImageManager)
            {
                ImageDisplayWidth = DisplayPreferences.PrimaryImageWidth,
                ImageDisplayHeight = DisplayPreferences.PrimaryImageHeight,
                ViewType = DisplayPreferences.ViewType,
                Item = item
            };
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
                item.ImageDisplayWidth = DisplayPreferences.PrimaryImageWidth;
                item.ImageDisplayHeight = DisplayPreferences.PrimaryImageHeight;
                item.ViewType = DisplayPreferences.ViewType;
            }
        }

        protected virtual bool SetBackdropsOnCurrentItemChanged
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Called when [current item changed].
        /// </summary>
        protected virtual void OnCurrentItemChanged()
        {
        }

        /// <summary>
        /// Currents the item changed timer callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CurrentItemChangedTimerCallback(object sender, EventArgs args)
        {
            DisposeTimer();
            
            // Fire notification events for the UI
            OnPropertyChanged("CurrentItem");

            // Alert subclasses
            OnCurrentItemChanged();
        }

        private void DisposeTimer()
        {
            lock (_timerLock)
            {
                if (CurrentSelectionTimer != null)
                {
                    CurrentSelectionTimer.Stop();
                }
            }
        }
    }
}
