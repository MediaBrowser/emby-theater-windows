using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home.TV
{
    /// <summary>
    /// Interaction logic for Spotlight.xaml
    /// </summary>
    public partial class Spotlight : UserControl
    {
        public event EventHandler ContentLoaded;

        private IApiClient ApiClient { get; set; }
        private IImageManager ImageManager { get; set; }
        private INavigationService Navigation { get; set; }
        private IApplicationWindow AppWindow { get; set; }

        public Spotlight(IApiClient apiClient, IImageManager imageManager, INavigationService navigation, IApplicationWindow appWindow)
        {
            AppWindow = appWindow;
            Navigation = navigation;
            ImageManager = imageManager;

            ApiClient = apiClient;

            InitializeComponent();

            BtnSpotlight.Click += BtnSpotlight_Click;
            BtnRecent1.Click += BtnSpotlight_Click;
            BtnRecent2.Click += BtnSpotlight_Click;
            BtnRecent3.Click += BtnSpotlight_Click;
            BtnRecent4.Click += BtnSpotlight_Click;
            BtnRecent5.Click += BtnSpotlight_Click;
            BtnRecent6.Click += BtnSpotlight_Click;
            BtnRecent7.Click += BtnSpotlight_Click;
            BtnRecent8.Click += BtnSpotlight_Click;

            BtnSpotlight.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent1.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent2.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent3.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent4.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent5.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent6.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent7.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent8.GotFocus += BtnSpotlight_GotFocus;
        }

        void BtnSpotlight_GotFocus(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var tile = (MultiItemTile)button.Content;

            var currentItem = tile.CurrentItem;

            if (currentItem != null)
            {
                AppWindow.SetBackdrops(currentItem);
            }
        }

        void BtnSpotlight_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var tile = (MultiItemTile)button.Content;

            var currentItem = tile.CurrentItem;

            if (currentItem != null)
            {
                Navigation.NavigateToItem(currentItem, "tv");
            }
        }

        internal static ImageSize GetImageSize()
        {
            return new ImageSize
            {
                Width = 368,
                Height = 207
            };
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ReloadRecentEpisodes();
        }

        private async void ReloadRecentEpisodes()
        {
            var itemsPerTile = 5;

            var result = await ApiClient.GetItemsAsync(new ItemQuery
            {
                UserId = ApiClient.CurrentUserId,
                IncludeItemTypes = new[] { "Episode" },
                Limit = 8 * itemsPerTile,
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                Recursive = true,
                ImageTypes = new[] { ImageType.Primary },
                Fields = new[] { ItemFields.SeriesInfo },
                Filters = new[] { ItemFilter.IsUnplayed }
            });

            var items = result.Items.OrderBy(i => Guid.NewGuid()).ToList();

            var spotlightTask = ReloadSpotlight(items.Select(i => i.SeriesId).Distinct());

            var size = GetImageSize();

            RecentEpisodes1.ImageHeight = RecentEpisodes2.ImageHeight = RecentEpisodes3.ImageHeight = RecentEpisodes4.ImageHeight = RecentEpisodes5.ImageHeight = RecentEpisodes6.ImageHeight = RecentEpisodes7.ImageHeight = RecentEpisodes8.ImageHeight = Convert.ToInt32(size.Height);
            RecentEpisodes1.ImageWidth = RecentEpisodes2.ImageWidth = RecentEpisodes3.ImageWidth = RecentEpisodes4.ImageWidth = RecentEpisodes5.ImageWidth = RecentEpisodes6.ImageWidth = RecentEpisodes7.ImageWidth = RecentEpisodes8.ImageWidth = Convert.ToInt32(size.Width);

            RecentEpisodes1.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Take(itemsPerTile).ToArray()
            };

            RecentEpisodes2.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(itemsPerTile).Take(itemsPerTile).ToArray()
            };

            RecentEpisodes3.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(2 * itemsPerTile).Take(itemsPerTile).ToArray()
            };

            RecentEpisodes4.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(3 * itemsPerTile).Take(itemsPerTile).ToArray()
            };

            RecentEpisodes5.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(4 * itemsPerTile).Take(itemsPerTile).ToArray()
            };

            RecentEpisodes6.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(5 * itemsPerTile).Take(itemsPerTile).ToArray()
            };

            RecentEpisodes7.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(6 * itemsPerTile).Take(itemsPerTile).ToArray()
            };

            RecentEpisodes8.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.Skip(7 * itemsPerTile).Take(itemsPerTile).ToArray()
            };

            await spotlightTask;
            
            if (ContentLoaded != null)
            {
                ContentLoaded(this, EventArgs.Empty);
            }
        }

        private async Task ReloadSpotlight(IEnumerable<string> seriesIds)
        {
            var result = await ApiClient.GetItemsAsync(new ItemQuery
            {
                UserId = ApiClient.CurrentUserId,
                IncludeItemTypes = new[] { "Series" },
                Limit = 10,
                SortBy = new[] { ItemSortBy.Random },
                Ids = seriesIds.ToArray(),
                Recursive = true,
                ImageTypes = new[] { ImageType.Backdrop }
            });

            var size = GetImageSize();

            SpotlightTile.ImageHeight = Convert.ToInt32(size.Height * 2 + 8);
            SpotlightTile.ImageWidth = Convert.ToInt32(size.Width * 2 + 8);

            SpotlightTile.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = result.Items.ToArray()
            };
        }
    }
}
