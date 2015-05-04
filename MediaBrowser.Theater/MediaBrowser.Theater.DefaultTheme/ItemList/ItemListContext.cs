using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels;
using MediaBrowser.Theater.DefaultTheme.SortModeMenu;
using MediaBrowser.Theater.DefaultTheme.SortModeMenu.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList
{
    public class ItemListContext
        : NavigationContext
    {
        private readonly ItemTileFactory _itemFactory;
        private readonly IPresenter _presenter;

        private ItemListViewModel _viewModel;

        public ItemListContext(IApplicationHost appHost, IPresenter presenter, ItemTileFactory itemFactory)
            : base(appHost)
        {
            _presenter = presenter;
            _itemFactory = itemFactory;

            Binder.Bind<SortModeMenuPath>(async path => {
                var context = appHost.CreateInstance(typeof (SortModeMenuContext)) as SortModeMenuContext;
                context.Items = _viewModel;

                return context;
            });
        }

        public ItemListParameters Parameters { get; set; }

        public override async Task Activate()
        {
            if (_viewModel == null || !_viewModel.IsActive) {
                _viewModel = new ItemListViewModel(Parameters, _itemFactory);
            }

            await _presenter.ShowPage(_viewModel);
        }
    }
}