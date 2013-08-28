using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
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
    public partial class FolderPage : BasePage, ISupportsItemThemeMedia, ISupportsBackdrops
    {
        private readonly DisplayPreferences _displayPreferences;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;
        private readonly IPresentationManager _presentationManager;
        private readonly INavigationService _navigationManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IThemeManager _themeManager;
        private readonly ILogger _logger;

        private readonly BaseItemDto _parentItem;

        private readonly ItemListViewModel _viewModel;

        public FolderPage(BaseItemDto parent, DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager, IPlaybackManager playbackManager, ILogger logger, IThemeManager themeManager)
        {
            _navigationManager = navigationManager;
            _playbackManager = playbackManager;
            _logger = logger;
            _themeManager = themeManager;
            _presentationManager = applicationWindow;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;

            _displayPreferences = displayPreferences;
            _parentItem = parent;

            InitializeComponent();

            Loaded += FolderPage_Loaded;
            Unloaded += FolderPage_Unloaded;

            _viewModel = new ItemListViewModel(GetItemsAsync, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigationManager, _playbackManager, _logger)
            {
                ImageDisplayHeightGenerator = GetImageDisplayHeight,
                DisplayNameGenerator = GetDisplayName,
                PreferredImageTypesGenerator = GetPreferredImageTypes,

                ShowSidebarGenerator = GetShowSidebar,
                ScrollDirectionGenerator = GetScrollDirection,

                AutoSelectFirstItem = true
            };

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;

            _viewModel.DisplayPreferences = _displayPreferences;

            DataContext = _viewModel;

            OnParentItemChanged();
        }

        private ScrollDirection GetScrollDirection(ItemListViewModel viewModel)
        {
            if (string.Equals(viewModel.ViewType, ViewTypes.List))
            {
                return ScrollDirection.Vertical;
            }
            if (string.Equals(viewModel.ViewType, ViewTypes.Thumbstrip))
            {
                return ScrollDirection.Horizontal;
            }

            return viewModel.DisplayPreferences == null
                       ? ScrollDirection.Horizontal
                       : viewModel.DisplayPreferences.ScrollDirection;
        }

        private ImageType[] GetPreferredImageTypes(ItemListViewModel viewModel)
        {
            return string.Equals(viewModel.ViewType, ViewTypes.Thumbstrip) || string.Equals(viewModel.ViewType, ViewTypes.List)
                       ? new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary }
                       : new[] { ImageType.Primary };
        }


        private bool GetShowSidebar(ItemListViewModel viewModel)
        {
            if (string.Equals(viewModel.ViewType, ViewTypes.List))
            {
                return true;
            }
            if (string.Equals(viewModel.ViewType, ViewTypes.Thumbstrip))
            {
                return false;
            }

            return viewModel.DisplayPreferences != null && viewModel.DisplayPreferences.ShowSidebar;
        }

        protected void OnParentItemChanged()
        {
            DefaultTheme.Current.PageContentDataContext.SetPageTitle(_parentItem);

            if (_parentItem.IsType("season") && _parentItem.IndexNumber.HasValue)
            {
                TxtParentName.Text = "Season " + _parentItem.IndexNumber.Value;
                TxtParentName.Visibility = Visibility.Visible;
            }
            else
            {
                TxtParentName.Visibility = Visibility.Collapsed;
            }
        }

        void FolderPage_Unloaded(object sender, RoutedEventArgs e)
        {
            HideViewButton();
        }

        void FolderPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_parentItem != null)
            {
                ShowViewButton();

                DefaultTheme.Current.PageContentDataContext.SetPageTitle(_parentItem);
            }
            else
            {
                HideViewButton();
            }
        }

        public static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IndexNumber.HasValue && !item.IsType("season"))
            {
                name = item.IndexNumber + " - " + name;
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

        private Task<ItemsResult> GetItemsAsync()
        {
            var query = new ItemQuery
            {
                ParentId = _parentItem.Id,

                Fields = new[]
                        {
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated,
                                 ItemFields.MediaStreams,
                                 ItemFields.Taglines,
                                 ItemFields.Genres,
                                 ItemFields.Overview,
                                 ItemFields.DisplayPreferencesId,
                                 ItemFields.ItemCounts
                        },

                UserId = _sessionManager.CurrentUser.Id,

                SortBy = !String.IsNullOrEmpty(_displayPreferences.SortBy)
                        ? new[] { _displayPreferences.SortBy }
                        : new[] { ItemSortBy.SortName },

                SortOrder = _displayPreferences.SortOrder
            };

            return _apiClient.GetItemsAsync(query);
        }

        public string ThemeMediaItemId
        {
            get { return _parentItem.Id; }
        }

        private double GetItemContainerHeight(ItemListViewModel viewModel)
        {
            var height = GetImageDisplayHeight(viewModel);

            // Add the bottom border
            if (string.Equals(viewModel.ViewType, ViewTypes.List))
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

        private void UpdateLogo(BaseItemDto item)
        {
            if (Sidebar.Visibility != Visibility.Visible)
            {
                if (item != null && string.Equals(_viewModel.ViewType, ViewTypes.Thumbstrip) && (item.HasArtImage || item.ParentArtImageTag.HasValue))
                {
                    SetLogo(_apiClient.GetArtImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Art
                    }));
                }
                else if (item != null && item.HasLogo)
                {
                    SetLogo(_apiClient.GetLogoImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Logo
                    }));
                }
                else if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
                {
                    SetLogo(_apiClient.GetArtImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Art
                    }));
                }
                else
                {
                    // Just hide it so that it still takes up the same amount of space
                    ImgLogo.Visibility = Visibility.Hidden;
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
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Image download cancelled: {0}", url);
            }
            catch (Exception ex)
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Handles the Click event of the ViewButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new DisplayPreferencesViewModel(_viewModel.DisplayPreferences, _apiClient,
                                                            _presentationManager, _sessionManager, _themeManager);

            var menu = new DisplayPreferencesMenu.DisplayPreferencesMenu(viewModel);

            menu.ShowModal(this.GetWindow());

            viewModel.Save();
        }
    }
}
