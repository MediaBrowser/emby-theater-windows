
namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class AutoLoginConfiguration
    /// </summary>
    public class AutoLoginConfiguration
    {
        /// <summary>
        /// Gets or sets the username to use in auto-login.
        /// </summary>
        /// <value>The username.</value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password hash to use in auto-login.
        /// </summary>
        /// <value>The password hash.</value>
        public string UserPasswordHash { get; set; }
    }
}
