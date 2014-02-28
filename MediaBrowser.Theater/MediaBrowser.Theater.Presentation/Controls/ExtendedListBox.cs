using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class ExtendedListBox
        : ListBox
    {


        public bool WrapSelection
        {
            get { return (bool)GetValue(WrapSelectionProperty); }
            set { SetValue(WrapSelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WrapSelection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WrapSelectionProperty =
            DependencyProperty.Register("WrapSelection", typeof(bool), typeof(ExtendedListBox), new PropertyMetadata(true));


        

        public ExtendedListBox()
        {
            SelectionChanged += ExtendedListBox_SelectionChanged;
        }

        void ExtendedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var container = ItemContainerGenerator.ContainerFromItem(SelectedItem) as FrameworkElement;
            if (container != null) {
                container.BringIntoView();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled || e.OriginalSource.Equals(this)) {
                base.OnKeyDown(e);
                return;
            }

            var horizontalScrollEnabled = ScrollViewer.GetHorizontalScrollBarVisibility(this) != ScrollBarVisibility.Disabled;
            var verticalScrollEnabled = ScrollViewer.GetVerticalScrollBarVisibility(this) != ScrollBarVisibility.Disabled;

            // wrap selected index if we are at the end and wrapping is enabled
            if (WrapSelection) {
                if (e.Key == Key.Left && SelectedIndex == 0 && horizontalScrollEnabled) {
                    SelectedIndex = Items.Count - 1;
                    FocusSelectedItem();
                    return;
                }

                if (e.Key == Key.Right && SelectedIndex == Items.Count - 1 && horizontalScrollEnabled) {
                    SelectedIndex = 0;
                    FocusSelectedItem();
                    return;
                }

                if (e.Key == Key.Up && SelectedIndex == 0 && verticalScrollEnabled) {
                    SelectedIndex = Items.Count - 1;
                    FocusSelectedItem();
                    return;
                }

                if (e.Key == Key.Down && SelectedIndex == Items.Count - 1 && verticalScrollEnabled) {
                    SelectedIndex = 0;
                    FocusSelectedItem();
                    return;
                }
            }

            // navigate out of the control on left/right if horizontal scrolling is disabled
            if (!horizontalScrollEnabled) {
                if (e.Key == Key.Left) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    return;
                }

                if (e.Key == Key.Right) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    return;
                }
            }

            // navigate out of the control on up/down if vertical scrolling is disabled
            if (!verticalScrollEnabled) {
                if (e.Key == Key.Up) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    return;
                }

                if (e.Key == Key.Down) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    return;
                }
            }

            // Let the base class do it's thing
            base.OnKeyDown(e);
        }

        private void FocusSelectedItem()
        {
            var container = ItemContainerGenerator.ContainerFromItem(SelectedItem) as FrameworkElement;
            if (container != null) {
                container.Focus();
            }
        }
    }
}
