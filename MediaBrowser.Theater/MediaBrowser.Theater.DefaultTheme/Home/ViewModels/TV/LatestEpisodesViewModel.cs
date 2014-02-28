using System.Collections.Generic;
using System.Linq;
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
    public class LatestEpisodesViewModel
        : BaseViewModel, IPanoramaPage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;

        public LatestEpisodesViewModel(Task<TvView> tvViewTask, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;

            Episodes = new RangeObservableCollection<ItemTileViewModel>();

            LoadItems(tvViewTask);
        }

        public RangeObservableCollection<ItemTileViewModel> Episodes { get; private set; }

        public string DisplayName
        {
            get { return "Latest Episodes"; }
        }

        public bool IsTitlePage
        {
            get { return false; }
        }

        private async void LoadItems(Task<TvView> tvViewTask)
        {
            TvView tvView = await tvViewTask;
            BaseItemDto[] items = tvView.LatestEpisodes.ToArray();

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