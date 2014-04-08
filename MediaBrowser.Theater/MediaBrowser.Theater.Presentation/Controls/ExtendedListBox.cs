using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class ExtendedListBox
        : ListBox
    {
        // Using a DependencyProperty as the backing store for WrapSelection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WrapSelectionProperty =
            DependencyProperty.Register("WrapSelection", typeof (bool), typeof (ExtendedListBox), new PropertyMetadata(true));


        public ExtendedListBox()
        {
            SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Continue);
            SelectionChanged += ExtendedListBox_SelectionChanged;
        }

        public bool WrapSelection
        {
            get { return (bool) GetValue(WrapSelectionProperty); }
            set { SetValue(WrapSelectionProperty, value); }
        }

        private void ExtendedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            bool horizontalScrollEnabled = ScrollViewer.GetHorizontalScrollBarVisibility(this) != ScrollBarVisibility.Disabled;
            bool verticalScrollEnabled = ScrollViewer.GetVerticalScrollBarVisibility(this) != ScrollBarVisibility.Disabled;

            // wrap selected index if we are at the end and wrapping is enabled
            if (WrapSelection && SelectedIndex != -1) {
                if (e.Key == Key.Left && SelectedIndex == 0 && horizontalScrollEnabled) {
                    SelectedIndex = Items.Count - 1;
                    FocusSelectedItem();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Right && SelectedIndex == Items.Count - 1 && horizontalScrollEnabled) {
                    SelectedIndex = 0;
                    FocusSelectedItem();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Up && SelectedIndex == 0 && verticalScrollEnabled) {
                    SelectedIndex = Items.Count - 1;
                    FocusSelectedItem();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Down && SelectedIndex == Items.Count - 1 && verticalScrollEnabled) {
                    SelectedIndex = 0;
                    FocusSelectedItem();
                    e.Handled = true;
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