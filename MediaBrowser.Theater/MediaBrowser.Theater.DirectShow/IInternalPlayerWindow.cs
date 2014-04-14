using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaBrowser.Theater.DirectShow
{
    public interface IInternalPlayerWindow
    {
        /// <summary>
        ///     Gets the windows forms host.
        /// </summary>
        /// <value>The windows forms host.</value>
        Form Form { get; }

        Size ContentPixelSize { get; }

        Action OnWMGRAPHNOTIFY { get; set; }
        Action OnDVDEVENT { get; set; }
        event EventHandler SizeChanged;
        event KeyEventHandler KeyDown;
        event MouseEventHandler MouseClick;
        event MouseEventHandler MouseMove;
    }

    public interface IInternalPlayerWindowManager
    {
        IInternalPlayerWindow Window { get; set; }
        event Action<IInternalPlayerWindow> WindowLoaded;
    }

    public class InternalPlayerWindowManager : IInternalPlayerWindowManager
    {
        private IInternalPlayerWindow _window;

        public IInternalPlayerWindow Window
        {
            get { return _window; }
            set
            {
                _window = value;

                if (_window != null) {
                    OnWindowLoaded(_window);
                }
            }
        }

        public event Action<IInternalPlayerWindow> WindowLoaded;

        protected virtual void OnWindowLoaded(IInternalPlayerWindow obj)
        {
            Action<IInternalPlayerWindow> handler = WindowLoaded;
            if (handler != null) {
                handler(obj);
            }
        }
    }
}