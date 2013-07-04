using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
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

namespace MediaBrowser.Plugins.DefaultTheme.Home.Movies
{
    /// <summary>
    /// Interaction logic for MovieSpotlight.xaml
    /// </summary>
    public partial class MovieSpotlight : UserControl
    {
        public event EventHandler ContentLoaded;

        private IApiClient ApiClient { get; set; }
        private IImageManager ImageManager { get; set; }
        private INavigationService Navigation { get; set; }
        private IPresentationManager AppWindow { get; set; }

        public MovieSpotlight(IApiClient apiClient, IImageManager imageManager, INavigationService navigation, IPresentationManager appWindow)
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
                Navigation.NavigateToItem(currentItem, "movies");
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
                Navigation.NavigateToItem(currentItem, "movies");
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ReloadRecent();
        }

        private async void ReloadRecent()
        {
            var totalItems = 50;

            var result = await ApiClient.GetItemsAsync(new ItemQuery
            {
                UserId = ApiClient.CurrentUserId,
                IncludeItemTypes = new[] { "Movie" },
                Limit = totalItems,
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                Recursive = true,
                ImageTypes = new[] { ImageType.Backdrop },
                Filters = new[] { ItemFilter.IsUnplayed }
            });

            var size = Movies.GetImageSize();

            var recentTiles = new[] { Recent1, Recent2, Recent3, Recent4, Recent5, Recent6, Recent7, Recent8 };

            var items = result.Items.ToList();

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
                        ImageDisplayWidth = Convert.ToInt32(size.Width),
                        ImageDisplayHeight = Convert.ToInt32(size.Height),
                        MedianPrimaryImageAspectRatio = aspectRatio,
                        ImageType = ImageType.Backdrop
                    };
                }

                index++;
            }

            // Put the rest in the spotlight, in random order
            ReloadSpotlight(result.Items.Skip(index));

            if (ContentLoaded != null)
            {
                ContentLoaded(this, EventArgs.Empty);
            }
        }

        private void ReloadSpotlight(IEnumerable<BaseItemDto> items)
        {
            var size = TV.TV.GetImageSize();

            SpotlightTile.ImageHeight = Convert.ToInt32(size.Height * 2 + 14);
            SpotlightTile.ImageWidth = Convert.ToInt32(size.Width * 2 + 14);

            SpotlightTile.DataContext = new ItemCollectionViewModel(ApiClient, ImageManager)
            {
                Items = items.OrderBy(i => Guid.NewGuid()).ToArray()
            };
        }
    }
}
