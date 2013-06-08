using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Class BaseDisplayPreferencesPage
    /// </summary>
    public class BaseDisplayPreferencesPage : Page
    {
        /// <summary>
        /// Gets or sets the display preferences window.
        /// </summary>
        /// <value>The display preferences window.</value>
        public DisplayPreferencesMenu DisplayPreferencesWindow { get; set; }
    }
}
