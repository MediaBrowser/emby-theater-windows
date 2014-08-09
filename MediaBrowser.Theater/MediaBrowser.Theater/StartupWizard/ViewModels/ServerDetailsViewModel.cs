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
        private string _address;

        public ServerDetailsViewModel(ILogManager logManager, IApiClient apiClient, ITheaterConfigurationManager config)
        {
            _apiClient = apiClient;
            _config = config;

            IsSearchingForServer = true;
            Address = "http://localhost:8096";

            Task.Run(async () => {
                try {
                    var info = await new ServerLocator().FindServer(500, CancellationToken.None).ConfigureAwait(false);
                    Address = info.Address;
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
        public string Address
        {
            get { return _address; }
            set
            {
                if (value == _address) {
                    return;
                }
                _address = value;
                OnPropertyChanged("Address");
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

                var url = string.Format("{0}/mediabrowser/system/info", Address);

                try {
                    using (var client = new HttpClient()) {
                        await client.GetStringAsync(url).ConfigureAwait(false);
                    }

                    _apiClient.ChangeServerLocation(Address);

                    _config.Configuration.ServerAddress = Address;
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