using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows.Forms.Integration;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class AppHiddenWIndow
    /// </summary>
    internal class AppHiddenWIndow : IHiddenWindow
    {
        /// <summary>
        /// Gets the windows forms host.
        /// </summary>
        /// <value>The windows forms host.</value>
        public WindowsFormsHost WindowsFormsHost
        {
            get { return App.Instance.HiddenWindow.WindowsFormsHost; }
        }
    }
}
