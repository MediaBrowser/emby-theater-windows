using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class ResumeEpisodesViewModel
       : BaseViewModel, IKnownSize, IHomePage
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;
        private readonly ItemTileFactory _itemFactory;

        private bool _isVisible;

        public ResumeEpisodesViewModel(BaseItemDto tvFolder, IConnectionManager connectionManager, ISessionManager sessionManager, ItemTileFactory itemFactory)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
            _itemFactory = itemFactory;

            Title = "MediaBrowser.Theater.DefaultTheme:Strings:Home_ResumeEpisodes_Title".Localize();
            SectionTitle = tvFolder.Name;

            Episodes = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 3; i++) {
                Episodes.Add(CreateEpisodeItem());
            }

            IsVisible = true;
            LoadItems(tvFolder);
        }

        public string SectionTitle { get; set; }
        public int Index { get; set; }

        public RangeObservableCollection<ItemTileViewModel> Episodes { get; private set; }

        public string Title { get; set; }

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

        private async void LoadItems(BaseItemDto tvFolder)
        {
            var apiClient = _connectionManager.GetApiClient(tvFolder);

            var result = await apiClient.GetItemsAsync(new ItemQuery {
                UserId = _sessionManager.CurrentUser.Id,
                ParentId = tvFolder.Id,
                IncludeItemTypes = new[] { "Episode" },
                Filters = new[] { ItemFilter.IsResumable },
                SortBy = new[] { ItemSortBy.DatePlayed, ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                Limit = 9,
                Recursive = true
            });

            var items = result.Items;

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
            OnPropertyChanged("Size");
        }

        private ItemTileViewModel CreateEpisodeItem()
        {
            var vm = _itemFactory.Create(null);
            vm.DesiredImageWidth = HomeViewModel.TileWidth;
            vm.DesiredImageHeight = HomeViewModel.TileHeight;
            vm.PreferredImageTypes = new[] { ImageType.Primary, ImageType.Screenshot, ImageType.Thumb, ImageType.Backdrop };
            vm.DisplayNameGenerator = i => i.GetDisplayName(new DisplayNameFormat(true, true));
            return vm;
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
