using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Theater.Api.Theming.ViewModels;
using MediaBrowser.Theater.StartupWizard.Prerequisites;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public enum InstallAction
    {
        Install,
        DoNotInstall
    }

    public class PrerequisiteViewModel
        : BaseViewModel
    {
        private readonly Prerequisite _prerequisite;
        private InstallAction _installAction;

        public Prerequisite Prerequisite
        {
            get { return _prerequisite; }
        }

        public IEnumerable<InstallAction> InstallActions
        {
            get { return Enum.GetValues(typeof (InstallAction)).Cast<InstallAction>(); }
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
            get { return _prerequisite.IsOptional; }
        }

        public bool IsRequired
        {
            get { return !IsOptional; }
        }

        public bool IsInstalled
        {
            get { return _prerequisite.IsInstalled; }
        }

        public bool IsNotInstalled
        {
            get { return !IsInstalled; }
        }

        public bool WillBeInstalled
        {
            get { return !IsInstalled && (!IsOptional || InstallAction == InstallAction.Install); }
        }
        
        public PrerequisiteViewModel(Prerequisite prerequisite)
        {
            _prerequisite = prerequisite;
            InstallAction = prerequisite.IsOptional ? InstallAction.DoNotInstall : InstallAction.Install;

            _prerequisite.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "IsInstalled") {
                    OnPropertyChanged("IsInstalled");
                    OnPropertyChanged("IsNotInstalled");
                    OnPropertyChanged("WillBeInstalled");
                }

                if (args.PropertyName == "IsOptional") {
                    OnPropertyChanged("IsOptional");
                    OnPropertyChanged("IsRequired");
                    OnPropertyChanged("WillBeInstalled");
                }
            };
        }
    }
}