using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Search
{
    public class SearchContext
        : NavigationContext
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly IPresenter _presenter;
        private readonly ILogManager _logManager;

        public SearchContext(ITheaterApplicationHost appHost, IPresenter presenter, ILogManager logManager)
            : base(appHost)
        {
            _appHost = appHost;
            _presenter = presenter;
            _logManager = logManager;
        }

        public override async Task Activate()
        {
            var viewModel = new HomeViewModel(_appHost, _logManager);
            await _presenter.ShowPage(viewModel);
        }
    }
}