using MediaBrowser.Common;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.General
{
    class GeneralSettingsPageFactory : ISystemSettingsPage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IApplicationHost _appHost;

        public GeneralSettingsPageFactory(ITheaterConfigurationManager config, IApplicationHost appHost)
        {
            _config = config;
            _appHost = appHost;
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
            return new GeneralSettingsPage(_config, _appHost);
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/weather6.png", UriKind.Relative); }
        }

        public int? Order
        {
            get { return 0; }
        }
    }
}
