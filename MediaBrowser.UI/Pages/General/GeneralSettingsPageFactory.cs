using MediaBrowser.Common;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.General
{
    class GeneralSettingsPageFactory : IOrderedSettingsPage
    {
        private readonly ITheaterConfigurationManager _config;

        public GeneralSettingsPageFactory(ITheaterConfigurationManager config)
        {
            _config = config;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "General"; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new GeneralSettingsPage(_config);
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/general.jpg", UriKind.Relative); }
        }

        public int Order
        {
            get { return 0; }
        }

        public SettingsPageCategory Category
        {
            get { return SettingsPageCategory.System; }
        }

        public bool IsVisible(UserDto user)
        {
            return true;
        }
    }
}
