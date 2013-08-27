using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Behaviors
{
    public class SuppressBackKeyBehavior
    {
        public static bool GetSuppressBackKey(UIElement control)
        {
            return (bool)control.GetValue(SuppressBackKeyProperty);
        }

        public static void SetSuppressBackKey(
          UIElement control, bool value)
        {
            control.SetValue(SuppressBackKeyProperty, value);
        }

        public static readonly DependencyProperty SuppressBackKeyProperty =
            DependencyProperty.RegisterAttached(
            "SuppressBackKey",
            typeof(bool),
            typeof(SuppressBackKeyBehavior),
            new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var control = depObj as UIElement;
            if (control == null)
                return;

            if (e.NewValue is bool == false)
                return;

            var val = (bool) e.NewValue;

            control.KeyDown -= control_KeyDown;
            
            if (val)
            {
                control.KeyDown += control_KeyDown;
            }
        }

        static void control_KeyDown(object sender, KeyEventArgs e)
        {
            // Eat this so we don't affect navigation
            if (!e.Handled && e.Key == Key.Back)
            {
                e.Handled = true;
            }
        }
    }
}
