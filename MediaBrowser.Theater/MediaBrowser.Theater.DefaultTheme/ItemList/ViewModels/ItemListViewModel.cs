using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels
{
    public class ItemListViewModel
        : BaseViewModel, IRootPresentationOptions
    {
        public const double ItemHeight = 500;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;

        public ItemListViewModel(Task<ItemsResult> items, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            Items = new RangeObservableCollection<ItemTileViewModel>();
            LoadItems(items);
        }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public Func<object, object> IndexSelector
        {
            get
            {
                return item => {
                    var viewModel = (ItemTileViewModel) item;
                    return GetSortKey(viewModel);
                };
            }
        }

        private static object GetSortKey(ItemTileViewModel viewModel)
        {
            string name = viewModel.Item.SortName ?? viewModel.Item.Name;
            if (name.Length > 0) {
                return name.First().ToString(CultureInfo.CurrentUICulture).ToUpper(CultureInfo.CurrentUICulture);
            }
            return string.Empty;
        }

        private async void LoadItems(Task<ItemsResult> itemsTask)
        {
            ItemsResult result = await itemsTask;
            IEnumerable<ItemTileViewModel> viewModels = result.Items.Select(dto => new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, dto) {
                DesiredImageHeight = ItemHeight,
                DownloadImagesAtExactSize = true
            });

            Items.AddRange(viewModels.OrderBy(GetSortKey));
        }

        public bool ShowMediaBrowserLogo { get { return false; } }
        public bool ShowCommandBar { get { return true; } }
        public bool ShowClock { get { return true; } }
        public string Title { get { return "Browse Movies"; } }
    }
}