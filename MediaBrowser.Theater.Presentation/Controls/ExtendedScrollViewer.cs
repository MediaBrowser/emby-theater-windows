using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// This subclass solves the problem of ScrollViewers eating KeyDown for all arrow keys
    /// </summary>
    public class ExtendedScrollViewer : ScrollViewer
    {
        /// <summary>
        /// Responds to specific keyboard input and invokes associated scrolling behavior.
        /// </summary>
        /// <param name="e">Required arguments for this event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled || e.OriginalSource.Equals(this))
            {
                base.OnKeyDown(e);
                return;
            } 
            
            // Don't eat left/right if horizontal scrolling is disabled
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                if (HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled || HorizontalOffset.Equals(0))
                {
                    return;
                }
            }

            // Don't eat up/down if vertical scrolling is disabled
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled || VerticalOffset.Equals(0))
                {
                    return;
                }
            }
           
            // Let the base class do it's thing
            base.OnKeyDown(e);
        }

        internal void SetScrollInfo(IScrollInfo info)
        {
            ScrollInfo = info;
        }
    }
}
