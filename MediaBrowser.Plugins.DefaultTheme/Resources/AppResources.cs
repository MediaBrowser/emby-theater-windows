using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;

namespace MediaBrowser.Plugins.DefaultTheme.Resources
{
    /// <summary>
    /// Class AppResources
    /// </summary>
    public partial class AppResources : ResourceDictionary
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static AppResources Instance { get; private set; }

        private readonly IPlaybackManager _playbackManager;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        private readonly INavigationService _navService;
        private readonly IPresentationManager _appWindow;
        private readonly ISessionManager _sessionManager;
        private readonly IUserInputManager _userInputManager;

        private Timer ClockTimer { get; set; }

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppResources" /> class.
        /// </summary>
        public AppResources(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, IPresentationManager appWindow, INavigationService navService, ISessionManager sessionManager, ILogger logger, IUserInputManager userInputManager)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _appWindow = appWindow;
            _navService = navService;
            _sessionManager = sessionManager;
            _logger = logger;
            _userInputManager = userInputManager;

            InitializeComponent();

            Instance = this;

            _playbackManager.PlaybackStarted += PlaybackManager_PlaybackStarted;
            _playbackManager.PlaybackCompleted += PlaybackManager_PlaybackCompleted;

            _appWindow.WindowLoaded += _appWindow_WindowLoaded;
        }

        /// <summary>
        /// Handles the WindowLoaded event of the _appWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _appWindow_WindowLoaded(object sender, EventArgs e)
        {
            _sessionManager.UserLoggedOut += sessionManager_UserLoggedOut;
            _sessionManager.UserLoggedIn += sessionManager_UserLoggedIn;

            ClockTimer = new Timer(ClockTimerCallback, null, 0, 10000);

            var navBar = TreeHelper.FindChild<NavigationBar>(_appWindow.Window, "NavigationBar");

            navBar.PlaybackManager = _playbackManager;
        }

        /// <summary>
        /// Handles the UserLoggedIn event of the sessionManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        async void sessionManager_UserLoggedIn(object sender, EventArgs e)
        {
            await _appWindow.Window.Dispatcher.InvokeAsync(async () =>
            {
                var panel = TreeHelper.FindChild<Button>(_appWindow.Window, "CurrentUserButton");

                panel.Visibility = Visibility.Visible;

                await UpdateUserImage(_sessionManager.CurrentUser, panel);

            });
        }

        /// <summary>
        /// Updates the user image.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="currentUserButton">The current user button.</param>
        /// <returns>Task.</returns>
        private async Task UpdateUserImage(UserDto user, Button currentUserButton)
        {
            if (user.HasPrimaryImage)
            {
                var imageUrl = _apiClient.GetUserImageUrl(user, new ImageOptions
                {
                    ImageType = ImageType.Primary,
                    MaxHeight = 48,
                    Quality = 100
                });

                try
                {
                    var img = await _imageManager.GetRemoteBitmapAsync(imageUrl);

                    var grid = (Grid) currentUserButton.Content;

                    var image = (Image)grid.Children[0];

                    image.Visibility = Visibility.Visible;
                    image.Source = img;
                    grid.Children[1].Visibility = Visibility.Collapsed;

                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error getting user image", ex);

                    SetDefaultUserImage(currentUserButton);
                }
            }
            else
            {
                SetDefaultUserImage(currentUserButton);
            }
        }

        /// <summary>
        /// Sets the default user image.
        /// </summary>
        /// <param name="currentUserButton">The current user button.</param>
        private void SetDefaultUserImage(Button currentUserButton)
        {
            var grid = (Grid)currentUserButton.Content;
            grid.Children[0].Visibility = Visibility.Collapsed;
            grid.Children[1].Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the UserLoggedOut event of the sessionManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        async void sessionManager_UserLoggedOut(object sender, EventArgs e)
        {
            await _appWindow.Window.Dispatcher.InvokeAsync(() =>
            {
                var panel = TreeHelper.FindChild<Button>(_appWindow.Window, "CurrentUserButton");

                panel.Visibility = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Clocks the timer callback.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        private async void ClockTimerCallback(object stateInfo)
        {
            await _appWindow.Window.Dispatcher.InvokeAsync(() =>
            {
                var left = TreeHelper.FindChild<TextBlock>(_appWindow.Window, "CurrentTimeLeft");
                var right = TreeHelper.FindChild<TextBlock>(_appWindow.Window, "CurrentTimeRight");

                if (left == null || right == null)
                {
                    return;
                }

                var now = DateTime.Now;

                left.Text = now.ToString("h:mm");

                if (CultureInfo.CurrentCulture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                {
                    var time = now.ToString("t");
                    var values = time.Split(' ');
                    right.Text = values[values.Length - 1].ToLower();
                }
                else
                {
                    right.Text = string.Empty;
                }

            });
        }

        /// <summary>
        /// BTNs the application back click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void BtnApplicationBackClick(object sender, RoutedEventArgs e)
        {
            await _navService.NavigateBack();
        }

        /// <summary>
        /// Handles the Click event of the NowPlayingButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void NowPlaying_Click(object sender, RoutedEventArgs e)
        {
            await _navService.NavigateToInternalPlayerPage();
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStopEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        async void PlaybackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            await _appWindow.Window.Dispatcher.InvokeAsync(() => NowPlayingButton.Visibility = Visibility.Collapsed);
        }

        /// <summary>
        /// Handles the PlaybackStarted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStartEventArgs" /> instance containing the event data.</param>
        async void PlaybackManager_PlaybackStarted(object sender, PlaybackStartEventArgs e)
        {
            if (e.Player is IInternalMediaPlayer)
            {
                await _appWindow.Window.Dispatcher.InvokeAsync(() => NowPlayingButton.Visibility = Visibility.Visible);
            }
        }

        /// <summary>
        /// Settingses the button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            await _navService.NavigateToSettingsPage();
        }

        async void BackButtonClick(object sender, RoutedEventArgs e)
        {
            await _navService.NavigateBack();
        }

        /// <summary>
        /// Get the Back Button.
        /// </summary>
        /// <value>The view button.</value>
        public Button BackButton
        {
            get
            {
                return TreeHelper.FindChild<Button>(_appWindow.Window, "BackButton");
            }
        }

        /// <summary>
        /// This is a common element that appears on every page.
        /// </summary>
        /// <value>The view button.</value>
        public Button ViewButton
        {
            get
            {
                return TreeHelper.FindChild<Button>(_appWindow.Window, "ViewButton");
            }
        }

        /// <summary>
        /// Gets the now playing button.
        /// </summary>
        /// <value>The now playing button.</value>
        private Button NowPlayingButton
        {
            get
            {
                return TreeHelper.FindChild<Button>(_appWindow.Window, "NowPlayingButton");
            }
        }

        /// <summary>
        /// This is a common element that appears on every page.
        /// </summary>
        /// <value>The page title panel.</value>
        public StackPanel PageTitlePanel
        {
            get
            {
                return TreeHelper.FindChild<StackPanel>(_appWindow.Window, "PageTitlePanel");
            }
        }

        /// <summary>
        /// Gets the content of the header.
        /// </summary>
        /// <value>The content of the header.</value>
        public StackPanel HeaderContent
        {
            get
            {
                return TreeHelper.FindChild<StackPanel>(_appWindow.Window, "HeaderContent");
            }
        }

        /// <summary>
        /// Sets the default page title.
        /// </summary>
        public void SetDefaultPageTitle()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            var text = new TextBlock();
            text.Text = "media";
            text.SetResourceReference(TextBlock.StyleProperty, "Heading2TextBlockStyle");
            panel.Children.Add(text);

            text = new TextBlock();
            text.Text = "browser";
            text.SetResourceReference(TextBlock.StyleProperty, "Heading2TextBlockStyle");
            text.Foreground = new SolidColorBrush(Color.FromRgb(82, 181, 75));
            panel.Children.Add(text);

            SetPageTitle(panel);
        }

        /// <summary>
        /// Clears the page title.
        /// </summary>
        public void ClearPageTitle()
        {
            PageTitlePanel.Children.Clear();
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="item">The item.</param>
        public async Task SetPageTitle(BaseItemDto item)
        {
            if (item.HasLogo || !string.IsNullOrEmpty(item.ParentLogoItemId))
            {
                var url = _apiClient.GetLogoImageUrl(item, new ImageOptions
                {
                    Quality = 100
                });

                try
                {
                    var image = await _imageManager.GetRemoteImageAsync(url);

                    image.SetResourceReference(Image.StyleProperty, "ItemLogo");
                    SetPageTitle(image);
                }
                catch (HttpException)
                {
                    SetPageTitleText(item);
                }
            }
            else
            {
                SetPageTitleText(item);
            }
        }

        /// <summary>
        /// Sets the page title text.
        /// </summary>
        /// <param name="item">The item.</param>
        private void SetPageTitleText(BaseItemDto item)
        {
            SetPageTitle(item.SeriesName ?? item.Album ?? item.Name);
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="title">The title.</param>
        public void SetPageTitle(string title)
        {
            var textblock = new TextBlock { Text = title, Margin = new Thickness(0, 5, 0, 0) };
            textblock.SetResourceReference(TextBlock.StyleProperty, "Heading2TextBlockStyle");

            SetPageTitle(textblock);
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="element">The element.</param>
        public void SetPageTitle(UIElement element)
        {
            var panel = PageTitlePanel;

            panel.Children.Clear();
            panel.Children.Add(element);
        }
    }
}
