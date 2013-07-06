using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.MediaPlayers
{
    public class MediaPlayersPageFactory : ISettingsPage
    {
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IPresentationManager _presentation;

        public MediaPlayersPageFactory(INavigationService nav, IPlaybackManager playbackManager, ITheaterConfigurationManager config, IPresentationManager presentation)
        {
            _nav = nav;
            _playbackManager = playbackManager;
            _config = config;
            _presentation = presentation;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Media Players"; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new MediaPlayersPage(_nav, _playbackManager, _config, _presentation);
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/mediaplayers.jpg", UriKind.Relative); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.System; }
        }

        public bool IsVisible(UserDto user)
        {
            return user != null && user.Configuration.IsAdministrator;
        }
    }
}
