using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;
using Key = System.Windows.Input.Key;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public interface IVirtualKeyboard
    {
        KeySet KeySet { get; set; }
        bool IsShiftHeld { get; }

        void EnterText(string text);
        void ToggleShift(bool capsLock = false);
        void Left();
        void Right();
        void Home();
        void End();
        void Delete();
        void Backspace();
        void Accept();
        void Cancel();

        event Action<string> TextEntered;
        event Action CaretMovedLeft;
        event Action CaretMovedRight;
        event Action CaretMovedHome;
        event Action CaretMovedEnd;
        event Action DeleteRequested;
        event Action BackspaceRequested;
        event Action Accepted;
        event Action Cancelled;
    }

    [TemplatePart(Name = "PART_Content", Type = typeof (ContentPresenter))]
    public class VirtualKeyboard : Control, IVirtualKeyboard
    {
        public static readonly DependencyProperty IsShiftHeldProperty =
            DependencyProperty.Register("IsShiftHeld", typeof (bool), typeof (VirtualKeyboard), new PropertyMetadata(false, OnShiftHeldChanged));

        public static readonly DependencyProperty KeySetProperty =
            DependencyProperty.Register("KeySet", typeof (KeySet), typeof (VirtualKeyboard), new PropertyMetadata(null, OnKeySetChanged));

        private ContentPresenter _contentPresenter;
        private bool _isCapsLock;
        private VirtualKey[][] _keyGrid;
        private int _selectedColumn;
        private int _selectedRow;
        private VirtualKeySet _virtualKeySet;
        
        static VirtualKeyboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKeyboard), new FrameworkPropertyMetadata(typeof (VirtualKeyboard)));
        }

        private Control Selected
        {
            get
            {
                Control[] row = _keyGrid[_selectedRow];
                int i = Clamp(_selectedColumn, 0, row.Length - 1);
                return row[i];
            }
        }

        private ContentPresenter ContentPresenter
        {
            get { return _contentPresenter; }
            set
            {
                _contentPresenter = value;

                if (_contentPresenter != null)
                    _contentPresenter.Content = _virtualKeySet;
            }
        }

        public bool IsKeyFocused
        {
            get { return _keyGrid != null && _keyGrid.Any(row => row.Any(key => key.PrimaryAction.IsFocused)); }
        }

        private VirtualKeySet VirtualKeySet
        {
            get { return _virtualKeySet; }
            set
            {
                var wasKeyFocused = IsKeyFocused;

                if (_virtualKeySet != null)
                    RemoveLogicalChild(_virtualKeySet);

                _virtualKeySet = value;

                if (_virtualKeySet != null)
                    AddLogicalChild(_virtualKeySet);

                if (ContentPresenter != null)
                    ContentPresenter.Content = _virtualKeySet;

                CalculateKeyGrid();

                if (wasKeyFocused)
                {
                    Focus();
                    FocusSelectedKey();
                }
            }
        }

        public KeySet KeySet
        {
            get { return (KeySet) GetValue(KeySetProperty); }
            set { SetValue(KeySetProperty, value); }
        }

        public bool IsShiftHeld
        {
            get { return (bool) GetValue(IsShiftHeldProperty); }
            set { SetValue(IsShiftHeldProperty, value); }
        }

        public void Left()
        {
            Action handler = CaretMovedLeft;
            if (handler != null) handler();
        }

        public void Right()
        {
            Action handler = CaretMovedRight;
            if (handler != null) handler();
        }

        public void Home()
        {
            Action handler = CaretMovedHome;
            if (handler != null) handler();
        }

        public void End()
        {
            Action handler = CaretMovedEnd;
            if (handler != null) handler();
        }

        public void Delete()
        {
            Action handler = DeleteRequested;
            if (handler != null) handler();
        }

        public void Backspace()
        {
            Action handler = BackspaceRequested;
            if (handler != null) handler();
        }

        public void Accept()
        {
            Action handler = Accepted;
            if (handler != null) handler();
        }

        public void Cancel()
        {
            Action handler = Cancelled;
            if (handler != null) handler();
        }

        public event Action<string> TextEntered;

        public event Action CaretMovedLeft;
        public event Action CaretMovedRight;
        public event Action CaretMovedHome;
        public event Action CaretMovedEnd;
        public event Action DeleteRequested;
        public event Action BackspaceRequested;
        public event Action Accepted;
        public event Action Cancelled;

        public void EnterText(string text)
        {
            if (IsShiftHeld && !_isCapsLock)
                IsShiftHeld = false;

            OnTextEntered(text);
        }

        public void ToggleShift(bool capsLock = false)
        {
            IsShiftHeld = !IsShiftHeld;
            _isCapsLock = capsLock;
        }

        public event EventHandler KeySetChanged;
        public event EventHandler IsShiftHeldChanged;

        protected virtual void OnTextEntered(string e)
        {
            Action<string> handler = TextEntered;
            if (handler != null) handler(e);
        }

        private static void OnKeySetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var keyboard = obj as VirtualKeyboard;
            if (keyboard != null)
            {
                var keyset = e.NewValue as KeySet;
                if (keyset != null)
                    keyboard.VirtualKeySet = new VirtualKeySet(keyboard, keyset);
                else
                    keyboard.VirtualKeySet = null;

                EventHandler handler = keyboard.KeySetChanged;
                if (handler != null) handler(keyboard, EventArgs.Empty);
            }
        }

        private static void OnShiftHeldChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var keyboard = obj as VirtualKeyboard;
            if (keyboard != null)
            {
                EventHandler handler = keyboard.IsShiftHeldChanged;
                if (handler != null) handler(keyboard, EventArgs.Empty);
            }
        }

//        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
//        {
//            base.OnGotKeyboardFocus(e);
//            FocusSelectedKey();
//        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsEnabled)
            {
                if (e.Key == Key.Left)
                {
                    MoveSelectionLeft();
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    MoveSelectionRight();
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    MoveSelectionUp();
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    MoveSelectionDown();
                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
        }

        private void CalculateKeyGrid()
        {
            if (_virtualKeySet == null)
            {
                _keyGrid = new VirtualKey[0][];
                return;
            }

            List<VirtualKeySetRow> rows = _virtualKeySet.Rows.ToList();

            _keyGrid = new VirtualKey[rows.Count][];
            for (int i = 0; i < rows.Count; i++)
            {
                VirtualKeySetRow row = rows[i];
                List<VirtualKey> keys = row.Keys.ToList();

                var width = (int) Math.Ceiling(row.HorizontalOffset + keys.Select(k => k.KeyWidth).Sum());

                _keyGrid[i] = new VirtualKey[width];

                double position = row.HorizontalOffset;
                int index = 0;
                for (int k = 0; k < keys.Count; k++)
                {
                    VirtualKey key = keys[k];
                    position += key.KeyWidth;
                    while (index < position - 0.5)
                    {
                        _keyGrid[i][index] = key;
                        index++;
                    }
                }
            }
        }

        public void MoveSelectionUp()
        {
            _selectedRow = Wrap(_selectedRow - 1, 0, _keyGrid.Length - 1);
            FocusSelectedKey();
        }

        public void MoveSelectionDown()
        {
            _selectedRow = Wrap(_selectedRow + 1, 0, _keyGrid.Length - 1);
            FocusSelectedKey();
        }

        public void MoveSelectionLeft()
        {
            if (IsRowASingleControl(_selectedRow))
                return;

            Control initial = Selected;
            while (initial == Selected)
            {
                _selectedColumn--;

                Control[] row = _keyGrid[_selectedRow];
                _selectedColumn = Wrap(_selectedColumn, 0, row.Length - 1);
            }

            FocusSelectedKey();
        }

        public void MoveSelectionRight()
        {
            if (IsRowASingleControl(_selectedRow))
                return;

            Control initial = Selected;
            while (initial == Selected)
            {
                _selectedColumn++;

                Control[] row = _keyGrid[_selectedRow];
                _selectedColumn = Wrap(_selectedColumn, 0, row.Length - 1);
            }

            FocusSelectedKey();
        }

        private bool IsRowASingleControl(int rowIndex)
        {
            Control[] row = _keyGrid[rowIndex];
            if (row.Length == 0)
                return true;

            return row.All(c => c == row[0]);
        }

        public void FocusSelectedKey()
        {
            Control selected = Selected;
            if (selected != null)
            {
                if (selected.IsLoaded)
                    selected.Focus();
                else
                {
                    RoutedEventHandler loaded = null;
                    loaded = (sender, e) =>
                    {
                        selected.Focus();
                        selected.Loaded -= loaded;
                    };

                    selected.Loaded += loaded;
                }
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;

            if (value > max)
                value = max;

            return value;
        }

        private static int Wrap(int value, int min, int max)
        {
            var range = max - min + 1;
            value -= min;

            while (value < 0) value += range;
            return (value % range) + min;
        }

        public override void OnApplyTemplate()
        {
            ContentPresenter = GetTemplateChild("PART_Content") as ContentPresenter;
            base.OnApplyTemplate();
        }
    }
}