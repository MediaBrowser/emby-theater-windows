using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public class PrerequisiteInstallationViewModel
        : BaseViewModel
    {
        private readonly Prerequisite _prerequisite;
        private bool _isInstalling;
        private double _installationProgress;
        private bool _installationFailed;

        public PrerequisiteInstallationViewModel(Prerequisite prerequisite)
        {
            _prerequisite = prerequisite;

            _prerequisite.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "Name") {
                    OnPropertyChanged("Name");
                }

                if (args.PropertyName == "IsInstalled") {
                    OnPropertyChanged("IsInstalled");
                }
            };
        }

        public string Name
        {
            get { return _prerequisite.Name; }
        }

        public string DownloadUrl
        {
            get { return _prerequisite.DownloadUrl; }
        }

        public bool RequiresManualInstallation
        {
            get { return _prerequisite.RequiresManualInstallation; }
        }

        public bool IsInstalled
        {
            get { return _prerequisite.IsInstalled; }
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
                await _prerequisite.Install(progress, new CancellationToken());
            }
            catch (Exception e) {
                InstallationFailed = true;
            }
            finally {
                IsInstalling = false;
            }
        }
    }

    public class PrerequisitesInstallationViewModel
        : BaseWizardPage
    {
        private readonly IEnumerable<Prerequisite> _prerequisites;

        public bool IsComplete
        {
            get { return _prerequisites.All(p => p.RequiresManualInstallation || p.IsInstalled); }
        }

        public PrerequisitesInstallationViewModel(IEnumerable<Prerequisite> prerequisites)
        {
            _prerequisites = prerequisites.ToList();
            Prerequisites = _prerequisites.Select(p => new PrerequisiteInstallationViewModel(p)).ToList();

            StartInstallation();
        }

        private async void StartInstallation()
        {
            var tasks = Prerequisites.Where(p => !p.RequiresManualInstallation).Select(p => Task.Run(async () => await p.Install()));
            await Task.WhenAll(tasks);

            OnPropertyChanged("IsComplete");
            OnPropertyChanged("HasErrors", false);
        }

        public IEnumerable<PrerequisiteInstallationViewModel> Prerequisites { get; private set; }

        public override async Task<bool> Validate()
        {
            if (!await base.Validate()) {
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