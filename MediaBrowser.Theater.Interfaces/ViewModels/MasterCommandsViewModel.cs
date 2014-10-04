using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Interfaces.ViewModels
{
    /// <summary>
    /// Represents a set of commands which can be executed from anywhere in the theme. Bind to this class to implement functionality and inherit to add functionality.
    /// </summary>
    public class MasterCommandsViewModel : BaseViewModel, IDisposable
    {
        private bool _serverCanSelfRestart;
        private bool _serverHasPendingRestart;

        protected readonly INavigationService NavigationService;
        protected readonly ISessionManager SessionManager;
        protected readonly IPresentationManager PresentationManager;
        protected readonly IConnectionManager ConnectionManager;
        protected readonly ILogger Logger;
        protected readonly IApplicationHost AppHost;
        protected readonly Dispatcher Dispatcher;

        public ICommand HomeCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand FullscreenVideoCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }
        public ICommand GoBackCommand { get; private set; }
        public ICommand RestartServerCommand { get; private set; }
        public ICommand RestartApplicationCommand { get; private set; }
        public ICommand ShutdownApplicationCommand { get; private set; }
        public ICommand ShutdownSystemCommand { get; private set; }
        public ICommand RestartSystemCommand { get; private set; }
        public ICommand SleepSystemCommand { get; private set; }

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

        private bool _homeEnabled;
        public bool HomeEnabled
        {
            get { return _homeEnabled; }

            set
            {
                var changed = _homeEnabled != value;

                _homeEnabled = value;
                if (changed)
                {
                    OnPropertyChanged("HomeEnabled");
                }
            }
        }

        public event EventHandler<EventArgs> PageNavigated;

        public MasterCommandsViewModel(INavigationService navigationService, ISessionManager sessionManager, IPresentationManager presentationManager, IConnectionManager connectionManager, ILogger logger, ITheaterApplicationHost appHost)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;

            NavigationService = navigationService;
            SessionManager = sessionManager;
            PresentationManager = presentationManager;
            ConnectionManager = connectionManager;
            Logger = logger;
            AppHost = appHost;

            AppHost.HasPendingRestartChanged += AppHostHasPendingRestartChanged;
            SessionManager.UserLoggedIn += SessionManager_UserLoggedIn;
            SessionManager.UserLoggedOut += SessionManager_UserLoggedOut;
            NavigationService.Navigated += NavigationService_Navigated;

            HomeCommand = new RelayCommand(i => GoHome());
            SearchCommand = new RelayCommand(i => GoSearch());
            FullscreenVideoCommand = new RelayCommand(i => NavigationService.NavigateToInternalPlayerPage());
            SettingsCommand = new RelayCommand(i => GoSettings());
            GoBackCommand = new RelayCommand(i => GoBack());

            RestartServerCommand = new RelayCommand(i => { });
            //RestartServerCommand = new RelayCommand(i => RestartServer());

            RestartApplicationCommand = new RelayCommand(i => RestartApplication());
            ShutdownApplicationCommand = new RelayCommand(i => ShutdownApplication());
            ShutdownSystemCommand = new RelayCommand(i => appHost.ShutdownSystem());
            RestartSystemCommand = new RelayCommand(i => appHost.RebootSystem());
            SleepSystemCommand = new RelayCommand(i => appHost.SetSystemToSleep());

            RefreshRestartServerNotification();
        }

        public void RefreshRestartApplicationNotification()
        {
            Dispatcher.InvokeAsync(() => ShowRestartApplicationNotification = AppHost.HasPendingRestart && SessionManager.CurrentUser != null);
        }

        public void RefreshRestartServerNotification()
        {
            Dispatcher.InvokeAsync(() => ShowRestartServerNotification = _serverHasPendingRestart && SessionManager.CurrentUser != null && SessionManager.CurrentUser.Configuration.IsAdministrator);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                AppHost.HasPendingRestartChanged -= AppHostHasPendingRestartChanged;
                SessionManager.UserLoggedIn -= SessionManager_UserLoggedIn;
                SessionManager.UserLoggedOut -= SessionManager_UserLoggedOut;
            }
        }

        private void BindEvents(IApiClient client)
        {
            UnbindEvents(client);
            client.RestartRequired += ServerEvents_RestartRequired;
            client.ServerRestarting += ServerEvents_ServerRestarting;
            client.ServerShuttingDown += ServerEvents_ServerShuttingDown;
        }

        private void UnbindEvents(IApiClient client)
        {
            client.RestartRequired -= ServerEvents_RestartRequired;
            client.ServerRestarting -= ServerEvents_ServerRestarting;
            client.ServerShuttingDown -= ServerEvents_ServerShuttingDown;
        }

        /// <summary>
        /// Navigates to the home page
        /// </summary>
        protected async virtual void GoHome()
        {
            if (SessionManager.CurrentUser != null)
            {
                await NavigationService.NavigateToHomePage();
            }
            else
            {
                await NavigationService.NavigateToLoginPage();
            }

            if (PageNavigated != null) { PageNavigated(this, new EventArgs()); }
        }

        protected async virtual void GoSearch()
        {
            await NavigationService.NavigateToSearchPage();
            if (PageNavigated != null) { PageNavigated(this, new EventArgs()); }
        }

        protected async virtual void GoSettings()
        {
            await NavigationService.NavigateToSettingsPage();
            if (PageNavigated != null) { PageNavigated(this, new EventArgs()); }
        }

        /// <summary>
        /// Navigates to the previous page
        /// </summary>
        protected virtual async void GoBack()
        {
            await NavigationService.NavigateBack();
        }

        protected void FocusElement()
        {
            var win = PresentationManager.Window;

            win.Activate();
            win.Focus();

            win.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        protected void RefreshHomeButton(Page currentPage)
        {
            HomeEnabled = SessionManager.CurrentUser != null && !(currentPage is IHomePage) && !(currentPage is ILoginPage);
        }

        protected virtual void OnPageNavigated(object sender, EventArgs e)
        {
            if (PageNavigated != null)
            {
                PageNavigated(sender, e);
            }
        }

        private async void ShutdownApplication()
        {
            try
            {
                await AppHost.Shutdown();
            }
            catch (Exception ex)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }

        /// <summary>
        /// Restarts the server.
        /// </summary>
        private async void RestartServer(IApiClient apiClient)
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

            if (result != MessageBoxResult.OK) return;
            try
            {
                var systemInfo = await apiClient.GetSystemInfoAsync(CancellationToken.None);

                if (systemInfo.HasPendingRestart)
                {
                    await apiClient.RestartServerAsync();

                    WaitForServerToRestart(apiClient);
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

            if (result != MessageBoxResult.OK) return;
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

        private async void WaitForServerToRestart(IApiClient apiClient)
        {
            PresentationManager.ShowModalLoadingAnimation();

            try
            {
                await WaitForServerToRestartInternal(apiClient);
            }
            finally
            {
                PresentationManager.HideModalLoadingAnimation();

                FocusElement();
            }
        }

        private async Task WaitForServerToRestartInternal(IApiClient apiClient)
        {
            var count = 0;

            while (count < 10)
            {
                await Task.Delay(3000);

                try
                {
                    await apiClient.GetPublicSystemInfoAsync(CancellationToken.None);
                    break;
                }
                catch (Exception)
                {

                }

                count++;
            }
        }

        protected virtual void SessionManager_UserLoggedIn(object sender, EventArgs e)
        {
            var apiClient = SessionManager.ActiveApiClient;
            BindEvents(apiClient);

            RefreshHomeButton(NavigationService.CurrentPage);

            RefreshRestartData(apiClient);
        }

        private async void RefreshRestartData(IApiClient apiClient)
        {
            try
            {
                var systemInfo = await apiClient.GetSystemInfoAsync(CancellationToken.None);
                _serverHasPendingRestart = systemInfo.HasPendingRestart;
                _serverCanSelfRestart = systemInfo.CanSelfRestart;
                RefreshRestartServerNotification();
            }
            catch (Exception)
            {

            }
        }

        protected virtual void SessionManager_UserLoggedOut(object sender, EventArgs e)
        {
            RefreshHomeButton(NavigationService.CurrentPage);
        }

        protected virtual void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            RefreshHomeButton(e.NewPage as Page);
        }

        private void ServerEvents_ServerShuttingDown(object sender, EventArgs e)
        {
            _serverHasPendingRestart = false;
            RefreshRestartServerNotification();
        }

        private void ServerEvents_ServerRestarting(object sender, EventArgs e)
        {
            _serverHasPendingRestart = false;
            RefreshRestartServerNotification();
        }

        private void ServerEvents_RestartRequired(object sender, EventArgs e)
        {
            _serverHasPendingRestart = true;
            RefreshRestartServerNotification();
        }

        private void AppHostHasPendingRestartChanged(object sender, EventArgs e)
        {
            RefreshRestartApplicationNotification();
        }
    }
}
