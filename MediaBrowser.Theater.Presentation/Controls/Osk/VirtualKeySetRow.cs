using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_Keys", Type = typeof (ItemsControl))]
    public class VirtualKeySetRow : Control
    {
        private readonly KeySetRow _row;

        private IEnumerable<VirtualKey> _keys;
        private ItemsControl _keysControl;

        static VirtualKeySetRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKeySetRow), new FrameworkPropertyMetadata(typeof (VirtualKeySetRow)));
        }

        public VirtualKeySetRow(VirtualKeyboard keyboard, KeySet keySet, KeySetRow row)
        {
            _row = row;
            Keys = row.Select(key => new VirtualKey(keyboard, keySet, row, key)).ToArray();
        }

        public double HorizontalOffset
        {
            get { return _row.HorizontalOffset; }
        }

        private ItemsControl KeysControl
        {
            get { return _keysControl; }
            set
            {
                _keysControl = value;

                if (_keysControl != null)
                {
                    _keysControl.ItemsSource = Keys;
                }
            }
        }

        public IEnumerable<VirtualKey> Keys
        {
            get { return _keys; }
            set
            {
                _keys = value;

                if (KeysControl != null)
                    KeysControl.ItemsSource = _keys;
            }
        }

        public override void OnApplyTemplate()
        {
            KeysControl = GetTemplateChild("PART_Keys") as ItemsControl;
            base.OnApplyTemplate();
        }
    }
}