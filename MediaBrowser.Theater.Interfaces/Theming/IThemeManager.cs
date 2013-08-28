using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Theming
{
    /// <summary>
    /// Interface IThemeManager
    /// </summary>
    public interface IThemeManager
    {
        /// <summary>
        /// Occurs when [theme loaded].
        /// </summary>
        event EventHandler<ItemEventArgs<ITheme>> ThemeLoaded;
        /// <summary>
        /// Occurs when [theme unloaded].
        /// </summary>
        event EventHandler<ItemEventArgs<ITheme>> ThemeUnloaded;

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="themes">The themes.</param>
        void AddParts(IEnumerable<ITheme> themes);

        /// <summary>
        /// Gets the themes.
        /// </summary>
        /// <value>The themes.</value>
        IEnumerable<ITheme> Themes { get; }

        /// <summary>
        /// Gets the current theme.
        /// </summary>
        /// <value>The current theme.</value>
        ITheme CurrentTheme { get; }

        /// <summary>
        /// Gets the default theme.
        /// </summary>
        /// <value>The default theme.</value>
        ITheme DefaultTheme { get; }

        /// <summary>
        /// Sets the current theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        Task LoadTheme(ITheme theme);

        /// <summary>
        /// Sets the current theme.
        /// </summary>
        Task LoadDefaultTheme();
    }
}
