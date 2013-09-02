using System;
using System.Windows.Forms;

namespace MediaBrowser.Theater.Interfaces.UserInput
{
    /// <summary>
    /// Interface IUserInputManager
    /// </summary>
    public interface IUserInputManager
    {
        /// <summary>
        /// Gets the last input time.
        /// </summary>
        /// <returns>DateTime.</returns>
        DateTime GetLastInputTime();

        /// <summary>
        /// Occurs when [key down].
        /// </summary>
        event KeyEventHandler KeyDown;

        /// <summary>
        /// Occurs when [mouse move].
        /// </summary>
        event MouseEventHandler MouseMove;
    }
}
