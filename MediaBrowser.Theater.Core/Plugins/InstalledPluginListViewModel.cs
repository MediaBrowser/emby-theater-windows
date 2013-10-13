using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Core.Plugins
{
    public class InstalledPluginListViewModel : BaseViewModel
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;
        private readonly IApplicationHost _appHost;

        private readonly RangeObservableCollection<InstalledPluginViewModel> _listItems = new RangeObservableCollection<InstalledPluginViewModel>();
        public ICommand NavigateCommand { get; private set; }

        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    ReloadPlugins();
                }
                return _listCollectionView;
            }

            private set
            {
                var changed = _listCollectionView != value;
                _listCollectionView = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        public int PluginCount
        {
            get { return _listItems.Count; }
        }

        private void ReloadPlugins()
        {
            _listItems.Clear();

            _listItems.AddRange(_appHost.Plugins.Select(i => new InstalledPluginViewModel(_appHost, _nav, _installationManager, _presentationManager)
            {
                Plugin = i
            }));

            OnPropertyChanged("PluginCount");
        }

        public InstalledPluginListViewModel(IApplicationHost appHost, INavigationService nav, IInstallationManager installationManager, IPresentationManager presentationManager)
        {
            _appHost = appHost;
            _nav = nav;
            _installationManager = installationManager;
            _presentationManager = presentationManager;

            NavigateCommand = new RelayCommand(Navigate);
        }

        private async void Navigate(object commandParameter)
        {
            var item = commandParameter as InstalledPluginViewModel;

            if (item != null)
            {
                try
                {
                    await _nav.Navigate(new InstalledPluginPage(item, _presentationManager));
                }
                catch
                {
                    _presentationManager.ShowDefaultErrorMessage();
                }
            }
        }

    }
}
