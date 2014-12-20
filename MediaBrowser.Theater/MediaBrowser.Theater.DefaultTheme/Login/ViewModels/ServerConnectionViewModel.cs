using System.Threading;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class ServerConnectionViewModel
        : BaseViewModel
    {
        private readonly ServerInfo _info;

        public ServerConnectionViewModel(IConnectionManager connectionManager, INavigator navigator, ServerInfo info)
        {
            _info = info;

            ConnectCommand = new RelayCommand(async arg => {
                if (_info == null) {
                    navigator.Navigate(Go.To.LocateServer());
                } else {
                    var result = await connectionManager.Connect(info, CancellationToken.None);
                }
            });
        }

        public string Name
        {
            get { return _info != null ? _info.Name : "Find Server"; }
        }

        public ICommand ConnectCommand { get; private set; }
    }
}