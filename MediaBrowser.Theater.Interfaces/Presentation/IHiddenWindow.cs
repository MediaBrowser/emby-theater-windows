using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IHiddenWindow
    /// </summary>
    public interface IHiddenWindow
    {
        event EventHandler SizeChanged;

        /// <summary>
        /// Gets the windows forms host.
        /// </summary>
        /// <value>The windows forms host.</value>
        Form Form { get; }

        Size ContentPixelSize { get; }
    }
}
