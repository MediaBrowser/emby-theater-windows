using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class DragBar : Control
    {
        private Window _window;

        static DragBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (DragBar), new FrameworkPropertyMetadata(typeof (DragBar)));
        }

        public DragBar()
        {
            Loaded += DragBar_Loaded;
            MouseDown += DragBar_MouseDown;
            Focusable = false;
        }

        private void DragBar_Loaded(object sender, RoutedEventArgs e)
        {
            _window = Window.GetWindow(this);
        }

        private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_window == null) {
                return;
            }

            if (e.ClickCount == 2) {
                _window.WindowState = _window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            } else if (e.LeftButton == MouseButtonState.Pressed) {
                _window.DragMove();
            }
        }
    }
}