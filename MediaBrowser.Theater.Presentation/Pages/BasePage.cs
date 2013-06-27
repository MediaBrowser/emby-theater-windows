using System;
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

            NavigationService.Navigating -= NavigationService_Navigating;
            NavigationService.Navigating += NavigationService_Navigating;

            if (_lastFocused == null)
            {
                FocusOnFirstLoad();
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
            if (e.Content.Equals(this))
            {
                FocusManager.SetFocusedElement(this, _lastFocused);
            }
            else
            {
                _lastFocused = FocusManager.GetFocusedElement(this);
            }
        }
    }
}
