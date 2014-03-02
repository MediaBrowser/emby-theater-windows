using System.Windows.Navigation;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using ListBox = System.Windows.Controls.ListBox;

namespace MediaBrowser.Plugins.DefaultTheme.Search
{
    /// <summary>
    /// Interaction logic for FolderPage.xaml
    /// </summary>
    public partial class SearchPage : BasePage, ISearchPage, ISupportsItemThemeMedia, IItemPage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navigationService;
        private readonly ILogger _logger;
        
        private readonly BaseItemDto _parentItem;
        private readonly SearchViewModel _searchViewModel;

        public SearchPage(BaseItemDto parent, IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, IPresentationManager presentation, INavigationService navigatioService, IPlaybackManager playbackManager, ILogger logger, IServerEvents serverEvents)
        {
            _logger = logger;
            _imageManager = imageManager;
            _navigationService = navigatioService;
            _apiClient = apiClient;
            _parentItem = parent;
           
            InitializeComponent();

            this.Loaded += SearchPage_Loaded;

            _searchViewModel = new SearchViewModel(presentation, _imageManager, _apiClient, sessionManager, navigatioService, playbackManager, _logger, serverEvents);

            _searchViewModel.MatchedItemsViewModel.PropertyChanged += PropertyChanged;
            _searchViewModel.MatchedPeopleViewModel.PropertyChanged += PropertyChanged;
         
            DataContext = _searchViewModel;
        }

        private void _nav_Navigating(object sender, NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            var win = this.GetWindow();
            if (win != null)    // win will only be !null when we are navigating away from search page
            {
                // unhook the keydown here, we don't have a window to unhook from on the unLoaded event
                win.PreviewKeyDown -= SearchPage_OnPreviewKeyDown;
                this.NavigationService.Navigating -= _nav_Navigating; // this method
            }
        }
        
        void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetPageTitle("Search");
            
            var win = this.GetWindow();
            win.PreviewKeyDown += SearchPage_OnPreviewKeyDown;

            //use the WPF NavigationService.Navigating event rather that the navigationManager.navigated event
            // as we need tp get the Window of the page before we navigate away from it 
            this.NavigationService.Navigating += _nav_Navigating;
        }


        private void SearchPage_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _logger.Debug("SearchPage_OnPreviewKeyDown {0}", e.Key);
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
                }
                else
                {
                    _searchViewModel.MatchedItemsViewModel.CurrentItem = null;
                    _searchViewModel.CurrentItem = null;
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

        public void Dispose()
        {
            _logger.Debug("SearchPage dispose");
        }
    }
}
