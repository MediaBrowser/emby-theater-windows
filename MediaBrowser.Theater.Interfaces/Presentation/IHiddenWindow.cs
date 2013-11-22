using System.Windows;
using System.Windows.Forms.Integration;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IHiddenWindow
    /// </summary>
    public interface IHiddenWindow
    {
        event SizeChangedEventHandler SizeChanged;

        /// <summary>
        /// Gets the windows forms host.
        /// </summary>
        /// <value>The windows forms host.</value>
        WindowsFormsHost WindowsFormsHost { get; }

        double ContentWidth { get; }

        double ContentHeight { get; }

        Size ContentPixelSize { get; }

        Window Window { get; }
    }
}
