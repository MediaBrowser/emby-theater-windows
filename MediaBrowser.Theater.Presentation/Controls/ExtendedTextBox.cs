using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// Class ExtendedTextBox
    /// </summary>
    public class ExtendedTextBox : TextBox
    {
        /// <summary>
        /// Invoked whenever an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">Provides data about the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
            {
                base.OnKeyDown(e);
                return;
            }

            // Eat this so we don't affect navigation
            if (e.Key == Key.Back)
            {
                e.Handled = true;
            }

            // Let the base class do it's thing
            base.OnKeyDown(e);
        }
    }
}
