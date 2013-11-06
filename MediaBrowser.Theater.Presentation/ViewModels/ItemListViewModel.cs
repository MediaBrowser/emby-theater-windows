using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class ItemListViewModel : BaseViewModel, IDisposable, IAcceptsPlayCommand
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navigationService;
        private readonly IPlaybackManager _playbackManager;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;

        private Timer _indexSelectionChangeTimer;
        private Timer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        private readonly Func<ItemListViewModel, Task<ItemsResult>> _getItemsDelegate;

        private readonly RangeObservableCollection<ItemViewModel> _listItems =
            new RangeObservableCollection<ItemViewModel>();

        public ICommand NavigateCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }

        public bool EnableBackdropsForCurrentItem { get; set; }
        public bool AutoSelectFirstItem { get; set; }
        public bool ShowLoadingAnimation { get; set; }

        private readonly Dispatcher _dispatcher;

        public ItemListViewModel(Func<ItemListViewModel, Task<ItemsResult>> getItemsDelegate, IPresentationManager presentationManager, IImageManager imageManager, IApiClient apiClient, INavigationService navigationService, IPlaybackManager playbackManager, ILogger logger, IServerEvents serverEvents)
        {
            EnableBackdropsForCurrentItem = true;

            _getItemsDelegate = getItemsDelegate;
            _navigationService = navigationService;
            _playbackManager = playbackManager;
            _logger = logger;
            _serverEvents = serverEvents;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _presentationManager = presentationManager;

            _indexOptionsCollectionView = new ListCollectionView(_indexOptions);
            _indexOptionsCollectionView.CurrentChanged += _indexOptionsCollectionView_CurrentChanged;

            NavigateCommand = new RelayCommand(Navigate);
            PlayCommand = new RelayCommand(Play);
        }

        public string ListType { get; set; }

        public Func<BaseItemDto, string> DisplayNameGenerator { get; set; }
        public Func<ItemListViewModel, double> ImageDisplayHeightGenerator { get; set; }
        public Func<ItemListViewModel, ImageType[]> PreferredImageTypesGenerator { get; set; }

        public ViewType Context { get; set; }

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

        private readonly RangeObservableCollection<TabItem> _indexOptions =
           new RangeObservableCollection<TabItem>();

        private readonly ListCollectionView _indexOptionsCollectionView;
        public ListCollectionView IndexOptionsCollectionView
        {
            get
            {
                return _indexOptionsCollectionView;
            }
        }

        public bool HasIndexOptions
        {
            get
            {
                return _indexOptions.Count > 0;
            }
        }

        public void AddIndexOptions(IEnumerable<TabItem> options)
        {
            _indexOptions.AddRange(options);

            OnPropertyChanged("HasIndexOptions");
        }

        private int _itemCount;
        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
            set
            {
                _itemCount = value;

                OnPropertyChanged("ItemCount");
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

        private int _currentItemIndex;
        public int CurrentItemIndex
        {
            get
            {
                return _currentItemIndex;
            }
            set
            {
                _currentItemIndex = value;

                OnPropertyChanged("CurrentItemIndex");
            }
        }

        private bool? _showTitle;
        public bool? ShowTitle
        {
            get { return _showTitle; }

            set
            {
                _showTitle = value;

                OnPropertyChanged("ShowTitle");
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

        private bool? _downloadImageAtExactSize;
        public bool? DownloadImageAtExactSize
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

        private Stretch? _imageStretch;
        public Stretch? ImageStretch
        {
            get { return _imageStretch; }

            set
            {
                var changed = _imageStretch != value;

                _imageStretch = value;

                if (changed)
                {
                    OnPropertyChanged("ImageStretch");
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
                    OnPropertyChanged("ImageWidth");
                }
            }
        }

        private TabItem _currentIndexOption;
        public TabItem CurrentIndexOption
        {
            get { return _currentIndexOption; }

            set
            {
                var changed = _currentIndexOption != value;

                _currentIndexOption = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentIndexOption");
                }
            }
        }

        public Func<ItemListViewModel, double> ItemContainerWidthGenerator { get; set; }
        public Func<ItemListViewModel, double> ItemContainerHeightGenerator { get; set; }

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

                    ReloadDisplayPreferencesValues();
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

        private async Task ReloadItems(bool isInitialLoad)
        {
            if (HasIndexOptions && CurrentIndexOption == null)
            {
                return;
            }

            if (ShowLoadingAnimation)
            {
                _presentationManager.ShowLoadingAnimation();
            }

            try
            {
                var result = await _getItemsDelegate(this);
                var items = result.Items;

                int? selectedIndex = null;

                if (isInitialLoad && AutoSelectFirstItem)
                {
                    selectedIndex = 0;
                }

                MedianPrimaryImageAspectRatio = result.Items.MedianPrimaryImageAspectRatio();

                _listItems.Clear();

                var imageTypes = GetPreferredImageTypes();

                var childWidth = Convert.ToInt32(ImageDisplayWidth);
                var imageDisplayHeight = Convert.ToInt32(GetImageDisplayHeight());

                var index = 0;

                var viewModels = items.Select(
                    i =>
                    {
                        var vm = new ItemViewModel(_apiClient, _imageManager, _playbackManager,
                                                   _presentationManager, _logger, _serverEvents)
                        {
                            DownloadPrimaryImageAtExactSize = DownloadImageAtExactSize ?? IsCloseToMedianPrimaryImageAspectRatio(i),
                            ImageHeight = imageDisplayHeight,
                            ImageWidth = childWidth,
                            Item = i,
                            ViewType = ViewType,
                            DisplayName = DisplayNameGenerator == null ? i.Name : DisplayNameGenerator(i),
                            DownloadImagesAtExactSize = DownloadImageAtExactSize ?? true,
                            PreferredImageTypes = imageTypes,
                            ListType = ListType,
                            ShowTitle = ShowTitle
                        };

                        var stretch = ImageStretch;

                        if (!stretch.HasValue && imageTypes.Length > 0)
                        {
                            var exact = imageTypes[0] == ImageType.Primary ? vm.DownloadPrimaryImageAtExactSize : vm.DownloadImagesAtExactSize;
                            stretch = exact ? Stretch.UniformToFill : Stretch.Uniform;
                        }

                        if (stretch.HasValue)
                        {
                            vm.ImageStretch = stretch.Value;
                        }

                        if (index < 30)
                        {
                            index++;
                            Task.Run(() => vm.DownloadImage());
                        }

                        return vm;
                    }
                    ).ToList();

                _listItems.AddRange(viewModels);

                ItemCount = items.Length;

                UpdateContainerSizes();

                if (selectedIndex.HasValue)
                {
                    ListCollectionView.MoveCurrentToPosition(selectedIndex.Value);
                }
            }
            catch (Exception ex)
            {
                _presentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                if (ShowLoadingAnimation)
                {
                    _presentationManager.HideLoadingAnimation();
                }
            }
        }

        private ImageType[] GetPreferredImageTypes()
        {
            return PreferredImageTypesGenerator == null ? new[] { ImageType.Primary } : PreferredImageTypesGenerator(this);
        }

        public double DefaultImageDisplayHeight
        {
            get
            {
                if (MedianPrimaryImageAspectRatio.HasValue)
                {
                    double height = ImageDisplayWidth;

                    return height / MedianPrimaryImageAspectRatio.Value;
                }

                return DisplayPreferences == null ? ImageDisplayWidth : DisplayPreferences.PrimaryImageHeight;
            }
        }

        private double GetImageDisplayHeight()
        {
            return ImageDisplayHeightGenerator == null ? DefaultImageDisplayHeight : ImageDisplayHeightGenerator(this);
        }

        void _listCollectionView_CurrentChanged(object sender, EventArgs e)
        {
            var item = ListCollectionView.CurrentItem as ItemViewModel;

            CurrentItemIndex = item == null ? -1 : _listItems.IndexOf(item) + 1;

            if (EnableBackdropsForCurrentItem)
            {
                if (item != null)
                {
                    _presentationManager.SetBackdrops(item.Item);
                }
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

        public Func<ItemListViewModel, bool> ShowSidebarGenerator { get; set; }
        public Func<ItemListViewModel, ScrollDirection> ScrollDirectionGenerator { get; set; }

        private void ReloadDisplayPreferencesValues()
        {
            var displayPreferences = DisplayPreferences;

            ViewType = displayPreferences.ViewType;
            ImageDisplayWidth = displayPreferences.PrimaryImageWidth;

            UpdateContainerSizes();

            if (ScrollDirectionGenerator != null)
            {
                ScrollDirection = ScrollDirectionGenerator(this);
            }

            if (ShowSidebarGenerator != null)
            {
                ShowSidebar = ShowSidebarGenerator(this);
            }

            var imageTypes = GetPreferredImageTypes();
            var imageDisplayHeight = GetImageDisplayHeight();

            var newWidth = Convert.ToInt32(ImageDisplayWidth);
            var newHeight = Convert.ToInt32(imageDisplayHeight);

            foreach (var item in _listItems.ToList())
            {
                item.PreferredImageTypes = imageTypes;
                item.SetDisplayPreferences(newWidth, newHeight, ViewType);
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
            return Math.Abs(currentAspectRatio - layoutAspectRatio) <= .3;
        }

        private void UpdateContainerSizes()
        {
            if (ItemContainerWidthGenerator != null)
            {
                ItemContainerWidth = ItemContainerWidthGenerator(this);
            }
            if (ItemContainerHeightGenerator != null)
            {
                ItemContainerHeight = ItemContainerHeightGenerator(this);
            }
        }

        private async void Navigate(object commandParameter)
        {
            var item = commandParameter as ItemViewModel;

            if (item != null)
            {
                try
                {
                    await _navigationService.NavigateToItem(item.Item, Context);
                }
                catch (Exception)
                {
                    _presentationManager.ShowDefaultErrorMessage();
                }
            }
        }

        private void Play(object commandParameter)
        {
            var item = commandParameter as ItemViewModel;

            if (item != null)
            {
                item.Play();
            }
        }

        private readonly object _indexSyncLock = new object();
        private object _lastIndexValue;

        void _indexOptionsCollectionView_CurrentChanged(object sender, EventArgs e)
        {
            if (!HasIndexOptions)
            {
                return;
            }

            lock (_indexSyncLock)
            {
                if (_lastIndexValue==null)
                {
                    OnIndexSelectionChange(null);
                    _lastIndexValue = _indexOptionsCollectionView.CurrentItem;
                    return;
                }

                if (_lastIndexValue == _indexOptionsCollectionView.CurrentItem)
                {
                    return;
                }

                _lastIndexValue = _indexOptionsCollectionView.CurrentItem;
                
                if (_indexSelectionChangeTimer == null)
                {
                    _indexSelectionChangeTimer = new Timer(OnIndexSelectionChange, null, 600, Timeout.Infinite);
                }
                else
                {
                    _indexSelectionChangeTimer.Change(600, Timeout.Infinite);
                }
            }
        }

        private void OnIndexSelectionChange(object state)
        {
            _dispatcher.InvokeAsync(UpdateCurrentIndex);
        }

        private void UpdateCurrentIndex()
        {
            CurrentIndexOption = IndexOptionsCollectionView.CurrentItem as TabItem;

            ReloadItems(false);
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

            if (_displayPreferences != null)
            {
                _displayPreferences.PropertyChanged += _displayPreferences_PropertyChanged;
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

            lock (_indexSyncLock)
            {
                if (_indexSelectionChangeTimer != null)
                {
                    _indexSelectionChangeTimer.Dispose();
                    _indexSelectionChangeTimer = null;
                }
            }
        }

        public void HandlePlayCommand()
        {
            Play(CurrentItem);
        }
    }
}
