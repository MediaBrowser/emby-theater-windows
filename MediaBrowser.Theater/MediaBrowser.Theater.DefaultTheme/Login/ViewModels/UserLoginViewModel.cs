using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public interface IUserImageViewModel : IViewModel
    {
        bool HasImage { get; }
        BitmapImage Image { get; }
        bool HasUsername { get; }
        string Username { get; }
    }

    public interface IUserLoginViewModel : IViewModel
    {
        bool RequiresUsername { get; }
        string Username { get; set; }
        bool RequiresPassword { get; }
        string Password { get; set; }
        bool RememberLogin { get; set; }
        ICommand LoginCommand { get; }
    }

    public class UserLoginViewModel
        : BaseViewModel, IUserImageViewModel, IUserLoginViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly UserDto _user;

        private BitmapImage _image;
        private CancellationTokenSource _imageCancellationTokenSource;
        private bool _isInvalidLoginDetails;
        private string _password;
        private bool _rememberMe;
        private string _username;

        public UserLoginViewModel(UserDto user, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, ILogManager logManager)
        {
            _user = user;
            _apiClient = apiClient;
            _imageManager = imageManager;

            ILogger logger = logManager.GetLogger("Login");

            LoginCommand = new RelayCommand(async arg => {
                try {
                    IsInvalidLoginDetails = false;
                    await sessionManager.LoginToServer(Username, Password, RememberLogin);
                } catch (UnauthorizedAccessException) {
                    IsInvalidLoginDetails = true;
                } catch (Exception e) {
                    logger.ErrorException("Error while attempting to login", e);
                }
            });
        }

        public bool IsInvalidLoginDetails
        {
            get { return _isInvalidLoginDetails; }
            private set
            {
                if (Equals(_isInvalidLoginDetails, value)) {
                    return;
                }

                _isInvalidLoginDetails = value;
                OnPropertyChanged();
            }
        }
        
        public bool HasUsername
        {
            get { return _user != null; }
        }

        public string Username
        {
            get { return _user != null ? _user.Name : _username; }
            set
            {
                if (_user != null) {
                    _user.Name = value;
                } else {
                    _username = value;
                }
            }
        }

        public bool HasImage
        {
            get { return _user != null && _user.HasPrimaryImage; }
        }

        public BitmapImage Image
        {
            get
            {
                if (_image == null && _imageCancellationTokenSource == null) {
                    DownloadImage();
                }

                return _image;
            }

            private set
            {
                if (Equals(_image, value)) {
                    return;
                }

                _image = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresUsername
        {
            get { return !HasUsername; }
        }

        public ICommand LoginCommand { get; private set; }

        public bool RequiresPassword
        {
            get { return _user == null || _user.HasPassword; }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (Equals(_password, value)) {
                    return;
                }

                _password = value;
                OnPropertyChanged();
            }
        }

        public bool RememberLogin
        {
            get { return _rememberMe; }
            set
            {
                if (Equals(_rememberMe, value)) {
                    return;
                }

                _rememberMe = value;
                OnPropertyChanged();
            }
        }

        private async void DownloadImage()
        {
            if (_user == null || string.IsNullOrEmpty(_user.PrimaryImageTag)) {
                return;
            }

            _imageCancellationTokenSource = new CancellationTokenSource();

            try {
                var options = new ImageOptions { ImageType = ImageType.Primary };
                Image = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetUserImageUrl(_user, options), _imageCancellationTokenSource.Token);
            } finally {
                _imageCancellationTokenSource.Dispose();
                _imageCancellationTokenSource = null;
            }
        }
    }
}