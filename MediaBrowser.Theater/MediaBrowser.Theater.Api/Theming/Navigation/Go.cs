namespace MediaBrowser.Theater.Api.Theming.Navigation
{
    /// <summary>
    ///     The Go class provides an attachment point for root path creation methods.
    /// </summary>
    public sealed class Go
    {
        /// <summary>
        ///     Provides a point for paths defined throughout the application to expose their creation APIs.
        /// </summary>
        public static readonly Go To = new Go();

        private Go()
        {
        }
    }
}