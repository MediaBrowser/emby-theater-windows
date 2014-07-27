using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Configuration;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Session
{
    /// <summary>
    /// Interface ISessionManager
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Occurs when [user logged in].
        /// </summary>
        event EventHandler<EventArgs> UserLoggedIn;
        /// <summary>
        /// Occurs when [user logged out].
        /// </summary>
        event EventHandler<EventArgs> UserLoggedOut;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        UserDto CurrentUser { get; }

        /// <summary>
        /// Logouts this instance.
        /// </summary>
        Task Logout();

        /// <summary>
        /// Logins the specified user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="rememberCredentials">if set to <c>true</c> [remember credentials].</param>
        /// <returns>Task.</returns>
        Task Login(string username, string password, bool rememberCredentials);

        /// <summary>
        /// Validates the saved login.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task.</returns>
        Task ValidateSavedLogin(AutoLoginConfiguration configuration);
    }
}
