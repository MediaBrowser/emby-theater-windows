using System;
using System.Windows.Controls;
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
        /// Gets the backdrop container.
        /// </summary>
        /// <value>The backdrop container.</value>
        FrameworkElement BackdropContainer { get; }

        /// <summary>
        /// Gets the window overlay.
        /// </summary>
        /// <value>The window overlay.</value>
        FrameworkElement WindowOverlay { get; }
        
        /// <summary>
        /// Gets the page content control.
        /// </summary>
        /// <value>The page content control.</value>
        ContentControl PageContentControl { get; }
        
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
