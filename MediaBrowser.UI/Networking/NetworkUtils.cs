using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MediaBrowser.UI.Networking
{
    public static class NetworkUtils
    {
        /// <summary>
        /// Gets the MAC address (<see cref="PhysicalAddress"/>) associated with the specified IP.
        /// </summary>
        /// <param name="ipAddress">The remote IP address.</param>
        /// <returns>The remote machine's MAC address.</returns>
        public static PhysicalAddress GetMacAddress(IPAddress ipAddress)
        {
            const int macAddressLength = 6;
            var length = macAddressLength;
            var macBytes = new byte[macAddressLength];
            NativeMethods.SendARP(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macBytes, ref length);
            return new PhysicalAddress(macBytes);
        }

        public async static Task<List<string>> ResolveIpAddressesForHostName(string hostName)
        {
            var outputAddresses = (from hostAddress in await Dns.GetHostAddressesAsync(hostName) select hostAddress.ToString()).ToList();
            return outputAddresses;
        }

        /// <summary>
        /// Gets the MAC addresses associated with the specified Host-name that are not blank matching 000000000000.
        /// </summary>
        /// <param name="hostName">The remote host-name.</param>
        /// <returns>The remote machine's MAC addresses.</returns>
        public async static Task<List<string>> ResolveMacAddressesForHostName(string hostName)
        {
            //Get a list of valid ip addresses for host-name
            var ipAddresses = await Dns.GetHostAddressesAsync(hostName);

            //Collate mac addresses for each ip address
            return ipAddresses.Select(ipAddress => GetMacAddress(ipAddress).ToString()).Where(x=> x != "000000000000").ToList();
        }
    }
}
