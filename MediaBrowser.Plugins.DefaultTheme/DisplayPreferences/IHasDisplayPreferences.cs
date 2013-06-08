
namespace MediaBrowser.Plugins.DefaultTheme.DisplayPreferences
{
    /// <summary>
    /// Interface IHasDisplayPreferences
    /// </summary>
    public interface IHasDisplayPreferences
    {
        /// <summary>
        /// Gets the display preferences.
        /// </summary>
        /// <value>The display preferences.</value>
        Model.Entities.DisplayPreferences DisplayPreferences { get; }

        /// <summary>
        /// Notifies the display preferences changed.
        /// </summary>
        void NotifyDisplayPreferencesChanged();
    }
}
