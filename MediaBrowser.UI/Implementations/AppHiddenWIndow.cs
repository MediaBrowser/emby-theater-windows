using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Media;

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

        public Size ContentPixelSize
        {
            get { return GetElementPixelSize(App.Instance.HiddenWindow.MainGrid); }
        }

        public Size GetElementPixelSize(Grid element)
        {
            Matrix transformToDevice;
            using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = hwndSource.CompositionTarget.TransformToDevice;

            var size = new Size(element.ActualWidth, element.ActualHeight);

            //if (element.DesiredSize == new Size())
            //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            return (Size)transformToDevice.Transform((Vector)size);
        }
    }
}
