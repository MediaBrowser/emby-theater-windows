using System.Linq;
using System.Runtime.InteropServices;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
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
        private readonly IPresentationManager _presentation;
        private readonly IPlaybackManager _playbackManager;
        private readonly INavigationService _nav;
        private readonly IServerEvents _serverEvents;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;
        private readonly IHiddenWindow _hiddenWindow;

        private Timer _activityTimer;
        private DateTime _lastMouseInput;

        private Timer _overlayTimer;
        private readonly object _overlayTimerLock = new object();

        private TransportOsdViewModel _viewModel;

        public FullscreenVideoPage(IUserInputManager userInputManager, IPlaybackManager playbackManager, INavigationService nav, IPresentationManager presentation, IApiClient apiClient, IImageManager imageManager, ILogger logger, IServerEvents serverEvents, IHiddenWindow hiddenWindow)
        {
            _userInputManager = userInputManager;
            _playbackManager = playbackManager;
            _nav = nav;
            _presentation = presentation;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _logger = logger;
            _serverEvents = serverEvents;
            _hiddenWindow = hiddenWindow;

            InitializeComponent();

            IsMouseIdle = false;
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
                    if (value)
                    {
                        Dispatcher.InvokeAsync(HideOsd, DispatcherPriority.Background);
                    }
                    else
                    {
                        Dispatcher.InvokeAsync(ShowOsd, DispatcherPriority.Background);
                    }
                }
            }
        }

        public void ShowOsd()
        {
            Dispatcher.InvokeAsync(ShowOnScreenDisplayInternal, DispatcherPriority.Background);
        }

        private void ShowOnScreenDisplayInternal()
        {
            if (_infoWindow != null)
            {
                return;
            }
            
            Osd.Visibility = Visibility.Visible;

            lock (_overlayTimerLock)
            {
                if (_overlayTimer == null)
                {
                    _overlayTimer = new Timer(OsdDisplayTimerCallback, null, 4000, Timeout.Infinite);
                }
                else
                {
                    _overlayTimer.Change(4000, Timeout.Infinite);
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

            if (IsMouseIdle)
            {
                HideOsd();
            }
            else
            {
                ShowOsd();
            }
        }

        public void HideOsd()
        {
            Dispatcher.InvokeAsync(() =>
            {
                Osd.Visibility = Visibility.Collapsed;
            }, DispatcherPriority.Background);
        }

        public void ToggleOsd()
        {
            if (Osd.Visibility == Visibility.Visible)
            {
                HideOsd();
            }
            else
            {
                ShowOsd();
                
            }
        }

        private InfoWindow _infoWindow;
        public void ShowInfoPanel()
        {
            Dispatcher.InvokeAsync(() =>
            {
                HideOsd();

                if (_infoWindow != null)
                {
                    return;
                }

                _infoWindow = new InfoWindow()
                {
                    DataContext = _viewModel

                };

                _infoWindow.ShowModal(_presentation.Window, _userInputManager);

                _infoWindow = null;

                IsMouseIdle = true;

            }, DispatcherPriority.Background);
        }

        public void HideInfoPanel()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_infoWindow != null)
                {
                    _infoWindow.Close();
                }

            }, DispatcherPriority.Background);
        }
       
        public void ToggleInfoPanel()
        {
            if (_infoWindow == null)
            {
                ShowInfoPanel();
            }
            else
            {
                HideInfoPanel();
            }
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += FullscreenVideoPage_Loaded;
            Unloaded += FullscreenVideoPage_Unloaded;
        }

        async void _playbackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            if (_infoWindow != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _infoWindow.Close();

                    _infoWindow = null;
                });
            }

            await _nav.NavigateBack();
        }

        void FullscreenVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _lastMouseInput = DateTime.Now;
            _activityTimer = new Timer(TimerCallback, null, 100, 100);
            
            _userInputManager.MouseMove += _userInputManager_MouseMove;

            var player = _playbackManager
                .MediaPlayers
                .OfType<IVideoPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player != null && player.RequiresGlobalMouseHook)
            {
                _userInputManager.GlobalMouseMove += _userInputManager_GlobalMouseMove;
            }

            _presentation.SetGlobalThemeContentVisibility(false);
            _playbackManager.PlaybackCompleted += _playbackManager_PlaybackCompleted;

            Osd.DataContext = _viewModel = new TransportOsdViewModel(_playbackManager, _apiClient, _imageManager, _presentation, _logger, _nav, _serverEvents);
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
            _userInputManager.GlobalMouseMove -= _userInputManager_GlobalMouseMove;
            _presentation.SetGlobalThemeContentVisibility(true);
            _playbackManager.PlaybackCompleted -= _playbackManager_PlaybackCompleted;

            if (_viewModel != null)
            {
                _viewModel.Dispose();
            }

            if (_infoWindow != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _infoWindow.Close();

                    _infoWindow = null;
                });
            }
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        /// <summary>
        /// Mouse moved event - figure if we have moved and update input time
        /// </summary>
        // mouse move is moving across 2 different windows
        // hiddenwindow & the OSD/Info, windows relaive position would cause problems
        // so use interop to call GetCursorPosition fro absolute screen position as we
        // are only interested in change in position.
         private System.Windows.Point? _lastMouseMovePoint;
        void _userInputManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_lastMouseMovePoint.HasValue)
            {
                _lastMouseMovePoint = GetCursorPosition();
                return;
            }
            if (_lastMouseMovePoint == GetCursorPosition())
            {
                return;
            }
            _lastMouseMovePoint = GetCursorPosition();
            _lastMouseInput = DateTime.Now;
        }

        void _userInputManager_GlobalMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!_lastMouseMovePoint.HasValue)
            {
                _lastMouseMovePoint = GetCursorPosition();
                return;
            }
            if (_lastMouseMovePoint == GetCursorPosition())
            {
                return;
            }
            _lastMouseMovePoint = GetCursorPosition();
            _lastMouseInput = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 4000;
        }
    }
}
