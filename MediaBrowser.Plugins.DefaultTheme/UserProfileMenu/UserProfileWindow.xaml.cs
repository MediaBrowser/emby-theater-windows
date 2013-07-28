using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.UserProfileMenu
{
    /// <summary>
    /// Interaction logic for UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow : BaseModalWindow
    {
        private readonly ISessionManager _session;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;

        public UserProfileWindow(ISessionManager session, IImageManager imageManager, IApiClient apiClient)
        {
            _session = session;
            _imageManager = imageManager;
            _apiClient = apiClient;
            InitializeComponent();

            Loaded += UserProfileWindow_Loaded;

            BtnClose.Click += BtnClose_Click;
            BtnLogout.Click += BtnLogout_Click;

            DataContext = this;
        }

        async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            await _session.Logout();

            CloseModal();
        }

        void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        void UserProfileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetUserImage();

            BtnClose.Focus();

            TxtUsername.Text = _session.CurrentUser.Name;
        }

        private async void SetUserImage()
        {
            if (_session.CurrentUser.HasPrimaryImage)
            {
                try
                {
                    UserImage.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetUserImageUrl(_session.CurrentUser, new ImageOptions
                    {
                        Height = 54
                    }));

                    UserDefaultImage.Visibility = Visibility.Collapsed;
                    UserImage.Visibility = Visibility.Visible;

                    return;
                }
                catch (HttpException)
                {
                    // Logged at lower levels
                }
            }

            SetDefaultUserImage();
        }

        private void SetDefaultUserImage()
        {
            UserDefaultImage.Visibility = Visibility.Visible;
            UserImage.Visibility = Visibility.Collapsed;
        }

    }
}
