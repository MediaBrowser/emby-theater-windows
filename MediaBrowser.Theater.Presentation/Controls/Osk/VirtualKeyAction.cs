using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_Visual", Type = typeof (ContentPresenter))]
    public class VirtualKeyAction : Control
    {
        private readonly VirtualKeyboard _keyboard;
        private readonly KeySet _keySet;
        private readonly KeyActionPair _pair;

        private ContentPresenter _visualControl;

        static VirtualKeyAction()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKeyAction), new FrameworkPropertyMetadata(typeof (VirtualKeyAction)));
        }

        public VirtualKeyAction(VirtualKeyboard keyboard, KeySet keySet, KeyActionPair pair)
        {
            _keyboard = keyboard;
            _keySet = keySet;
            _pair = pair;

            _keyboard.IsShiftHeldChanged += IsShiftHeldChanged;
            _keyboard.KeySetChanged += KeySetChanged;
        }

        void KeySetChanged(object sender, System.EventArgs e)
        {
            var keyboard = sender as VirtualKeyboard;
            if (keyboard != null && keyboard.KeySet != _keySet)
            {
                _keyboard.IsShiftHeldChanged -= IsShiftHeldChanged;
                _keyboard.KeySetChanged -= KeySetChanged;
            }
        }

        private void IsShiftHeldChanged(object sender, System.EventArgs e)
        {
            if (VisualControl != null)
                VisualControl.Content = Text;
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

        public override void OnApplyTemplate()
        {
            VisualControl = GetTemplateChild("PART_Visual") as ContentPresenter;
            base.OnApplyTemplate();
        }
    }
}