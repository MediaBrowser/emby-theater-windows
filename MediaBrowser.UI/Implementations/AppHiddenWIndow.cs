using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows;
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

        public double ContentWidth
        {
            get { return App.Instance.HiddenWindow.MainGrid.ActualWidth; }
        }

        public double ContentHeight
        {
            get { return App.Instance.HiddenWindow.MainGrid.ActualHeight; }
        }

        public event SizeChangedEventHandler SizeChanged
        {
            add { App.Instance.HiddenWindow.SizeChanged += value; }
            remove
            {
                App.Instance.HiddenWindow.SizeChanged -= value;
            }
        }
    }
}
