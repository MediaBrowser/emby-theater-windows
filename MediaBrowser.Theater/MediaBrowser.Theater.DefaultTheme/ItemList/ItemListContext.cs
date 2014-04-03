using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels;

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
        private readonly IServerEvents _serverEvents;

        private ItemListViewModel _viewModel;

        public ItemListContext(IApplicationHost appHost, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, IPresenter presenter, ISessionManager sessionManager) : base(appHost)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _presenter = presenter;
            _sessionManager = sessionManager;
        }

        public ItemListParameters Parameters { get; set; }

        public override async Task Activate()
        {
            if (_viewModel == null || !_viewModel.IsActive) {
                _viewModel = new ItemListViewModel(Parameters.Items, Parameters.Title, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager);
            }

            await _presenter.ShowPage(_viewModel);
        }
    }
}