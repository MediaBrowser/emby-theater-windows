using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class HomePageInfo : IHomePage
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

        private readonly ILogger _logger;

        public HomePageInfo(IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager presentationManager, INavigationService navigationManager, ILogger logger)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _sessionManager = sessionManager;
            _presentationManager = presentationManager;
            _navigationManager = navigationManager;
            _logger = logger;
        }

        public string Name
        {
            get { return "Default"; }
        }

        public Page GetHomePage(BaseItemDto rootFolder)
        {
            return new HomePage(rootFolder, _apiClient, _imageManager, _sessionManager, _presentationManager, _navigationManager, _logger);
        }
    }
}
