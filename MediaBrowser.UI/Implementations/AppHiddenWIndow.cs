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

        public event KeyEventHandler KeyDown
        {
            add { App.Instance.HiddenWindow.KeyDown += value; }
            remove
            {
                App.Instance.HiddenWindow.KeyDown -= value;
            }
        }

        public event MouseEventHandler MouseClick
        {
            add { App.Instance.HiddenWindow.MouseClick += value; }
            remove
            {
                App.Instance.HiddenWindow.MouseClick -= value;
            }
        }

        public Size ContentPixelSize
        {
            get { return new Size(App.Instance.HiddenWindow.Width, App.Instance.HiddenWindow.Height); }
        }

        public Action OnWMGRAPHNOTIFY
        {
            get
            {
                return App.Instance.HiddenWindow.OnWMGRAPHNOTIFY;
            }
            set
            {
                App.Instance.HiddenWindow.OnWMGRAPHNOTIFY = value;
            }
        }

        public Action OnDVDEVENT
        {
            get
            {
                return App.Instance.HiddenWindow.OnDVDEVENT;
            }
            set
            {
                App.Instance.HiddenWindow.OnDVDEVENT = value;
            }
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
