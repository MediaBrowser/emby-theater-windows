using System;
using WindowsForms = System.Windows.Forms;
using WindowsInput = System.Windows.Input;

namespace MediaBrowser.Theater.Interfaces.UserInput
{
    public class AppCommandEventArgs: EventArgs
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
        event WindowsForms.MouseEventHandler GlobalMouseMove;

        /// <summary>
        /// Occurs when [key down]. Local to MBT
        /// </summary>
        event WindowsInput.KeyEventHandler KeyDown;

        /// <summary>
        /// Occurs when [key down]. Local to MBT
        /// </summary>
        event WindowsInput.MouseEventHandler MouseMove;

        /// <summary>
        /// Occurs when an AppCommand is sent to MBT Main window. Local to MBT
        /// </summary>
        event AppCommandEventHandler AppCommand;

        /// <summary>
        /// Route a KeyDown event via the input Manager
        /// </summary>
        void OnKeyDown(WindowsInput.KeyEventArgs e);

        /// <summary>
        /// Route a mouse move event via the input Manager
        /// </summary>
        void OnMouseMove(WindowsInput.MouseEventArgs e);

        // <summary>
        // Send a Key to currently active element, 
        // </summary>
        void SendKeyDownEventToFocusedElement(WindowsInput.Key key);

        // <summary>
        // Send a text string to currently active element, 
        // </summary>
        void SendTextInputToFocusedElement(string inputText);
    }
}
