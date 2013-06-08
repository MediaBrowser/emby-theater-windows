using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Pages
{
    /// <summary>
    /// Class BaseInternalPlayerPage
    /// </summary>
    public abstract class BaseInternalPlayerPage : Page
    {
        protected BaseInternalPlayerPage(INavigationService navigationManager)
        {
            NavigationManager = navigationManager;
        }

        protected INavigationService NavigationManager { get; private set; }
        
        //protected override void OnLoaded()
        //{
        //    base.OnLoaded();

        //    App.Instance.ApplicationWindow.WindowBackgroundContent.Visibility = Visibility.Collapsed;
        //    App.Instance.ApplicationWindow.PageContent.Visibility = Visibility.Collapsed;

        //    UIKernel.Instance.PlaybackManager.PlaybackCompleted -= PlaybackManager_PlaybackCompleted;
        //    UIKernel.Instance.PlaybackManager.PlaybackCompleted += PlaybackManager_PlaybackCompleted;
        //}

        /// <summary>
        /// Handles the PlaybackCompleted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStartEventArgs" /> instance containing the event data.</param>
        void PlaybackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            NavigationManager.NavigateBack();
        }

        //protected override void OnUnloaded()
        //{
        //    UIKernel.Instance.PlaybackManager.PlaybackCompleted -= PlaybackManager_PlaybackCompleted;

        //    base.OnUnloaded();

        //    App.Instance.ApplicationWindow.PageContent.Visibility = Visibility.Visible;
        //    App.Instance.ApplicationWindow.WindowBackgroundContent.Visibility = Visibility.Visible;
        //}
    }
}
