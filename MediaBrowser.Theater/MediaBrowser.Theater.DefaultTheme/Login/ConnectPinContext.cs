using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login
{
    public class ConnectPinContext
        : NavigationContext
    {
        private readonly INavigator _navigator;
        private readonly IPresenter _presenter;

        private ConnectPinViewModel _connectPin;

        public ConnectPinContext(IApplicationHost appHost, INavigator navigator, IPresenter presenter)
            : base(appHost)
        {
            _navigator = navigator;
            _presenter = presenter;
        }

        public override async Task Activate()
        {
            if (_connectPin == null || !_connectPin.IsActive) {
                _connectPin = new ConnectPinViewModel();
            }

            await _presenter.ShowPage(_connectPin);
        }
    }
}
