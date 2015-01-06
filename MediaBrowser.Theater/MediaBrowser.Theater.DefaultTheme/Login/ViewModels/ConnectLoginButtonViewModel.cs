using System.Windows.Input;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class ConnectLoginButtonViewModel
        : BaseViewModel
    {
        public ConnectLoginButtonViewModel(INavigator navigator)
        {
            LoginViaConnectCommand = new RelayCommand(arg => navigator.Navigate(Go.To.ConnectLogin()));
        }

        public ICommand LoginViaConnectCommand { get; private set; }
    }
}