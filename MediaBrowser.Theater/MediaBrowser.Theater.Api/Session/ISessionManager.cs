using System;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Configuration;

namespace MediaBrowser.Theater.Api.Session
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
        Task LoginToServer(string username, string password, bool rememberCredentials);

        /// <summary>
        /// Gets or sets the active API client.
        /// </summary>
        /// <value>The active API client.</value>
        IApiClient ActiveApiClient { get; }

        string LocalUserId { get; }

        string ConnectUserId { get; }

        string UserName { get; }

        string UserImageUrl { get; }

        UserConfiguration UserConfiguration { get; }
    }
}