using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Theater.Interfaces.UserInput;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// Class BaseModalWindow
    /// </summary>
    public class BaseModalWindow : BaseWindow
    {
        IUserInputManager _userInputManager;

        // KeyDown Events are not being routed to the application main window for modal dialogs fro some reason
        // so we will eplictily route then to the InputManager
        //
        // TODO - replace with addhandler to add a routed event handler so we can forward it to
        // input manager only if routed event leave hadled  = false (currently tab are arrows use go via routed events)
        protected override void OnKeyDown(KeyEventArgs e)
        {
              base.OnKeyDown(e);

              if (_userInputManager != null && ! e.Handled)
              {
                _userInputManager.OnKeyDown(e);
              }
        }

        /// <summary>
        /// Shows the modal.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void ShowModal(Window owner, IUserInputManager userInputManager = null)
        {
            _userInputManager = userInputManager;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            AllowsTransparency = true;

            Width = owner.Width;
            Height = owner.Height;
            Top = owner.Top;
            Left = owner.Left;
            WindowState = owner.WindowState;
            Owner = owner;

     
            ShowDialog();

            owner.Activate();
        }

        /// <summary>
        /// Called when [browser back].
        /// </summary>
        protected override void OnBrowserBack()
        {
            base.OnBrowserBack();

            CloseModal();
        }

        /// <summary>
        /// Closes the modal.
        /// </summary>
        protected virtual void CloseModal()
        {
            Close();
        }
    }
}
