using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;
using Key = MediaBrowser.Theater.Presentation.Controls.Osk.Layout.Key;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_PrimaryAction", Type = typeof (ContentPresenter))]
    public class VirtualKey : Control
    {
        // Using a DependencyProperty as the backing store for EnableAlternativesPopup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableAlternativesPopupProperty =
            DependencyProperty.Register("EnableAlternativesPopup", typeof (bool), typeof (VirtualKey), new PropertyMetadata(true));


        private readonly Key _key;
        private readonly KeySet _keySet;
        private readonly VirtualKeyboard _keyboard;
        private readonly FlowDirection _popupFlowDirection;
        private Popup _alternativesPopup;

        private VirtualKeyAction _primaryAction;
        private ContentPresenter _primaryActionControl;


        static VirtualKey()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKey),
                new FrameworkPropertyMetadata(typeof (VirtualKey)));
        }

        public VirtualKey(VirtualKeyboard keyboard, KeySet keySet, KeySetRow row, Key key)
        {
            _keyboard = keyboard;
            _keySet = keySet;
            _key = key;

            if (row.IndexOf(key) < row.Count/2)
                _popupFlowDirection = FlowDirection.LeftToRight;
            else
                _popupFlowDirection = FlowDirection.RightToLeft;

            PrimaryAction = new VirtualKeyAction(keyboard, keySet, key.DefaultAction, key.Width);
            Loaded += (s, e) =>
            {
                if (AlternativesPopup != null)
                {
                    if (_popupFlowDirection == FlowDirection.LeftToRight)
                    {
                        AlternativesPopup.HorizontalOffset = -ActualWidth;
                        AlternativesPopup.Placement = PlacementMode.Right;
                    }
                    else
                    {
                        AlternativesPopup.HorizontalOffset = ActualWidth;
                        AlternativesPopup.Placement = PlacementMode.Left;
                    }
                }
            };
        }

        public double KeyWidth
        {
            get { return _key.Width; }
        }

        public bool EnableAlternativesPopup
        {
            get { return (bool) GetValue(EnableAlternativesPopupProperty); }
            set { SetValue(EnableAlternativesPopupProperty, value); }
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

        private Popup AlternativesPopup
        {
            get { return _alternativesPopup; }
            set
            {
                if (_alternativesPopup != null)
                {
                    _alternativesPopup.Opened -= AlternativesPopupOpened;
                    _alternativesPopup.Closed -= AlternativesPopupClosed;
                }

                _alternativesPopup = value;

                if (_alternativesPopup != null)
                {
                    _alternativesPopup.HorizontalOffset = -ActualWidth;

                    _alternativesPopup.Opened += AlternativesPopupOpened;
                    _alternativesPopup.Closed += AlternativesPopupClosed;
                }
            }
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

        private void AlternativesPopupClosed(object sender, EventArgs e)
        {
            _keyboard.IsEnabled = true;
            _keyboard.FocusSelectedKey();
        }

        private void AlternativesPopupOpened(object sender, EventArgs e)
        {
            _keyboard.IsEnabled = false;

            var selection = AlternativesPopup.Child as AlternateKeySelection;
            if (selection != null)
                selection.FocusSelectedKey();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _keyboard.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Multiply || e.Key == System.Windows.Input.Key.D8)
            {
                ToggleAlternativesPopup();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                ToggleAlternativesPopup();
                e.Handled = true;
            }

            base.OnMouseDown(e);
        }

        private void ToggleAlternativesPopup()
        {
            if (AlternativesPopup.IsOpen)
                AlternativesPopup.IsOpen = false;
            else
            {
                if (!EnableAlternativesPopup || !_key.AlternativeActions.Any())
                    return;

                var defaultKey = new VirtualKeyAction(_keyboard, _keySet, _key.DefaultAction, KeyWidth);
                IEnumerable<VirtualKeyAction> alternativeKeys = _key.AlternativeActions.Select(a => new VirtualKeyAction(_keyboard, _keySet, a, KeyWidth));

                var alternativesSelection = new AlternateKeySelection(defaultKey, alternativeKeys) {
                    FlowDirection = _popupFlowDirection
                };

                AlternativesPopup.Child = alternativesSelection;
                AlternativesPopup.IsOpen = true;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            PrimaryAction.Focus();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PrimaryActionControl = GetTemplateChild("PART_PrimaryAction") as ContentPresenter;
            AlternativesPopup = GetTemplateChild("PART_AlternativesPopup") as Popup;
        }
    }
}