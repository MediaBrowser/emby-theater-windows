namespace MediaBrowser.Theater.Api.Theming
{
    public interface ITheme
    {
        /// <summary>
        ///     Gets the name of the theme.
        /// </summary>
        string Name { get; }


        /// <summary>
        ///     Starts the theme. The theme is expected to present its GUI.
        /// </summary>
        void Start();
    }
}