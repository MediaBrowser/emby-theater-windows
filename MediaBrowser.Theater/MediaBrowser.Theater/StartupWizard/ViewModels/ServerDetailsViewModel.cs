using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
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

        private readonly IConnectionManager _connectionManager;
        private ConnectionStatus _status;
        private bool _isSearchingForServer;
        private string _address;

        private CancellationTokenSource _validationToken;

        public ServerDetailsViewModel(ILogManager logManager, IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;

            IsSearchingForServer = true;
            Address = "http://localhost:8096";

            Task.Run(async () => {
                try {
                    var info = (await new ServerLocator().FindServers(500, CancellationToken.None).ConfigureAwait(false)).First();
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

            if (_validationToken != null) {
                _validationToken.Cancel();
            }
            
            using (await _validationLock.LockAsync()) {
                if (!await base.Validate().ConfigureAwait(false)) {
                    return false;
                }

                _validationToken = new CancellationTokenSource();

                Status = ConnectionStatus.Checking;
                
                try {
                    var result = await _connectionManager.Connect(Address, _validationToken.Token);
                    if (result.State == ConnectionState.Unavailable) {
                        Status = ConnectionStatus.Failed;
                        return false;
                    }

                    Status = ConnectionStatus.Ok;
                    return true;

                }
                catch (TaskCanceledException) {
                    Status = ConnectionStatus.Checking;
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