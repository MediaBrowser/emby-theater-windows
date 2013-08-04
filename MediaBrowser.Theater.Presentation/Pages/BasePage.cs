using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MediaBrowser.Theater.Presentation.Pages
{
    /// <summary>
    /// Class BasePage
    /// </summary>
    public abstract class BasePage : Page
    {
        /// <summary>
        /// The _last focused
        /// </summary>
        private IInputElement _lastFocused;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += BasePage_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the BasePage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BasePage_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetIsFocusScope(this, true);

            var nav = NavigationService;

            nav.Navigating -= NavigationService_Navigating;
            nav.Navigating += NavigationService_Navigating;

            nav.Navigated -= NavigationService_Navigated;
            nav.Navigated += NavigationService_Navigated;

            if (_lastFocused == null)
            {
                FocusOnFirstLoad();
            }
        }

        void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content.Equals(this))
            {
                FocusManager.SetFocusedElement(this, _lastFocused);
            }
        }

        /// <summary>
        /// Focuses the on first load.
        /// </summary>
        protected virtual void FocusOnFirstLoad()
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        /// <summary>
        /// Handles the Navigating event of the NavigationService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs"/> instance containing the event data.</param>
        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            _lastFocused = FocusManager.GetFocusedElement(this);
        }
    }
}
