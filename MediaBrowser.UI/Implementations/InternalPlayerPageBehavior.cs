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
        /// Initializes a new instance of the <see cref="InternalPlayerPageBehavior"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        public InternalPlayerPageBehavior(Page page)
        {
            _page = page;
        }

        /// <summary>
        /// Adjusts the presentation for playback.
        /// </summary>
        public void AdjustPresentationForPlayback()
        {
            _page.Loaded += _page_Loaded;
            _page.Unloaded += _page_Unloaded;
        }

        /// <summary>
        /// Handles the Loaded event of the _page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void _page_Loaded(object sender, RoutedEventArgs e)
        {
            App.Instance.ApplicationWindow.WindowBackgroundContent.Visibility = Visibility.Collapsed;
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
            
            App.Instance.ApplicationWindow.WindowBackgroundContent.Visibility = Visibility.Visible;
        }
    }
}
