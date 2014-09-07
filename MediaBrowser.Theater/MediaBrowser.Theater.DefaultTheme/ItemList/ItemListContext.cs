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

namespace MediaBrowser.Theater.DefaultTheme.ItemList
{
    public class ItemListContext
        : NavigationContext
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IPresenter _presenter;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;

        private ItemListViewModel _viewModel;

        public ItemListContext(IApplicationHost appHost, IApiClient apiClient, IImageManager imageManager,
                               IServerEvents serverEvents, INavigator navigator, IPresenter presenter,
                               ISessionManager sessionManager, IPlaybackManager playbackManager) : base(appHost)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _presenter = presenter;
            _sessionManager = sessionManager;
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
                _viewModel = new ItemListViewModel(Parameters, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager, _playbackManager);
            }

            await _presenter.ShowPage(_viewModel);
        }
    }
}