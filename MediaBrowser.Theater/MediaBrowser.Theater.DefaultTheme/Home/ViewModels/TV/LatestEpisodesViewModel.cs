using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class LatestEpisodesViewModel
        : BaseViewModel, IKnownSize, IHomePage
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;
        private readonly ItemTileFactory _itemFactory;

        private bool _isVisible;

        public LatestEpisodesViewModel(BaseItemDto tvFolder, IConnectionManager connectionManager, ISessionManager sessionManager, ItemTileFactory itemFactory)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _itemFactory = itemFactory;

            SectionTitle = tvFolder.Name;
            Title = "MediaBrowser.Theater.DefaultTheme:Strings:Home_LatestEpisodes_Title".Localize();

            Episodes = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 9; i++) {
                Episodes.Add(CreateEpisodeItem());
            }

            IsVisible = false;
            LoadItems(tvFolder);
        }

        public RangeObservableCollection<ItemTileViewModel> Episodes { get; private set; }

        public string Title { get; set; }

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

        public string SectionTitle { get; set; }

        public int Index { get; set; }

        public Size Size
        {
            get
            {
                if (Episodes.Count == 0) {
                    return new Size();
                }

                var width = (int) Math.Ceiling(Episodes.Count/3.0);

                return new Size(width*(HomeViewModel.TileWidth + 2*HomeViewModel.TileMargin) + HomeViewModel.SectionSpacing,
                                3*(HomeViewModel.TileHeight + 2*HomeViewModel.TileMargin));
            }
        }

        private async void LoadItems(BaseItemDto tvFolder)
        {
            var apiClient = _connectionManager.GetApiClient(tvFolder);

            var result = await apiClient.GetLatestItems(new LatestItemsQuery {
                UserId = _sessionManager.CurrentUser.Id,
                ParentId = tvFolder.Id,
                GroupItems = true,
                Limit = 9
            });

            for (int i = 0; i < result.Length; i++) {
                if (Episodes.Count > i) {
                    Episodes[i].Item = result[i];
                } else {
                    ItemTileViewModel vm = CreateEpisodeItem();
                    vm.Item = result[i];

                    Episodes.Add(vm);
                }
            }

            if (Episodes.Count > result.Length) {
                List<ItemTileViewModel> toRemove = Episodes.Skip(result.Length).ToList();
                Episodes.RemoveRange(toRemove);
            }

            IsVisible = Episodes.Count > 0;
            OnPropertyChanged("Size");
        }

        private ItemTileViewModel CreateEpisodeItem()
        {
            var vm = _itemFactory.Create(null);
            vm.DesiredImageWidth = HomeViewModel.TileWidth;
            vm.DesiredImageHeight = HomeViewModel.TileHeight;
            vm.PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Screenshot, ImageType.Thumb, ImageType.Primary };
            vm.DisplayNameGenerator = i => i.GetDisplayName(new DisplayNameFormat(true, true));
            return vm;
        }
    }
}