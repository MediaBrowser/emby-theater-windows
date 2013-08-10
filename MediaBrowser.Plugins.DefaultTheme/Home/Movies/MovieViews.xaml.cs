using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home.Movies
{
    /// <summary>
    /// Interaction logic for MovieViews.xaml
    /// </summary>
    public partial class MovieViews : UserControl
    {
        private readonly IApiClient _apiClient;

        private readonly IImageManager _imageManager;
        private readonly ISessionManager _session;

        private readonly Model.Entities.DisplayPreferences _displayPreferences;

        public MovieViews(Model.Entities.DisplayPreferences displayPreferences, IApiClient apiClient, IImageManager imageManager, ISessionManager session)
        {
            _displayPreferences = displayPreferences;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _session = session;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var itemCounts = await _apiClient.GetItemCountsAsync(_session.CurrentUser.Id);

            ReloadAllMovies(itemCounts);
            ReloadTrailers(itemCounts);
            ReloadBoxsets(itemCounts);
        }

        private async void ReloadAllMovies(ItemCounts itemCounts)
        {
            if (itemCounts.MovieCount == 0)
            {
                BtnAllMovies.Visibility = Visibility.Collapsed;
                return;
            }

            BtnAllMovies.Visibility = Visibility.Visible;
            
            AllMoviesTile.ImageHeight = _displayPreferences.PrimaryImageHeight;
            AllMoviesTile.ImageWidth = _displayPreferences.PrimaryImageWidth;
            AllMoviesTile.FixedTitle = "Movies";

            var result = await _apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                Recursive = true,
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                Limit = 10,
                UserId = _session.CurrentUser.Id,
                SortBy = new[] { "Random" }
            });

            AllMoviesTile.DataContext = new RotatingCollectionViewModel(_apiClient, _imageManager)
            {
                Items = result.Items
            };
        }

        private async void ReloadBoxsets(ItemCounts itemCounts)
        {
            if (itemCounts.BoxSetCount == 0)
            {
                BtnBoxsets.Visibility = Visibility.Collapsed;
                return;
            }
            
            BtnBoxsets.Visibility = Visibility.Visible;

            BoxsetsTile.ImageHeight = _displayPreferences.PrimaryImageHeight;
            BoxsetsTile.ImageWidth = _displayPreferences.PrimaryImageWidth;
            BoxsetsTile.FixedTitle = "Box sets";

            var result = await _apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Boxset" },
                Recursive = true,
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                Limit = 10,
                UserId = _session.CurrentUser.Id,
                SortBy = new[] { "Random" }
            });

            BoxsetsTile.DataContext = new RotatingCollectionViewModel(_apiClient, _imageManager)
            {
                Items = result.Items
            };
        }

        private async void ReloadTrailers(ItemCounts itemCounts)
        {
            if (itemCounts.TrailerCount == 0)
            {
                BtnTrailers.Visibility = Visibility.Collapsed;
                return;
            }

            BtnTrailers.Visibility = Visibility.Visible;

            TrailersTile.ImageHeight = _displayPreferences.PrimaryImageHeight;
            TrailersTile.ImageWidth = _displayPreferences.PrimaryImageWidth;
            TrailersTile.FixedTitle = "Trailers";

            var result = await _apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Trailer" },
                Recursive = true,
                ImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                Limit = 10,
                UserId = _session.CurrentUser.Id,
                SortBy = new[] { "Random" }
            });

            TrailersTile.DataContext = new RotatingCollectionViewModel(_apiClient, _imageManager)
            {
                Items = result.Items
            };
        }
    }
}
