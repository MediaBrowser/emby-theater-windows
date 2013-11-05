using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Extensions;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.ListPage
{
    /// <summary>
    /// Interaction logic for FolderPage.xaml
    /// </summary>
    public partial class FolderPage : BasePage, ISupportsItemThemeMedia, ISupportsBackdrops, IItemPage, IHasDisplayPreferences
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IPresentationManager _presentationManager;
        private readonly ILogger _logger;

        private readonly BaseItemDto _parentItem;

        private readonly ItemListViewModel _viewModel;

        private readonly ListPageConfig _options;

        public FolderPage(BaseItemDto parent, DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, IPresentationManager presentation, INavigationService navigationManager, IPlaybackManager playbackManager, ILogger logger, IServerEvents serverEvents, ListPageConfig options)
        {
            _logger = logger;
            _presentationManager = presentation;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _options = options;

            _parentItem = parent;

            InitializeComponent();

            Loaded += FolderPage_Loaded;

            SetDefaults(displayPreferences);

            _viewModel = new ItemListViewModel(vm => options.CustomItemQuery(vm, displayPreferences), _presentationManager, _imageManager, _apiClient, navigationManager, playbackManager, _logger, serverEvents)
            {
                ImageDisplayHeightGenerator = GetImageDisplayHeight,
                DisplayNameGenerator = options.DisplayNameGenerator ?? GetDisplayName,
                PreferredImageTypesGenerator = GetPreferredImageTypes,

                ShowSidebarGenerator = GetShowSidebar,
                ScrollDirectionGenerator = GetScrollDirection,

                AutoSelectFirstItem = true,

                ShowLoadingAnimation = true
            };

            _viewModel.AddIndexOptions(options.IndexOptions);

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;

            _viewModel.DisplayPreferences = displayPreferences;

            if (!string.IsNullOrEmpty(options.IndexValue))
            {
                var index = options.IndexOptions.First(i => string.Equals(i.Name, options.IndexValue));
                _viewModel.IndexOptionsCollectionView.MoveCurrentTo(index);
            }

            DataContext = _viewModel;
        }

        void FolderPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetPageTitle(_parentItem);
        }

        private void SetDefaults(DisplayPreferences displayPreferences)
        {
            if (string.IsNullOrEmpty(displayPreferences.ViewType))
            {
                displayPreferences.ViewType = _options.DefaultViewType;
            }

            if (string.Equals(displayPreferences.ViewType, ListViewTypes.Poster, StringComparison.OrdinalIgnoreCase))
            {
                displayPreferences.PrimaryImageWidth = _options.PosterImageWidth;
            }
            else if (string.Equals(displayPreferences.ViewType, ListViewTypes.PosterStrip, StringComparison.OrdinalIgnoreCase))
            {
                displayPreferences.PrimaryImageWidth = _options.PosterStripImageWidth;
            }
            else if (string.Equals(displayPreferences.ViewType, ListViewTypes.List, StringComparison.OrdinalIgnoreCase))
            {
                displayPreferences.PrimaryImageWidth = _options.ListImageWidth;
            }
            else if (string.Equals(displayPreferences.ViewType, ListViewTypes.Thumbstrip, StringComparison.OrdinalIgnoreCase))
            {
                displayPreferences.PrimaryImageWidth = _options.ThumbImageWidth;
            }
        }

        public ViewType ViewType
        {
            get { return _viewModel.Context; }
            set { _viewModel.Context = value; }
        }

        private ScrollDirection GetScrollDirection(ItemListViewModel viewModel)
        {
            if (string.Equals(viewModel.ViewType, ListViewTypes.List))
            {
                return ScrollDirection.Vertical;
            }
            if (string.Equals(viewModel.ViewType, ListViewTypes.Thumbstrip))
            {
                return ScrollDirection.Horizontal;
            }
            if (string.Equals(viewModel.ViewType, ListViewTypes.PosterStrip))
            {
                return ScrollDirection.Horizontal;
            }

            return viewModel.DisplayPreferences == null
                       ? ScrollDirection.Horizontal
                       : viewModel.DisplayPreferences.ScrollDirection;
        }

        private ImageType[] GetPreferredImageTypes(ItemListViewModel viewModel)
        {
            return string.Equals(viewModel.ViewType, ListViewTypes.Thumbstrip) || string.Equals(viewModel.ViewType, ListViewTypes.List)
                       ? GetListViewPreferredImageTypes()
                       : new[] { ImageType.Primary };
        }

        private ImageType[] GetListViewPreferredImageTypes()
        {
            if (_parentItem.IsType("Season") || _parentItem.IsType("Series"))
            {
                return new[] { ImageType.Primary, ImageType.Thumb };
            }

            return new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary };
        }

        private bool GetShowSidebar(ItemListViewModel viewModel)
        {
            if (string.Equals(viewModel.ViewType, ListViewTypes.List))
            {
                return true;
            }
            if (string.Equals(viewModel.ViewType, ListViewTypes.Thumbstrip))
            {
                return false;
            }
            if (string.Equals(viewModel.ViewType, ListViewTypes.PosterStrip))
            {
                return false;
            }

            return viewModel.DisplayPreferences != null && viewModel.DisplayPreferences.ShowSidebar;
        }

        private void SetPageTitle(BaseItemDto parentItem)
        {
            if (!string.IsNullOrEmpty(_options.PageTitle))
            {
                _presentationManager.SetPageTitle(_options.PageTitle);
            }
            else
            {
                DefaultTheme.Current.PageContentDataContext.SetPageTitle(parentItem);
            }
        }

        public static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IndexNumber.HasValue && !item.IsType("season"))
            {
                name = item.IndexNumber + " - " + name;
            }

            if (item.ParentIndexNumber.HasValue && item.IsAudio)
            {
                name = item.ParentIndexNumber + "." + name;
            }

            return name;
        }

        void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "ViewType") || string.Equals(e.PropertyName, "ImageWidth") || string.Equals(e.PropertyName, "MedianPrimaryImageAspectRatio"))
            {
                _viewModel.ItemContainerWidth = _viewModel.ImageDisplayWidth + 20;
                _viewModel.ItemContainerHeight = GetItemContainerHeight(_viewModel);
            }

            if (string.Equals(e.PropertyName, "CurrentItem"))
            {
                var currentViewModel = _viewModel.CurrentItem;

                if (currentViewModel != null)
                {
                    if (currentViewModel.Item != null)
                    {
                        UpdateLogo(currentViewModel.Item);
                    }
                }
            }
        }

        public static ItemFields[] QueryFields = new[]
            {
                ItemFields.PrimaryImageAspectRatio,
                ItemFields.DateCreated,
                ItemFields.MediaStreams,
                ItemFields.Taglines,
                ItemFields.Genres,
                ItemFields.Overview,
                ItemFields.DisplayPreferencesId
            };

        public string ThemeMediaItemId
        {
            get { return _parentItem.Id; }
        }

        public BaseItemDto PageItem
        {
            get { return _parentItem; }
        }

        private double GetItemContainerHeight(ItemListViewModel viewModel)
        {
            var height = GetImageDisplayHeight(viewModel);

            // Add the bottom border
            if (string.Equals(viewModel.ViewType, ListViewTypes.List))
            {
                height += 12;
            }
            else
            {
                // 20 = double the margin between items as defined in the resource file
                height += 20;
            }

            return height;
        }

        private double GetImageDisplayHeight(ItemListViewModel viewModel)
        {
            var imageDisplayWidth = viewModel.ImageDisplayWidth;
            var medianPrimaryImageAspectRatio = viewModel.MedianPrimaryImageAspectRatio ?? 0;

            var imageType = GetPreferredImageTypes(viewModel).First();

            if (imageType == ImageType.Backdrop || imageType == ImageType.Screenshot || imageType == ImageType.Thumb)
            {
                double height = imageDisplayWidth;
                return height / AspectRatioHelper.GetAspectRatio(ImageType.Backdrop, medianPrimaryImageAspectRatio);
            }

            if (!medianPrimaryImageAspectRatio.Equals(0) && imageType == ImageType.Primary)
            {
                double height = imageDisplayWidth;
                height /= medianPrimaryImageAspectRatio;

                return height;
            }

            return viewModel.DefaultImageDisplayHeight;
        }

        private void UpdateLogo(BaseItemDto item)
        {
            if (Sidebar.Visibility != Visibility.Visible)
            {
                var isStripView = string.Equals(_viewModel.ViewType, ListViewTypes.Thumbstrip) ||
                                  string.Equals(_viewModel.ViewType, ListViewTypes.PosterStrip);

                if (isStripView && item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
                {
                    SetLogo(_apiClient.GetArtImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Art
                    }));
                    ImgLogo.MaxHeight = 140;
                }
                else if (isStripView && item != null && (item.HasLogo || item.ParentLogoImageTag.HasValue))
                {
                    SetLogo(_apiClient.GetLogoImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Logo,
                        CropWhitespace = false
                    }));
                    ImgLogo.MaxHeight = 140;
                }
                else if (item != null && (item.HasLogo))
                {
                    SetLogo(_apiClient.GetImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Logo,
                        CropWhitespace = false
                    }));
                    ImgLogo.MaxHeight = 80;
                }
                else if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
                {
                    SetLogo(_apiClient.GetArtImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Art
                    }));
                    ImgLogo.MaxHeight = 80;
                }
                else
                {
                    // Just hide it so that it still takes up the same amount of space
                    ImgLogo.Visibility = Visibility.Hidden;
                    TxtLogoName.Visibility = Visibility.Visible;
                }
            }
        }

        private CancellationTokenSource _logoCancellationTokenSource;

        private async void SetLogo(string url)
        {
            if (_logoCancellationTokenSource != null)
            {
                _logoCancellationTokenSource.Cancel();
                _logoCancellationTokenSource.Dispose();
            }

            _logoCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var token = _logoCancellationTokenSource.Token;

                var img = await _imageManager.GetRemoteBitmapAsync(url, token);

                token.ThrowIfCancellationRequested();

                ImgLogo.Source = img;

                ImgLogo.Visibility = Visibility.Visible;
                TxtLogoName.Visibility = Visibility.Collapsed;
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Image download cancelled: {0}", url);
            }
            catch (Exception)
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
                TxtLogoName.Visibility = Visibility.Visible;
            }
        }

        public void ShowDisplayPreferencesMenu()
        {
            var viewModel = new DisplayPreferencesViewModel(_viewModel.DisplayPreferences, _presentationManager);

            var menu = new ViewWindow(viewModel, _options);

            menu.ShowModal(this.GetWindow());

            viewModel.Save();
        }
    }
}
