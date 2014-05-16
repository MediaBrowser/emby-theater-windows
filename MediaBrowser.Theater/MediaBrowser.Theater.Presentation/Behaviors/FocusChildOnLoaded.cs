using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace MediaBrowser.Theater.Presentation.Behaviors
{
    public class FocusChildOnLoaded
        : Behavior<FrameworkElement>
    {
        private IInputElement _lastFocused;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += Loaded;
            AssociatedObject.PreviewGotKeyboardFocus += GotKeyboardFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= Loaded;
            AssociatedObject.PreviewGotKeyboardFocus -= GotKeyboardFocus;
        }

        private async void Loaded(object sender, RoutedEventArgs args)
        {
            FocusManager.SetIsFocusScope(AssociatedObject, true);

            if (_lastFocused == null) {
                AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
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