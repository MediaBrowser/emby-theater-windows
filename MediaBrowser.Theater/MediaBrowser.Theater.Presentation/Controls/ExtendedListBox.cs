using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class ExtendedListBox
        : ListBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled || e.OriginalSource.Equals(this)) {
                base.OnKeyDown(e);
                return;
            }

            // Don't eat left/right if horizontal scrolling is disabled
            if (ScrollViewer.GetHorizontalScrollBarVisibility(this) == ScrollBarVisibility.Disabled) {
                if (e.Key == Key.Left) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    return;
                }

                if (e.Key == Key.Right) {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    return;
                }
            }

            // Don't eat up/down if vertical scrolling is disabled
            if (ScrollViewer.GetVerticalScrollBarVisibility(this) == ScrollBarVisibility.Disabled) {
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
    }
}
