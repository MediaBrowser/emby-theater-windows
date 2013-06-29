using System;
using System.Collections.Generic;

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
        event EventHandler<EventArgs> ThemeLoaded;

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
        /// Sets the current theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        void LoadTheme(ITheme theme);

        /// <summary>
        /// Sets the current theme.
        /// </summary>
        void LoadDefaultTheme();
    }
}
