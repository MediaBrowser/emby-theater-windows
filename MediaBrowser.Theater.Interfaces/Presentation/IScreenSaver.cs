using System;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IScreenSaver
    /// </summary>
    public interface IScreensaver
    {
       /// <summary>
       /// Gets the name.
       /// </summary>
       /// <value>The name.</value>
       string Name { get; }

        /// <summary>
        /// close the screen saver
        /// </summary>
        void ShowModal();

        /// <summary>
        /// close the screen saver
        /// </summary>
        void Close();
    }
}
