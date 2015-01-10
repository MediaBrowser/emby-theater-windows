using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Connect;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class ConnectPinViewModel
        : BaseViewModel
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly IConnectionManager _connectionManager;
        private string _pin;
        private bool _hasErrors;
        private ICommand _generatePinCommand;
        private string _errorDescription;

        private PinCreationResult _pinResult;
        private Timer _pinTimer;

        public string Pin
        {
            get { return _pin; }
            private set
            {
                if (value == _pin) {
                    return;
                }
                _pin = value;
                OnPropertyChanged();
            }
        }

        public bool HasErrors
        {
            get { return _hasErrors; }
            private set
            {
                if (value.Equals(_hasErrors)) {
                    return;
                }
                _hasErrors = value;
                OnPropertyChanged();
            }
        }

        public ICommand GeneratePinCommand
        {
            get { return _generatePinCommand; }
            private set
            {
                if (Equals(value, _generatePinCommand)) {
                    return;
                }
                _generatePinCommand = value;
                OnPropertyChanged();
            }
        }

        public string ErrorDescription
        {
            get { return _errorDescription; }
            private set
            {
                if (value == _errorDescription) {
                    return;
                }
                _errorDescription = value;
                OnPropertyChanged();
            }
        }

        public ConnectPinViewModel(ITheaterApplicationHost appHost, IConnectionManager connectionManager)
        {
            _appHost = appHost;
            _connectionManager = connectionManager;
        }

        public override Task Initialize()
        {
            LoadPin();
            return base.Initialize();
        }

        private async void LoadPin()
        {
            StopPinTimer();

            try {
                HasErrors = false;

                _pinResult = await _connectionManager.CreatePin().ConfigureAwait(false);
                Pin = _pinResult.Pin;
                
                StartPinTimer();
            }
            catch {
                OnPinError();
            }
        }

        private void OnPinError()
        {
            ErrorDescription = "MediaBrowser.Theater.DefaultTheme:Strings:Login_Connect_PinError".Localize();
            HasErrors = true;
        }

        private void OnPinExpired()
        {
            ErrorDescription = "MediaBrowser.Theater.DefaultTheme:Strings:Login_Connect_PinExpired".Localize();
            HasErrors = true;
        }

        private void StopPinTimer()
        {
            if (_pinTimer != null) {
                _pinTimer.Dispose();
                _pinTimer = null;
            }
        }

        private void StartPinTimer()
        {
            const int intervalSeconds = 7;
            _pinTimer = new Timer(state => PollForPinUpdate(), null, TimeSpan.FromSeconds(intervalSeconds), TimeSpan.FromSeconds(intervalSeconds));
        }

        private async void PollForPinUpdate()
        {
            try {
                var pinStatus = await _connectionManager.GetPinStatus(_pinResult);

                if (pinStatus.IsExpired) {
                    OnPinExpired();
                } else if (pinStatus.IsConfirmed) {
                    OnPinConfirmed();
                }
            }
            catch (Exception) { }
        }

        private async void OnPinConfirmed()
        {
            StopPinTimer();

            try {
                await _connectionManager.ExchangePin(_pinResult).ConfigureAwait(false);
                
                var result = await _connectionManager.Connect(CancellationToken.None);
                if (result.State == ConnectionState.Unavailable) {
                    result.State = ConnectionState.ServerSelection;
                }

                await _appHost.HandleConnectionStatus(result);
            }
            catch (Exception) {
                OnPinError();
            }
        }

        protected override void OnClosed()
        {
            StopPinTimer();
            base.OnClosed();
        }
    }
}
