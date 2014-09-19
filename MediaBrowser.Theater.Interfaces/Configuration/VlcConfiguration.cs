
namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class VlcConfiguration
    /// </summary>
    public class VlcConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable gpu acceleration].
        /// </summary>
        /// <value><c>true</c> if [enable gpu acceleration]; otherwise, <c>false</c>.</value>
        public bool EnableGpuAcceleration { get; set; }

        public AudioLayout AudioLayout { get; set; }
    }

    public enum AudioLayout
    {
        Stereo,
        Five1,
        Six1,
        Seven1,
        Mono,
        Spdif
    }
}
