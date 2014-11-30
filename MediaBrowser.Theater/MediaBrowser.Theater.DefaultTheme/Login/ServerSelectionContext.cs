using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login
{
    public class ServerSelectionContext
        : NavigationContext
    {
        private readonly IPresenter _presenter;
        private readonly INavigator _navigator;
        private readonly IConnectionManager _connectionManager;

        private ServerSelectionViewModel _serverSelectionViewModel;

        public ServerSelectionContext(IApplicationHost appHost, IPresenter presenter, INavigator navigator, IConnectionManager connectionManager) : base(appHost)
        {
            _presenter = presenter;
            _navigator = navigator;
            _connectionManager = connectionManager;
        }

        public IEnumerable<ServerInfo> Servers { get; set; }

        public override async Task Activate()
        {
            if (_serverSelectionViewModel == null || !_serverSelectionViewModel.IsActive) {
                _serverSelectionViewModel = new ServerSelectionViewModel(_connectionManager, _navigator, Servers);
            }

            await _presenter.ShowPage(_serverSelectionViewModel);
        }
    }
}