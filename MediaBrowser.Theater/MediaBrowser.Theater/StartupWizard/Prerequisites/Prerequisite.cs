using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Annotations;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public abstract class Prerequisite
        : INotifyPropertyChanged
    {
        private string _name;
        private bool _isInstalled;
        private bool _isOptional;
        private bool _requiresManualInstallation;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Name
        {
            get { return _name; }
            protected set
            {
                if (value == _name) {
                    return;
                }
                _name = value;
                OnPropertyChanged();
            }
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
            }
        }

        public bool IsOptional
        {
            get { return _isOptional; }
            protected set
            {
                if (value.Equals(_isOptional)) {
                    return;
                }
                _isOptional = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresManualInstallation
        {
            get { return _requiresManualInstallation; }
            protected set
            {
                if (value.Equals(_requiresManualInstallation)) {
                    return;
                }
                _requiresManualInstallation = value;
                OnPropertyChanged();
            }
        }

        public string DownloadUrl { get; protected set; }

        public virtual Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            UpdateInstallStatus();

            if (!IsInstalled) {
                throw new Exception("Failed to install " + Name);
            }

            return Task.FromResult(true);
        }

        protected abstract bool CheckInstallStatus();

        public void UpdateInstallStatus()
        {
            IsInstalled = CheckInstallStatus();
        }
    }
}