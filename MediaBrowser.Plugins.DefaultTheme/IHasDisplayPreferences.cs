
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.DefaultTheme.ListPage;

namespace MediaBrowser.Plugins.DefaultTheme
{
    /// <summary>
    /// Interface IHasDisplayPreferences
    /// </summary>
    public interface IHasDisplayPreferences
    {
        /// <summary>
        /// Retrieves the current display preferences for the instance
        /// </summary>
        /// <returns></returns>
        DisplayPreferences GetDisplayPreferences();

        ListPageConfig GetListPageConfig();

        /// <summary>
        /// Gets a value indicating whether this instance has sort options.
        /// </summary>
        /// <value><c>true</c> if this instance has sort options; otherwise, <c>false</c>.</value>
        bool HasSortOptions { get; }
    }
}