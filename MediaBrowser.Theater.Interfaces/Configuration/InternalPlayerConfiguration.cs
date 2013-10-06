
namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class InternalPlayerConfiguration
    /// </summary>
    public class InternalPlayerConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable reclock].
        /// </summary>
        /// <value><c>true</c> if [enable reclock]; otherwise, <c>false</c>.</value>
        public bool EnableReclock { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable madvr].
        /// </summary>
        /// <value><c>true</c> if [enable madvr]; otherwise, <c>false</c>.</value>
        public bool EnableMadvr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable xy sub filter].
        /// </summary>
        /// <value><c>true</c> if [enable xy sub filter]; otherwise, <c>false</c>.</value>
        public bool EnableXySubFilter { get; set; }
    }
}
