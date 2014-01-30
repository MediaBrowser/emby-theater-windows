using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Navigation;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Extensions;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;

namespace MediaBrowser.Plugins.DefaultTheme.Search
{
    /// <summary>
    /// Interaction logic for FolderPage.xaml
    /// </summary>
    public partial class SearchPage : BasePage, ISupportsItemThemeMedia, IItemPage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;
        
        private readonly BaseItemDto _parentItem;
        private readonly SearchViewModel _searchViewModel;

        public SearchPage(BaseItemDto parent, IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, IPresentationManager presentation, INavigationService navigationManager, IPlaybackManager playbackManager, ILogger logger, IServerEvents serverEvents)
        {
            _logger = logger;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _parentItem = parent;
           
            InitializeComponent();

            this.Loaded += SearchPage_Loaded;
            
            _searchViewModel = new SearchViewModel(presentation, _imageManager, _apiClient, sessionManager, navigationManager, playbackManager, _logger, serverEvents);

            _searchViewModel.MatchedItemsViewModel.PropertyChanged += PropertyChanged;
            _searchViewModel.MatchedPeopleViewModel.PropertyChanged += PropertyChanged;
            //_searchViewModel.PropertyChanged += PropertyChanged;

            DataContext = _searchViewModel;
        }

        private void NavigationServiceOnNavigating(object sender, NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            if (navigatingCancelEventArgs.NavigationMode == NavigationMode.Back)
            {
                // must be return to previous page, unhook the search - can't use unloaded event, window is already gone
                var win = this.GetWindow();
                if (win != null)
                    win.PreviewKeyDown -= SearchPage_OnPreviewKeyDown;
            }
        }
      
        void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetPageTitle("Search");
            
            var win = this.GetWindow();
            win.PreviewKeyDown += SearchPage_OnPreviewKeyDown;

            this.NavigationService.Navigating += NavigationServiceOnNavigating;
        }

        private void SearchPage_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ExtendedListBox x = AlphabetMenu;
            _searchViewModel.AlphaInputViewModel.UserInput_PreviewKeyDown(sender, e, AlphabetMenu);
        }

        private Boolean ListBoxHasFocus(ListBox listBox)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                var item = listBox.Items[i];
                var lbi = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                if (lbi.IsFocused)
                {
                    return true;
                }
            }
            return false;
        }

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "CurrentItem"))
            {
                if (ListBoxHasFocus(MatchedItemsListBox))
                {
                    _searchViewModel.CurrentItem = _searchViewModel.MatchedItemsViewModel.CurrentItem;
                    if (_searchViewModel.CurrentItem != null)
                        UpdateLogo(_searchViewModel.CurrentItem.Item);
                }
                else
                {
                    _searchViewModel.MatchedItemsViewModel.CurrentItem = null;
                    _searchViewModel.CurrentItem = null;
                    UpdateLogo(null);
                }
             }
        }

        public ViewType ViewType
        {
            get { return /*_viewModel.Context;*/ ViewType.Movies; }
            //set { _viewModel.Context = value; }
        }
   

        private void SetPageTitle(String pageTitle)
        {
            DefaultTheme.Current.PageContentDataContext.PageTitle = pageTitle;
        }

            
        public string ThemeMediaItemId
        {
            get { return _parentItem.Id; }
        }

        public BaseItemDto PageItem
        {
            get { return _parentItem; }
        }

        private void UpdateLogo(BaseItemDto item)
        {
            var isStripView = true;

            if (isStripView && item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
            {
                SetLogo(_apiClient.GetArtImageUrl(item, new ImageOptions{ImageType = ImageType.Art}));
                ImgLogo.MaxHeight = 120;
                ImgLogo.Margin = new Thickness(0, 0, 40, 0);
                TxtLogoName.Visibility = Visibility.Collapsed;
            }
            else if (isStripView && item != null && (item.HasLogo || item.ParentLogoImageTag.HasValue))
            {
                SetLogo(_apiClient.GetLogoImageUrl(item, new ImageOptions {ImageType = ImageType.Logo, CropWhitespace = false }));
                ImgLogo.MaxHeight = 120;
                ImgLogo.Margin = new Thickness(0, 0, 40, 0);

                TxtLogoName.Visibility = Visibility.Collapsed;
            }
            else if (item != null && (item.HasLogo))
            {
                SetLogo(_apiClient.GetImageUrl(item, new ImageOptions { ImageType = ImageType.Logo, CropWhitespace = false }));
                ImgLogo.MaxHeight = 80;
                ImgLogo.Margin = new Thickness(0, 0, 40, 0);

                TxtLogoName.Visibility = Visibility.Collapsed;
            }
            else if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
            {
                SetLogo(_apiClient.GetArtImageUrl(item, new ImageOptions {ImageType = ImageType.Art}));
                ImgLogo.MaxHeight = 80;
                ImgLogo.Margin = new Thickness(0, 0, 40, 0);
                TxtLogoName.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
                ImgLogo.Margin = new Thickness(0, 0, 0, 0);
                ImgLogo.Source = null;
             
                TxtLogoName.Visibility = item == null ?  Visibility.Collapsed : Visibility.Visible;
               
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

        ~SearchPage()
        {
              //  var win = this.GetWindow();
              //  if (win != null)
              //        win.PreviewKeyDown -= SearchPage_OnPreviewKeyDown;
        }

    }
}
