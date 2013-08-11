using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for FullscreenVideoPage.xaml
    /// </summary>
    public partial class FullscreenVideoPage : BaseFullscreenVideoPage
    {
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;

        private Timer _overlayTimer;
        private readonly object _timerLock = new object();

        public FullscreenVideoPage(IUserInputManager userInputManager, IImageManager imageManager, IApiClient apiClient, IPlaybackManager playbackManager)
            : base(userInputManager)
        {
            _imageManager = imageManager;
            _apiClient = apiClient;
            _playbackManager = playbackManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += FullscreenVideoPage_Loaded;
            Unloaded += FullscreenVideoPage_Unloaded;
            MainGrid.Visibility = Visibility.Visible;
        }

        protected override void OnMouseIdleChanged()
        {
            base.OnMouseIdleChanged();

            if (!IsMouseIdle)
            {
                ShowOnScreenOverlay();
            }
        }

        private void ShowOnScreenOverlay()
        {
            MainGrid.Visibility = Visibility.Visible;

            lock (_timerLock)
            {
                if (_overlayTimer == null)
                {
                    _overlayTimer = new Timer(TimerCallback, null, 5000, Timeout.Infinite);
                }
                else
                {
                    _overlayTimer.Change(5000, Timeout.Infinite);
                }
            }
        }

        private void TimerCallback(object state)
        {
            DisposeTimer();
        }

        private void DisposeTimer()
        {
            lock (_timerLock)
            {
                if (_overlayTimer != null)
                {
                    _overlayTimer.Dispose();
                    _overlayTimer = null;
                }
            }

            Dispatcher.InvokeAsync(() =>
            {
                if (IsMouseIdle)
                {
                    MainGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowOnScreenOverlay();
                }

            }, DispatcherPriority.Background);
        }

        void FullscreenVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            BaseItemDto currentMedia = null;

            var player = _playbackManager.MediaPlayers
                .OfType<IInternalMediaPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player != null)
            {
                currentMedia = player.CurrentMedia;

                NavBarContent.Content = new NavigationBar(_playbackManager, _imageManager, _apiClient, player);
            }

            UpdateLogo(currentMedia);

            ShowOnScreenOverlay();
        }

        void FullscreenVideoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeTimer();
        }

        private async void UpdateLogo(BaseItemDto media)
        {
            ImgLogo.Visibility = Visibility.Collapsed;
            
            if (media != null && (media.HasLogo || !string.IsNullOrEmpty(media.ParentLogoItemId)))
            {
                try
                {
                    ImgLogo.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetLogoImageUrl(media, new ImageOptions
                    {
                        Height = 100
                    }));

                    ImgLogo.Visibility = Visibility.Visible;
                }
                catch
                {
                    // Already logged at lower levels
                }
            }
        }
    }
}
