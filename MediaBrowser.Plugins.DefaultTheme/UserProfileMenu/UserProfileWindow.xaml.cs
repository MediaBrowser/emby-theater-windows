using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.UserProfileMenu
{
    /// <summary>
    /// Interaction logic for UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow : BaseModalWindow
    {
        private readonly ISessionManager _session;

        public UserProfileWindow(ISessionManager session, IImageManager imageManager, IApiClient apiClient)
            : base()
        {
            _session = session;

            InitializeComponent();

            Loaded += UserProfileWindow_Loaded;
            Unloaded += UserProfileWindow_Unloaded;

            BtnClose.Click += BtnClose_Click;

            ContentGrid.DataContext = new UserDtoViewModel(apiClient, imageManager, session)
            {
                User = session.CurrentUser,
                ImageHeight = 54
            };

            MainGrid.DataContext = this;
        }

        void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        void UserProfileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            _session.UserLoggedOut += _session_UserLoggedOut;
        }

        void UserProfileWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _session.UserLoggedOut -= _session_UserLoggedOut;
        }

        void _session_UserLoggedOut(object sender, EventArgs e)
        {
            CloseModal();
        }
    }
}
