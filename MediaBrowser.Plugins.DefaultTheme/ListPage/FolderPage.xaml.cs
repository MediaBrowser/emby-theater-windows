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
        private readonly string _displayPreferencesId;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;
        private readonly IPresentationManager _presentationManager;
        private readonly INavigationService _navigationManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly ILogger _logger;

        private readonly BaseItemDto _parentItem;

        private ItemListViewModel _viewModel;

        public FolderPage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager, IPlaybackManager playbackManager, ILogger logger)
        {
            _navigationManager = navigationManager;
            _playbackManager = playbackManager;
            _logger = logger;
            _presentationManager = applicationWindow;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;

            _displayPreferencesId = displayPreferencesId;
            _parentItem = parent;

            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += FolderPage_Loaded;
            Unloaded += FolderPage_Unloaded;

            DataContext = _viewModel = new ItemListViewModel(GetItemsAsync, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigationManager, _playbackManager, _logger)
            {
                DisplayPreferencesId = _displayPreferencesId,
                ItemContainerHeight = 200,
                ItemContainerWidth = 200,
                ImageDisplayHeightGenerator = GetImageDisplayHeight,
                ImageDisplayWidth = 200,
                DisplayNameGenerator = GetDisplayName,
                PreferredImageTypesGenerator = GetPreferredImageTypes
            };

            OnParentItemChanged();
        }

        protected async void OnParentItemChanged()
        {
            var pageTitleTask = PageTitlePanel.Current.SetPageTitle(_parentItem);

            if (_parentItem.IsType("season") && _parentItem.IndexNumber.HasValue)
            {
                TxtParentName.Text = "Season " + _parentItem.IndexNumber.Value;
                TxtParentName.Visibility = Visibility.Visible;
            }
            else
            {
                TxtParentName.Visibility = Visibility.Collapsed;
            }

            await pageTitleTask;
        }

        void FolderPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.PropertyChanged -= _viewModel_PropertyChanged;
            HideViewButton();
        }

        async void FolderPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;

            if (_parentItem != null)
            {
                ShowViewButton();

                await PageTitlePanel.Current.SetPageTitle(_parentItem);
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

        private ImageType[] GetPreferredImageTypes(ItemListViewModel viewModel)
        {
            return string.Equals(viewModel.ViewType, ViewTypes.Thumbstrip) ? new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary } : new[] { ImageType.Primary };
        }

        void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "ViewType") || string.Equals(e.PropertyName, "ImageWidth") || string.Equals(e.PropertyName, "MedianPrimaryImageAspectRatio"))
            {
                _viewModel.ItemContainerWidth = GetItemContainerWidth(_viewModel.ViewType, _viewModel.ImageDisplayWidth);
                _viewModel.ItemContainerHeight = GetItemContainerHeight(_viewModel);

            }

            if (string.Equals(e.PropertyName, "CurrentItem"))
            {
                var item = _viewModel.CurrentItem;

                UpdateFields(item == null ? null : item.Item);

                if (item != null)
                {
                    if (item.Item != null)
                    {
                        UpdateLogo(item.Item);
                    }
                }
            }
        }

        private void UpdateFields(BaseItemDto item)
        {
            TxtGenres.Text = item != null && item.Genres != null ? string.Join(" / ", item.Genres.Take(3).ToArray()) : string.Empty;
        }

        private Task<ItemsResult> GetItemsAsync(DisplayPreferences displayPreferences)
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

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                        ? new[] { displayPreferences.SortBy }
                        : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder
            };

            return _apiClient.GetItemsAsync(query);
        }

        public string ThemeMediaItemId
        {
            get { return _parentItem.Id; }
        }

        private double GetItemContainerWidth(string viewType, double imageDisplayWidth)
        {
            if (String.Equals(viewType, ViewTypes.List))
            {
                return 1600;
            }

            // 14 = double the margin between items as defined in the resource file
            return imageDisplayWidth + 20;
        }

        private double GetItemContainerHeight(ItemListViewModel viewModel)
        {
            // 14 = double the margin between items as defined in the resource file
            return GetImageDisplayHeight(viewModel) + 20;
        }

        private double GetImageDisplayHeight(ItemListViewModel viewModel)
        {
            var viewType = viewModel.ViewType;
            var imageDisplayWidth = viewModel.ImageDisplayWidth;
            var medianPrimaryImageAspectRatio = viewModel.MedianPrimaryImageAspectRatio ?? 0;

            if (String.Equals(viewType, ViewTypes.Thumbstrip) || String.Equals(viewType, ViewTypes.List))
            {
                double height = imageDisplayWidth;
                return height / AspectRatioHelper.GetAspectRatio(ImageType.Backdrop, medianPrimaryImageAspectRatio);
            }

            if (!medianPrimaryImageAspectRatio.Equals(0))
            {
                if (String.IsNullOrEmpty(viewType) || String.Equals(viewType, ViewTypes.Poster))
                {
                    double height = imageDisplayWidth;
                    height /= medianPrimaryImageAspectRatio;

                    return height;
                }
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
            const int maxheight = 100;

            if (Sidebar.Visibility != Visibility.Visible)
            {
                if (item != null && item.HasLogo)
                {
                    SetLogo(item, _apiClient.GetLogoImageUrl(item, new ImageOptions
                    {
                        Height = maxheight,
                        ImageType = ImageType.Logo
                    }));
                }
                else if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
                {
                    SetLogo(item, _apiClient.GetArtImageUrl(item, new ImageOptions
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
        }

        private CancellationTokenSource _logoCancellationTokenSource;

        private async void SetLogo(BaseItemDto item, string url)
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
                TxtBottomName.Visibility = Visibility.Collapsed;
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("Image download cancelled: {0}", url);
            }
            catch
            {
                SetDefaultLogo(item);
            }
        }

        private void SetDefaultLogo(BaseItemDto item)
        {
            // Just hide it so that it still takes up the same amount of space
            ImgLogo.Visibility = Visibility.Hidden;
            TxtBottomName.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Click event of the ViewButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new DisplayPreferencesViewModel(_viewModel.DisplayPreferences, _apiClient,
                                                            _presentationManager, _sessionManager);

            var menu = new DisplayPreferencesMenu.DisplayPreferencesMenu(viewModel);

            menu.ShowModal(this.GetWindow());

            viewModel.Save();
        }
    }
}
