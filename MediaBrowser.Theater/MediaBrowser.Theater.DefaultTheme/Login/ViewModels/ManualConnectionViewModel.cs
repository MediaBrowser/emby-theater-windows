using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class ManualConnectionViewModel
        : BaseViewModel
    {
        private readonly object _cancellationLock = new object();
        private readonly AsyncLock _connectionLock = new AsyncLock();
        private readonly IConnectionManager _connectionManager;
        private CancellationTokenSource _cancellationTokenSource;
        private ICommand _connectCommand;
        private volatile bool _connectionSuccessful;
        private bool _hasConnectionFailed;
        private bool _isConnecting;
        private string _serverHost;
        private string _serverPort;

        public ManualConnectionViewModel(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;

            ServerHost = "localhost";
            ServerPort = "8096";
            IsConnecting = false;
            HasConnectionFailed = false;
            ConnectCommand = new RelayCommand(arg => AttemptConnection());
        }

        public string ServerHost
        {
            get { return _serverHost; }
            set
            {
                if (value == _serverHost) {
                    return;
                }
                _serverHost = value;
                OnPropertyChanged();
            }
        }

        public string ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (value == _serverPort) {
                    return;
                }
                _serverPort = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand
        {
            get { return _connectCommand; }
            private set
            {
                if (Equals(value, _connectCommand)) {
                    return;
                }
                _connectCommand = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnecting
        {
            get { return _isConnecting; }
            private set
            {
                if (value.Equals(_isConnecting)) {
                    return;
                }
                _isConnecting = value;
                OnPropertyChanged();
            }
        }

        public bool HasConnectionFailed
        {
            get { return _hasConnectionFailed; }
            private set
            {
                if (value.Equals(_hasConnectionFailed)) {
                    return;
                }
                _hasConnectionFailed = value;
                OnPropertyChanged();
            }
        }

        private async Task AttemptConnection()
        {
            CancellationTokenSource tokenSource;

            lock (_cancellationLock) {
                if (_cancellationTokenSource != null) {
                    _cancellationTokenSource.Cancel();
                }

                _cancellationTokenSource = tokenSource = new CancellationTokenSource();
            }

            using (await _connectionLock.LockAsync()) {
                if (_connectionSuccessful || tokenSource.Token.IsCancellationRequested) {
                    return;
                }

                IsConnecting = true;
                HasConnectionFailed = false;

                try {
                    ConnectionResult result = await _connectionManager.Connect(string.Format("{0}:{1}", ServerHost, ServerPort), tokenSource.Token).ConfigureAwait(false);
                    tokenSource.Token.ThrowIfCancellationRequested();

                    if (result.State == ConnectionState.Unavailable) {
                        HasConnectionFailed = true;
                    } else {
                        _connectionSuccessful = true;
                    }
                }
                catch (TaskCanceledException) { }
                catch {
                    HasConnectionFailed = true;
                }
                finally {
                    IsConnecting = false;
                }
            }
        }
    }
}