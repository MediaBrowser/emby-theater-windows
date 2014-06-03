using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.Commands
{
    public class OpenSideMenuMenuCommand
        : IGlobalMenuCommand
    {
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;

        public OpenSideMenuMenuCommand(INavigator navigator, ISessionManager sessionManager,IImageManager imageManager, IApiClient apiClient)
        {
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            ExecuteCommand = new RelayCommand(arg => navigator.Navigate(Go.To.SideMenu()));
        }

        public IViewModel IconViewModel 
        {
            get { return new OpenSideMenuCommandViewModel(_sessionManager, _imageManager, _apiClient); }
        }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Sidebar_OpenCommand".Localize(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.User; }
        }

        public int SortOrder
        {
            get { return 0; }
        }

        public ICommand ExecuteCommand { get; private set; }

        public bool EvaluateVisibility(INavigationPath currentPath)
        {
            return true;
        }
    }

    public class OpenSideMenuCommandViewModel : BaseViewModel
    {
        private UserDto _user;
        private CancellationTokenSource _imageCancellationTokenSource;

        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private BitmapImage _image;

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

        public bool HasImage
        {
            get { return Image != null; }
        }

        public OpenSideMenuCommandViewModel(ISessionManager session, IImageManager imageManager, IApiClient apiClient)
        {
            _imageManager = imageManager;
            _apiClient = apiClient;

            session.UserLoggedOut += (s, e) => Image = null;
            session.UserLoggedIn += (s, e) => {
                _user = session.CurrentUser;
                DownloadImage();
            };

            _user = session.CurrentUser;
            DownloadImage();
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
            }
            finally {
                _imageCancellationTokenSource.Dispose();
                _imageCancellationTokenSource = null;
            }
        }
    }
}
