using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Plugins.DefaultTheme
{
    public class DefaultThemePageContentViewModel : PageContentViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public DefaultThemePageContentViewModel(IApiClient apiClient, IImageManager imageManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
        }

        private bool _showLogoImage;
        public bool ShowLogoImage
        {
            get { return _showLogoImage; }

            set
            {
                var changed = _showLogoImage != value;

                _showLogoImage = value;

                if (changed)
                {
                    OnPropertyChanged("ShowLogoImage");
                }
            }
        }

        private BitmapImage _logoImage;
        public BitmapImage LogoImage
        {
            get { return _logoImage; }

            set
            {
                _logoImage = value;

                OnPropertyChanged("LogoImage");
            }
        }

        public async void SetPageTitle(BaseItemDto item)
        {
            if (item.HasLogo || !string.IsNullOrEmpty(item.ParentLogoItemId))
            {
                var url = _apiClient.GetLogoImageUrl(item, new ImageOptions
                {
                });

                try
                {
                    LogoImage = await _imageManager.GetRemoteBitmapAsync(url);

                    ShowLogoImage = true;
                    ShowDefaultPageTitle = false;
                }
                catch
                {
                    SetPageTitleText(item);
                }
            }
            else
            {
                SetPageTitleText(item);
            }
        }

        private void SetPageTitleText(BaseItemDto item)
        {
            var title = item.Name;

            if (item.IsType("Season"))
            {
                title = item.SeriesName + " | " + item.Name;
            }
            else if (item.IsType("Episode"))
            {
                title = item.SeriesName;

                if (item.ParentIndexNumber.HasValue)
                {
                    title += " | " + string.Format("Season {0}", item.ParentIndexNumber.Value.ToString());
                }
            }
            else if (item.IsType("MusicAlbum"))
            {
                if (!string.IsNullOrEmpty(item.AlbumArtist))
                {
                    title = item.AlbumArtist + " | " + title;
                }
            }

            PageTitle = title;
            ShowDefaultPageTitle = string.IsNullOrEmpty(PageTitle);
            ShowLogoImage = false;
        }

        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (string.Equals(name, "PageTitle"))
            {
                ShowLogoImage = false;
            }
            else if (string.Equals(name, "ShowDefaultPageTitle"))
            {
                if (ShowDefaultPageTitle)
                {
                    ShowLogoImage = false;
                }
            }
        }
    }
}
