using System.Linq;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Timer = System.Threading.Timer;

namespace MediaBrowser.Theater.Core.FullscreenVideo
{
    /// <summary>
    /// Interaction logic for FullscreenVideoPage.xaml
    /// </summary>
    public partial class FullscreenVideoPage : BasePage, IFullscreenVideoPage, ISupportsThemeMedia
    {
        private readonly IUserInputManager _userInputManager;
        private readonly IThemeManager _themeManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly INavigationService _nav;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        
        private Timer _activityTimer;
        private DateTime _lastMouseInput;

        private Timer _overlayTimer;
        private readonly object _overlayTimerLock = new object();

        private TransportOsdViewModel _viewModel;
        
        public FullscreenVideoPage(IUserInputManager userInputManager, IPlaybackManager playbackManager, INavigationService nav, IThemeManager themeManager, IApiClient apiClient, IImageManager imageManager)
        {
            _userInputManager = userInputManager;
            _playbackManager = playbackManager;
            _nav = nav;
            _themeManager = themeManager;
            _apiClient = apiClient;
            _imageManager = imageManager;

            InitializeComponent();
        }

        /// <summary>
        /// The _is mouse idle
        /// </summary>
        private bool _isMouseIdle = true;

        private bool IsMouseIdle
        {
            get { return _isMouseIdle; }
            set
            {
                var changed = _isMouseIdle != value;

                _isMouseIdle = value;

                if (changed)
                {
                    Dispatcher.InvokeAsync(ShowOsd, DispatcherPriority.Background);
                }
            }
        }

        private void ShowOsd()
        {
            Osd.Visibility = Visibility.Visible;

            lock (_overlayTimerLock)
            {
                if (_overlayTimer == null)
                {
                    _overlayTimer = new Timer(OsdDisplayTimerCallback, null, 5000, Timeout.Infinite);
                }
                else
                {
                    _overlayTimer.Change(5000, Timeout.Infinite);
                }
            }
        }

        private void OsdDisplayTimerCallback(object state)
        {
            DisposeOsdTimer();
        }

        private void DisposeOsdTimer()
        {
            lock (_overlayTimerLock)
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
                    Osd.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowOsd();
                }

            }, DispatcherPriority.Background);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += FullscreenVideoPage_Loaded;
            Unloaded += FullscreenVideoPage_Unloaded;
        }

        async void _playbackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            await _nav.NavigateBack();
        }

        void FullscreenVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _activityTimer = new Timer(TimerCallback, null, 100, 100);

            _userInputManager.MouseMove += _userInputManager_MouseMove;
            _themeManager.CurrentTheme.SetGlobalContentVisibility(false);
            _playbackManager.PlaybackCompleted += _playbackManager_PlaybackCompleted;

            Osd.DataContext = _viewModel = new TransportOsdViewModel(_playbackManager, _apiClient, _imageManager);
        }

        void FullscreenVideoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_activityTimer != null)
            {
                _activityTimer.Dispose();
                _activityTimer = null;
            }

            DisposeOsdTimer();

            _userInputManager.MouseMove -= _userInputManager_MouseMove;
            _themeManager.CurrentTheme.SetGlobalContentVisibility(true);
            _playbackManager.PlaybackCompleted -= _playbackManager_PlaybackCompleted;

            if (_viewModel != null)
            {
                _viewModel.Dispose();
            }
        }

        void _userInputManager_MouseMove(object sender, MouseEventArgs e)
        {
            _lastMouseInput = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 4000;
        }
    }
}
