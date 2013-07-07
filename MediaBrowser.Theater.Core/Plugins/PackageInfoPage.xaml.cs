using MediaBrowser.Common;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Linq;
using System.Net.Cache;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for PackageInfoPage.xaml
    /// </summary>
    public partial class PackageInfoPage : BasePage
    {
        private readonly PackageInfo _packageInfo;
        private readonly IApplicationHost _appHost;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;

        public PackageInfoPage(PackageInfo packageInfo, IApplicationHost appHost, IInstallationManager installationManager, INavigationService nav)
        {
            _packageInfo = packageInfo;
            _appHost = appHost;
            _installationManager = installationManager;
            _nav = nav;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            BtnInstall.Click += BtnInstall_Click;

            TxtName.Text = _packageInfo.name;

            TxtTagline.Text = _packageInfo.shortDescription ?? string.Empty;

            TxtTagline.Visibility = string.IsNullOrEmpty(_packageInfo.shortDescription)
                                        ? Visibility.Collapsed
                                        : Visibility.Visible;

            TxtDescription.Text = (_packageInfo.overview ?? string.Empty).StripHtml();

            SelectVersion.SelectedItemChanged += SelectVersion_SelectedItemChanged;

            TxtDeveloper.Text = string.IsNullOrEmpty(_packageInfo.owner) ? string.Empty : "Developer: " + _packageInfo.owner;

            LoadVersions();
            LoadImage();
        }

        async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            var version = _packageInfo.versions.First(i => string.Equals(i.versionStr, SelectVersion.SelectedValue));

            _installationManager.InstallPackage(version, new Progress<double>(), CancellationToken.None);

            await _nav.NavigateToSettingsPage();

            _nav.RemovePagesFromHistory(3);
        }

        private void LoadVersions()
        {
            SelectVersion.Options = _packageInfo.versions.OrderBy(i => i.version)
                .Select(i => new SelectListItem
            {
                Text = i.versionStr + " " + i.classification.ToString(),
                Value = i.versionStr

            }).ToList();

            SetInitialSelectedValue();

            SelectVersion_SelectedItemChanged(null, EventArgs.Empty);
        }

        private void SetInitialSelectedValue()
        {
            var updateLevel = PackageVersionClass.Release;

            var installedPlugin = _appHost.Plugins.FirstOrDefault(i => string.Equals(i.Name, _packageInfo.name, StringComparison.OrdinalIgnoreCase));

            if (installedPlugin != null)
            {
                updateLevel = installedPlugin.Configuration.UpdateClass;
            }

            var versions = _packageInfo.versions.OrderByDescending(i => i.version).ToList();

            var version = versions.FirstOrDefault(i => i.classification <= updateLevel) ?? versions.FirstOrDefault();

            if (version != null)
            {
                SelectVersion.SelectedValue = version.versionStr;
            }
        }

        void SelectVersion_SelectedItemChanged(object sender, EventArgs e)
        {
            var value = SelectVersion.SelectedValue;

            var version = _packageInfo.versions.FirstOrDefault(i => string.Equals(i.versionStr, value));

            if (version == null || string.IsNullOrEmpty(version.description))
            {
                PnlReleaseNotes.Visibility = Visibility.Collapsed;
            }
            else
            {
                PnlReleaseNotes.Visibility = Visibility.Visible;
                TxtReleaseNotes.Text = version.description;
            }
        }

        private void LoadImage()
        {
            var imageUrl = _packageInfo.thumbImage;

            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = _packageInfo.previewImage;
            }

            if (!string.IsNullOrEmpty(imageUrl))
            {
                var bitmap = new BitmapImage
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    CacheOption = BitmapCacheOption.OnDemand,
                    UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
                };

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl);
                bitmap.EndInit();

                RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.Fant);
                PackageImage.Source = bitmap;
            }
        }
    }
}
