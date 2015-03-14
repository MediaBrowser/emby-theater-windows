using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Properties;
using MediaBrowser.Theater.Mpdn;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public abstract class Prerequisite
        : INotifyPropertyChanged
    {
        private string _name;
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
        
        public abstract Task<IUpdate> GetInstaller();

        public abstract bool CheckInstallStatus();
    }
}