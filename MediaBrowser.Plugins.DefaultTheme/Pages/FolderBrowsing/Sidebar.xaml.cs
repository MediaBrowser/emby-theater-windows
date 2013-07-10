using System;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using System.Windows.Controls;
using System.Windows;
using MediaBrowser.Theater.Interfaces.Presentation;

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
            if (item.HasLogo)
            {
                LogoImage.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetLogoImageUrl(item, new ImageOptions
                {
                    MaxHeight = 120,
                    MaxWidth = 500
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

            TxtGenres.Visibility = item.Genres.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            TxtGenres.Text = string.Join(" / ", item.Genres.ToArray());

            TxtOverview.Text = item.Overview;
        }
    }
}
