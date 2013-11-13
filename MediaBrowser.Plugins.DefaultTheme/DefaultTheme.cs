using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Details;
using MediaBrowser.Plugins.DefaultTheme.Home;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        private readonly IServerEvents _serverEvents;
        private readonly IApplicationHost _appHost;
        private readonly ITheaterConfigurationManager _config;

        public static DefaultTheme Current;

        public DefaultTheme(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, INavigationService navService, ISessionManager sessionManager, IPresentationManager presentationManager, ILogManager logManager, IServerEvents serverEvents, IApplicationHost appHost, ITheaterConfigurationManager config)
        {
            Current = this;

            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _navService = navService;
            _sessionManager = sessionManager;
            _presentationManager = presentationManager;
            _serverEvents = serverEvents;
            _appHost = appHost;
            _config = config;
            _logger = logManager.GetLogger(GetType().Name);
        }

        /// <summary>
        /// Gets the global resources.
        /// </summary>
        /// <returns>IEnumerable{ResourceDictionary}.</returns>
        public IEnumerable<ResourceDictionary> GetResources()
        {
            var namespaceName = GetType().Namespace;

            return new[] { "AppResources", "HomePageResources", "Details", "VolumeOsd", "TransportOsd", "InfoPanel", "DisplayPreferences" }.Select(i => new ResourceDictionary
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
        public Page GetItemPage(BaseItemDto item, ViewType context)
        {
            var itemViewModel = new ItemViewModel(_apiClient, _imageManager, _playbackManager, _presentationManager, _logger, _serverEvents)
            {
                Item = item,
                ImageWidth = 550,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Thumb }
            };

            return new DetailPage(itemViewModel, _presentationManager)
            {
                DataContext = new DetailPageViewModel(itemViewModel, _apiClient, _sessionManager, _imageManager, _presentationManager, _playbackManager, _navService, _logger, _serverEvents, context)
            };
        }

        private readonly string[] _folderTypesWithDetailPages = new[] { "series", "musicalbum", "musicartist" };

        /// <summary>
        /// Gets the folder page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <param name="displayPreferences">The display preferences.</param>
        /// <returns>Page.</returns>
        public Page GetFolderPage(BaseItemDto item, ViewType context, DisplayPreferences displayPreferences)
        {
            if (!_folderTypesWithDetailPages.Contains(item.Type, StringComparer.OrdinalIgnoreCase))
            {
                var options = GetListPageConfig(item, context);

                return new FolderPage(item, displayPreferences, _apiClient, _imageManager, _presentationManager, _navService, _playbackManager, _logger, _serverEvents, options);
            }

            return GetItemPage(item, context);
        }

        private ListPageConfig GetListPageConfig(BaseItemDto item, ViewType context)
        {
            var config = new ListPageConfig();

            if (context == ViewType.Tv || item.IsType("season"))
            {
                TvViewModel.SetDefaults(config);

                if (item.IsType("season"))
                {
                    config.DefaultViewType = ListViewTypes.List;

                    config.PosterImageWidth = 480;
                    config.PosterStripImageWidth = 592;
                    config.ThumbImageWidth = 592;
                }
            }
            else if (context == ViewType.Movies)
            {
                MoviesViewModel.SetDefaults(config);
            }
            else if (context == ViewType.Games)
            {
                GamesViewModel.SetDefaults(config, item.GameSystem);
            }

            if (item.IsFolder)
            {
                config.CustomItemQuery = (vm, displayPreferences) =>
                {
                    var query = new ItemQuery
                    {
                        UserId = _sessionManager.CurrentUser.Id,

                        ParentId = item.Id,

                        SortBy = new[] { ItemSortBy.SortName },

                        SortOrder = displayPreferences.SortOrder,

                        Fields = FolderPage.QueryFields
                    };

                    return _apiClient.GetItemsAsync(query);
                };
            }

            return config;
        }

        /// <summary>
        /// Gets the person page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <param name="mediaItemId">The media item id.</param>
        /// <returns>Page.</returns>
        public Page GetPersonPage(BaseItemDto item, ViewType context, string mediaItemId = null)
        {
            var page = (DetailPage)GetItemPage(item, context);

            page.ItemByNameMediaItemId = mediaItemId;

            return page;
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

        public string DefaultHomePageName
        {
            get { return "Default"; }
        }

        internal DefaultThemePageContentViewModel PageContentDataContext { get; private set; }

        public PageContentViewModel CreatePageContentDataContext()
        {
            PageContentDataContext = new DefaultThemePageContentViewModel(_navService, _sessionManager, _apiClient, _imageManager, _presentationManager, _playbackManager, _logger, _appHost, _serverEvents, _config);

            return PageContentDataContext;
        }
    }
}
