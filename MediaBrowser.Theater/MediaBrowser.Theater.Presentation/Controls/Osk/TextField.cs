using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class TextField : Control
    {
        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (TextField), new PropertyMetadata("", OnTextChanged));

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (TextField), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for IsMasked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaskedProperty =
            DependencyProperty.Register("IsMasked", typeof (bool), typeof (TextField), new PropertyMetadata(false, OnTextChanged));


        // Using a DependencyProperty as the backing store for VisibleText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibleTextProperty =
            DependencyProperty.Register("VisibleText", typeof (string), typeof (TextField), new PropertyMetadata(""));


        // Using a DependencyProperty as the backing store for TextDisabledBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextDisabledBrushProperty =
            DependencyProperty.Register("TextDisabledBrush", typeof (Brush), typeof (TextField), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(133, 133, 133))));


        // Using a DependencyProperty as the backing store for BackgroundHoverBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundHoverBrushProperty =
            DependencyProperty.Register("BackgroundHoverBrush", typeof (Brush), typeof (TextField), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(62, 62, 66))));


        // Using a DependencyProperty as the backing store for TextHoverBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextHoverBrushProperty =
            DependencyProperty.Register("TextHoverBrush", typeof (Brush), typeof (TextField), new PropertyMetadata(new SolidColorBrush(Colors.White)));


        // Using a DependencyProperty as the backing store for BorderHoverBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderHoverBrushProperty =
            DependencyProperty.Register("BorderHoverBrush", typeof (Brush), typeof (TextField), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(62, 62, 66))));


        // Using a DependencyProperty as the backing store for Watermark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof (string), typeof (TextField), new PropertyMetadata(null));


        static TextField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TextField), new FrameworkPropertyMetadata(typeof (TextField)));
        }

        public string Watermark
        {
            get { return (string) GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public Brush TextDisabledBrush
        {
            get { return (Brush) GetValue(TextDisabledBrushProperty); }
            set { SetValue(TextDisabledBrushProperty, value); }
        }

        public Brush BackgroundHoverBrush
        {
            get { return (Brush) GetValue(BackgroundHoverBrushProperty); }
            set { SetValue(BackgroundHoverBrushProperty, value); }
        }

        public Brush TextHoverBrush
        {
            get { return (Brush) GetValue(TextHoverBrushProperty); }
            set { SetValue(TextHoverBrushProperty, value); }
        }

        public Brush BorderHoverBrush
        {
            get { return (Brush) GetValue(BorderHoverBrushProperty); }
            set { SetValue(BorderHoverBrushProperty, value); }
        }

        public bool IsMasked
        {
            get { return (bool) GetValue(IsMaskedProperty); }
            set { SetValue(IsMaskedProperty, value); }
        }

        public string VisibleText
        {
            get { return (string) GetValue(VisibleTextProperty); }
            set { SetValue(VisibleTextProperty, value); }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var field = d as TextField;
            if (field == null) {
                return;
            }

            if (field.IsMasked && !string.IsNullOrEmpty(field.Text)) {
                field.VisibleText = "**********";
            } else {
                field.VisibleText = field.Text;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                ShowVirtualKeyboard();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            ShowVirtualKeyboard();
            e.Handled = true;

            base.OnMouseDown(e);
        }

        private void ShowVirtualKeyboard()
        {
            Window window = Window.GetWindow(this);
            var keyboard = new VirtualKeyboardModalWindow(Title, Text);

            keyboard.Accepted += () => Text = keyboard.Text;

            keyboard.ShowModal(window);
        }
    }
}