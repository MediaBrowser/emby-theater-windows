using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_Keys", Type = typeof (ItemsControl))]
    public class AlternateKeySelection
        : Control
    {
        private readonly List<VirtualKeyAction> _keys;
        private ItemsControl _keyCollection;

        private int _selectedKey;

        public AlternateKeySelection(VirtualKeyAction defaultKey, IEnumerable<VirtualKeyAction> alternativeKeys)
        {
            _keys = new List<VirtualKeyAction>();
            _keys.Add(defaultKey);
            _keys.AddRange(alternativeKeys);

            foreach (var key in _keys)
            {
                key.PressedChanged += () =>
                {
                    if (!key.IsPressed)
                        Close();
                };
            }
        }

        public ItemsControl KeyCollection
        {
            get { return _keyCollection; }
            set
            {
                _keyCollection = value;

                if (_keyCollection != null)
                    _keyCollection.ItemsSource = _keys;
            }
        }

        public override void OnApplyTemplate()
        {
            KeyCollection = GetTemplateChild("PART_Keys") as ItemsControl;

            base.OnApplyTemplate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                Left();
                e.Handled = true;
            }

            if (e.Key == Key.Right)
            {
                Right();
                e.Handled = true;
            }

            if (e.Key == Key.Escape || e.Key == Key.BrowserBack)
            {
                Close();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        private void Close()
        {
            var popup = Parent as Popup;
            if (popup != null)
                popup.IsOpen = false;
        }

        private void Left()
        {
            if (FlowDirection == FlowDirection.LeftToRight)
                FocusKey(_selectedKey - 1);
            else
                FocusKey(_selectedKey + 1);
        }

        private void Right()
        {
            if (FlowDirection == FlowDirection.LeftToRight)
                FocusKey(_selectedKey + 1);
            else
                FocusKey(_selectedKey - 1);
        }

        private void FocusKey(int i)
        {
            i %= _keys.Count;
            while (i < 0)
                i += _keys.Count;

            _selectedKey = i;

            _keys[_selectedKey].Focus();
        }

        public void FocusSelectedKey()
        {
            FocusKey(_selectedKey);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (_keys.Any(k => k.IsFocused))
                Focus();
        }
    }
}