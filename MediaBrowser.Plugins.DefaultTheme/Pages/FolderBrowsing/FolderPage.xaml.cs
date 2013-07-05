using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Pages;
using System;

namespace MediaBrowser.Plugins.DefaultTheme.Pages.FolderBrowsing
{
    /// <summary>
    /// Interaction logic for FolderPage.xaml
    /// </summary>
    public partial class FolderPage : BasePage
    {
        private readonly string _displayPreferencesId;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;
        private readonly IPresentationManager _presentationManager;
        private readonly INavigationService _navigationManager;

        private readonly BaseItemDto _parentItem;

        private Model.Entities.DisplayPreferences _displayPreferences;
        
        public FolderPage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager)
        {
            _navigationManager = navigationManager;
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

            try
            {
                _displayPreferences = await _apiClient.GetDisplayPreferencesAsync(_displayPreferencesId);
            }
            catch (HttpException)
            {
                // Already logged at lower levels
                _displayPreferences = new Model.Entities.DisplayPreferences();
            }

            MainGrid.Children.Add(new ItemsListControl(_parentItem, _displayPreferences, _apiClient, _imageManager, _sessionManager, _navigationManager, _presentationManager));
        }

        protected override void FocusOnFirstLoad()
        {
        }
    }
}
