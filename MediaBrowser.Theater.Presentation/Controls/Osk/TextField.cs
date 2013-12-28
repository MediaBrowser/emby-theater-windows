using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class TextField : Control
    {
        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (TextField), new PropertyMetadata(""));


        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (TextField), new PropertyMetadata(""));


        static TextField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TextField), new FrameworkPropertyMetadata(typeof (TextField)));
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
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