using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Theater.Core.Appearance
{
    public class AppearancePageFactory : ISettingsPage
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Appearance"; }
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../Resources/Images/Settings/appearance.png", UriKind.Relative); }
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
            get { return typeof(AppearancePage); }
        }
    }
}
