using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Controls;

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
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new VlcConfigPage();
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/weather6.png", UriKind.Relative); }
        }
    }
}
