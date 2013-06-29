using System;
using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows.Controls;

namespace MediaBrowser.UI.Pages.NetworkSettings
{
    /// <summary>
    /// Class NetworkSettingsPageFactory
    /// </summary>
    public class NetworkSettingsPageFactory : ISystemSettingsPage
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
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        public Page GetPage()
        {
            return new NetworkSettingsPage();
        }

        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        public Uri ThumbUri
        {
            get { return new Uri("../../Resources/Images/Settings/network.jpg", UriKind.Relative); }
        }

        public int? Order
        {
            get { return null; }
        }
    }
}
