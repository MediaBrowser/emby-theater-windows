using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Theater.Core.NetworkSettings
{
    /// <summary>
    /// Class NetworkSettingsPageFactory
    /// </summary>
    public class NetworkSettingsPageFactory : ISettingsPage
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Network"; }
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../Resources/Images/Settings/network.jpg", UriKind.Relative); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.System; }
        }

        public bool IsVisible(UserDto user)
        {
            return true;
        }

        public Type PageType
        {
            get { return typeof(NetworkSettingsPage); }
        }
    }
}
