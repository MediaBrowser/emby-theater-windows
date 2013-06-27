using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Plugins.DefaultTheme.Home;
using MediaBrowser.Plugins.DefaultTheme.Pages;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;
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
        private readonly IPresentationManager _appWindow;
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// The _theme manager
        /// </summary>
        private readonly IThemeManager _themeManager;
        private readonly IUserInputManager _userInputManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTheme" /> class.
        /// </summary>
        /// <param name="playbackManager">The playback manager.</param>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="navService">The nav service.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="appWindow">The app window.</param>
        /// <param name="logManager">The log manager.</param>
        /// <param name="themeManager">The theme manager.</param>
        /// <param name="userInputManager">The user input manager.</param>
        public DefaultTheme(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, INavigationService navService, ISessionManager sessionManager, IPresentationManager appWindow, ILogManager logManager, IThemeManager themeManager, IUserInputManager userInputManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _navService = navService;
            _sessionManager = sessionManager;
            _appWindow = appWindow;
            _themeManager = themeManager;
            _userInputManager = userInputManager;
            _logger = logManager.GetLogger(GetType().Name);
        }

        /// <summary>
        /// Gets the global resources.
        /// </summary>
        /// <returns>IEnumerable{ResourceDictionary}.</returns>
        public IEnumerable<ResourceDictionary> GetGlobalResources()
        {
            return new ResourceDictionary[] { 

                new NavBarResources(),
                new AppResources(_playbackManager, _imageManager, _apiClient, _appWindow, _navService, _sessionManager, _logger, _userInputManager),
                new HomePageResources()
            };
        }

        /// <summary>
        /// Gets the login page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetLoginPage()
        {
            return new LoginPage(_apiClient, _imageManager, _navService, _sessionManager, _appWindow, _themeManager);
        }

        /// <summary>
        /// Gets the internal player page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetInternalPlayerPage()
        {
            return new InternalPlayerPage();
        }

        /// <summary>
        /// Gets the home page.
        /// </summary>
        /// <param name="rootItem">The root item.</param>
        /// <returns>Page.</returns>
        public Page GetHomePage(BaseItemDto rootItem)
        {
            return new HomePage(rootItem, rootItem.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _appWindow, _navService, _themeManager);
        }

        /// <summary>
        /// Gets the item page.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Page.</returns>
        public Page GetItemPage(BaseItemDto item, string context)
        {
            if (item.IsFolder)
            {
                return new ListPage(item, item.DisplayPreferencesId, _apiClient, _imageManager, _sessionManager, _appWindow, _navService, _themeManager);
            }

            return new DetailPage(item, _imageManager, _playbackManager, _apiClient, _sessionManager, _appWindow, _themeManager);
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
            return ShowMessage(options, _appWindow.Window);
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
            var win = new ModalWindow
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
            AppResources.Instance.SetPageTitle(title);
        }

        /// <summary>
        /// Sets the default page title.
        /// </summary>
        public void SetDefaultPageTitle()
        {
            AppResources.Instance.SetDefaultPageTitle();
        }

        public string Name
        {
            get { return "Default"; }
        }
    }
}
