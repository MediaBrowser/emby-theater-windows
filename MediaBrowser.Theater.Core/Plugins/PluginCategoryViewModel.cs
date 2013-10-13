using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace MediaBrowser.Theater.Core.Plugins
{
    public class PluginCategoryViewModel : BaseViewModel
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;
        private readonly IApplicationHost _appHost;

        public ICommand NavigateCommand { get; private set; }
        
        private string _displayName;
        public string Name
        {
            get { return _displayName; }

            set
            {
                var changed = !string.Equals(_displayName, value);

                _displayName = value;

                if (changed)
                {
                    OnPropertyChanged("Name");
                }
            }
        }

        private List<PackageInfoViewModel> _packages = new List<PackageInfoViewModel>();

        public PluginCategoryViewModel(IPresentationManager presentationManager, IInstallationManager installationManager, INavigationService nav, IApplicationHost appHost)
        {
            _presentationManager = presentationManager;
            _installationManager = installationManager;
            _nav = nav;
            _appHost = appHost;

            NavigateCommand = new RelayCommand(Navigate);
        }

        public List<PackageInfoViewModel> Packages
        {
            get { return _packages; }

            set
            {
                _packages = value;

                OnPropertyChanged("Packages");
            }
        }

        private async void Navigate(object commandParameter)
        {
            var item = commandParameter as PackageInfoViewModel;

            if (item != null)
            {
                try
                {
                    await _nav.Navigate(new PackageInfoPage(item.PackageInfo, _appHost, _installationManager, _nav));
                }
                catch
                {
                    _presentationManager.ShowDefaultErrorMessage();
                }
            }
        }
    }
}
