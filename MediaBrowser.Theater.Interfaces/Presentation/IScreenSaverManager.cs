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
        /// <param name="overRideScreenSaverName">override the default selected screensaver</param>
        /// </summary>
        void ShowScreensaver(bool forceShowScreensaver, string overRideScreenSaverName = null);

        /// <summary>
        /// Gets/Set the current screen save factory list
        /// </summary>
        /// <value>The c current screen save factory list.</value>
        IEnumerable<IScreensaverFactory> ScreensaverFactories { get;}
      
        /// <summary>
        /// Gets/Set the current selected screen saver name
        /// </summary>
        /// <value>The current selected screen saver.</value>
        string CurrentScreensaverName { get; set; }
    }
}
