using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class LatestTrailersViewModel
        : BaseViewModel, IPanoramaPage, IKnownSize
    {
        const double PosterHeight = (HomeViewModel.TileHeight * 1.5) + HomeViewModel.TileMargin;
        const double PosterWidth = PosterHeight * 2 / 3.0;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;

        private bool _isVisible;

        public LatestTrailersViewModel(Task<MoviesView> moviesViewTask, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;

            Trailers = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 8; i++) {
                Trailers.Add(CreateMovieItem());
            }

            IsVisible = true;
            LoadItems(moviesViewTask);
        }

        public RangeObservableCollection<ItemTileViewModel> Trailers { get; private set; }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_LatestTrailers_Title".Localize(); }
        }

        public bool IsTitlePage
        {
            get { return false; }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (Equals(_isVisible, value))
                {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        private async void LoadItems(Task<MoviesView> moviesViewTask)
        {
            MoviesView moviesView = await moviesViewTask;
            BaseItemDto[] items = moviesView.LatestTrailers.ToArray();

            for (int i = 0; i < items.Length; i++) {
                if (Trailers.Count > i) {
                    Trailers[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateMovieItem();
                    vm.Item = items[i];

                    Trailers.Add(vm);
                }
            }

            if (Trailers.Count > items.Length) {
                List<ItemTileViewModel> toRemove = Trailers.Skip(items.Length).ToList();
                Trailers.RemoveRange(toRemove);
            }

            IsVisible = Trailers.Count > 0;
            OnPropertyChanged("Size");
        }

        private ItemTileViewModel CreateMovieItem()
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, /*_playbackManager,*/ null) {
                ImageWidth = PosterWidth,
                ImageHeight = PosterHeight,
                ShowDisplayName = false,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                DownloadImagesAtExactSize = true
            };
        }

        public Size Size
        {
            get
            {
                int width = (int)Math.Ceiling(Trailers.Count / 2.0);

                return new Size(width*(PosterWidth + 2*HomeViewModel.TileMargin) + HomeViewModel.SectionSpacing,
                                2*(PosterHeight + 2*HomeViewModel.TileMargin));
            }
        }
    }
}
