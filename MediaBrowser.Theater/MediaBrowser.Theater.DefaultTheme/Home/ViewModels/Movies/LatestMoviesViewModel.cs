using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class LatestMoviesViewModel
        : BaseViewModel, IKnownSize, IHomePage
    {
        private const double PosterHeight = (HomeViewModel.TileHeight*1.5) + HomeViewModel.TileMargin;
        private const double PosterWidth = PosterHeight*2/3.0;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;

        private bool _isVisible;

        public LatestMoviesViewModel(Task<MoviesView> moviesViewTask, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;

            Movies = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 8; i++) {
                Movies.Add(CreateMovieItem());
            }

            IsVisible = true;
            LoadItems(moviesViewTask);
        }

        public RangeObservableCollection<ItemTileViewModel> Movies { get; private set; }

        public string Title
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_LatestMovies_Title".Localize(); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (Equals(_isVisible, value)) {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public string SectionTitle
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_MoviesSectionTitle".Localize(); }
        }

        public Size Size
        {
            get
            {
                if (Movies.Count == 0) {
                    return new Size();
                }

                var width = (int) Math.Ceiling(Movies.Count/2.0);

                return new Size(width*(PosterWidth + 2*HomeViewModel.TileMargin) + HomeViewModel.SectionSpacing,
                                2*(PosterWidth + 2*HomeViewModel.TileMargin));
            }
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

            IsVisible = Movies.Count > 0;
        }

        private ItemTileViewModel CreateMovieItem()
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _playbackManager, null) {
                DesiredImageWidth = PosterWidth,
                DesiredImageHeight = PosterHeight,
                ShowDisplayName = false,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb }
            };
        }
    }
}