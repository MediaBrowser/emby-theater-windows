using System;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IScreenSaver
    /// </summary>
    public interface IScreensaver
    {
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
