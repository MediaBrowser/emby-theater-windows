using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.Home;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme
{
    public class HomePageFactory : IHomePage
    {
        /// <summary>
        /// The _api client
        /// </summary>
        private readonly IApiClient _apiClient;
        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;
        /// <summary>
        /// The _nav service
        /// </summary>
        private readonly INavigationService _navService;
        /// <summary>
        /// The _session manager
        /// </summary>
        private readonly ISessionManager _sessionManager;
        /// <summary>
        /// The _app window
        /// </summary>
        private readonly IPresentationManager _presentationManager;
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// The _theme manager
        /// </summary>
        private readonly IThemeManager _themeManager;

        public HomePageFactory(IApiClient apiClient, IImageManager imageManager, INavigationService navService, ISessionManager sessionManager, IPresentationManager presentationManager, IThemeManager themeManager, ILogger logger)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _navService = navService;
            _sessionManager = sessionManager;
            _presentationManager = presentationManager;
            _themeManager = themeManager;
            _logger = logger;
        }

        public string Name
        {
            get { return "Default"; }
        }

        public Page GetPage()
        {
            return new HomePage(_apiClient, _imageManager, _sessionManager, _presentationManager, _navService, _themeManager, _logger);
        }
    }
}
