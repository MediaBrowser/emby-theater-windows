using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Api.Session
{
    /// <summary>
    ///     Interface ISessionManager
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        ///     Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        UserDto CurrentUser { get; }

        /// <summary>
        ///     Occurs when [user logged in].
        /// </summary>
        event EventHandler<EventArgs> UserLoggedIn;

        /// <summary>
        ///     Occurs when [user logged out].
        /// </summary>
        event EventHandler<EventArgs> UserLoggedOut;

        /// <summary>
        ///     Logouts this instance.
        /// </summary>
        Task Logout();

        /// <summary>
        ///     Logins the specified user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Task.</returns>
        Task Login(string username, string password, bool rememberCredentials);

        /// <summary>
        ///     Logins the specified user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password hash string.</param>
        /// <returns>Task.</returns>
        Task LoginWithHash(string username, string passwordHash, bool rememberCredentials);
    }
}