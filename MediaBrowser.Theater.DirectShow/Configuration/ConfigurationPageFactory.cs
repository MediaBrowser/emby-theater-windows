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
            get { return "Video Player"; }
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("pack://application:,,,/MediaBrowser.Theater.DirectShow;component/Resources/config.jpg", UriKind.Absolute); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.System; }
        }

        public bool IsVisible(UserDto user)
        {
            return user != null;
        }

        public Type PageType
        {
            get { return typeof(ConfigurationPage); }
        }
    }
}
