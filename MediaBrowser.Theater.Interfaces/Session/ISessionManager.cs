using MediaBrowser.Model.Dto;
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
        /// <returns>Task.</returns>
        Task Login(string username, string password, bool rememberCredentials);

        /// <summary>
        /// Logins the specified user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password hash string.</param>
        /// <returns>Task.</returns>
        Task LoginWithHash(string username, string passwordHash, bool rememberCredentials);
    }
}
