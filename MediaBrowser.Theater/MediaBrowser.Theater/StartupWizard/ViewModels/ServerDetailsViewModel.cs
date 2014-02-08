using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Configuration;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public enum ConnectionStatus
    {
        Ok,
        Checking,
        Failed
    }

    public class ServerDetailsViewModel
        : BaseWizardPage
    {
        private readonly AsyncLock _validationLock = new AsyncLock();

        private readonly IApiClient _apiClient;
        private readonly ITheaterConfigurationManager _config;
        private string _hostName;
        private int _port;
        private ConnectionStatus _status;
        private bool _isSearchingForServer;
        
        public ServerDetailsViewModel(ILogManager logManager, IApiClient apiClient, ITheaterConfigurationManager config)
        {
            _apiClient = apiClient;
            _config = config;

            IsSearchingForServer = true;
            HostName = _config.Configuration.ServerHostName ?? "127.0.0.1";
            Port = _config.Configuration.ServerApiPort;

            Task.Run(async () => {
                try {
                    var address = await new ServerLocator().FindServer(500, CancellationToken.None).ConfigureAwait(false);
                    var parts = address.ToString().Split(':');

                    HostName = parts[0];
                    Port = address.Port;
                }
                catch (Exception e) {
                    var log = logManager.GetLogger("SetupWizard.SeverDetails");
                    log.ErrorException("Failed to discover server", e);
                }
                finally {
                    IsSearchingForServer = false;
                }
            });
        }

        [Required]
        [StringLength(255)]
        public string HostName
        {
            get { return _hostName; }
            set
            {
                if (value == _hostName) {
                    return;
                }
                _hostName = value;
                OnPropertyChanged();
            }
        }

        [Range(0, ushort.MaxValue)]
        public int Port
        {
            get { return _port; }
            set
            {
                if (value == _port) {
                    return;
                }
                _port = value;
                OnPropertyChanged();
            }
        }

        public bool IsSearchingForServer
        {
            get { return _isSearchingForServer; }
            private set
            {
                if (value.Equals(_isSearchingForServer)) {
                    return;
                }
                _isSearchingForServer = value;
                OnPropertyChanged();
            }
        }

        public ConnectionStatus Status
        {
            get { return _status; }
            private set
            {
                if (value == _status) {
                    return;
                }
                _status = value;
                OnPropertyChanged("Status", false);
                OnPropertyChanged("FailedToFindServer", false);
            }
        }

        public bool FailedToFindServer
        {
            get { return Status == ConnectionStatus.Failed; }
        }

        public override async Task<bool> Validate()
        {
            Debug.WriteLine("Validating");

            using (await _validationLock.LockAsync()) {
                if (!await base.Validate().ConfigureAwait(false)) {
                    return false;
                }

                Status = ConnectionStatus.Checking;

                var url = string.Format("http://{0}:{1}/mediabrowser/system/info", HostName, Port);

                try {
                    using (var client = new HttpClient()) {
                        await client.GetStringAsync(url).ConfigureAwait(false);
                    }

                    _apiClient.ChangeServerLocation(HostName, Port);

                    _config.Configuration.ServerApiPort = Port;
                    _config.Configuration.ServerHostName = HostName;
                    _config.SaveConfiguration();

                    Status = ConnectionStatus.Ok;
                    return true;
                }
                catch (Exception) {
                    Status = ConnectionStatus.Failed;
                    return false;
                }
            }
        }
    }
}