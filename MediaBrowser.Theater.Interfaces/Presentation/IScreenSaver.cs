using System;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IScreenSaver
    /// </summary>
    public interface IScreensaver
    {
        /// <summary>
        /// Screensaver name
        /// </summary>
       new String Name { get; }

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
