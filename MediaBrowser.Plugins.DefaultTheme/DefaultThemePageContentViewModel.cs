using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.DefaultTheme.UserProfileMenu;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Plugins.DefaultTheme
{
    public class DefaultThemePageContentViewModel : PageContentViewModel
    {
        private readonly IImageManager _imageManager;
        private readonly ITheaterConfigurationManager _config;

        public DefaultThemePageContentViewModel(INavigationService navigationService, ISessionManager sessionManager, IApiClient apiClient, IImageManager imageManager, IPresentationManager presentation, IPlaybackManager playbackManager, ILogger logger, ITheaterApplicationHost appHost, IServerEvents serverEvents, ITheaterConfigurationManager config)
            : base(navigationService, sessionManager, playbackManager, logger, appHost, apiClient, presentation, serverEvents)
        {
            _imageManager = imageManager;
            _config = config;

            MasterCommands = new DefaultThemePageMasterCommandsViewModel(navigationService, sessionManager, presentation, apiClient, logger, appHost, serverEvents, imageManager);

            NavigationService.Navigated += NavigationService_Navigated;
            SessionManager.UserLoggedIn += SessionManager_UserLoggedIn;
            SessionManager.UserLoggedOut += SessionManager_UserLoggedOut;

            _config.UserConfigurationUpdated += _config_UserConfigurationUpdated;
        }

        void _config_UserConfigurationUpdated(object sender, UserConfigurationUpdatedEventArgs e)
        {
            UpdateUserConfiguredValues();
        }

        void SessionManager_UserLoggedOut(object sender, EventArgs e)
        {
            ShowBackButton = true;
        }

        void SessionManager_UserLoggedIn(object sender, EventArgs e)
        {
            UpdateUserImage();
            UpdateUserConfiguredValues();
        }

        private void UpdateUserConfiguredValues()
        {
            var config = _config.GetUserTheaterConfiguration(SessionManager.CurrentUser.Id);

            ShowBackButton = config.ShowBackButton;
        }

        private async void UpdateUserImage()
        {
            var user = SessionManager.CurrentUser;

            if (user.HasPrimaryImage)
            {
                var imageUrl = ApiClient.GetUserImageUrl(user, new ImageOptions
                {
                    ImageType = ImageType.Primary
                });

                try
                {
                    UserImage = await _imageManager.GetRemoteBitmapAsync(imageUrl);

                    ShowDefaultUserImage = false;
                }
                catch (Exception ex)
                {
                    ShowDefaultUserImage = true;
                }
            }
            else
            {
                ShowDefaultUserImage = true;
            }
        }

        void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            var hasDisplayPreferences = e.NewPage as IHasDisplayPreferences;

            if (hasDisplayPreferences == null)
            {
                MasterCommands.DisplayPreferencesEnabled = false;
                MasterCommands.SortEnabled = false;
            }
            else
            {
                MasterCommands.DisplayPreferencesEnabled = true;
                MasterCommands.SortEnabled = hasDisplayPreferences.HasSortOptions;
            }

            var isLoginPage = e.NewPage as ILoginPage;

            if (isLoginPage == null)
            {
                ShowSettingsButton = false;
            }
            else
            {
                ShowSettingsButton = true;
            }

            ShowSearchButton = (e.NewPage as ISupportSearch) != null;
        }

        private BitmapImage _userImage;
        public BitmapImage UserImage
        {
            get { return _userImage; }

            set
            {
                _userImage = value;

                OnPropertyChanged("UserImage");
            }
        }

        private bool _showDefaultUserImage;
        public bool ShowDefaultUserImage
        {
            get { return _showDefaultUserImage; }

            set
            {
                var changed = _showDefaultUserImage != value;

                _showDefaultUserImage = value;

                if (changed)
                {
                    OnPropertyChanged("ShowDefaultUserImage");
                }
            }
        }

        private bool _showBackButton = true;
        public bool ShowBackButton
        {
            get { return _showBackButton; }

            set
            {
                var changed = _showBackButton != value;

                _showBackButton = value;
                if (changed)
                {
                    OnPropertyChanged("ShowBackButton");
                }
            }
        }

        private bool _showLogoImage;
        public bool ShowLogoImage
        {
            get { return _showLogoImage; }

            set
            {
                var changed = _showLogoImage != value;

                _showLogoImage = value;

                if (changed)
                {
                    OnPropertyChanged("ShowLogoImage");
                }
            }
        }

        private bool _showSettingsButton;
        public bool ShowSettingsButton
        {
            get { return _showSettingsButton; }

            set
            {
                var changed = _showSettingsButton != value;

                _showSettingsButton = value;

                if (changed)
                {
                    OnPropertyChanged("ShowSettingsButton");
                }
            }
        }

        private bool _showSearchButton = true;
        public bool ShowSearchButton
        {
            get { return _showSearchButton; }

            set
            {
                var changed = _showSearchButton != value;

                _showSearchButton = value;

                if (changed)
                {
                    OnPropertyChanged("ShowSearchButton");
                }
            }
        }

        private BitmapImage _logoImage;
        public BitmapImage LogoImage
        {
            get { return _logoImage; }

            set
            {
                _logoImage = value;

                OnPropertyChanged("LogoImage");
            }
        }

        private string _timeLeft;
        public string TimeLeft
        {
            get { return _timeLeft; }

            set
            {
                var changed = !string.Equals(_timeLeft, value);

                _timeLeft = value;
                if (changed)
                {
                    OnPropertyChanged("TimeLeft");
                }
            }
        }

        private string _timeRight;
        public string TimeRight
        {
            get { return _timeRight; }

            set
            {
                var changed = !string.Equals(_timeRight, value);

                _timeRight = value;
                if (changed)
                {
                    OnPropertyChanged("TimeRight");
                }
            }
        }

        private DefaultThemePageMasterCommandsViewModel _masterCommands;
        public new DefaultThemePageMasterCommandsViewModel MasterCommands
        {
            get { return _masterCommands; }
            set
            {
                if (_masterCommands != value)
                {
                    _masterCommands = value;
                    OnPropertyChanged("MasterCommands");
                }
            }
        }

        public async void SetPageTitle(BaseItemDto item)
        {
            if (item.HasLogo || !string.IsNullOrEmpty(item.ParentLogoItemId))
            {
                var url = ApiClient.GetLogoImageUrl(item, new ImageOptions
                {
                });

                try
                {
                    LogoImage = await _imageManager.GetRemoteBitmapAsync(url);

                    ShowDefaultPageTitle = false;
                    PageTitle = string.Empty;
                    ShowLogoImage = true;
                }
                catch
                {
                    SetPageTitleText(item);
                }
            }
            else
            {
                SetPageTitleText(item);
            }
        }

        private void SetPageTitleText(BaseItemDto item)
        {
            var title = item.Name;

            if (item.IsType("Season"))
            {
                title = item.SeriesName + " | " + item.Name;
            }
            else if (item.IsType("Episode"))
            {
                title = item.SeriesName;

                if (item.ParentIndexNumber.HasValue)
                {
                    title += " | " + string.Format("Season {0}", item.ParentIndexNumber.Value.ToString());
                }
            }
            else if (item.IsType("MusicAlbum"))
            {
                if (!string.IsNullOrEmpty(item.AlbumArtist))
                {
                    title = item.AlbumArtist + " | " + title;
                }
            }

            PageTitle = title;
            ShowDefaultPageTitle = string.IsNullOrEmpty(PageTitle);
            ShowLogoImage = false;
        }

        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (string.Equals(name, "PageTitle"))
            {
                ShowLogoImage = false;

                if (!string.IsNullOrEmpty(PageTitle))
                {
                    ShowDefaultPageTitle = false;
                }
            }
            else if (string.Equals(name, "ShowDefaultPageTitle"))
            {
                if (ShowDefaultPageTitle)
                {
                    ShowLogoImage = false;
                    PageTitle = string.Empty;
                }
            }
            else if (string.Equals(name, "DateTime"))
            {
                UpdateTime();
            }
        }

        private void UpdateTime()
        {
            var now = DateTime;

            var nowString = now.ToShortTimeString();

            if (nowString.IndexOf("am", StringComparison.OrdinalIgnoreCase) != -1 ||
                nowString.IndexOf("pm", StringComparison.OrdinalIgnoreCase) != -1)
            {
                TimeLeft = now.ToString("h:mm");
                var time = now.ToString("t");
                var values = time.Split(' ');
                TimeRight = values[values.Length - 1].ToLower();
            }
            else
            {
                TimeLeft = now.ToShortTimeString();
                TimeRight = string.Empty;
            }
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                NavigationService.Navigated -= NavigationService_Navigated;
                SessionManager.UserLoggedIn -= SessionManager_UserLoggedIn;
                SessionManager.UserLoggedOut -= SessionManager_UserLoggedOut;
            }

            base.Dispose(dispose);
        }
    }
}