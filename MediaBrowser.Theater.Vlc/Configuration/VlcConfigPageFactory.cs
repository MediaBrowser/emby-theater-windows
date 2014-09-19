using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Theater.Vlc.Configuration
{
    public class VlcConfigPageFactory : ISettingsPage
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "VLC"; }
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("pack://application:,,,/MediaBrowser.Theater.Vlc;component/Resources/Images/settings.jpg", UriKind.Absolute); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.Plugin; }
        }

        public bool IsVisible(UserDto user)
        {
            return user != null && user.Configuration.IsAdministrator;
        }

        public Type PageType
        {
            get { return typeof(VlcConfigPage); }
        }
    }
}
