using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class LoginViewModel
        : BaseViewModel, IRootPresentationOptions
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogManager _logManager;
        private readonly ISessionManager _session;

        private readonly ObservableCollection<IViewModel> _users;

        public LoginViewModel(ISessionManager session, ILogManager logManager, IImageManager imageManager, IApiClient apiClient)
        {
            _session = session;
            _logManager = logManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _users = new ObservableCollection<IViewModel> { new UserLoginViewModel(null, _apiClient, _imageManager, _session, _logManager) };

            LoadUsers();
        }

        public ObservableCollection<IViewModel> Users
        {
            get { return _users; }
        }

        public bool ShowMediaBrowserLogo
        {
            get { return true; }
        }

        public bool ShowCommandBar
        {
            get { return true; }
        }

        public bool ShowClock
        {
            get { return true; }
        }

        public string Title
        {
            get { return null; }
        }

        private async void LoadUsers()
        {
            UserDto[] users = await _apiClient.GetPublicUsersAsync();

            Action action = () => {
                foreach (UserLoginViewModel user in users.Select(u => new UserLoginViewModel(u, _apiClient, _imageManager, _session, _logManager))) {
                    _users.Add(user);
                }
            };

            await action.OnUiThreadAsync();
        }
    }
}