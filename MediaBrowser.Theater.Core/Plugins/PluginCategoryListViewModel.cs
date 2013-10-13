using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace MediaBrowser.Theater.Core.Plugins
{
    public class PluginCategoryListViewModel : BaseViewModel
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;
        private readonly IApplicationHost _appHost;

        private readonly RangeObservableCollection<PluginCategoryViewModel> _listItems = new RangeObservableCollection<PluginCategoryViewModel>();

        private ListCollectionView _listCollectionView;

        public PluginCategoryListViewModel(IPresentationManager presentationManager, IInstallationManager installationManager, INavigationService nav, IApplicationHost appHost)
        {
            _presentationManager = presentationManager;
            _installationManager = installationManager;
            _nav = nav;
            _appHost = appHost;
        }

        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    ReloadList();
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

        private async void ReloadList()
        {
            try
            {
                var packages = await _installationManager.GetAvailablePackagesWithoutRegistrationInfo(CancellationToken.None);

                packages = packages.Where(i => i.versions != null && i.versions.Count > 0);

                _listItems.Clear();

                var categories = packages
                    .Where(i => i.type == PackageType.UserInstalled && i.targetSystem == PackageTargetSystem.MBTheater)
                    .OrderBy(i => string.IsNullOrEmpty(i.category) ? "General" : i.category)
                    .GroupBy(i => string.IsNullOrEmpty(i.category) ? "General" : i.category)
                    .ToList();

                _listItems.AddRange(categories.Select(i => new PluginCategoryViewModel(_presentationManager, _installationManager, _nav, _appHost)
                {
                    Name = i.Key,

                    Packages = i.Select(p => new PackageInfoViewModel(_installationManager, _nav)
                    {
                        PackageInfo = p

                    }).ToList()
                }));
            }
            catch (Exception)
            {
                _presentationManager.ShowDefaultErrorMessage();
            }
        }
    }
}
