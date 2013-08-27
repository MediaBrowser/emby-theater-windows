using System.Windows;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// Class BaseModalWindow
    /// </summary>
    public class BaseModalWindow : BaseWindow
    {
        /// <summary>
        /// Shows the modal.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void ShowModal(Window owner)
        {
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
