using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class WelcomePageViewModel
        : BaseViewModel, IHasRootPresentationOptions
    {
        public ICommand LoginViaConnectCommand { get; private set; }
        public ICommand LoginDirectCommand { get; private set; }

        public WelcomePageViewModel(INavigator navigator)
        {
            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false
            };

            LoginViaConnectCommand = new RelayCommand(arg => navigator.Navigate(Go.To.ConnectLogin()));
            LoginDirectCommand = new RelayCommand(arg => navigator.Navigate(Go.To.ServerSelection()));
        }

        public RootPresentationOptions PresentationOptions { get; private set; }
    }
}
