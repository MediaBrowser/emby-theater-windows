using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsForms = System.Windows.Forms;
using WindowsInput = System.Windows.Input;

namespace MediaBrowser.Theater.Interfaces.UserInput
{
    public class AppCommandEventArgs
    {
        public int Cmd;
        public Boolean Handled = false;
    }

    public delegate void AppCommandEventHandler(object sender, AppCommandEventArgs appCommandEventArgs);


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
        /// Occurs when [key press], gloabl across all apps
        /// </summary>
        event WindowsForms.KeyPressEventHandler GlobalKeyPress;

        /// <summary>
        /// Occurs when [key down]. gloabl across all apps
        /// </summary>
        event WindowsForms.KeyEventHandler GlobalKeyDown;

        /// <summary>
        /// Occurs when [mouse move].
        /// </summary>
        event WindowsForms.MouseEventHandler MouseMove;

        /// <summary>
        /// Occurs when [key down]. Local to MBT
        /// </summary>
        event WindowsInput.KeyEventHandler KeyDown;

        /// <summary>
        /// Occurs when an AppCommand is sent to MBT Main window. Local to MBT
        /// </summary>
        event AppCommandEventHandler AppCommand;

        /// <summary>
        /// Route a KeyDown event via the input Manager
        /// </summary>
        void OnKeyDown(WindowsInput.KeyEventArgs e);
    }
}
