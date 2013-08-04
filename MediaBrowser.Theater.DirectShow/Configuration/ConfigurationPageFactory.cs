using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Theater.DirectShow.Configuration
{
    class GeneralSettingsPageFactory : ISettingsPage
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Internal Player"; }
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../Resources/Images/config.jpg", UriKind.Relative); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.System; }
        }

        public bool IsVisible(UserDto user)
        {
            return user != null && user.Configuration.IsAdministrator;
        }

        public Type PageType
        {
            get { return typeof(ConfigurationPage); }
        }
    }
}
