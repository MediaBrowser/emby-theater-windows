using System;
using MediaBrowser.Model.Dto;
using System.Collections.Generic;
using System.Windows;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IApplicationWindow
    /// </summary>
    public interface IApplicationWindow
    {
        /// <summary>
        /// Occurs when [window loaded].
        /// </summary>
        event EventHandler<EventArgs> WindowLoaded;
        
        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        Window Window { get; }

        /// <summary>
        /// Clears the backdrops.
        /// </summary>
        void ClearBackdrops();

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="item">The item.</param>
        void SetBackdrops(BaseItemDto item);

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="paths">The paths.</param>
        void SetBackdrops(IEnumerable<string> paths);
    }
}
