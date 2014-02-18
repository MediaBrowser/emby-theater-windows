using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace MediaBrowser.Theater.Presentation.Behaviors
{
    public class FocusChildOnFocused
        : Behavior<FrameworkElement>
    {
        private IInputElement _lastFocused;

        public FocusNavigationDirection NavigationDirection { get; set; }

        public FocusChildOnFocused()
        {
            NavigationDirection = FocusNavigationDirection.First;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.GotFocus += Focused;
            AssociatedObject.PreviewGotKeyboardFocus += GotKeyboardFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.GotFocus -= Focused;
            AssociatedObject.PreviewGotKeyboardFocus -= GotKeyboardFocus;
        }

        private void Focused(object sender, RoutedEventArgs args)
        {
            FocusManager.SetIsFocusScope(AssociatedObject, true);

            if (_lastFocused == null) {
                AssociatedObject.MoveFocus(new TraversalRequest(NavigationDirection));
            } else {
                FocusManager.SetFocusedElement(AssociatedObject, _lastFocused);
            }
        }

        private void GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _lastFocused = e.NewFocus;
        }
    }
}