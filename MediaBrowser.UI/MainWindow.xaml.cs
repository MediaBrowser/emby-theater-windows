using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.Implementations;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow, IDisposable
    {
        /// <summary>
        /// Gets or sets the system activity timer.
        /// </summary>
        private Timer ActivityTimer { get; set; }
        /// <summary>
        /// Threshold for time beteen input events to register as activity
        /// </summary>
        private readonly TimeSpan _activityThreshold = TimeSpan.FromTicks(1);
        /// <summary>
        /// Threshold between input events before mouse fades
        /// </summary>
        private readonly TimeSpan _mouseFadeThreshold = TimeSpan.FromMilliseconds(3000);
        /// <summary>
        /// Gets or sets the backdrop timer.
        /// </summary>
        /// <value>The backdrop timer.</value>
        private Timer BackdropTimer { get; set; }
        /// <summary>
        /// Gets or sets the current backdrops.
        /// </summary>
        /// <value>The current backdrops.</value>
        private string[] CurrentBackdrops { get; set; }

        /// <summary>
        /// The _current backdrop index
        /// </summary>
        private int _currentBackdropIndex;
        /// <summary>
        /// Gets or sets the index of the current backdrop.
        /// </summary>
        /// <value>The index of the current backdrop.</value>
        public int CurrentBackdropIndex
        {
            get { return _currentBackdropIndex; }
            set
            {
                _currentBackdropIndex = value;
                OnPropertyChanged("CurrentBackdropIndex");
                Dispatcher.InvokeAsync(OnBackdropIndexChanged);
            }
        }

        /// <summary>
        /// The _is mouse idle
        /// </summary>
        private bool _isMouseIdle = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse idle.
        /// </summary>
        /// <value><c>true</c> if this instance is mouse idle; otherwise, <c>false</c>.</value>
        public bool IsMouseIdle
        {
            get { return _isMouseIdle; }
            set
            {
                var changed = _isMouseIdle != value;

                _isMouseIdle = value;

                if (changed)
                {
                    Dispatcher.InvokeAsync(() => Cursor = value ? Cursors.None : Cursors.Arrow);
                    OnPropertyChanged("IsMouseIdle");
                }
            }
        }

        /// <summary>
        /// Checks windows to see if hold long since the last input event
        /// eg. Mouse and keyboard
        /// </summary>
        /// <returns>Timespan of how long since last event</returns>
        public static TimeSpan GetLastInput()
        {
            var plii = new LASTINPUTINFO();
            plii.cbSize = (uint)Marshal.SizeOf(plii);

            if (GetLastInputInfo(ref plii))
                return TimeSpan.FromMilliseconds(Environment.TickCount - plii.dwTime);

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private readonly ILogger _logger;
        private readonly IPlaybackManager _playbackManager;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IApplicationHost _appHost;
        private readonly IApplicationWindow _appWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow(ILogger logger, IPlaybackManager playbackManager, IApiClient apiClient, IImageManager imageManager, IApplicationHost appHost, IApplicationWindow appWindow)
            : base()
        {
            _logger = logger;
            _playbackManager = playbackManager;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _appHost = appHost;
            _appWindow = appWindow;

            InitializeComponent();

            NavigationBar.PlaybackManager = playbackManager;
            WindowCommands.ApplicationHost = _appHost;

            ActivityTimer = new Timer(ActivityCallback, null, 100, 100);

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant);
        }

        private void ActivityCallback(object state)
        {
            var lastTime = GetLastInput();

            if (lastTime > _mouseFadeThreshold)// Order is important here.
            {
                IsMouseIdle = true;
            }
            else if (lastTime > _activityThreshold)
            {
                IsMouseIdle = false;
            }
        }

        /// <summary>
        /// Called when [loaded].
        /// </summary>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            DataContext = App.Instance;

            DragBar.MouseDown += DragableGridMouseDown;

            ((TheaterApplicationWindow)_appWindow).OnWindowLoaded();
        }

        /// <summary>
        /// Called when [backdrop index changed].
        /// </summary>
        private async void OnBackdropIndexChanged()
        {
            var currentBackdropIndex = CurrentBackdropIndex;

            if (currentBackdropIndex == -1)
            {
                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (BackdropContainer.Content != null)
                {
                    BackdropContainer.Content = new FrameworkElement();
                }
                return;
            }

            try
            {
                var bitmap = await _imageManager.GetRemoteBitmapAsync(CurrentBackdrops[currentBackdropIndex]);

                var img = new Image
                {
                    Source = bitmap
                };

                img.SetResourceReference(StyleProperty, "BackdropImage");

                BackdropContainer.Content = img;
            }
            catch (HttpException)
            {
                if (currentBackdropIndex == 0)
                {
                    BackdropContainer.Content = new FrameworkElement();
                }
            }
        }

        /// <summary>
        /// Dragables the grid mouse down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void DragableGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Gets the page frame.
        /// </summary>
        /// <value>The page frame.</value>
        private TransitionFrame PageFrame
        {
            get
            {
                // Finding the grid that is generated by the ControlTemplate of the Button
                return TreeHelper.FindChild<TransitionFrame>(PageContent, "PageFrame");
            }
        }

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        internal DispatcherOperation Navigate(Page page)
        {
            _logger.Info("Navigating to " + page.GetType().Name);

            return Dispatcher.InvokeAsync(() => PageFrame.NavigateWithTransition(page));
        }

        /// <summary>
        /// Sets the backdrop based on a BaseItemDto
        /// </summary>
        /// <param name="item">The item.</param>
        internal void SetBackdrops(BaseItemDto item)
        {
            var urls = _apiClient.GetBackdropImageUrls(item, new ImageOptions
            {
                MaxWidth = Convert.ToInt32(SystemParameters.VirtualScreenWidth),
                MaxHeight = Convert.ToInt32(SystemParameters.VirtualScreenHeight)
            });

            SetBackdrops(urls);
        }

        /// <summary>
        /// Sets the backdrop based on a list of image files
        /// </summary>
        /// <param name="backdrops">The backdrops.</param>
        internal void SetBackdrops(string[] backdrops)
        {
            // Don't reload the same backdrops
            if (CurrentBackdrops != null && backdrops.SequenceEqual(CurrentBackdrops))
            {
                return;
            }

            DisposeBackdropTimer();
            CurrentBackdrops = backdrops;

            if (backdrops == null || backdrops.Length == 0)
            {
                CurrentBackdropIndex = -1;

                // Setting this to null doesn't seem to clear out the content
                // Have to check it for null or get startup errors
                if (BackdropContainer.Content != null)
                {
                    BackdropContainer.Content = new FrameworkElement();
                }
                return;
            }

            CurrentBackdropIndex = 0;

            // We only need the timer if there's more than one backdrop
            if (backdrops.Length > 1)
            {
                BackdropTimer = new Timer(state =>
                {
                    // Don't display backdrops during video playback
                    if (_playbackManager.MediaPlayers.Any(p => p.PlayState != PlayState.Idle && p.CurrentMedia.IsVideo))
                    {
                        return;
                    }

                    var index = CurrentBackdropIndex + 1;

                    if (index >= backdrops.Length)
                    {
                        index = 0;
                    }

                    CurrentBackdropIndex = index;

                }, null, 6000, 6000);
            }
        }

        /// <summary>
        /// Disposes the backdrop timer.
        /// </summary>
        public void DisposeBackdropTimer()
        {
            if (BackdropTimer != null)
            {
                BackdropTimer.Dispose();
            }
        }
        /// <summary>
        /// Disposes the Activity Timer
        /// </summary>
        public void DisposeActivityTimer()
        {
            if (ActivityTimer != null)
            {
                ActivityTimer.Dispose();
            }
        }

        /// <summary>
        /// Clears the backdrops.
        /// </summary>
        internal void ClearBackdrops()
        {
            SetBackdrops(new string[] { });
        }

        /// <summary>
        /// Navigates the back.
        /// </summary>
        internal DispatcherOperation NavigateBack()
        {
            return Dispatcher.InvokeAsync(() =>
            {
                if (PageFrame.NavigationService.CanGoBack)
                {
                    PageFrame.GoBackWithTransition();
                }
            });
        }

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        internal DispatcherOperation NavigateForward()
        {
            return Dispatcher.InvokeAsync(() =>
            {
                if (PageFrame.NavigationService.CanGoForward)
                {
                    PageFrame.GoForwardWithTransition();
                }
            });
        }

        /// <summary>
        /// Called when [browser back].
        /// </summary>
        protected override void OnBrowserBack()
        {
            base.OnBrowserBack();

            NavigateBack();
        }

        /// <summary>
        /// Called when [browser forward].
        /// </summary>
        protected override void OnBrowserForward()
        {
            base.OnBrowserForward();

            NavigateForward();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DisposeBackdropTimer();
            DisposeActivityTimer();
        }
    }
}
