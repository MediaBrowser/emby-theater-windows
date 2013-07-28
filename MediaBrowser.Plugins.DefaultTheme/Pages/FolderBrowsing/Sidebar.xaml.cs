using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Pages.FolderBrowsing
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        private BaseItemDto _item;
        public BaseItemDto Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;

                if (value != null)
                {
                    OnItemChanged(value);
                }
            }
        }

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public Sidebar(IApiClient apiClient, IImageManager imageManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            InitializeComponent();
        }

        private async void OnItemChanged(BaseItemDto item)
        {
            UpdateLogo(item);

            if (item.HasLogo)
            {
                LogoImage.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetLogoImageUrl(item, new ImageOptions
                {
                }));

                LogoImage.Visibility = Visibility.Visible;
                TxtTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                LogoImage.Visibility = Visibility.Collapsed;
                TxtTitle.Text = item.Name;
                TxtTitle.Visibility = Visibility.Visible;
            }

            TxtGenres.Visibility = item.Genres.Count > 0 && !item.IsType("episode") && !item.IsType("season") ? Visibility.Visible : Visibility.Collapsed;

            TxtGenres.Text = string.Join(" / ", item.Genres.Take(3).ToArray());

            TxtOverview.Text = item.Overview;
        }

        private async void UpdateLogo(BaseItemDto item)
        {
            const int maxheight = 120;

            if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
            {
                ImgLogo.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetArtImageUrl(item, new ImageOptions
                {
                    Height = maxheight,
                    ImageType = ImageType.Art
                }));

                ImgLogo.Visibility = Visibility.Visible;
            }
            else
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
            }
        }

    }
}
