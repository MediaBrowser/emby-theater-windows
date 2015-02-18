using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Configuration;
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
        /// Gets or sets the local user identifier.
        /// </summary>
        /// <value>The local user identifier.</value>
        string LocalUserId { get; }

        /// <summary>
        /// Gets or sets the connect user identifier.
        /// </summary>
        /// <value>The connect user identifier.</value>
        string ConnectUserId { get; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        string UserName { get; }

        /// <summary>
        /// Gets or sets the user image URL.
        /// </summary>
        /// <value>The user image URL.</value>
        string UserImageUrl { get; }

        /// <summary>
        /// Gets or sets the user configuration.
        /// </summary>
        /// <value>The user configuration.</value>
        UserConfiguration UserConfiguration { get; }

        /// <summary>
        /// Logouts this instance.
        /// </summary>
        Task Logout();

        /// <summary>
        /// Logins the specified user.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="rememberCredentials">if set to <c>true</c> [remember credentials].</param>
        /// <returns>Task.</returns>
        Task LoginToServer(IApiClient apiClient, string username, string password, bool rememberCredentials);

        /// <summary>
        /// Validates the saved login.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>Task.</returns>
        Task ValidateSavedLogin(ConnectionResult result);
        /// <summary>
        /// Gets or sets the active API client.
        /// </summary>
        /// <value>The active API client.</value>
        IApiClient ActiveApiClient { get; }
    }
}
