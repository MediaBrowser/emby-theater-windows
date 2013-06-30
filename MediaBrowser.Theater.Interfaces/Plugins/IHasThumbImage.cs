using System;

namespace MediaBrowser.Theater.Interfaces.Plugins
{
    /// <summary>
    /// Plugins should implement this to display a thumb image on the installed plugins page
    /// </summary>
    public interface IHasThumbImage
    {
        /// <summary>
        /// Gets the thumb URI.
        /// </summary>
        /// <value>The thumb URI.</value>
        Uri ThumbUri { get; }
    }
}
