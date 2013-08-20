using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Core.Modals;
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

        /// <summary>
        /// Shows the default error message.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ShowDefaultErrorMessage()
        {
            ShowMessage(new MessageBoxInfo
            {
                Caption = "Error",
                Text = "There was an error processing the request.",
                Icon = MessageBoxIcon.Error,
                Button = MessageBoxButton.OK

            });
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>MessageBoxResult.</returns>
        public MessageBoxResult ShowMessage(MessageBoxInfo options)
        {
            return ShowMessage(options, Window);
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="parentWindow">The parent window.</param>
        /// <returns>MessageBoxResult.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public MessageBoxResult ShowMessage(MessageBoxInfo options, Window parentWindow)
        {
            var win = new MessageBoxWindow(options);

            win.ShowModal(parentWindow);

            return win.MessageBoxResult;
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
