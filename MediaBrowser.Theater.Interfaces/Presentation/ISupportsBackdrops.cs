using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface ISupportsBackdrops
    /// </summary>
    public interface ISupportsBackdrops
    {
    }

    /// <summary>
    /// Interface ISupportsItemBackdrops
    /// </summary>
    public interface ISupportsItemBackdrops : ISupportsBackdrops
    {
        /// <summary>
        /// Gets the backdrop item.
        /// </summary>
        /// <value>The backdrop item.</value>
        BaseItemDto BackdropItem { get; }
    }

}
