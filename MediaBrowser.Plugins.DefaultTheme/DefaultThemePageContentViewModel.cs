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
                var changed = _logoImage != value;

                _logoImage = value;

                if (changed)
                {
                    OnPropertyChanged("LogoImage");
                }
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
            PageTitle = item.SeriesName ?? item.Album ?? item.Name;
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
