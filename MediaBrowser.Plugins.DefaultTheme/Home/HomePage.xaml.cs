using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, ISupportsItemBackdrops
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        private readonly IImageManager _imageManager;

        /// <summary>
        /// Gets the session manager.
        /// </summary>
        /// <value>The session manager.</value>
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        private readonly IPresentationManager _presentationManager;

        /// <summary>
        /// Gets the navigation manager.
        /// </summary>
        /// <value>The navigation manager.</value>
        private readonly INavigationService _navigationManager;

        private readonly BaseItemDto _parentItem;

        private readonly HomePageViewModel _viewModel;
        private readonly ILogger _logger;

        public HomePage(BaseItemDto parentItem, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager, ILogger logger)
        {
            _navigationManager = navigationManager;
            _logger = logger;
            _presentationManager = applicationWindow;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _parentItem = parentItem;

            _viewModel = new HomePageViewModel(applicationWindow, apiClient, sessionManager, _logger, imageManager, _navigationManager);

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            DataContext = _viewModel;

            Loaded += HomePage_Loaded;

            base.OnInitialized(e);
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
        }

        public BaseItemDto BackdropItem
        {
            get { return _parentItem; }
        }
    }
}
