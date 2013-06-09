using System.Windows.Controls;
using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class TheaterApplicationWindow
    /// </summary>
    internal class TheaterApplicationWindow : IApplicationWindow
    {
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TheaterApplicationWindow" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TheaterApplicationWindow(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        public Window Window
        {
            get { return App.Instance.ApplicationWindow; }
        }

        /// <summary>
        /// Clears the backdrops.
        /// </summary>
        public void ClearBackdrops()
        {
            App.Instance.ApplicationWindow.ClearBackdrops();
        }

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetBackdrops(BaseItemDto item)
        {
            App.Instance.ApplicationWindow.SetBackdrops(item);
        }

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="paths">The paths.</param>
        public void SetBackdrops(IEnumerable<string> paths)
        {
            App.Instance.ApplicationWindow.SetBackdrops(paths.ToArray());
        }

        /// <summary>
        /// Called when [window loaded].
        /// </summary>
        internal void OnWindowLoaded()
        {
            EventHelper.FireEventIfNotNull(WindowLoaded, null, EventArgs.Empty, _logger);
        }

        /// <summary>
        /// Occurs when [window loaded].
        /// </summary>
        public event EventHandler<EventArgs> WindowLoaded;


        /// <summary>
        /// Gets the backdrop container.
        /// </summary>
        /// <value>The backdrop container.</value>
        public FrameworkElement BackdropContainer
        {
            get
            {
                return App.Instance.ApplicationWindow.BackdropContainer;
            }
        }

        /// <summary>
        /// Gets the page content control.
        /// </summary>
        /// <value>The page content control.</value>
        public ContentControl PageContentControl
        {
            get { return App.Instance.ApplicationWindow.PageContent; }
        }

        /// <summary>
        /// Gets the window overlay.
        /// </summary>
        /// <value>The window overlay.</value>
        public FrameworkElement WindowOverlay
        {
            get
            {
                return App.Instance.ApplicationWindow.WindowBackgroundContent;
            }
        }
    }
}
