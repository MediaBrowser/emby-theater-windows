using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.Implementations;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private DateTime _lastMouseInput;
        /// <summary>
        /// Gets or sets the system activity timer.
        /// </summary>
        private Timer ActivityTimer { get; set; }
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

        private readonly ILogger _logger;
        private readonly IPlaybackManager _playbackManager;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IApplicationHost _appHost;
        private readonly IPresentationManager _appWindow;
        private readonly IUserInputManager _userInput;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow(ILogger logger, IPlaybackManager playbackManager, IApiClient apiClient, IImageManager imageManager, IApplicationHost appHost, IPresentationManager appWindow, IUserInputManager userInput)
            : base()
        {
            _logger = logger;
            _playbackManager = playbackManager;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _appHost = appHost;
            _appWindow = appWindow;
            _userInput = userInput;

            InitializeComponent();

            WindowCommands.ApplicationHost = _appHost;

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant);
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = App.Instance;

            DragBar.MouseDown += DragableGridMouseDown;

            ActivityTimer = new Timer(TimerCallback, null, 100, 100);

            ((TheaterApplicationWindow)_appWindow).OnWindowLoaded();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _userInput.MouseMove -= _userInput_MouseMove;

            base.OnClosing(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _lastMouseInput = DateTime.Now;
        }

        void _userInput_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _lastMouseInput = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 3000;
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
        internal Task Navigate(Page page)
        {
            _logger.Info("Navigating to " + page.GetType().Name);
            var task = new TaskCompletionSource<bool>();

            Dispatcher.InvokeAsync(async () =>
            {
                await PageFrame.NavigateWithTransition(page);

                task.TrySetResult(true);
            });

            return task.Task;
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
                    if (_playbackManager.MediaPlayers.Any(p =>
                    {
                        if (p.PlayState != PlayState.Idle)
                        {
                            var media = p.CurrentMedia;
                            return media != null && (media.IsVideo || media.IsGame);
                        }
                        return false;
                    }))
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
        internal Task NavigateBack()
        {
            var task = new TaskCompletionSource<bool>();

            Dispatcher.InvokeAsync(async () =>
            {
                if (PageFrame.NavigationService.CanGoBack)
                {
                    await PageFrame.GoBackWithTransition();
                }
                task.TrySetResult(true);
            });

            return task.Task;
        }

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        internal Task NavigateForward()
        {
            var task = new TaskCompletionSource<bool>();

            Dispatcher.InvokeAsync(async () =>
            {
                if (PageFrame.NavigationService.CanGoForward)
                {
                    await PageFrame.GoForwardWithTransition();
                }
                task.TrySetResult(true);
            });

            return task.Task;
        }

        internal void ClearNavigationHistory()
        {
            var frame = PageFrame;

            if (frame.CanGoBack)
            {
                while (frame.RemoveBackEntry() != null)
                {

                }
            }
        }

        /// <summary>
        /// Removes the pages from history.
        /// </summary>
        /// <param name="count">The count.</param>
        internal void RemovePagesFromHistory(int count)
        {
            var frame = PageFrame;

            var current = 0;

            if (frame.CanGoBack)
            {
                while (current < count && frame.RemoveBackEntry() != null)
                {
                    current++;
                }
            }
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
