using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Annotations;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
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
            protected set
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

        public abstract Task Install(IProgress<double> progress, CancellationToken cancellationToken);
    }

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
            InstallAction = InstallAction.Install;

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

    public class LavFiltersPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public LavFiltersPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "LAV Filters";
            IsOptional = false;
            IsInstalled = mediaFilters.IsLavSplitterInstalled() && mediaFilters.IsLavVideoInstalled() && mediaFilters.IsLavAudioInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallLavFilters(progress, cancellationToken).ConfigureAwait(false);
            IsInstalled = true;
        }
    }
    
    public class XyVsFilterPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public XyVsFilterPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "xy-VSFilter";
            IsOptional = false;
            IsInstalled = mediaFilters.IsXyVsFilterInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallXyVsFilter(progress, cancellationToken).ConfigureAwait(false);
            IsInstalled = true;
        }
    }

    public class XySubFilterPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public XySubFilterPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "XySubFilter";
            IsOptional = true;
            IsInstalled = mediaFilters.IsXySubFilterInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallXySubFilter(progress, cancellationToken).ConfigureAwait(false);
            IsInstalled = true;
        }
    }

    public class MadVrPrerequisite
        : Prerequisite
    {
        public MadVrPrerequisite(IMediaFilters mediaFilters)
        {
            Name = "MadVR";
            IsOptional = true;
            IsInstalled = mediaFilters.IsMadVrInstalled();
            RequiresManualInstallation = true;
            DownloadUrl = "http://forum.doom9.org/showthread.php?t=146228";
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class ReClockPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public ReClockPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "ReClock";
            IsOptional = true;
            IsInstalled = mediaFilters.IsReClockInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallReClock(progress, cancellationToken).ConfigureAwait(false);
            IsInstalled = true;
        }
    }

    public class PrerequisitesViewModel
        : BaseWizardPage
    {
        public List<PrerequisiteViewModel> Prerequisites { get; private set; }

        public PrerequisitesViewModel(IMediaFilters mediaFilters)
        {
            var prerequisites = new List<Prerequisite> {
                new LavFiltersPrerequisite(mediaFilters),
                new XyVsFilterPrerequisite(mediaFilters),
                new XySubFilterPrerequisite(mediaFilters),
                new MadVrPrerequisite(mediaFilters),
                new ReClockPrerequisite(mediaFilters)
            };

            Prerequisites = prerequisites.Select(p => new PrerequisiteViewModel(p)).ToList();

            foreach (var prerequisite in Prerequisites) {
                prerequisite.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == "WillBeInstalled") {
                        OnPropertyChanged("HasCustomNextPage");
                    }
                };
            }
        }

        public override bool HasCustomNextPage
        {
            get { return Prerequisites.Any(p => p.WillBeInstalled); }
        }

        public override IWizardPage Next()
        {
            return new PrerequisitesInstallationViewModel(Prerequisites.Where(p => p.WillBeInstalled).Select(p => p.Prerequisite));
        }
    }
}