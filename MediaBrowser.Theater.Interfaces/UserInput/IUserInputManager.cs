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
        /// Occurs when [key down].
        /// </summary>
        event KeyEventHandler KeyDown;

        /// <summary>
        /// Occurs when [mouse move].
        /// </summary>
        event MouseEventHandler MouseMove;
    }
}
