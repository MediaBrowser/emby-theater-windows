
namespace MediaBrowser.Plugins.DefaultTheme
{
    /// <summary>
    /// Interface IHasDisplayPreferences
    /// </summary>
    public interface IHasDisplayPreferences
    {
        /// <summary>
        /// Shows the display preferences menu.
        /// </summary>
        void ShowDisplayPreferencesMenu();

        void ShowSortMenu();

        /// <summary>
        /// Gets a value indicating whether this instance has sort options.
        /// </summary>
        /// <value><c>true</c> if this instance has sort options; otherwise, <c>false</c>.</value>
        bool HasSortOptions { get; }
    }
}