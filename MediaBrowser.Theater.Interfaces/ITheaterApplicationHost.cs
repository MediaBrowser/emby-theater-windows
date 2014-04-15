using System.Threading.Tasks;
using MediaBrowser.Common;

namespace MediaBrowser.Theater.Interfaces
{
    /// <summary>
    /// Interface ITheaterApplicationHost
    /// </summary>
    public interface ITheaterApplicationHost : IApplicationHost
    {
        /// <summary>
        /// Shutdowns the system.
        /// </summary>
        void ShutdownSystem();

        /// <summary>
        /// Reboots the system.
        /// </summary>
        void RebootSystem();

        /// <summary>
        /// Sets the system to sleep.
        /// </summary>
        void SetSystemToSleep();

        /// <summary>
        /// Sends a WOL command to the server details listed in WolConfiguration.
        /// </summary>
        Task SendWolCommand();
    }
}
