using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;
using Key = System.Windows.Input.Key;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof (TextBox))]
    [TemplatePart(Name = "PART_Keyboard", Type = typeof (VirtualKeyboard))]
    [TemplatePart(Name = "PART_HideTransition", Type = typeof (Storyboard))]
    public class VirtualKeyboardTextField : Control
    {
        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (VirtualKeyboardTextField), new PropertyMetadata(""));

        private KeySet _defaultKeySet;
        private VirtualKeyboard _keyboard;
        private MinimalTextBox _textBox;


        static VirtualKeyboardTextField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VirtualKeyboardTextField), new FrameworkPropertyMetadata(typeof (VirtualKeyboardTextField)));
        }

        public VirtualKeyboardTextField()
        {
            Loaded += (s, e) =>
            {
                if (DefaultKeySet != null)
                    Keyboard.FocusSelectedKey();
            };
        }

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public KeySet DefaultKeySet
        {
            get { return _defaultKeySet; }
            set
            {
                _defaultKeySet = value;
                if (Keyboard != null && Keyboard.KeySet == null)
                    Keyboard.KeySet = _defaultKeySet;
            }
        }

        private MinimalTextBox TextBox
        {
            get { return _textBox; }
            set
            {
                UnbindKeyboardEvents();
                _textBox = value;
                BindKeyboardEvents();
            }
        }

        private VirtualKeyboard Keyboard
        {
            get { return _keyboard; }
            set
            {
                UnbindKeyboardEvents();

                _keyboard = value;
                if (_keyboard != null && _keyboard.KeySet == null)
                    _keyboard.KeySet = DefaultKeySet;

                BindKeyboardEvents();
            }
        }

        private Storyboard HideTransition { get; set; }

        public string Text
        {
            get
            {
                if (TextBox != null)
                    return TextBox.Text;

                return null;
            }
            set
            {
                if (TextBox != null)
                    TextBox.Text = value;
            }
        }

        public int CaretIndex
        {
            get
            {
                if (TextBox != null)
                    return TextBox.VirtualCaretIndex;

                return 0;
            }
            set
            {
                if (TextBox != null)
                    TextBox.VirtualCaretIndex = value;
            }
        }

        public event Action Accepted;

        public virtual void Accept()
        {
            Action sendEvent = () =>
            {
                Action handler = Accepted;
                if (handler != null) handler();
            };

            if (HideTransition != null)
            {
                Storyboard transition = HideTransition.Clone();

                transition.Completed += (s, e) => sendEvent();
                transition.Begin(this, Template);
            }
            else
                sendEvent();
        }

        public event Action Cancelled;

        public virtual void Cancel()
        {
            Action sendEvent = () =>
            {
                Action handler = Cancelled;
                if (handler != null) handler();
            };

            if (HideTransition != null)
            {
                Storyboard transition = HideTransition.Clone();

                transition.Completed += (s, e) => sendEvent();
                transition.Begin(this, Template);
            }
            else
                sendEvent();
        }

        private void BindKeyboardEvents()
        {
            if (_keyboard != null && TextBox != null)
            {
                _keyboard.TextEntered += EnableVirtualKeyboardOnTextEntered;
                _keyboard.CaretMovedLeft += EnableVirtualKeyboard;
                _keyboard.CaretMovedRight += EnableVirtualKeyboard;
                _keyboard.DeleteRequested += EnableVirtualKeyboard;
                _keyboard.BackspaceRequested += EnableVirtualKeyboard;
                _keyboard.Accepted += Accept;
                _keyboard.Cancelled += Cancel;

                _keyboard.TextEntered += TextBox.EnterText;
                _keyboard.CaretMovedLeft += TextBox.Left;
                _keyboard.CaretMovedRight += TextBox.Right;
                _keyboard.DeleteRequested += TextBox.Delete;
                _keyboard.BackspaceRequested += TextBox.Backspace;
            }
        }

        private void UnbindKeyboardEvents()
        {
            if (_keyboard != null && TextBox != null)
            {
                _keyboard.TextEntered -= TextBox.EnterText;
                _keyboard.CaretMovedLeft -= TextBox.Left;
                _keyboard.CaretMovedRight -= TextBox.Right;
                _keyboard.DeleteRequested -= TextBox.Delete;
                _keyboard.BackspaceRequested -= TextBox.Backspace;
                _keyboard.Accepted -= Accept;
                _keyboard.Cancelled -= Cancel;

                _keyboard.TextEntered -= EnableVirtualKeyboardOnTextEntered;
                _keyboard.CaretMovedLeft -= EnableVirtualKeyboard;
                _keyboard.CaretMovedRight -= EnableVirtualKeyboard;
                _keyboard.DeleteRequested -= EnableVirtualKeyboard;
                _keyboard.BackspaceRequested -= EnableVirtualKeyboard;
            }
        }

        private void EnableVirtualKeyboardOnTextEntered(string text)
        {
            EnableVirtualKeyboard();
        }

        private void EnableVirtualKeyboard()
        {
            if (Keyboard != null && !Keyboard.IsKeyFocused)
                Keyboard.Focus();
        }

        public override void OnApplyTemplate()
        {
            TextBox = GetTemplateChild("PART_TextBox") as MinimalTextBox;
            Keyboard = GetTemplateChild("PART_Keyboard") as VirtualKeyboard;
            HideTransition = Template.Resources["PART_HideTransition"] as Storyboard;

            base.OnApplyTemplate();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (TextBox != null && !TextBox.IsFocused && !IsVirtualKeyboardControlKey(e.Key))
            {
                TextBox.Focus();
                TextBox.RaiseEvent(e);
            }

            if (TextBox != null && TextBox.IsFocused)
            {
                if (IsUpDownKey(e.Key))
                {
                    if (Keyboard != null)
                        Keyboard.FocusSelectedKey();

                    e.Handled = true;
                }

                if (e.Key == Key.Enter)
                {
                    Accept();
                    e.Handled = true;
                }

                if (e.Key == Key.Escape)
                {
                    Cancel();
                    e.Handled = false;
                }
            }

            base.OnPreviewKeyDown(e);
        }

        private bool IsUpDownKey(Key key)
        {
            return key == Key.Up || key == Key.Down;
        }

        private bool IsVirtualKeyboardControlKey(Key key)
        {
            switch (key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                case Key.Enter:
                case Key.Escape:
                    return true;
                default:
                    return false;
            }
        }
    }
}