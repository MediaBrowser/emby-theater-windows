using Gma.UserActivityMonitor;
using MediaBrowser.Theater.Interfaces.UserInput;
using System.Windows.Forms;

namespace MediaBrowser.Theater.Implementations.UserInput
{
    /// <summary>
    /// Class UserInputManager
    /// </summary>
    public class UserInputManager : IUserInputManager
    {
        /// <summary>
        /// Occurs when [key down].
        /// </summary>
        public event KeyEventHandler KeyDown
        {
            add
            {
                HookManager.KeyDown += value;
            }
            remove
            {
                HookManager.KeyDown -= value;
            }
        }

        /// <summary>
        /// Occurs when [mouse move].
        /// </summary>
        public event MouseEventHandler MouseMove
        {
            add
            {
                HookManager.MouseMove += value;
            }
            remove
            {
                HookManager.MouseMove -= value;
            }
        }
    }
}
