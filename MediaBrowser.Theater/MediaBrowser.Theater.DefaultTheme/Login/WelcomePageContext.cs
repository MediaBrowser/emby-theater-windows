using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login
{
    public class WelcomePageContext
        : NavigationContext
    {
        private readonly INavigator _navigator;
        private readonly IPresenter _presenter;

        private WelcomePageViewModel _welcomePage;

        public WelcomePageContext(IApplicationHost appHost, INavigator navigator, IPresenter presenter)
            : base(appHost)
        {
            _navigator = navigator;
            _presenter = presenter;
        }

        public override async Task Activate()
        {
            if (_welcomePage == null || !_welcomePage.IsActive) {
                _welcomePage = new WelcomePageViewModel(_navigator);
            }

            await _presenter.ShowPage(_welcomePage);
        }
    }
}
