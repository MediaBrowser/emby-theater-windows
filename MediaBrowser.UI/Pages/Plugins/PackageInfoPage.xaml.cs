using MediaBrowser.Common.Extensions;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Linq;
using System.Net.Cache;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for PackageInfoPage.xaml
    /// </summary>
    public partial class PackageInfoPage : BasePage
    {
        private readonly PackageInfo _packageInfo;

        public PackageInfoPage(PackageInfo packageInfo)
        {
            _packageInfo = packageInfo;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            TxtName.Text = _packageInfo.name;

            TxtTagline.Text = _packageInfo.shortDescription ?? string.Empty;

            TxtTagline.Visibility = string.IsNullOrEmpty(_packageInfo.shortDescription)
                                        ? Visibility.Collapsed
                                        : Visibility.Visible;

            TxtDescription.Text = (_packageInfo.overview ?? string.Empty).StripHtml();

            SelectVersion.SelectedItemChanged += SelectVersion_SelectedItemChanged;

            LoadVersions();
            LoadImage();
        }

        private void LoadVersions()
        {
            SelectVersion.Options = _packageInfo.versions.OrderByDescending(i => i.version)
                .Select(i => new SelectListItem
            {
                Text = i.versionStr + " " + i.classification.ToString(),
                Value = i.versionStr

            }).ToList();

            SelectVersion.SelectedValue = SelectVersion.Options[0].Value;

            SelectVersion_SelectedItemChanged(null, EventArgs.Empty);
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
