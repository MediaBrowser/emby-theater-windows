using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.SortModeMenu.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SortModeMenu
{
    public class SortModeMenuContext
        : NavigationContext
    {
        private readonly IPresenter _presenter;

        public SortModeMenuContext(IApplicationHost appHost, IPresenter presenter) : base(appHost)
        {
            _presenter = presenter;
        }

        public IHasItemSortModes Items { get; set; }

        public override Task Activate()
        {
            var viewModel = new SortModeMenuViewModel(Items);
            _presenter.ShowPopup(viewModel);

            return Task.FromResult(0);
        }
    }
}