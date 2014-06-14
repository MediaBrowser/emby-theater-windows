using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class ResumeEpisodesViewModel
       : BaseViewModel, IKnownSize, IHomePage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;

        private bool _isVisible;

        public ResumeEpisodesViewModel(Task<TvView> tvViewTask, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;

            Episodes = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 3; i++) {
                Episodes.Add(CreateEpisodeItem());
            }

            IsVisible = true;
            LoadItems(tvViewTask);
        }

        public string SectionTitle { get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_TVSectionTitle".Localize(); } }
        public int Index { get; set; }

        public RangeObservableCollection<ItemTileViewModel> Episodes { get; private set; }

        public string Title
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_ResumeEpisodes_Title".Localize(); }
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
                OnPropertyChanged("Size");
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
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _playbackManager, null) {
                DesiredImageWidth = HomeViewModel.TileWidth,
                DesiredImageHeight = HomeViewModel.TileHeight,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Screenshot, ImageType.Thumb, ImageType.Backdrop },
                DisplayNameGenerator = TvSpotlightViewModel.GetDisplayName
            };
        }

        public Size Size
        {
            get
            {
                if (Episodes.Count == 0) {
                    return new Size();
                }

                int width = (int)Math.Ceiling(Episodes.Count / 3.0);

                return new Size(width*(HomeViewModel.TileWidth + 2*HomeViewModel.TileMargin) + HomeViewModel.SectionSpacing,
                                3*(HomeViewModel.TileHeight + 2*HomeViewModel.TileMargin));
            }
        }
    }
}
