using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Threading;
using System.Windows;

namespace MediaBrowser.Theater.Core.Plugins
{
    public class PackageInfoViewModel : BaseViewModel
    {
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;

        public PackageInfoViewModel(IInstallationManager installationManager, INavigationService nav)
        {
            _installationManager = installationManager;
            _nav = nav;
        }

        private PackageInfo _packageInfo;
        public PackageInfo PackageInfo
        {
            get { return _packageInfo; }
            set
            {
                _packageInfo = value;
                OnPropertyChanged("PackageInfo");
                OnPropertyChanged("Name");
                OnPropertyChanged("ThumbUri");
                OnPropertyChanged("DefaultImageVisibility");
                OnPropertyChanged("ThumbImageVisibility");
                OnPropertyChanged("PremiumImageVisibility");
                OnPropertyChanged("SupporterImageVisibility");
            }
        }

        public string Name
        {
            get { return _packageInfo == null ? null : _packageInfo.name; }
        }
        public string ShortDescription
        {
            get { return _packageInfo == null ? null : _packageInfo.shortDescription; }
        }
        public string Overview
        {
            get { return _packageInfo == null ? null : (_packageInfo.overview ?? string.Empty).StripHtml(); }
        }
        public string Developer
        {
            get { return _packageInfo == null ? null : _packageInfo.owner; }
        }
        public string ThumbUri
        {
            get { return _packageInfo == null ? null : _packageInfo.thumbImage; }
        }

        public Visibility DefaultImageVisibility
        {
            get { return _packageInfo == null ? Visibility.Visible : string.IsNullOrEmpty(_packageInfo.thumbImage) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility ThumbImageVisibility
        {
            get { return _packageInfo == null ? Visibility.Collapsed : string.IsNullOrEmpty(_packageInfo.thumbImage) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility PremiumImageVisibility
        {
            get { return _packageInfo == null ? Visibility.Collapsed : _packageInfo.isRegistered && !_packageInfo.isPremium ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility SupporterImageVisibility
        {
            get { return _packageInfo == null ? Visibility.Collapsed : _packageInfo.isPremium ? Visibility.Visible : Visibility.Collapsed; }
        }

        public async void Install(PackageVersionInfo version)
        {
            _installationManager.InstallPackage(version, new Progress<double>(), CancellationToken.None);

            await _nav.NavigateToSettingsPage();

            _nav.RemovePagesFromHistory(3);
        }
    }
}
