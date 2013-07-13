using System;

namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class UserConfigurationUpdatedEventArgs
    /// </summary>
    public class UserConfigurationUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public UserTheaterConfiguration Configuration { get; set; }
    }
}
