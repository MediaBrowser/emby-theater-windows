using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Theater.Mpdn;
using MediaBrowser.Theater.Presentation.ViewModels;
using MediaBrowser.Theater.StartupWizard.Prerequisites;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public enum InstallAction
    {
        Install,
        Update,
        DoNotInstall
    }

    public class PrerequisiteViewModel
        : BaseViewModel
    {
        private readonly Prerequisite _prerequisite;
        private InstallAction _installAction;
        private IUpdate _update;

        public Prerequisite Prerequisite
        {
            get { return _prerequisite; }
        }

        public IUpdate Update
        {
            get { return _update; }
            private set
            {
                if (Equals(value, _update)) {
                    return;
                }

                _update = value;
                OnPropertyChanged();
                OnPropertyChanged("InstallActions");
                OnPropertyChanged("IsSearching");
                OnPropertyChanged("IsInstalled");
                OnPropertyChanged("IsUpdateAvailable");
                OnPropertyChanged("IsNewInstallAvailable");
                OnPropertyChanged("IsOptional");
//                OnPropertyChanged("IsInstallOrUpdateAvailable");

                if (Update == null || Prerequisite.IsOptional) {
                    InstallAction = InstallAction.DoNotInstall;
                }
                if (Update != null && !Prerequisite.IsOptional && Update.Type == UpdateType.NewRelease) {
                    InstallAction = InstallAction.Update;
                }
            }
        }

        public IEnumerable<InstallAction> InstallActions
        {
            get
            {
                if (Update == null) {
                    return new[] { InstallAction.DoNotInstall };
                }

                if (Update.Type == UpdateType.NewInstall || Update.Type == UpdateType.Unavailable) {
                    return new List<InstallAction> {
                        InstallAction.DoNotInstall,
                        InstallAction.Install
                    };
                }

                if (Update.Type == UpdateType.NewRelease) {
                    return new List<InstallAction> {
                        InstallAction.DoNotInstall,
                        InstallAction.Update
                    };
                }

                return new[] { InstallAction.DoNotInstall };
            }
        }

        public string Name
        {
            get { return _prerequisite.Name; }
        }

        public InstallAction InstallAction
        {
            get { return _installAction; }
            set
            {
                if (value == _installAction) {
                    return;
                }

                _installAction = value;
                OnPropertyChanged();
                OnPropertyChanged("WillBeInstalled");
            }
        }

        public bool IsOptional
        {
            get { return _prerequisite.IsOptional || (Update != null && Update.Type == UpdateType.NewRelease); }
        }

        public bool IsSearching
        {
            get { return Update == null; }
        }

        public bool IsInstalled
        {
            get { return Update.Type == UpdateType.UpToDate; }
        }

        public bool IsUpdateAvailable
        {
            get { return Update.Type == UpdateType.NewRelease; }
        }

        public bool IsNewInstallAvailable
        {
            get { return Update.Type == UpdateType.NewInstall; }
        }

//        public bool IsInstallOrUpdateAvailable
//        {
//            get { return IsUpdateAvailable || IsNewInstallAvailable || ; }
//        }

        public bool WillBeInstalled
        {
            get { return !IsInstalled && (!IsOptional || InstallAction != InstallAction.DoNotInstall); }
        }
        
        public PrerequisiteViewModel(Prerequisite prerequisite)
        {
            _prerequisite = prerequisite;
            InstallAction = prerequisite.IsOptional ? InstallAction.DoNotInstall : InstallAction.Install;
        }

        public async Task FindUpdate()
        {
            Update = null;
            Update = await _prerequisite.GetInstaller().ConfigureAwait(false);
        }
    }
}