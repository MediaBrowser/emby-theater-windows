using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Windows.Forms;
using Size = System.Drawing.Size;

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
        public Form Form
        {
            get { return App.Instance.HiddenWindow; }
        }

        public event EventHandler SizeChanged
        {
            add { App.Instance.HiddenWindow.SizeChanged += value; }
            remove
            {
                App.Instance.HiddenWindow.SizeChanged -= value;
            }
        }

        public Size ContentPixelSize
        {
            get { return new Size(App.Instance.HiddenWindow.Width, App.Instance.HiddenWindow.Height); }
        }

        //private Size GetElementPixelSize(Grid element)
        //{
        //    Matrix transformToDevice;
        //    using (var hwndSource = new HwndSource(new HwndSourceParameters()))
        //            transformToDevice = hwndSource.CompositionTarget.TransformToDevice;

        //    var size = new Size(element.ActualWidth, element.ActualHeight);

        //    //if (element.DesiredSize == new Size())
        //    //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        //    return (Size)transformToDevice.Transform((Vector)size);
        //}
    }
}
