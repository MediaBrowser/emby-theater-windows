using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Configuration;

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
        /// <param name="rememberCredentials"><c>true</c> if login credentials should be saved; else <c>false</c>.</param>
        /// <returns>Task.</returns>
        Task Login(string username, string password, bool rememberCredentials);

        /// <summary>
        /// Validates the saved login.
        /// </summary>
        /// <param name="configuration">The auto login configuration.</param>
        Task ValidateSavedLogin(AutoLoginConfiguration configuration);
    }
}