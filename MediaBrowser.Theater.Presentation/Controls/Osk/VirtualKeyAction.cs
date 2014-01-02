using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;
using Key = System.Windows.Input.Key;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_Visual", Type = typeof (ContentPresenter))]
    public class VirtualKeyAction : Control
    {
        // Using a DependencyProperty as the backing store for IsPressed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register("IsPressed", typeof(bool), typeof(VirtualKeyAction), new PropertyMetadata(false, OnPressedChanged));

        private readonly KeySet _keySet;
        private readonly VirtualKeyboard _keyboard;
        private readonly KeyActionPair _pair;
        private readonly double _width;

        private ContentPresenter _visualControl;

        public event Action Executed;
        public event Action PressedChanged;

        static VirtualKeyAction()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKeyAction), new FrameworkPropertyMetadata(typeof (VirtualKeyAction)));
        }

        public VirtualKeyAction(VirtualKeyboard keyboard, KeySet keySet, KeyActionPair pair, double keyWidth)
        {
            _keyboard = keyboard;
            _keySet = keySet;
            _pair = pair;
            _width = keyWidth;

            _keyboard.IsShiftHeldChanged += IsShiftHeldChanged;
            _keyboard.KeySetChanged += KeySetChanged;
        }

        public double KeyWidth
        {
            get { return _width; }
        }

        public bool IsPressed
        {
            get { return (bool) GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        private ContentPresenter VisualControl
        {
            get { return _visualControl; }
            set
            {
                _visualControl = value;
                if (_visualControl != null)
                    _visualControl.Content = Text;
            }
        }

        public KeyAction ActiveAction
        {
            get { return _keyboard.IsShiftHeld ? _pair.Alternative : _pair.Default; }
        }

        public string Text
        {
            get { return ActiveAction.GetVisual(_keyboard); }
        }

        private static void OnPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = d as VirtualKeyAction;
            if (key != null)
            {
                if (key.IsPressed)
                    key.Execute();

                Action handler = key.PressedChanged;
                if (handler != null) handler();
            }
        }

        private void Execute()
        {
            ActiveAction.Execute(_keyboard);

            var handler = Executed;
            if (handler != null) handler();
        }

        private void KeySetChanged(object sender, EventArgs e)
        {
            var keyboard = sender as VirtualKeyboard;
            if (keyboard != null && keyboard.KeySet != _keySet)
            {
                _keyboard.IsShiftHeldChanged -= IsShiftHeldChanged;
                _keyboard.KeySetChanged -= KeySetChanged;
            }
        }

        private void IsShiftHeldChanged(object sender, EventArgs e)
        {
            if (VisualControl != null)
                VisualControl.Content = Text;
        }

        public override void OnApplyTemplate()
        {
            VisualControl = GetTemplateChild("PART_Visual") as ContentPresenter;
            base.OnApplyTemplate();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
                IsPressed = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
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
            if (e.Key == Key.Enter || e.Key == Key.Return)
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
    }
}