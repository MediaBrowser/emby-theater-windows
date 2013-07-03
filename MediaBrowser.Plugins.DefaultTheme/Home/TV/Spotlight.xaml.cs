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
using System.Threading.Tasks;
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
        private IPresentationManager AppWindow { get; set; }

        public Spotlight(IApiClient apiClient, IImageManager imageManager, INavigationService navigation, IPresentationManager appWindow)
        {
            AppWindow = appWindow;
            Navigation = navigation;
            ImageManager = imageManager;

            ApiClient = apiClient;

            InitializeComponent();

            BtnSpotlight.Click += BtnSpotlight_Click;
            BtnRecent1.Click += BtnRecent1_Click;
            BtnRecent1.Click += BtnRecent1_Click;
            BtnRecent2.Click += BtnRecent1_Click;
            BtnRecent3.Click += BtnRecent1_Click;
            BtnRecent4.Click += BtnRecent1_Click;
            BtnRecent5.Click += BtnRecent1_Click;
            BtnRecent6.Click += BtnRecent1_Click;
            BtnRecent7.Click += BtnRecent1_Click;
            BtnRecent8.Click += BtnRecent1_Click;

            BtnSpotlight.GotFocus += BtnSpotlight_GotFocus;
            BtnRecent1.GotFocus += BtnRecent1_GotFocus;
            BtnRecent1.GotFocus += BtnRecent1_GotFocus;
            BtnRecent2.GotFocus += BtnRecent1_GotFocus;
            BtnRecent3.GotFocus += BtnRecent1_GotFocus;
            BtnRecent4.GotFocus += BtnRecent1_GotFocus;
            BtnRecent5.GotFocus += BtnRecent1_GotFocus;
            BtnRecent6.GotFocus += BtnRecent1_GotFocus;
            BtnRecent7.GotFocus += BtnRecent1_GotFocus;
            BtnRecent8.GotFocus += BtnRecent1_GotFocus;
        }

        void BtnRecent1_GotFocus(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var tile = (HomePageTile)button.Content;

            var currentItem = tile.Item;

            if (currentItem != null)
            {
                AppWindow.SetBackdrops(currentItem);
            }
        }

        void BtnRecent1_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var tile = (HomePageTile)button.Content;

            var currentItem = tile.Item;

            if (currentItem != null)
            {
                Navigation.NavigateToItem(currentItem, "tv");
            }
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ReloadRecentEpisodes();
        }

        private async void ReloadRecentEpisodes()
        {
            var result = await ApiClient.GetItemsAsync(new ItemQuery
            {
                UserId = ApiClient.CurrentUserId,
                IncludeItemTypes = new[] { "Episode" },
                Limit = 50,
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                Recursive = true,
                ImageTypes = new[] { ImageType.Primary },
                Fields = new[] { ItemFields.SeriesInfo },
                Filters = new[] { ItemFilter.IsUnplayed }
            });

            var items = result.Items.ToList();

            var spotlightTask = ReloadSpotlight(items.Select(i => i.SeriesId).Distinct());

            var size = TV.GetImageSize();

            var recentTiles = new[] { Recent1, Recent2, Recent3, Recent4, Recent5, Recent6, Recent7, Recent8 };

            var index = 0;
            foreach (var tile in recentTiles)
            {
                if (items.Count < index)
                {
                    tile.Visibility = Visibility.Collapsed;
                }
                else
                {
                    double aspectRatio = 16;
                    aspectRatio /= 9;

                    tile.DataContext = new BaseItemDtoViewModel(ApiClient, ImageManager)
                    {
                        Item = items[index],
                        ImageWidth = Convert.ToInt32(size.Width),
                        MeanPrimaryImageAspectRatio = aspectRatio
                    };
                }

                index++;
            }

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

            var size = TV.GetImageSize();

            SpotlightTile.ImageHeight = Convert.ToInt32(size.Height * 2 + 14);
            SpotlightTile.ImageWidth = Convert.ToInt32(size.Width * 2 + 14);

            SpotlightTile.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = result.Items.ToArray()
            };
        }
    }
}
