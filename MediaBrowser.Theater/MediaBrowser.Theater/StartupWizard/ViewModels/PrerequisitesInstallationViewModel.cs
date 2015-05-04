using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;
using MediaBrowser.Theater.StartupWizard.Prerequisites;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public class PrerequisiteInstallationViewModel
        : BaseViewModel
    {
        private readonly PrerequisiteViewModel _prerequisite;
        private readonly IHttpClient _httpClient;
        private bool _isInstalling;
        private double _installationProgress;
        private bool _installationFailed;
        private bool _isInstalled;

        public PrerequisiteInstallationViewModel(PrerequisiteViewModel prerequisite, IHttpClient httpClient)
        {
            _prerequisite = prerequisite;
            _httpClient = httpClient;
        }

        public string Name
        {
            get { return _prerequisite.Name; }
        }

        public string DownloadUrl
        {
            get { return _prerequisite.Prerequisite.DownloadUrl; }
        }

        public bool RequiresManualInstallation
        {
            get { return _prerequisite.Prerequisite.RequiresManualInstallation; }
        }

        public bool IsInstalled
        {
            get { return _isInstalled; }
            private set
            {
                if (value.Equals(_isInstalled)) {
                    return;
                }

                _isInstalled = value;
                OnPropertyChanged();

                if (_isInstalled) {
                    InstallationFailed = false;
                }
            }
        }

        public bool IsInstalling
        {
            get { return _isInstalling; }
            private set
            {
                if (value.Equals(_isInstalling)) {
                    return;
                }
                _isInstalling = value;
                OnPropertyChanged();
            }
        }

        public double InstallationProgress
        {
            get { return _installationProgress; }
            private set
            {
                if (value == _installationProgress) {
                    return;
                }
                _installationProgress = value;
                OnPropertyChanged();
            }
        }

        public bool InstallationFailed
        {
            get { return _installationFailed; }
            private set
            {
                if (value.Equals(_installationFailed)) {
                    return;
                }
                _installationFailed = value;
                OnPropertyChanged();
            }
        }

        public async Task Install()
        {
            if (RequiresManualInstallation) {
                return;
            }

            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, value) => InstallationProgress = value;

            try {
                IsInstalling = true;
                await _prerequisite.Update.Install(progress, _httpClient).ConfigureAwait(false);

                UpdateInstallStatus();
                if (!IsInstalled) {
                    InstallationFailed = true;
                }
            }
            catch (Exception) {
                InstallationFailed = true;
            }
            finally {
                IsInstalling = false;
            }
        }

        public void UpdateInstallStatus()
        {
            IsInstalled = _prerequisite.Prerequisite.CheckInstallStatus();
        }
    }

    public class PrerequisitesInstallationViewModel
        : BaseWizardPage
    {
        public bool IsComplete
        {
            get { return Prerequisites.All(p => p.IsInstalled); }
        }

        public PrerequisitesInstallationViewModel(IEnumerable<PrerequisiteViewModel> prerequisites, IHttpClient httpClient)
        {
            Prerequisites = prerequisites.Select(p => new PrerequisiteInstallationViewModel(p, httpClient)).ToList();
            StartInstallation();
        }

        private async void StartInstallation()
        {
            foreach (var item in Prerequisites.Where(p => !p.RequiresManualInstallation)) {
                await item.Install().ConfigureAwait(false);
            }

            if (!IsComplete) {
                // keep checking install status to confirm manual installation
                Task.Run(async () => {
                    while (!IsComplete) {
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

                        foreach (var item in Prerequisites.Where(p => !p.IsInstalled)) {
                            item.UpdateInstallStatus();
                        }
                    }

                    OnPropertyChanged("IsComplete");
                    OnPropertyChanged("HasErrors", false);
                });
            }

            OnPropertyChanged("IsComplete");
            OnPropertyChanged("HasErrors", false);
        }

        public IEnumerable<PrerequisiteInstallationViewModel> Prerequisites { get; private set; }

        public override async Task<bool> Validate()
        {
            if (!await base.Validate().ConfigureAwait(false)) {
                return false;
            }

            OnPropertyChanged("HasErrors", false);
            return IsComplete;
        }

        public override bool HasErrors
        {
            get { return !IsComplete || base.HasErrors; }
        }
    }
}