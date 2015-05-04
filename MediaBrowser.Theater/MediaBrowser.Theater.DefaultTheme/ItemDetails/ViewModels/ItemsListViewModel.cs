using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemsListViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly ItemTileFactory _itemFactory;
        private readonly ItemsResult _itemsResult;
        private readonly ImageType[] _preferredImageTypes;

        private bool _isVisible;

        public ItemsListViewModel(ItemsResult itemsResult, ItemTileFactory itemFactory)
        {
            _itemsResult = itemsResult;
            _itemFactory = itemFactory;

            string itemType = itemsResult.Items.Length > 0 ? itemsResult.Items.First().Type : null;
            if (itemType == "Episode") {
                _preferredImageTypes = new[] { ImageType.Screenshot, ImageType.Thumb, ImageType.Art, ImageType.Primary };
            } else {
                _preferredImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Art };
            }

            Title = SelectHeader(itemsResult.Items.Length > 0 ? itemsResult.Items.First().Type : null);
            Items = new RangeObservableCollection<ItemTileViewModel>();
            LoadItems();
        }

        public static double ItemHeight
        {
            get
            {
                const double available = 3*HomeViewModel.TileHeight + 6*HomeViewModel.TileMargin;
                return available/3 - 2*HomeViewModel.TileMargin;
            }
        }

        public double ListHeight
        {
            get { return 3*ItemHeight + 6*HomeViewModel.TileMargin; }
        }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

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

        public int SortOrder
        {
            get { return 2; }
        }

        public string Title { get; set; }

        public Size Size
        {
            get
            {
                if (Items.Count == 0) {
                    return new Size();
                }

                Size itemSize = Items.First().Size;

                return new Size(itemSize.Width + 2*HomeViewModel.TileMargin + 20, ListHeight + 20);
            }
        }

        private void LoadItems()
        {
            IEnumerable<ItemTileViewModel> items = _itemsResult.Items.Select(i => {
                ItemTileViewModel vm = _itemFactory.Create(i);
                vm.DesiredImageHeight = ItemHeight;
                vm.PreferredImageTypes = _preferredImageTypes;
                return vm;
            });

            Items.Clear();
            Items.AddRange(items);

            IsVisible = Items.Count > 0;
            OnPropertyChanged("Size");
        }

        internal static string SelectHeader(string itemType)
        {
            switch (itemType) {
                case "Series":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_SeriesHeader".Localize();
                case "Season":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_SeasonsHeader".Localize();
                case "Episode":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_EpisodesHeader".Localize();
                case "Artist":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_AlbumsHeader".Localize();
                case "Album":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_TracksHeader".Localize();
                case "Movie":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_MoviesHeader".Localize();
                default:
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_ItemsHeader".Localize();
            }
        }
    }
}