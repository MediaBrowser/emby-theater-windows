using System.Threading.Tasks;
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

        public SearchContext(ITheaterApplicationHost appHost, IPresenter presenter)
            : base(appHost)
        {
            _appHost = appHost;
            _presenter = presenter;
        }

        public override async Task Activate()
        {
            var viewModel = new HomeViewModel(_appHost);
            await _presenter.ShowPage(viewModel);
        }
    }
}