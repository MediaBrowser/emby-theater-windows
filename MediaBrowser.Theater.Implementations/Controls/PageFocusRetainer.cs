using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MediaBrowser.Theater.Implementations.Controls
{
    /// <summary>
    /// Allows a page to remember the focused element when navigating back and forward
    /// </summary>
    public class PageFocusRetainer
    {
        /// <summary>
        /// The _last focused
        /// </summary>
        private IInputElement _lastFocused;

        /// <summary>
        /// The _page
        /// </summary>
        private readonly Page _page;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageFocusRetainer"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        public PageFocusRetainer(Page page)
        {
            _page = page;
        }

        public void RetainFocusAfterNavigation()
        {
            _page.Loaded += page_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void page_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetIsFocusScope(_page, true);

            _page.NavigationService.Navigating -= NavigationService_Navigating;
            _page.NavigationService.Navigating += NavigationService_Navigating;
        }

        /// <summary>
        /// Handles the Navigating event of the NavigationService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs"/> instance containing the event data.</param>
        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content.Equals(this))
            {
                FocusManager.SetFocusedElement(_page, _lastFocused);
            }
            else
            {
                _lastFocused = FocusManager.GetFocusedElement(_page);
            }
        }
    }
}
