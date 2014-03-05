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
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class ResumeEpisodesViewModel
       : BaseViewModel, IPanoramaPage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;

        private bool _isVisible;

        public ResumeEpisodesViewModel(Task<TvView> tvViewTask, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;

            Episodes = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 3; i++) {
                Episodes.Add(CreateEpisodeItem());
            }

            IsVisible = false;
            LoadItems(tvViewTask);
        }

        public RangeObservableCollection<ItemTileViewModel> Episodes { get; private set; }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_ResumeEpisodes_Title"; }
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

        private async void LoadItems(Task<TvView> tvViewTask)
        {
            TvView tvView = await tvViewTask;
            BaseItemDto[] items = tvView.ResumableEpisodes.ToArray();

            for (int i = 0; i < items.Length; i++) {
                if (Episodes.Count > i) {
                    Episodes[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateEpisodeItem();
                    vm.Item = items[i];

                    Episodes.Add(vm);
                }
            }

            if (Episodes.Count > items.Length) {
                List<ItemTileViewModel> toRemove = Episodes.Skip(items.Length).ToList();
                Episodes.RemoveRange(toRemove);
            }

            IsVisible = Episodes.Count > 0;
        }

        private ItemTileViewModel CreateEpisodeItem()
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, /*_playbackManager,*/ null) {
                ImageWidth = HomeViewModel.TileWidth,
                ImageHeight = HomeViewModel.TileHeight,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Screenshot, ImageType.Thumb, ImageType.Backdrop },
                DisplayNameGenerator = TvSpotlightViewModel.GetDisplayName,
                DownloadImagesAtExactSize = true
            };
        }
    }
}
