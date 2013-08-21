using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for PanoramaDetailPage.xaml
    /// </summary>
    public partial class PanoramaDetailPage : BasePage, ISupportsItemThemeMedia, ISupportsItemBackdrops
    {
        private readonly ItemViewModel _itemViewModel;

        public PanoramaDetailPage(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, IPresentationManager presentation, IPlaybackManager playback)
        {
            InitializeComponent();

            Loaded += PanoramaDetailPage_Loaded;

            _itemViewModel = new ItemViewModel(apiClient, imageManager)
            {
                Item = item
            };

            DataContext = new DetailPageViewModel(_itemViewModel, apiClient, sessionManager, imageManager, presentation, playback);

            SetTitle(_itemViewModel.Item);
        }

        public string ThemeMediaItemId
        {
            get { return _itemViewModel.Item.Id; }
        }

        async void PanoramaDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            await PageTitlePanel.Current.SetPageTitle(_itemViewModel.Item);
        }

        private void SetTitle(BaseItemDto item)
        {
            if (item.Taglines.Count > 0)
            {
                TxtName.Text = item.Taglines[0];
                TxtName.Visibility = Visibility.Visible;
            }
            else if (item.IsType("episode"))
            {
                TxtName.Text = GetEpisodeTitle(item);
                TxtName.Visibility = Visibility.Visible;
            }
            else if (item.IsType("audio"))
            {
                TxtName.Text = GetSongTitle(item);
                TxtName.Visibility = Visibility.Visible;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
            }
        }

        internal static string GetEpisodeTitle(BaseItemDto item)
        {
            var title = item.Name;

            if (item.IndexNumber.HasValue)
            {
                title = "Ep. " + item.IndexNumber.Value + ": " + title;
            }

            if (item.ParentIndexNumber.HasValue)
            {
                title = "Season " + item.ParentIndexNumber.Value + ", " + title;
            }

            return title;
        }

        private string GetSongTitle(BaseItemDto item)
        {
            return item.Name;
        }

        public BaseItemDto BackdropItem
        {
            get { return _itemViewModel.Item; }
        }
    }
}
