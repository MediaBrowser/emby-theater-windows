using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IApplicationWindow
    /// </summary>
    public interface IPresentationManager
    {
        /// <summary>
        /// Occurs when [window loaded].
        /// </summary>
        event EventHandler<EventArgs> WindowLoaded;

        /// <summary>
        /// Gets the apps.
        /// </summary>
        /// <value>The apps.</value>
        IEnumerable<ITheaterApp> Apps { get; }

        /// <summary>
        /// Gets the settings pages.
        /// </summary>
        /// <value>The settings pages.</value>
        IEnumerable<ISettingsPage> SettingsPages { get; }

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

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="apps">The apps.</param>
        /// <param name="settingsPages">The settings pages.</param>
        void AddParts(IEnumerable<ITheaterApp> apps, IEnumerable<ISettingsPage> settingsPages);

    }
}
