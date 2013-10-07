using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Theater.Core.Advanced
{
    public class AppearancePageFactory : ISettingsPage
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Advanced"; }
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../Resources/Images/Settings/advanced.jpg", UriKind.Relative); }
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
            get { return typeof(AdvancedPage); }
        }
    }
}
