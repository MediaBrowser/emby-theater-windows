using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class LoginViewModel
        : BaseViewModel
    {
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly ILogManager _logManager;
        private readonly ISessionManager _session;

        private readonly ObservableCollection<IViewModel> _users;

        public LoginViewModel(ISessionManager session, ILogManager logManager, IImageManager imageManager, IApiClient apiClient, INavigator navigator)
        {
            _session = session;
            _logManager = logManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _users = new ObservableCollection<IViewModel> { new UserLoginViewModel(null, _session.ActiveApiClient, _imageManager, _session, _logManager) };

            ChangeServerCommand = new RelayCommand(arg => navigator.Navigate(Go.To.ServerSelection()));

            LoadUsers();
        }

        public ObservableCollection<IViewModel> Users
        {
            get { return _users; }
        }

        public ICommand ChangeServerCommand { get; private set; }

        private async void LoadUsers()
        {
            UserDto[] users = await _apiClient.GetPublicUsersAsync();

            Action action = () => {
                foreach (UserLoginViewModel user in users.Select(u => new UserLoginViewModel(u, _apiClient, _imageManager, _session, _logManager))) {
                    _users.Insert(0, user);
                }
            };

            await action.OnUiThreadAsync();
        }
    }
}