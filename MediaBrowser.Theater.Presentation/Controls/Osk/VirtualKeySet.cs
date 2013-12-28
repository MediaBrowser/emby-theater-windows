using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_Rows", Type = typeof (ItemsControl))]
    public class VirtualKeySet : Control
    {
        private IEnumerable<VirtualKeySetRow> _rows;
        private ItemsControl _rowsControl;

        static VirtualKeySet()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKeySet), new FrameworkPropertyMetadata(typeof (VirtualKeySet)));
        }

        public VirtualKeySet(VirtualKeyboard keyboard, KeySet keySet)
        {
            Rows = keySet.Select(row => new VirtualKeySetRow(keyboard, keySet, row)).ToArray();
        }

        private ItemsControl RowsControl
        {
            get { return _rowsControl; }
            set
            {
                _rowsControl = value;

                if (_rowsControl != null)
                    _rowsControl.ItemsSource = Rows;
            }
        }

        public IEnumerable<VirtualKeySetRow> Rows
        {
            get { return _rows; }
            set
            {
                _rows = value;

                if (RowsControl != null)
                    RowsControl.ItemsSource = _rows;
            }
        }

        public override void OnApplyTemplate()
        {
            RowsControl = GetTemplateChild("PART_Rows") as ItemsControl;
            base.OnApplyTemplate();
        }
    }
}