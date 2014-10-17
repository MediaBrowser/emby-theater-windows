using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Connect;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for StartupWizardPage2.xaml
    /// </summary>
    public partial class StartupWizardPage2 : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        private PinCreationResult _pinResult;
        private Timer _pinTimer;

        public StartupWizardPage2(INavigationService nav, IConnectionManager connectionManager, IPresentationManager presentation, ILogger logger)
        {
            _nav = nav;
            _connectionManager = connectionManager;
            _presentation = presentation;
            _logger = logger;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LoadPin();

            Loaded += StartupWizardPage_Loaded;
            BtnSkip.Click += BtnSkip_Click;
            BtnBack.Click += BtnBack_Click;
            BtnResetPin.Click += BtnResetPin_Click;
        }

        void BtnResetPin_Click(object sender, RoutedEventArgs e)
        {
            LoadPin();
        }

        private async void LoadPin()
        {
            Dispatcher.InvokeAsync(() =>
            {
                TxtPin.Visibility = Visibility.Visible;
                PinError.Visibility = Visibility.Collapsed;
            });

            StopPinTimer();
            TxtPin.Text = string.Empty;

            try
            {
                _pinResult = await _connectionManager.CreatePin().ConfigureAwait(false);

                Dispatcher.InvokeAsync(() => TxtPin.Text = _pinResult.Pin);

                StartPinTimer();
            }
            catch (Exception)
            {
                OnPinError();
            }
        }

        private void OnPinError()
        {
            Dispatcher.InvokeAsync(() =>
            {
                TxtPin.Visibility = Visibility.Collapsed;
                PinError.Visibility = Visibility.Visible;

                TxtPinError.Text = "An error has occurred while attempting to communicate with mediabrowser.tv. Click the button below to try again or skip to connect to your server manually.";
            });
        }

        private void OnPinExpired()
        {
            Dispatcher.InvokeAsync(() =>
            {
                TxtPin.Visibility = Visibility.Collapsed;
                PinError.Visibility = Visibility.Visible;

                TxtPinError.Text = "Your pin has expired. Click the button below to generate a new pin or skip to connect to your server manually.";
            });
        }

        private async void PollForPinUpdate()
        {
            try
            {
                var pinStatus = await _connectionManager.GetPinStatus(_pinResult);

                if (pinStatus.IsExpired)
                {
                    OnPinExpired();
                }
                else if (pinStatus.IsConfirmed)
                {
                    OnPinConfirmed();
                }
            }
            catch (Exception)
            {
            }
        }

        private void StartPinTimer()
        {
            _pinTimer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        private void TimerCallback(object state)
        {
            PollForPinUpdate();
        }

        private void StopPinTimer()
        {
            if (_pinTimer != null)
            {
                _pinTimer.Dispose();
                _pinTimer = null;
            }
        }

        async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            StopPinTimer();
            await _nav.NavigateBack();
        }

        void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            GoNext();
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }

        private async void OnPinConfirmed()
        {
            StopPinTimer();

            try
            {
                await _connectionManager.ExchangePin(_pinResult).ConfigureAwait(false);
            }
            catch (Exception)
            {
                OnPinError();
            }
        }

        private async void GoNext()
        {
            StopPinTimer();

            try
            {
                var connectionResult = await _connectionManager.Connect(CancellationToken.None);

                App.Instance.NavigateFromConnectionResult(connectionResult);
                return;
            }
            catch (Exception)
            {
            }

            await _nav.Navigate(new StartupPageServerEntry(_nav, _connectionManager, _presentation, _logger));
        }
    }
}
