using MediaBrowser.Theater.Presentation.Pages;

namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Class BaseDisplayPreferencesPage
    /// </summary>
    public class BaseDisplayPreferencesPage : BasePage
    {
        /// <summary>
        /// Gets or sets the display preferences window.
        /// </summary>
        /// <value>The display preferences window.</value>
        public DisplayPreferencesMenu DisplayPreferencesWindow { get; set; }
    }
}
