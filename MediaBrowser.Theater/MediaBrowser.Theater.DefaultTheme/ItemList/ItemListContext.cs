using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels;
using MediaBrowser.Theater.DefaultTheme.SortModeMenu;
using MediaBrowser.Theater.DefaultTheme.SortModeMenu.ViewModels;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.DefaultTheme.ItemList
{
    public class ItemListContext
        : NavigationContext
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IPresenter _presenter;
        private readonly IPlaybackManager _playbackManager;

        private ItemListViewModel _viewModel;

        public ItemListContext(IApplicationHost appHost, IConnectionManager connectionManager, IImageManager imageManager,
                               INavigator navigator, IPresenter presenter, IPlaybackManager playbackManager)
            : base(appHost)
        {
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _presenter = presenter;
            _playbackManager = playbackManager;

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
                _viewModel = new ItemListViewModel(Parameters, _connectionManager, _imageManager, _navigator, _playbackManager);
            }

            await _presenter.ShowPage(_viewModel);
        }
    }
}