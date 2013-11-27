using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.System
{
    /// <summary>
    /// Interface IMediaFilters
    /// </summary>
    public interface IMediaFilters
    {
        /// <summary>
        /// Determines whether [is xy vs filter installed].
        /// </summary>
        /// <returns><c>true</c> if [is xy vs filter installed]; otherwise, <c>false</c>.</returns>
        bool IsXyVsFilterInstalled();

        /// <summary>
        /// Installs the xy vs filter.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task InstallXyVsFilter(IProgress<double> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Determines whether [is lav filters installed].
        /// </summary>
        /// <returns><c>true</c> if [is lav filters installed]; otherwise, <c>false</c>.</returns>
        bool IsLavSplitterInstalled();

        /// <summary>
        /// Determines whether [is lav audio installed].
        /// </summary>
        /// <returns><c>true</c> if [is lav audio installed]; otherwise, <c>false</c>.</returns>
        bool IsLavAudioInstalled();

        /// <summary>
        /// Determines whether [is lav video installed].
        /// </summary>
        /// <returns><c>true</c> if [is lav video installed]; otherwise, <c>false</c>.</returns>
        bool IsLavVideoInstalled();
        
        /// <summary>
        /// Installs the lav filters.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task InstallLavFilters(IProgress<double> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Launches the lav audio configuration.
        /// </summary>
        void LaunchLavAudioConfiguration();

        /// <summary>
        /// Launches the lav splitter configuration.
        /// </summary>
        void LaunchLavSplitterConfiguration();
    }
}
