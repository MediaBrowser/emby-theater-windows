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
    public class UserLoginViewModel
        : BaseViewModel, IHasImage
    {
        private readonly UserDto _user;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        private BitmapImage _image;
        private CancellationTokenSource _imageCancellationTokenSource;
        private string _password;
        private bool _rememberMe;
        private bool _isInvalidLoginDetails;

        public UserLoginViewModel(UserDto user, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, ILogManager logManager)
        {
            _user = user;
            _apiClient = apiClient;
            _imageManager = imageManager;

            var logger = logManager.GetLogger("Login");

            LoginCommand = new RelayCommand(async arg => {
                try
                {
                    IsInvalidLoginDetails = false;
                    await sessionManager.Login(Username, Password, RememberMe);
                }
                catch (UnauthorizedAccessException)
                {
                    IsInvalidLoginDetails = true;
                }
                catch (Exception e)
                {
                    logger.ErrorException("Error while attempting to login", e);
                }
            });
        }

        public ICommand LoginCommand { get; private set; }

        public bool IsInvalidLoginDetails
        {
            get { return _isInvalidLoginDetails; }
            private set
            {
                if (Equals(_isInvalidLoginDetails, value))
                {
                    return;
                }

                _isInvalidLoginDetails = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (Equals(_password, value))
                {
                    return;
                }

                _password = value;
                OnPropertyChanged();
            }
        }

        public bool RememberMe
        {
            get { return _rememberMe; }
            set
            {
                if (Equals(_rememberMe, value))
                {
                    return;
                }

                _rememberMe = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return _user.Name; }
        }
        
        public bool HasPassword
        {
            get { return _user.HasPassword; }
        }

        public bool HasImage
        {
            get { return _user.HasPrimaryImage; }
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

        private async void DownloadImage()
        {
            if (!_user.PrimaryImageTag.HasValue) {
                return;
            }

            _imageCancellationTokenSource = new CancellationTokenSource();

            try {
                var options = new ImageOptions { ImageType = ImageType.Primary };
                Image = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetUserImageUrl(_user, options), _imageCancellationTokenSource.Token);
            }
            finally {
                _imageCancellationTokenSource.Dispose();
                _imageCancellationTokenSource = null;
            }
        }
    }
}