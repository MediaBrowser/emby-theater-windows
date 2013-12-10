using System;
using System.Windows.Input;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.UserProfileMenu;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Session;

namespace MediaBrowser.Plugins.DefaultTheme
{
    public class DefaultThemePageMasterCommandsViewModel : MasterCommandsViewModel
    {
        protected readonly IImageManager ImageManager;

        public ICommand UserCommand { get; private set; }
        public ICommand DisplayPreferencesCommand { get; private set; }
        public ICommand SortOptionsCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public DefaultThemePageMasterCommandsViewModel(INavigationService navigationService, ISessionManager sessionManager, IPresentationManager presentationManager, IApiClient apiClient, ILogger logger, IApplicationHost appHost, IServerEvents serverEvents, IImageManager imageManager) 
            : base(navigationService, sessionManager, presentationManager, apiClient, logger, appHost, serverEvents)
        {
            ImageManager = imageManager;

            UserCommand = new RelayCommand(i => ShowUserMenu());
            DisplayPreferencesCommand = new RelayCommand(i => ShowDisplayPreferences());
            SortOptionsCommand = new RelayCommand(i => ShowSortMenu());
           LogoutCommand = new RelayCommand(i => Logout());
        }

        protected virtual void ShowUserMenu()
        {
            new UserProfileWindow(this, SessionManager, ImageManager, ApiClient).ShowModal(PresentationManager.Window);
        }

        protected virtual void ShowDisplayPreferences()
        {
            var page = NavigationService.CurrentPage as IHasDisplayPreferences;

            if (page != null)
            {
                page.ShowDisplayPreferencesMenu();
            }
        }

        protected virtual void ShowSortMenu()
        {
            var page = NavigationService.CurrentPage as IHasDisplayPreferences;

            if (page != null)
            {
                page.ShowSortMenu();
            }
        }

        protected async void Logout()
        {
            if (SessionManager.CurrentUser == null)
            {
                throw new InvalidOperationException("The user is not logged in.");
            }

            await SessionManager.Logout();
        }
    }
}
