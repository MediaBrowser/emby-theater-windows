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
        Task Login(string username, string password);

        /// <summary>
        /// Logins the specified user with a hash value.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="hash">The byte[] hash array.</param>
        /// <returns>Task.</returns>
        Task Login(string username, byte[] hash);

        /// <summary>
        /// Computes a SHA1 hash value for the specified string value.
        /// </summary>
        /// <param name="data">string value to be hashed.</param>
        /// <returns>Computed byte[] hash.</returns>
        byte[] ComputeHash(string data);
    }
}
