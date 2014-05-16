using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class MinimalTextBox
        : TextBox
    {
        static  MinimalTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MinimalTextBox), new FrameworkPropertyMetadata(typeof(MinimalTextBox)));
        }

        private readonly Timer _blinkTimer;
        private FrameworkElement _caretControl;
        private int _caretPosition;

        public MinimalTextBox()
        {
            _blinkTimer = new Timer { AutoReset = true, Interval = 750 };
            _blinkTimer.Elapsed += (s, e) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (CaretControl != null && !IsFocused)
                    {
                        if (CaretControl.Visibility == Visibility.Visible)
                            CaretControl.Visibility = Visibility.Hidden;
                        else
                            CaretControl.Visibility = Visibility.Visible;
                    }
                });
            };

            _blinkTimer.Start();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (CaretControl != null)
                CaretControl.Visibility = Visibility.Hidden;

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (CaretControl != null)
                CaretControl.Visibility = Visibility.Visible;

            CaretPosition = CaretIndex;
            
            base.OnLostFocus(e);
        }

        private FrameworkElement CaretControl
        {
            get { return _caretControl; }
            set
            {
                _caretControl = value;
                PlaceCaret();
            }
        }

        public override void OnApplyTemplate()
        {
            CaretControl = GetTemplateChild("PART_Caret") as FrameworkElement;

            base.OnApplyTemplate();
        }

        private int CaretPosition
        {
            get { return _caretPosition; }
            set
            {
                _caretPosition = ClampCaretPosition(value);

                PlaceCaret();

                if (CaretControl != null)
                {
                    _blinkTimer.Stop();
                    _blinkTimer.Start();
                    CaretControl.Visibility = Visibility.Visible;
                }

                CaretIndex = _caretPosition;
            }
        }

        public int VirtualCaretIndex
        {
            get
            {
                if (IsFocused)
                    return CaretIndex;

                return CaretPosition;
            }
            set
            {
                CaretIndex = value;
                CaretPosition = value;
            }
        }

        private int ClampCaretPosition(int caret)
        {
            if (caret < 0)
                caret = 0;
            if (caret > Text.Length)
                caret = Text.Length;

            return caret;
        }

        private void PlaceCaret()
        {
            if (CaretControl != null)
            {
                Size textSize = MeasureString(Text.Substring(0, CaretPosition));
                CaretControl.Margin = new Thickness(textSize.Width, 0, 0, 0);
            }
        }

        private Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public void EnterText(string text)
        {
            Text = Text.Insert(CaretPosition, text);
            CaretPosition += text.Length;
        }

        public void Left()
        {
            CaretPosition--;
        }

        public void Right()
        {
            CaretPosition++;
        }

        public void Home()
        {
            CaretPosition = 0;
        }

        public void End()
        {
            CaretPosition = Text.Length;
        }

        public void Backspace()
        {
            if (CaretPosition > 0)
            {
                CaretPosition--;
                Delete();
            }
        }

        public void Delete()
        {
            if (CaretPosition < Text.Length)
            {
                Text = Text.Remove(CaretPosition, 1);
                CaretIndex = CaretPosition;
            }
        }
    }
}
