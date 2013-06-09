using System;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Implementations.Controls;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.UI.Pages;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class NavigationService
    /// </summary>
    internal class NavigationService : INavigationService
    {
        /// <summary>
        /// The _theme manager
        /// </summary>
        private readonly IThemeManager _themeManager;
        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly Func<IPlaybackManager> _playbackManagerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        /// <param name="themeManager">The theme manager.</param>
        /// <param name="playbackManagerFactory">The playback manager factory.</param>
        public NavigationService(IThemeManager themeManager, Func<IPlaybackManager> playbackManagerFactory)
        {
            _themeManager = themeManager;
            _playbackManagerFactory = playbackManagerFactory;
        }

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation Navigate(Page page)
        {
            new PageFocusRetainer(page).RetainFocusAfterNavigation();

            return App.Instance.ApplicationWindow.Navigate(page);
        }

        /// <summary>
        /// Navigates to settings page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateToSettingsPage()
        {
            return Navigate(new SettingsPage());
        }

        /// <summary>
        /// Navigates to login page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateToLoginPage()
        {
            return App.Instance.ApplicationWindow.Dispatcher.InvokeAsync(() => Navigate(_themeManager.CurrentTheme.GetLoginPage()));
        }

        /// <summary>
        /// Navigates to internal player page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateToInternalPlayerPage()
        {
            var page = _themeManager.CurrentTheme.GetInternalPlayerPage();

            new InternalPlayerPageBehavior(page, _playbackManagerFactory(), this).AdjustPresentationForPlayback();

            return Navigate(page);
        }

        /// <summary>
        /// Navigates to home page.
        /// </summary>
        /// <param name="rootItem">The root item.</param>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateToHomePage(BaseItemDto rootItem)
        {
            return Navigate(_themeManager.CurrentTheme.GetHomePage(rootItem));
        }

        /// <summary>
        /// Navigates to item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateToItem(BaseItemDto item, string context)
        {
            return Navigate(_themeManager.CurrentTheme.GetItemPage(item, context));
        }

        /// <summary>
        /// Navigates the back.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateBack()
        {
            return App.Instance.ApplicationWindow.NavigateBack();
        }

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public DispatcherOperation NavigateForward()
        {
            return App.Instance.ApplicationWindow.NavigateForward();
        }
    }
}
