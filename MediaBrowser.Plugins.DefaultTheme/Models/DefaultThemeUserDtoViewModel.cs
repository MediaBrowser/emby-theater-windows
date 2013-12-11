using System;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.ViewModels;
using MediaBrowser.Model.ApiClient;

namespace MediaBrowser.Plugins.DefaultTheme.Models
{
    public class DefaultThemeUserDtoViewModel : UserDtoViewModel
    {
        private DefaultThemePageMasterCommandsViewModel _masterCommands;
        public DefaultThemePageMasterCommandsViewModel MasterCommands
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

        public DefaultThemeUserDtoViewModel(DefaultThemePageMasterCommandsViewModel masterCommands, IApiClient apiClient, IImageManager imageManager, ISessionManager session)
            :base(apiClient, imageManager, session)
        {
            MasterCommands = masterCommands;
        }

        /// <summary>
        /// Overrides base command to give logout responsibility to the master commands class rather than the internal method in the parent.
        /// </summary>
        protected override void Logout()
        {
            if (!string.Equals(User.Id, Session.CurrentUser.Id))
            {
                throw new InvalidOperationException("The user is not logged in.");
            }

            MasterCommands.LogoutCommand.Execute(null);
        }
    }
}
