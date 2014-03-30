using System.Collections.Generic;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IScreensaverManager
    {
        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="screensaverFactories">The screen savers factories.</param>
        void AddParts(IEnumerable<IScreensaverFactory> screensaverFactories);

        /// <summary>
        /// Is a Screensaver running
        /// </summary>
        bool ScreensaverIsRunning { get;  }

        /// <summary>
        /// Stop screensaver running (if one is running)
        /// </summary>
        void StopScreenSaver();

        /// <summary>
        /// Stop scren saver running (if one is running)
        /// <param name="forceShowScreensaver">Show the Screensave even regardless of screensave tieout</param>
        /// </summary>
        void ShowScreensaver(bool forceShowScreensaver);

    }
}
