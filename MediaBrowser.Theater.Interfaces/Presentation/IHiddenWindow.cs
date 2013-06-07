using System.Windows.Forms.Integration;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IHiddenWindow
    /// </summary>
    public interface IHiddenWindow
    {
        /// <summary>
        /// Gets the windows forms host.
        /// </summary>
        /// <value>The windows forms host.</value>
        WindowsFormsHost WindowsFormsHost { get; }
    }
}
