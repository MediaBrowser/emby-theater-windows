
namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class AutoLoginConfiguration
    /// </summary>
    public class AutoLoginConfiguration
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the password hash to use in auto-login.
        /// </summary>
        /// <value>The password hash.</value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the server identifier.
        /// </summary>
        /// <value>The server identifier.</value>
        public string ServerId { get; set; }
    }
}
