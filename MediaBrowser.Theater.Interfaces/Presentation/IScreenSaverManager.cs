using System;
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
        /// Show  the current selected screen saver
        /// <param name="forceShowScreensaver">Show the Screensave even regardless of screensave timeout</param>
        /// </summary>
        void ShowScreensaver(bool forceShowScreensaver);

        /// <summary>
        /// Gets the screensavers
        /// </summary>
        /// <value>The screen savers.</value>
        IEnumerable<IScreensaver> Screensavers { get; }

        /// <summary>
        /// Gets/Set the current selected screen saver 
        /// </summary>
        /// <value>The current selected screen saver.</value>
        IScreensaver CurrentScreensaver { get; set; }

    }
}
