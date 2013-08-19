using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class TheaterApplicationWindow
    /// </summary>
    internal class TheaterApplicationWindow : IPresentationManager
    {
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;
        private readonly IThemeManager _themeManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TheaterApplicationWindow" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="themeManager">The theme manager.</param>
        public TheaterApplicationWindow(ILogger logger, IThemeManager themeManager)
        {
            _logger = logger;
            _themeManager = themeManager;
        }

        public IEnumerable<IHomePage> HomePages { get; private set; }

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
            App.Instance.ApplicationWindow.RotatingBackdrops.ClearBackdrops();
        }

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetBackdrops(BaseItemDto item)
        {
            App.Instance.ApplicationWindow.RotatingBackdrops.SetBackdrops(item);
        }

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="paths">The paths.</param>
        public void SetBackdrops(IEnumerable<string> paths)
        {
            App.Instance.ApplicationWindow.RotatingBackdrops.SetBackdrops(paths.ToArray());
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


        /// <summary>
        /// Gets the apps.
        /// </summary>
        /// <value>The apps.</value>
        private IEnumerable<ITheaterApp> Apps { get; set; }

        /// <summary>
        /// Gets the settings pages.
        /// </summary>
        /// <value>The settings pages.</value>
        public IEnumerable<ISettingsPage> SettingsPages { get; private set; }

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="apps">The apps.</param>
        /// <param name="settingsPages">The settings pages.</param>
        /// <param name="homePages">The home pages.</param>
        public void AddParts(IEnumerable<ITheaterApp> apps, IEnumerable<ISettingsPage> settingsPages, IEnumerable<IHomePage> homePages)
        {
            Apps = apps;
            SettingsPages = settingsPages;
            HomePages = homePages;
        }

        public void ShowDefaultErrorMessage()
        {
            _themeManager.CurrentTheme.ShowDefaultErrorMessage();
        }

        public MessageBoxResult ShowMessage(MessageBoxInfo options)
        {
            return _themeManager.CurrentTheme.ShowMessage(options);
        }

        public void ShowNotification(string caption, string text, BitmapImage icon)
        {
        }

        public void SetPageTitle(string title)
        {
            _themeManager.CurrentTheme.SetPageTitle(title);
        }

        public void SetDefaultPageTitle()
        {
            _themeManager.CurrentTheme.SetDefaultPageTitle();
        }

        public IEnumerable<ITheaterApp> GetApps(UserDto user)
        {
            return Apps;
        }

        public void AddResourceDictionary(ResourceDictionary resource)
        {
            _logger.Info("Adding resource {0}", resource.GetType().Name);

            Application.Current.Resources.MergedDictionaries.Add(resource);
        }

        public void RemoveResourceDictionary(ResourceDictionary resource)
        {
            _logger.Info("Removing resource {0}", resource.GetType().Name);
            
            Application.Current.Resources.MergedDictionaries.Remove(resource);
        }
    }
}
