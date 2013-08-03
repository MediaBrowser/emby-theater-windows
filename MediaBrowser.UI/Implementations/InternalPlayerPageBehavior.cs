using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Theming;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class InternalPlayerPageBehavior
    /// </summary>
    internal class InternalPlayerPageBehavior
    {
        /// <summary>
        /// The _page
        /// </summary>
        private readonly Page _page;
        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;
        /// <summary>
        /// The _nav
        /// </summary>
        private readonly INavigationService _nav;

        private readonly IThemeManager _themeManager;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalPlayerPageBehavior"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="playbackManager">The playback manager.</param>
        /// <param name="nav">The nav.</param>
        public InternalPlayerPageBehavior(Page page, IPlaybackManager playbackManager, INavigationService nav, IThemeManager themeManager)
        {
            _page = page;
            _playbackManager = playbackManager;
            _nav = nav;
            _themeManager = themeManager;
        }

        /// <summary>
        /// Adjusts the presentation for playback.
        /// </summary>
        public void AdjustPresentationForPlayback()
        {
            _page.Loaded += _page_Loaded;
            _page.Unloaded += _page_Unloaded;
            _playbackManager.PlaybackCompleted += _playbackManager_PlaybackCompleted;
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the _playbackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStopEventArgs"/> instance containing the event data.</param>
        async void _playbackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            await _nav.NavigateBack();
        }

        /// <summary>
        /// Handles the Loaded event of the _page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void _page_Loaded(object sender, RoutedEventArgs e)
        {
            App.Instance.ApplicationWindow.WindowBackgroundContent.Visibility = Visibility.Collapsed;
            App.Instance.ApplicationWindow.PageContent.Visibility = Visibility.Collapsed;

            _themeManager.CurrentTheme.SetGlobalContentVisibility(false);
        }

        /// <summary>
        /// Handles the Unloaded event of the _page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void _page_Unloaded(object sender, RoutedEventArgs e)
        {
            _page.Loaded -= _page_Loaded;
            _page.Unloaded -= _page_Unloaded;
            _playbackManager.PlaybackCompleted -= _playbackManager_PlaybackCompleted;
            
            App.Instance.ApplicationWindow.PageContent.Visibility = Visibility.Visible;
            App.Instance.ApplicationWindow.WindowBackgroundContent.Visibility = Visibility.Visible;

            _themeManager.CurrentTheme.SetGlobalContentVisibility(true);
        }
    }
}
