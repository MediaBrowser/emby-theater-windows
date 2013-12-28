using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;
using Key = MediaBrowser.Theater.Presentation.Controls.Osk.Layout.Key;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_PrimaryAction", Type = typeof (ContentPresenter))]
    public class VirtualKey : Control
    {
        // Using a DependencyProperty as the backing store for IsPressed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register("IsPressed", typeof (bool), typeof (VirtualKey), new PropertyMetadata(false, OnPressedChanged));

        private static void OnPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = d as VirtualKey;
            if (key != null && key.IsPressed)
                key.Execute();
        }

        private readonly Key _key;
        private readonly VirtualKeyboard _keyboard;

        private VirtualKeyAction _primaryAction;
        private ContentPresenter _primaryActionControl;

        private void Execute()
        {
            PrimaryAction.ActiveAction.Execute(_keyboard);
        }

        static VirtualKey()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKey),
                new FrameworkPropertyMetadata(typeof (VirtualKey)));
        }

        public VirtualKey(VirtualKeyboard keyboard, KeySet keySet, Key key)
        {
            _keyboard = keyboard;
            _key = key;

            PrimaryAction = new VirtualKeyAction(keyboard, keySet, key.DefaultAction);
        }

        public bool IsPressed
        {
            get { return (bool) GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        private ContentPresenter PrimaryActionControl
        {
            get { return _primaryActionControl; }
            set
            {
                _primaryActionControl = value;

                if (_primaryActionControl != null)
                    _primaryActionControl.Content = PrimaryAction;
            }
        }

        public double KeyWidthScale
        {
            get { return _key.Width; }
        }

        public VirtualKeyAction PrimaryAction
        {
            get { return _primaryAction; }
            set
            {
                _primaryAction = value;

                if (PrimaryActionControl != null)
                    PrimaryActionControl.Content = _primaryAction;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _keyboard.Focus();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            IsPressed = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                IsPressed = true;
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            IsPressed = false;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                IsPressed = false;
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsPressed = false;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            IsPressed = false;
        }

        public override void OnApplyTemplate()
        {
            PrimaryActionControl = GetTemplateChild("PART_PrimaryAction") as ContentPresenter;
            base.OnApplyTemplate();
        }
    }
}