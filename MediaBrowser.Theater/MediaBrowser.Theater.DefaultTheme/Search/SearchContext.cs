using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Search
{
    public class SearchContext
        : NavigationContext
    {
        private readonly IPresenter _presenter;

        public SearchContext(IApplicationHost appHost, IPresenter presenter)
            : base(appHost)
        {
            _presenter = presenter;
        }

        public override async Task Activate()
        {
            var viewModel = new HomeViewModel();
            await _presenter.ShowPage(viewModel);
        }
    }
}