using System.Collections.Generic;

namespace MediaBrowser.Theater.Api.Configuration
{
    /// <summary>
    /// Class WOLConfiguration
    /// </summary>
    public class WolConfiguration
    {
        /// <summary>
        /// Gets or sets the port to use for WOL and WOW
        /// </summary>
        /// <value>Port</value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the HostIpAddresses to use for the magic packets
        /// </summary>
        /// <value>The HostIpAddresses.</value>
        public List<string> HostIpAddresses { get; set; }

        /// <summary>
        /// Gets or sets the HostMacAddresses to use for the magic packets
        /// </summary>
        /// <value>The HostMacAddresses.</value>
        public List<string> HostMacAddresses { get; set; }

        /// <summary>
        /// The number of times the system will try to connect following a WOL attempt.
        /// </summary>
        public int WakeAttempts { get; set; }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        public string HostName { get; set; }

        public WolConfiguration()
        {
            HostIpAddresses = new List<string>();
            HostMacAddresses = new List<string>();

            Port = 9;
            WakeAttempts = 1;
        }
    }
}
