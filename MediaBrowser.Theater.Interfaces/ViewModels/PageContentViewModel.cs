using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Interfaces.ViewModels
{
    public class PageContentViewModel : BaseViewModel, IDisposable
    {
        protected readonly INavigationService NavigationService;
        protected readonly ISessionManager SessionManager;
        protected readonly IPlaybackManager PlaybackManager;
        protected readonly ILogger Logger;
        protected readonly IApplicationHost AppHost;
        protected readonly IApiClient ApiClient;
        protected readonly IPresentationManager PresentationManager;
        protected readonly IServerEvents ServerEvents;

        public ICommand HomeCommand { get; private set; }
        public ICommand FullscreenVideoCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        public ICommand RestartServerCommand { get; private set; }
        public ICommand RestartApplicationCommand { get; private set; }

        private readonly Timer _clockTimer;
        private readonly Dispatcher _dispatcher;

        public PageContentViewModel(INavigationService navigationService, ISessionManager sessionManager, IPlaybackManager playbackManager, ILogger logger, IApplicationHost appHost, IApiClient apiClient, IPresentationManager presentationManager, IServerEvents serverEvents)
        {
            NavigationService = navigationService;
            SessionManager = sessionManager;
            PlaybackManager = playbackManager;
            Logger = logger;
            AppHost = appHost;
            ApiClient = apiClient;
            PresentationManager = presentationManager;
            ServerEvents = serverEvents;

            NavigationService.Navigated += NavigationServiceNavigated;
            SessionManager.UserLoggedIn += SessionManagerUserLoggedIn;
            SessionManager.UserLoggedOut += SessionManagerUserLoggedOut;
            PlaybackManager.PlaybackStarted += PlaybackManager_PlaybackStarted;
            PlaybackManager.PlaybackCompleted += PlaybackManager_PlaybackCompleted;

            SettingsCommand = new RelayCommand(i => NavigationService.NavigateToSettingsPage());
            HomeCommand = new RelayCommand(i => NavigationService.NavigateToHomePage());
            FullscreenVideoCommand = new RelayCommand(i => NavigationService.NavigateToInternalPlayerPage());
            RestartServerCommand = new RelayCommand(i => RestartServer());
            RestartApplicationCommand = new RelayCommand(i => RestartApplication());

            _dispatcher = Dispatcher.CurrentDispatcher;

            _clockTimer = new Timer(ClockTimerCallback, null, 0, 10000);

            IsLoggedIn = SessionManager.CurrentUser != null;
            var page = NavigationService.CurrentPage;
            IsOnHomePage = page is IHomePage;
            IsOnFullscreenVideo = page is IFullscreenVideoPage;

            ServerEvents.RestartRequired += ServerEvents_RestartRequired;
            ServerEvents.ServerRestarting += ServerEvents_ServerRestarting;
            ServerEvents.ServerShuttingDown += ServerEvents_ServerShuttingDown;
            ServerEvents.Connected += ServerEvents_Connected;
            AppHost.HasPendingRestartChanged += AppHostHasPendingRestartChanged;
            RefreshRestartApplicationNotification();

            // If already connected, get system info now.
            RefreshRestartServerNotification();
        }

        private bool _serverHasPendingRestart;
        private bool _serverCanSelfRestart;

        async void ServerEvents_Connected(object sender, EventArgs e)
        {
            try
            {
                var systemInfo = await ApiClient.GetSystemInfoAsync();
                _serverHasPendingRestart = systemInfo.HasPendingRestart;
                _serverCanSelfRestart = systemInfo.CanSelfRestart;
                RefreshRestartServerNotification();
            }
            catch (Exception)
            {

            }
        }

        void ServerEvents_ServerShuttingDown(object sender, EventArgs e)
        {
            _serverHasPendingRestart = false;
            RefreshRestartServerNotification();
        }

        void ServerEvents_ServerRestarting(object sender, EventArgs e)
        {
            _serverHasPendingRestart = false;
            RefreshRestartServerNotification();
        }

        void ServerEvents_RestartRequired(object sender, EventArgs e)
        {
            _serverHasPendingRestart = true;
            RefreshRestartServerNotification();
        }

        void AppHostHasPendingRestartChanged(object sender, EventArgs e)
        {
            RefreshRestartApplicationNotification();
        }

        private void RefreshRestartApplicationNotification()
        {
            _dispatcher.InvokeAsync(() => ShowRestartApplicationNotification = AppHost.HasPendingRestart && SessionManager.CurrentUser != null && SessionManager.CurrentUser.Configuration.IsAdministrator);
        }

        private void RefreshRestartServerNotification()
        {
            _dispatcher.InvokeAsync(() => ShowRestartServerNotification = _serverHasPendingRestart && SessionManager.CurrentUser != null && SessionManager.CurrentUser.Configuration.IsAdministrator);
        }

        void PlaybackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            IsPlayingInternalVideo = false;
        }

        void PlaybackManager_PlaybackStarted(object sender, PlaybackStartEventArgs e)
        {
            IsPlayingInternalVideo = e.Player is IInternalMediaPlayer && e.Player is IVideoPlayer && e.Player.CurrentMedia != null && e.Player.CurrentMedia.IsVideo;
        }

        private void ClockTimerCallback(object state)
        {
            _dispatcher.InvokeAsync(() => OnPropertyChanged("DateTime"), DispatcherPriority.Background);
        }

        void SessionManagerUserLoggedOut(object sender, EventArgs e)
        {
            IsLoggedIn = SessionManager.CurrentUser != null;
            RefreshRestartApplicationNotification();
            RefreshRestartServerNotification();
        }

        void SessionManagerUserLoggedIn(object sender, EventArgs e)
        {
            IsLoggedIn = SessionManager.CurrentUser != null;
            RefreshRestartApplicationNotification();
            RefreshRestartServerNotification();
        }

        void NavigationServiceNavigated(object sender, NavigationEventArgs e)
        {
            IsOnHomePage = e.NewPage is IHomePage;
            IsOnFullscreenVideo = e.NewPage is IFullscreenVideoPage;
        }

        protected DateTime DateTime
        {
            get { return DateTime.Now; }
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }

            set
            {
                var changed = _isLoggedIn != value;

                _isLoggedIn = value;
                if (changed)
                {
                    OnPropertyChanged("IsLoggedIn");
                }
            }
        }

        private bool _showRestartServerNotification;
        public bool ShowRestartServerNotification
        {
            get { return _showRestartServerNotification; }

            set
            {
                var changed = _showRestartServerNotification != value;

                _showRestartServerNotification = value;
                if (changed)
                {
                    OnPropertyChanged("ShowRestartServerNotification");
                }
            }
        }

        private bool _showRestartApplicationNotification;
        public bool ShowRestartApplicationNotification
        {
            get { return _showRestartApplicationNotification; }

            set
            {
                var changed = _showRestartApplicationNotification != value;

                _showRestartApplicationNotification = value;
                if (changed)
                {
                    OnPropertyChanged("ShowRestartApplicationNotification");
                }
            }
        }

        private bool _isOnHomePage;
        public bool IsOnHomePage
        {
            get { return _isOnHomePage; }

            set
            {
                var changed = _isOnHomePage != value;

                _isOnHomePage = value;

                if (changed)
                {
                    OnPropertyChanged("IsOnHomePage");
                }
            }
        }

        private bool _isOnFullscreenVideo;
        public bool IsOnFullscreenVideo
        {
            get { return _isOnFullscreenVideo; }

            set
            {
                var changed = _isOnFullscreenVideo != value;

                _isOnFullscreenVideo = value;
                if (changed)
                {
                    OnPropertyChanged("IsOnFullscreenVideo");
                }
            }
        }

        private bool _isPlayingInternalVideo;
        public bool IsPlayingInternalVideo
        {
            get { return _isPlayingInternalVideo; }

            set
            {
                var changed = _isPlayingInternalVideo != value;

                _isPlayingInternalVideo = value;
                if (changed)
                {
                    OnPropertyChanged("IsPlayingInternalVideo");
                }
            }
        }

        private bool _showGlobalContent = true;
        public bool ShowGlobalContent
        {
            get { return _showGlobalContent; }

            set
            {
                var changed = _showGlobalContent != value;

                _showGlobalContent = value;

                if (changed)
                {
                    OnPropertyChanged("ShowGlobalContent");
                }
            }
        }

        private bool _showDefaultPageTitle = true;
        public bool ShowDefaultPageTitle
        {
            get { return _showDefaultPageTitle; }

            set
            {
                var changed = _showDefaultPageTitle != value;

                _showDefaultPageTitle = value;

                if (changed)
                {
                    OnPropertyChanged("ShowDefaultPageTitle");
                }
            }
        }

        private string _pageTitle;
        public string PageTitle
        {
            get { return _pageTitle; }

            set
            {
                var changed = !string.Equals(_pageTitle, value);

                _pageTitle = value;

                if (changed)
                {
                    OnPropertyChanged("PageTitle");
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Restarts the server.
        /// </summary>
        private async void RestartServer()
        {
            if (!_serverCanSelfRestart)
            {
                PresentationManager.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OK,
                    Caption = "Restart Media Browser Server",
                    Icon = MessageBoxIcon.Information,
                    Text = "Please logon to Media Browser Server and restart in order to finish applying updates."
                });

                return;
            }

            var result = PresentationManager.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OKCancel,
                Caption = "Restart Media Browser Server",
                Icon = MessageBoxIcon.Information,
                Text = "Please restart Media Browser Server in order to finish applying updates."
            });

            if (result == MessageBoxResult.OK)
            {
                try
                {
                    var systemInfo = await ApiClient.GetSystemInfoAsync();

                    if (systemInfo.HasPendingRestart)
                    {
                        await ApiClient.RestartServerAsync();

                        WaitForServerToRestart();
                    }

                    _serverHasPendingRestart = false;
                    RefreshRestartServerNotification();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error restarting server.", ex);

                    PresentationManager.ShowDefaultErrorMessage();
                }
            }
        }

        private async void WaitForServerToRestart()
        {
            PresentationManager.ShowModalLoadingAnimation();

            try
            {
                await WaitForServerToRestartInternal();
            }
            finally
            {
                PresentationManager.HideModalLoadingAnimation();

                FocusElement();
            }
        }

        private void FocusElement()
        {
            var win = PresentationManager.Window;

            win.Activate();
            win.Focus();

            win.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private async Task WaitForServerToRestartInternal()
        {
            var count = 0;

            while (count < 10)
            {
                await Task.Delay(3000);

                try
                {
                    await ApiClient.GetSystemInfoAsync();
                    break;
                }
                catch
                {

                }

                count++;
            }
        }


        /// <summary>
        /// Restarts the application.
        /// </summary>
        private async void RestartApplication()
        {
            var result = PresentationManager.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OKCancel,
                Caption = "Restart Media Browser Theater",
                Icon = MessageBoxIcon.Information,
                Text = "Please restart to finish updating Media Browser Theater."
            });

            if (result == MessageBoxResult.OK)
            {
                try
                {
                    await AppHost.Restart();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error restarting application.", ex);

                    PresentationManager.ShowDefaultErrorMessage();
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            Logger.Info("Disposing {0}", GetType().Name);

            if (dispose)
            {
                _clockTimer.Dispose();

                ServerEvents.Connected -= ServerEvents_Connected;
                ServerEvents.RestartRequired -= ServerEvents_RestartRequired;
                ServerEvents.ServerRestarting -= ServerEvents_ServerRestarting;
                ServerEvents.ServerShuttingDown -= ServerEvents_ServerShuttingDown;
                AppHost.HasPendingRestartChanged -= AppHostHasPendingRestartChanged;
                NavigationService.Navigated -= NavigationServiceNavigated;
                SessionManager.UserLoggedIn -= SessionManagerUserLoggedIn;
                SessionManager.UserLoggedOut -= SessionManagerUserLoggedOut;
                PlaybackManager.PlaybackStarted -= PlaybackManager_PlaybackStarted;
                PlaybackManager.PlaybackCompleted -= PlaybackManager_PlaybackCompleted;
            }
        }
    }
}
