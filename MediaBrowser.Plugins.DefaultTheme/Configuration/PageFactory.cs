using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Plugins.DefaultTheme.Configuration
{
    public class PageFactory : ISettingsPage
    {
        public string Name
        {
            get { return "Default Theme"; }
        }

        public Uri ThumbUri
        {
            get { return new Uri("pack://application:,,,/MediaBrowser.Plugins.DefaultTheme;component/Resources/Images/appearance.png", UriKind.Absolute); }
        }

        public Type PageType
        {
            get { return typeof(ConfigurationPage); }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.Plugin; }
        }

        public bool IsVisible(UserDto user)
        {
            return user != null;
        }
    }
}
