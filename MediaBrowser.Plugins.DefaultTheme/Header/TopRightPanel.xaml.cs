using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.UserProfileMenu;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Header
{
    /// <summary>
    /// Interaction logic for TopRightPanel.xaml
    /// </summary>
    public partial class TopRightPanel : UserControl
    {
        internal static ISessionManager SessionManager { get; set; }
        internal static IApiClient ApiClient { get; set; }
        internal static IImageManager ImageManager { get; set; }
        internal static ILogger Logger { get; set; }
        internal static INavigationService Navigation { get; set; }
        internal static IPlaybackManager PlaybackManager { get; set; }

        private Timer ClockTimer { get; set; }

        internal static TopRightPanel Current;

        public TopRightPanel()
        {
            Current = this;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += TopRightPanel_Loaded;
            Unloaded += TopRightPanel_Unloaded;
            FullscreenButton.Click += FullscreenButton_Click;

            SessionManager.UserLoggedIn += SessionManager_UserLoggedIn;
            SessionManager.UserLoggedOut += SessionManager_UserLoggedOut;

            PlaybackManager.PlaybackStarted += PlaybackManager_PlaybackStarted;
            PlaybackManager.PlaybackCompleted += PlaybackManager_PlaybackCompleted;

            Navigation.Navigated += Navigation_Navigated;

            CurrentUserButton.Click += CurrentUserButton_Click;
            ViewButton.Click += ViewButton_Click;
        }

        void Navigation_Navigated(object sender, NavigationEventArgs e)
        {
            ViewButton.Visibility = e.NewPage is IHasDisplayPreferences
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;
        }

        void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var page = Navigation.CurrentPage as IHasDisplayPreferences;

            if (page != null)
            {
                page.ShowDisplayPreferencesMenu();
            }
        }

        void CurrentUserButton_Click(object sender, RoutedEventArgs e)
        {
            new UserProfileWindow(SessionManager, ImageManager, ApiClient).ShowModal(this.GetWindow());
        }

        void TopRightPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ClockTimer = new Timer(ClockTimerCallback, null, 0, 10000);
        }

        void TopRightPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            SessionManager.UserLoggedIn -= SessionManager_UserLoggedIn;
            SessionManager.UserLoggedOut -= SessionManager_UserLoggedOut;

            PlaybackManager.PlaybackStarted -= PlaybackManager_PlaybackStarted;
            PlaybackManager.PlaybackCompleted -= PlaybackManager_PlaybackCompleted;

            if (ClockTimer != null)
            {
                ClockTimer.Dispose();
                ClockTimer = null;
            }
        }

        /// <summary>
        /// Clocks the timer callback.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        private void ClockTimerCallback(object stateInfo)
        {
            Dispatcher.InvokeAsync(UpdateTime, DispatcherPriority.Background);
        }

        private void UpdateTime()
        {
            var left = CurrentTimeLeft;
            var right = CurrentTimeRight;

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
        }

        async void SessionManager_UserLoggedOut(object sender, EventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                CurrentUserButton.Visibility = Visibility.Collapsed;
            });
        }

        async void SessionManager_UserLoggedIn(object sender, EventArgs e)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                CurrentUserButton.Visibility = Visibility.Visible;

                await UpdateUserImage(SessionManager.CurrentUser, CurrentUserButton);

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
                var imageUrl = ApiClient.GetUserImageUrl(user, new ImageOptions
                {
                    ImageType = ImageType.Primary
                });

                try
                {
                    var img = await ImageManager.GetRemoteBitmapAsync(imageUrl);

                    var grid = (Grid)currentUserButton.Content;

                    var image = (Image)grid.Children[0];

                    image.Visibility = Visibility.Visible;
                    image.Source = img;
                    grid.Children[1].Visibility = Visibility.Collapsed;

                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error getting user image", ex);

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
        /// Settingses the button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            await Navigation.NavigateToSettingsPage();
        }

        /// <summary>
        /// Handles the Click event of the NowPlayingButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            await Navigation.NavigateToInternalPlayerPage();
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStopEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        async void PlaybackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => FullscreenButton.Visibility = Visibility.Collapsed);
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
                await Dispatcher.InvokeAsync(() => FullscreenButton.Visibility = Visibility.Visible);
            }
        }
    }
}
