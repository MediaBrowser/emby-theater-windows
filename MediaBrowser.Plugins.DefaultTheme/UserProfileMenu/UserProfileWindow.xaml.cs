using System.Windows.Media.Animation;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Plugins.DefaultTheme.Models;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
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

        public UserProfileWindow(DefaultThemePageMasterCommandsViewModel masterCommands, ISessionManager session, IImageManager imageManager, IApiClient apiClient)
            : base()
        {
            _session = session;

            InitializeComponent();

            Loaded += UserProfileWindow_Loaded;
            Unloaded += UserProfileWindow_Unloaded;
            masterCommands.PageNavigated += masterCommands_SettingsPageNavigated;

            BtnClose.Click += BtnClose_Click;

            ContentGrid.DataContext = new DefaultThemeUserDtoViewModel(masterCommands, apiClient, imageManager, session)
            {
                User = session.CurrentUser,
                ImageHeight = 54
            };

            MainGrid.DataContext = this;
        }

        protected override void CloseModal()
        {
            var closeModalStoryboard = (Storyboard) FindResource("ClosingModalStoryboard");
            closeModalStoryboard.Completed += closeModalStoryboard_Completed;
            closeModalStoryboard.Begin();
        }

        void closeModalStoryboard_Completed(object sender, EventArgs e)
        {
            base.CloseModal();
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

        void masterCommands_SettingsPageNavigated(object sender, EventArgs e)
        {
            CloseModal();
        }
    }
}
