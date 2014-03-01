using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class LatestMoviesViewModel
        : BaseViewModel, IPanoramaPage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;

        public LatestMoviesViewModel(Task<MoviesView> moviesViewTask, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;

            Movies = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 8; i++) {
                Movies.Add(CreateMovieItem());
            }

            LoadItems(moviesViewTask);
        }

        public RangeObservableCollection<ItemTileViewModel> Movies { get; private set; }

        public string DisplayName
        {
            get { return "Latest Movies"; }
        }

        public bool IsTitlePage
        {
            get { return false; }
        }

        private async void LoadItems(Task<MoviesView> moviesViewTask)
        {
            MoviesView tvView = await moviesViewTask;
            BaseItemDto[] items = tvView.LatestMovies.ToArray();

            for (int i = 0; i < items.Length; i++) {
                if (Movies.Count > i) {
                    Movies[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateMovieItem();
                    vm.Item = items[i];

                    Movies.Add(vm);
                }
            }

            if (Movies.Count > items.Length) {
                List<ItemTileViewModel> toRemove = Movies.Skip(items.Length).ToList();
                Movies.RemoveRange(toRemove);
            }
        }

        private ItemTileViewModel CreateMovieItem()
        {
            const double posterHeight = (HomeViewModel.TileHeight*1.5) + HomeViewModel.TileMargin;
            const double posterWidth = posterHeight*2/3.0;

            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, /*_playbackManager,*/ null) {
                ImageWidth = posterWidth,
                ImageHeight = posterHeight,
                ShowDisplayName = false,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                DownloadImagesAtExactSize = true
            };
        }
    }
}
