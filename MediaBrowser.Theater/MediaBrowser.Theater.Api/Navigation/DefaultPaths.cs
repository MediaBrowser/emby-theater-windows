using System.Collections.Generic;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Api.Navigation
{
    // Application wide paths that all themes are expected to implement

    /// <summary>
    ///     The path to the user login page.
    /// </summary>
    public class LoginPath : NavigationPath { }

    /// <summary>
    ///     The path to the server selection page.
    /// </summary>
    public class ServerSelectionPath : NavigationPathArg<IEnumerable<ServerInfo>> { }

    /// <summary>
    ///     The path to the locate server page.
    /// </summary>
    public class LocateServerPath : NavigationPath { }

    /// <summary>
    ///     The path to the user selection page.
    /// </summary>
    public class UserSelectionPath : NavigationPathArg<IApiClient> { }

    /// <summary>
    ///     The path to the MB Connect login page.
    /// </summary>
    public class ConnectLoginPath : NavigationPath { }

    /// <summary>
    ///     The path to the home page.
    /// </summary>
    public class HomePath : NavigationPath { }

    /// <summary>
    ///     The path to application settings.
    /// </summary>
    public class SettingsPath : NavigationPath { }

    /// <summary>
    ///     The path to the currently playing media page.
    /// </summary>
    public class ActiveMediaPath : NavigationPath { }

    /// <summary>
    ///     The path to the "Now Playing" playlist.
    /// </summary>
    public class NowPlayingPlaylistPath : NavigationPath { }

    /// <summary>
    ///     The path to the page for the specified item.
    /// </summary>
    public class ItemPath : NavigationPathArg<BaseItemDto> { }

    /// <summary>
    ///     The path to the search page.
    /// </summary>
    public class SearchPath : NavigationPath { }

    /// <summary>
    ///     The path to the full screen playback page.
    /// </summary>
    public class FullScreenPlaybackPath : NavigationPath { }

    #region Fluent Interface

    /// <summary>
    ///     Contains extension methods to the Go class, to add an API for creating the default navigation paths.
    /// </summary>
    public static class DefaultPathExtensions
    {
        /// <summary>
        ///     Gets a path to the home path.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the home page.</returns>
        public static HomePath Home(this Go go)
        {
            return new HomePath();
        }

        /// <summary>
        ///     Gets a path to the settings page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the settings page.</returns>
        public static SettingsPath Settings(this Go go)
        {
            return new SettingsPath();
        }

        /// <summary>
        ///     Gets a path to the active media page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the active media page.</returns>
        public static ActiveMediaPath ActiveMedia(this Go go)
        {
            return new ActiveMediaPath();
        }

        /// <summary>
        ///     Gets a path to the specified items page.
        /// </summary>
        /// <param name="item">The item to view.</param>
        /// <param name="go"></param>
        /// <returns>A path to the page for the specified item.</returns>
        public static ItemPath Item(this Go go, BaseItemDto item)
        {
            return new ItemPath { Parameter = item };
        }

        /// <summary>
        ///     Gets a path to the user login page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the user login page.</returns>
        public static LoginPath Login(this Go go)
        {
            return new LoginPath();
        }

        /// <summary>
        ///     Gets a path to the server selection page.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="servers">The available servers, or <code>null</code> to enable auto-discovery.</param>
        /// <returns>A path to the server selection page.</returns>
        public static ServerSelectionPath ServerSelection(this Go go, IEnumerable<ServerInfo> servers = null)
        {
            return new ServerSelectionPath { Parameter = servers };
        }

        /// <summary>
        ///     Gets a path to the locate server page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the locate server page.</returns>
        public static LocateServerPath LocateServer(this Go go)
        {
            return new LocateServerPath();
        }

        /// <summary>
        ///     Gets a path to the user selection page.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="apiClient">The API client for the local server to log in to.</param>
        /// <returns>A path to the user selection page.</returns>
        public static UserSelectionPath UserSelection(this Go go, IApiClient apiClient)
        {
            return new UserSelectionPath { Parameter = apiClient };
        }

        /// <summary>
        ///     Gets a path to the MB Connect login page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the MB Connect login page.</returns>
        public static ConnectLoginPath ConnectLogin(this Go go)
        {
            return new ConnectLoginPath();
        }

        /// <summary>
        ///     Gets a path to the search page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the search page.</returns>
        public static SearchPath Search(this Go go)
        {
            return new SearchPath();
        }

        /// <summary>
        ///     Gets a path to the full screen playback page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the full screen playback page.</returns>
        public static FullScreenPlaybackPath FullScreenPlayback(this Go go)
        {
            return new FullScreenPlaybackPath();
        }
    }

    #endregion
}