using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class ItemListViewModel : BaseViewModel, IDisposable
    {
        private IPresentationManager PresentationManager { get; set; }
        private IApiClient ApiClient { get; set; }
        private IImageManager ImageManager { get; set; }
        private ISessionManager SessionManager { get; set; }
        private INavigationService NavigationService { get; set; }

        private Timer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        private readonly Func<DisplayPreferences, Task<ItemsResult>> _getItemsDelegate;

        private readonly RangeObservableCollection<ItemViewModel> _listItems =
            new RangeObservableCollection<ItemViewModel>();

        public ICommand NavigateCommand { get; private set; }

        private readonly Dispatcher _dispatcher;

        public ItemListViewModel(Func<DisplayPreferences, Task<ItemsResult>> getItemsDelegate, IPresentationManager presentationManager, IImageManager imageManager, IApiClient apiClient, ISessionManager sessionManager, INavigationService navigationService)
        {
            _getItemsDelegate = getItemsDelegate;
            NavigationService = navigationService;
            SessionManager = sessionManager;
            ApiClient = apiClient;
            ImageManager = imageManager;
            _dispatcher = Dispatcher.CurrentDispatcher;
            PresentationManager = presentationManager;

            NavigateCommand = new RelayCommand(Navigate);
        }

        public Func<BaseItemDto, string> DisplayNameGenerator { get; set; }
        public Func<ItemListViewModel, double> ImageDisplayHeightGenerator { get; set; }
        public Func<ItemListViewModel, ImageType[]> PreferredImageTypesGenerator { get; set; }

        public string DisplayPreferencesId { get; set; }

        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    _listCollectionView.CurrentChanged += _listCollectionView_CurrentChanged;
                    ReloadItems(true);
                }

                return _listCollectionView;
            }

            set
            {
                var changed = _listCollectionView != value;
                _listCollectionView = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        private ItemViewModel _currentItem;
        public ItemViewModel CurrentItem
        {
            get { return _currentItem; }

            set
            {
                var changed = _currentItem != value;

                _currentItem = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentItem");
                }
            }
        }

        private double? _medianPrimaryImageAspectRatio;
        public double? MedianPrimaryImageAspectRatio
        {
            get { return _medianPrimaryImageAspectRatio; }

            set
            {
                var changed = !_medianPrimaryImageAspectRatio.Equals(value);

                _medianPrimaryImageAspectRatio = value;

                if (changed)
                {
                    OnPropertyChanged("MedianPrimaryImageAspectRatio");
                }
            }
        }

        private bool _downloadImageAtExactSize;
        public bool DownloadImageAtExactSize
        {
            get { return _downloadImageAtExactSize; }

            set
            {
                var changed = _downloadImageAtExactSize != value;

                _downloadImageAtExactSize = value;

                if (changed)
                {
                    OnPropertyChanged("DownloadImageAtExactSize");
                }
            }
        }

        private bool _showSidebar;
        public bool ShowSidebar
        {
            get { return _showSidebar; }

            set
            {
                var changed = _showSidebar != value;

                _showSidebar = value;

                if (changed)
                {
                    OnPropertyChanged("ShowSidebar");
                }
            }
        }

        private string _viewType;
        public string ViewType
        {
            get { return _viewType; }

            set
            {
                var changed = !string.Equals(_viewType, value);

                _viewType = value;

                if (changed)
                {
                    OnPropertyChanged("ViewType");
                }
            }
        }

        private double _imageDisplayWidth;
        public double ImageDisplayWidth
        {
            get { return _imageDisplayWidth; }

            set
            {
                var changed = !double.Equals(_imageDisplayWidth, value);

                _imageDisplayWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageDisplayWidth");
                }
            }
        }

        private double _itemContainerHeight;
        public double ItemContainerHeight
        {
            get { return _itemContainerHeight; }

            set
            {
                var changed = !double.Equals(_itemContainerHeight, value);

                _itemContainerHeight = value;

                if (changed)
                {
                    OnPropertyChanged("ItemContainerHeight");
                }
            }
        }

        private double _itemContainerWidth;
        public double ItemContainerWidth
        {
            get { return _itemContainerWidth; }

            set
            {
                var changed = !double.Equals(_itemContainerWidth, value);

                _itemContainerWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ItemContainerWidth");
                }
            }
        }

        private ScrollDirection _scrollDirection;
        public ScrollDirection ScrollDirection
        {
            get { return _scrollDirection; }

            set
            {
                var changed = _scrollDirection != value;

                _scrollDirection = value;

                if (changed)
                {
                    OnPropertyChanged("ScrollDirection");
                }
            }
        }

        private DisplayPreferences _displayPreferences;
        public DisplayPreferences DisplayPreferences
        {
            get { return _displayPreferences; }

            set
            {
                var changed = _displayPreferences != value;

                _displayPreferences = value;

                if (value != null)
                {
                    _displayPreferences.PropertyChanged += _displayPreferences_PropertyChanged;
                }

                if (changed)
                {
                    OnPropertyChanged("DisplayPreferences");
                }
            }
        }

        async void _displayPreferences_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReloadDisplayPreferencesValues();

            if (string.Equals(e.PropertyName, "SortBy") || string.Equals(e.PropertyName, "SortOrder"))
            {
                await ReloadItems(false);
            }
        }

        public Task Initialize()
        {
            _listCollectionView = new ListCollectionView(_listItems);
            _listCollectionView.CurrentChanged += _listCollectionView_CurrentChanged;

            return ReloadItems(true);
        }

        private async Task ReloadItems(bool isInitialLoad)
        {
            if (isInitialLoad && !string.IsNullOrEmpty(DisplayPreferencesId))
            {
                await ReloadDisplayPreferences();
            }

            // Record the current item
            var currentItem = _listCollectionView.CurrentItem as ItemViewModel;

            try
            {
                var result = await _getItemsDelegate(DisplayPreferences);
                var items = result.Items;

                int? selectedIndex = null;

                if (isInitialLoad)
                {
                    selectedIndex = 0;
                }
                else if (currentItem != null)
                {
                    var index = Array.FindIndex(items, i => string.Equals(i.Id, currentItem.Item.Id));

                    if (index != -1)
                    {
                        selectedIndex = index;
                    }
                }

                MedianPrimaryImageAspectRatio = result.Items.MedianPrimaryImageAspectRatio();

                _listItems.Clear();

                var imageTypes = GetPreferredImageTypes();
                var imageDisplayHeight = GetImageDisplayHeight();

                _listItems.AddRange(items.Select(i => new ItemViewModel(ApiClient, ImageManager)
                {
                    DownloadPrimaryImageAtExactSize = IsCloseToMedianPrimaryImageAspectRatio(i),
                    ImageDisplayHeight = imageDisplayHeight,
                    ImageDisplayWidth = ImageDisplayWidth,
                    Item = i,
                    ViewType = ViewType,
                    DisplayName = DisplayNameGenerator == null ? i.Name : DisplayNameGenerator(i),
                    DownloadImagesAtExactSize = true,
                    PreferredImageTypes = imageTypes
                }));

                if (selectedIndex.HasValue)
                {
                    ListCollectionView.MoveCurrentToPosition(selectedIndex.Value);
                }
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }

        private ImageType[] GetPreferredImageTypes()
        {
            return PreferredImageTypesGenerator == null ? new[] { ImageType.Primary } : PreferredImageTypesGenerator(this);
        }

        public double DefaultImageDisplayHeight
        {
            get { return DisplayPreferences == null ? ImageDisplayWidth : DisplayPreferences.PrimaryImageHeight; }
        }

        private double GetImageDisplayHeight()
        {
            return ImageDisplayHeightGenerator == null ? DefaultImageDisplayHeight : ImageDisplayHeightGenerator(this);
        }

        void _listCollectionView_CurrentChanged(object sender, EventArgs e)
        {
            var item = ListCollectionView.CurrentItem as ItemViewModel;

            if (item != null)
            {
                PresentationManager.SetBackdrops(item.Item);
            }

            lock (_syncLock)
            {
                if (_selectionChangeTimer == null)
                {
                    _selectionChangeTimer = new Timer(OnSelectionTimerFired, null, 300, Timeout.Infinite);
                }
                else
                {
                    _selectionChangeTimer.Change(300, Timeout.Infinite);
                }
            }
        }
        private void OnSelectionTimerFired(object state)
        {
            _dispatcher.InvokeAsync(UpdateCurrentItem);
        }

        private void UpdateCurrentItem()
        {
            CurrentItem = ListCollectionView.CurrentItem as ItemViewModel;
        }

        private async Task ReloadDisplayPreferences()
        {
            try
            {
                DisplayPreferences = await ApiClient.GetDisplayPreferencesAsync(DisplayPreferencesId, SessionManager.CurrentUser.Id, "DefaultTheme");
            }
            catch
            {
                // Already logged at lower levels
                return;
            }

            ReloadDisplayPreferencesValues();
        }

        private void ReloadDisplayPreferencesValues()
        {
            ViewType = DisplayPreferences.ViewType;
            ImageDisplayWidth = DisplayPreferences.PrimaryImageWidth;
            ScrollDirection = DisplayPreferences.ScrollDirection;
            ShowSidebar = DisplayPreferences.ShowSidebar;

            var imageTypes = GetPreferredImageTypes();
            var imageDisplayHeight = GetImageDisplayHeight();

            foreach (var item in _listItems.ToList())
            {
                item.PreferredImageTypes = imageTypes;
                item.SetDisplayPreferences(ImageDisplayWidth, imageDisplayHeight, ViewType);
            }
        }

        private bool IsCloseToMedianPrimaryImageAspectRatio(BaseItemDto item)
        {
            if (ImageDisplayWidth.Equals(0))
            {
                return true;
            }

            var imageDisplayHeight = GetImageDisplayHeight();

            var layoutAspectRatio = ImageDisplayWidth / imageDisplayHeight;
            var currentAspectRatio = item.PrimaryImageAspectRatio ?? MedianPrimaryImageAspectRatio ?? layoutAspectRatio;

            // Preserve the exact AR if it deviates from the median significantly
            return Math.Abs(currentAspectRatio - layoutAspectRatio) <= .25;
        }

        public async void Navigate(object commandParameter)
        {
            var item = commandParameter as ItemViewModel;

            if (item != null)
            {
                try
                {
                    await NavigationService.NavigateToItem(item.Item);
                }
                catch
                {
                    PresentationManager.ShowDefaultErrorMessage();
                }
            }
        }

        public void Dispose()
        {
            DisposeTimer();

            if (_listCollectionView != null)
            {
                _listCollectionView.CurrentChanged -= _listCollectionView_CurrentChanged;
            }

            foreach (var item in _listItems.ToList())
            {
                item.Dispose();
            }
        }

        private void DisposeTimer()
        {
            lock (_syncLock)
            {
                if (_selectionChangeTimer != null)
                {
                    _selectionChangeTimer.Dispose();
                    _selectionChangeTimer = null;
                }
            }
        }
    }
}
