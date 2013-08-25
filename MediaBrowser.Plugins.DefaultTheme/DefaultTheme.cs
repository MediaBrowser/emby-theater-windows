using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.Details;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme
{
    /// <summary>
    /// Class Theme
    /// </summary>
    public class DefaultTheme : ITheme
    {
        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;
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

        public DefaultTheme(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, INavigationService navService, ISessionManager sessionManager, IPresentationManager presentationManager, ILogManager logManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _navService = navService;
            _sessionManager = sessionManager;
            _presentationManager = presentationManager;
            _logger = logManager.GetLogger(GetType().Name);

            TopRightPanel.SessionManager = _sessionManager;
            TopRightPanel.ApiClient = _apiClient;
            TopRightPanel.ImageManager = _imageManager;
            TopRightPanel.Logger = _logger;
            TopRightPanel.Navigation = _navService;
            TopRightPanel.PlaybackManager = _playbackManager;

            PageTitlePanel.ApiClient = _apiClient;
            PageTitlePanel.ImageManager = _imageManager;
        }

        /// <summary>
        /// Gets the global resources.
        /// </summary>
        /// <returns>IEnumerable{ResourceDictionary}.</returns>
        public IEnumerable<ResourceDictionary> GetResources()
        {
            var namespaceName = GetType().Namespace;

            return new[] { "AppResources", "HomePageResources", "Details", "VolumeOsd", "TransportOsd", "DisplayPreferences" }.Select(i => new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/" + namespaceName + ";component/Resources/" + i + ".xaml", UriKind.Absolute)

            });
        }

        /// <summary>
        /// Gets the item page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        public FrameworkElement GetItemPage(BaseItemDto item, string context)
        {
            var itemViewModel = new ItemViewModel(_apiClient, _imageManager, _playbackManager, _presentationManager, _logger)
            {
                Item = item,
                ImageWidth = 550
            };

            return new PanoramaDetailPage(itemViewModel)
            {
                DataContext = new DetailPageViewModel(itemViewModel, _apiClient, _sessionManager, _imageManager, _presentationManager, _playbackManager, _navService, _logger)
            };
        }

        /// <summary>
        /// Gets the folder page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="displayPreferences">The display preferences.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        public FrameworkElement GetFolderPage(BaseItemDto item, DisplayPreferences displayPreferences, string context)
        {
            if (!item.IsType("series") && !item.IsType("musicalbum"))
            {
                return new FolderPage(item, displayPreferences, _apiClient, _imageManager, _sessionManager, _presentationManager, _navService, _playbackManager, _logger);
            }

            return GetItemPage(item, context);
        }

        /// <summary>
        /// Gets the person page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        public FrameworkElement GetPersonPage(BaseItemDto item, string context)
        {
            return GetItemPage(item, context);
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="title">The title.</param>
        public void SetPageTitle(string title)
        {
            PageTitlePanel.Current.SetPageTitle(title);
        }

        /// <summary>
        /// Sets the default page title.
        /// </summary>
        public void SetDefaultPageTitle()
        {
            PageTitlePanel.Current.SetDefaultPageTitle();
        }

        public virtual string Name
        {
            get { return "Default"; }
        }

        private string ThemeColorResource
        {
            get { return "ThemeDark"; }
        }

        private ResourceDictionary _skinColorResource;

        public void Load()
        {
            var namespaceName = GetType().Namespace;

            _skinColorResource = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/" + namespaceName + ";component/Resources/" + ThemeColorResource + ".xaml", UriKind.Absolute)

            };

            _presentationManager.AddResourceDictionary(_skinColorResource);
        }

        public void Unload()
        {
            if (_skinColorResource != null)
            {
                _presentationManager.AddResourceDictionary(_skinColorResource);
            }
        }

        public void SetGlobalContentVisibility(bool visible)
        {
            TopRightPanel.Current.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            PageTitlePanel.Current.MainGrid.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public string DefaultHomePageName
        {
            get { return "Default"; }
        }
    }
}
