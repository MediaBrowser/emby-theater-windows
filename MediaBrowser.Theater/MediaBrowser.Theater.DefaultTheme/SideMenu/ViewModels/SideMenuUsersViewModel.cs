using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels
{
    public class SideMenuUsersViewModel
        : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ISessionManager _sessionManager;

        private BitmapImage _image;
        private CancellationTokenSource _imageCancellationTokenSource;
        private UserDto _user;

        public SideMenuUsersViewModel(ISessionManager sessionManager, IImageManager imageManager, IApiClient apiClient)
        {
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;

            sessionManager.UserLoggedOut += (s, e) => Image = null;
            sessionManager.UserLoggedIn += (s, e) => {
                _user = sessionManager.CurrentUser;
                DownloadImage();
            };

            _user = sessionManager.CurrentUser;
            DownloadImage();
            
            AdditionalUsers = new ObservableCollection<AdditionalUserViewModel>();
        }

        public ICommand LoginAdditionalUser { get; private set; }

        public string Username
        {
            get { return _user == null ? null : _user.Name; }
        }

        public ObservableCollection<AdditionalUserViewModel> AdditionalUsers { get; private set; }
        public ICommand AddAdditionalUser { get; private set; }

        public bool HasImage
        {
            get { return Image != null; }
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
                OnPropertyChanged("HasImage");
            }
        }

        private async void DownloadImage()
        {
            if (_user == null || !_user.PrimaryImageTag.HasValue) {
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

    public class AdditionalUserViewModel
        : BaseViewModel
    {
        public string Username { get; set; }
        public BitmapImage Image { get; set; }
        public ICommand RemoveFromSessionCommand { get; set; }
    }
}