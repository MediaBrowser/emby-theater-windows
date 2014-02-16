using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Api.Navigation
{
    // Application wide paths that all themes are expected to implement

    /// <summary>
    ///     The path to the user login page.
    /// </summary>
    public class LoginPath : NavigationPath { }

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
        /// Gets a path to the user login page.
        /// </summary>
        /// <param name="go"></param>
        /// <returns>A path to the user login page.</returns>
        public static LoginPath Login(this Go go)
        {
            return new LoginPath();
        }
    }

    #endregion
}