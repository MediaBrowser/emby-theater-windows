using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.Session;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MediaBrowser.Theater.Interfaces.ViewModels;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class AppListViewModel : BaseViewModel
    {
        public IPresentationManager PresentationManager { get; private set; }
        public ISessionManager SessionManager { get; private set; }
        public ICommand LaunchCommand { get; private set; }
        
        public ILogger Logger { get; private set; }

        private readonly RangeObservableCollection<AppViewModel> _listItems =
            new RangeObservableCollection<AppViewModel>();

        public AppListViewModel(IPresentationManager presentationManager, ISessionManager sessionManager, ILogger logger)
        {
            Logger = logger;
            SessionManager = sessionManager;
            PresentationManager = presentationManager;

            LaunchCommand = new RelayCommand(Launch);
            
            Apps = new ListCollectionView(_listItems);

            ReloadApps();
        }

        private ListCollectionView _apps;
        public ListCollectionView Apps
        {
            get { return _apps; }

            set
            {
                var changed = _apps != value;
                _apps = value;

                if (changed)
                {
                    OnPropertyChanged("Apps");
                }
            }
        }

        private void ReloadApps()
        {
            _listItems.AddRange(PresentationManager.GetApps(SessionManager.CurrentUser)
                .Select(i => new AppViewModel(PresentationManager, Logger) { App = i }));
        }

        public void Launch(object commandParameter)
        {
            var current = commandParameter as AppViewModel;

            if (current != null)
            {
                current.Launch();
            }
        }
    }
}
