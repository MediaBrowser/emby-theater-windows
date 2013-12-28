using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout;
using Key = System.Windows.Input.Key;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OnScreenKeyboard.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OnScreenKeyboard.Controls;assembly=OnScreenKeyboard.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:VirtualKeyboardWindow/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Keyboard", Type = typeof(VirtualKeyboard))]
    [TemplatePart(Name = "PART_HideTransition", Type=typeof(Storyboard))]
    public class VirtualKeyboardTextField : Control
    {
        private MinimalTextBox _textBox;
        private VirtualKeyboard _keyboard;
        private Storyboard _hideTransition;
        private KeySet _defaultKeySet;
        
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VirtualKeyboardTextField), new PropertyMetadata(""));

        
        static VirtualKeyboardTextField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualKeyboardTextField), new FrameworkPropertyMetadata(typeof(VirtualKeyboardTextField)));
        }

        public VirtualKeyboardTextField()
        {
            Loaded += (s, e) =>
            {
                if (DefaultKeySet != null)
                    Keyboard.FocusSelectedKey();
            };
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
                var transition = HideTransition.Clone();

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
                var transition = HideTransition.Clone();

                transition.Completed += (s, e) => sendEvent();
                transition.Begin(this, Template);
            }
            else
                sendEvent();
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
            get { return _keyboard;  }
            set
            {
                UnbindKeyboardEvents();
                
                _keyboard = value;
                if (_keyboard != null && _keyboard.KeySet == null)
                    _keyboard.KeySet = DefaultKeySet;

                BindKeyboardEvents();
            }
        }

        private Storyboard HideTransition
        {
            get { return _hideTransition; }
            set { _hideTransition = value; }
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
