using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Plugins.DefaultTheme.Details;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Plugins.DefaultTheme.Pages;
using MediaBrowser.Plugins.DefaultTheme.Pages.FolderBrowsing;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        private readonly IThemeManager _themeManager;

        private readonly IUserInputManager _userInput;
        
        public DefaultTheme(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, INavigationService navService, ISessionManager sessionManager, IPresentationManager presentationManager, ILogManager logManager, IThemeManager themeManager, IUserInputManager userInput)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _navService = navService;
            _sessionManager = sessionManager;
            _presentationManager = presentationManager;
            _themeManager = themeManager;
            _userInput = userInput;
            _logger = logManager.GetLogger(GetType().Name);

            TopRightPanel.SessionManager = _sessionManager;
            TopRightPanel.ApiClient = _apiClient;
            TopRightPanel.ImageManager = _imageManager;
            TopRightPanel.Logger = _logger;
            TopRightPanel.Navigation = _navService;
            TopRightPanel.PlaybackManager = _playbackManager;
            TopRightPanel.UserInputManager = _userInput;
            
            PageTitlePanel.ApiClient = _apiClient;
            PageTitlePanel.ImageManager = _imageManager;

            _presentationManager.WindowLoaded += _presentationManager_WindowLoaded;
        }

        void _presentationManager_WindowLoaded(object sender, EventArgs e)
        {
            var namespaceName = GetType().Namespace;

            // These are going to be loaded no matter which theme is active
            _presentationManager.AddResourceDictionary(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/" + namespaceName + ";component/Resources/ListBoxItems.xaml", UriKind.Absolute)
            });

            _presentationManager.AddResourceDictionary(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/" + namespaceName + ";component/Resources/Popups.xaml", UriKind.Absolute)
            });

            _presentationManager.AddResourceDictionary(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/" + namespaceName + ";component/Resources/HomePageResources.xaml", UriKind.Absolute)
            });
        }

        protected virtual string ThemeColorResource
        {
            get { return "ThemeDark"; }
        }

        /// <summary>
        /// Gets the global resources.
        /// </summary>
        /// <returns>IEnumerable{ResourceDictionary}.</returns>
        private IEnumerable<ResourceDictionary> GetGlobalResources()
        {
            var namespaceName = GetType().Namespace;

            return new[] { ThemeColorResource, "NavBarResources", "AppResources", "DisplayPreferences" }.Select(i => new ResourceDictionary
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
        public Page GetItemPage(BaseItemDto item, string context)
        {
            if (item.IsFolder && !item.IsType("series") && !item.IsType("musicalbum"))
            {
                return new FolderPage(item, item.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _presentationManager, _navService, _userInput);
            }

            return new PanoramaDetailPage(item, _apiClient, _sessionManager, _presentationManager, _imageManager, _navService, _playbackManager, _themeManager);
        }

        /// <summary>
        /// Gets the person page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        public Page GetPersonPage(BaseItemDto item, string context)
        {
            return new PanoramaDetailPage(item, _apiClient, _sessionManager, _presentationManager, _imageManager, _navService, _playbackManager, _themeManager);
        }

        /// <summary>
        /// Shows the default error message.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ShowDefaultErrorMessage()
        {
            ShowMessage(new MessageBoxInfo
            {
                Caption = "Error",
                Text = "There was an error processing the request.",
                Icon = MessageBoxIcon.Error,
                Button = MessageBoxButton.OK

            });
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>MessageBoxResult.</returns>
        public MessageBoxResult ShowMessage(MessageBoxInfo options)
        {
            return ShowMessage(options, _presentationManager.Window);
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="parentWindow">The parent window.</param>
        /// <returns>MessageBoxResult.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public MessageBoxResult ShowMessage(MessageBoxInfo options, Window parentWindow)
        {
            var win = new ModalWindow(_userInput, _navService)
            {
                Caption = options.Caption,
                Button = options.Button,
                MessageBoxImage = options.Icon,
                Text = options.Text
            };

            win.ShowModal(parentWindow);

            return win.MessageBoxResult;
        }

        /// <summary>
        /// Shows the notification.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="text">The text.</param>
        /// <param name="icon">The icon.</param>
        public void ShowNotification(string caption, string text, BitmapImage icon)
        {
            throw new NotImplementedException();
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

        private List<ResourceDictionary> _globalResources;

        public void Load()
        {
            _globalResources = GetGlobalResources().ToList();

            foreach (var resource in _globalResources)
            {
                _presentationManager.AddResourceDictionary(resource);
            }
        }

        public void Unload()
        {
            foreach (var resource in _globalResources)
            {
                _presentationManager.RemoveResourceDictionary(resource);
            }

            foreach (var resource in _globalResources.OfType<IDisposable>())
            {
                resource.Dispose();
            }

            _globalResources.Clear();
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

        public Page GetFullscreenVideoPage()
        {
            return new FullscreenVideoPage(_userInput, _imageManager, _apiClient, _playbackManager);
        }
    }
}
