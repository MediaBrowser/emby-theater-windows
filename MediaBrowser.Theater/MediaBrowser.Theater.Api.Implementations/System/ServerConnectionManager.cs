using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Api.Configuration;

namespace MediaBrowser.Theater.Api.System
{
    public class ServerConnectionManager
        : IServerConnectionManager
    {
        private readonly IApiClient _apiClient;
        private readonly ITheaterConfigurationManager _configurationManager;
        private readonly ILogger _logger;

        public ServerConnectionManager(ILogManager logManager, ITheaterConfigurationManager configurationManager, IApiClient apiClient)
        {
            _logger = logManager.GetLogger("ServerManager");
            _configurationManager = configurationManager;
            _apiClient = apiClient;
        }

        public async Task<bool> AttemptServerConnection()
        {
            bool foundServer = false;

            SystemInfo systemInfo = null;

            //Try and send WOL now to give system time to wake
            await SendWakeOnLanCommand();

            try {
                systemInfo = await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);

                foundServer = true;
            }
            catch (Exception ex) {
                _logger.ErrorException("Error connecting to server using saved connection information. Host: {0}, Port {1}", ex,
                                       _apiClient.ServerHostName, _apiClient.ServerApiPort);
            }

            if (foundServer) {
                //Check WOL config
                if (_configurationManager.Configuration.WakeOnLanConfiguration == null) {
                    _configurationManager.Configuration.WakeOnLanConfiguration = new WolConfiguration();
                    _configurationManager.SaveConfiguration();
                }

                WolConfiguration wolConfig = _configurationManager.Configuration.WakeOnLanConfiguration;

                try {
                    List<string> currentIpAddresses = await NetworkUtils.ResolveIpAddressesForHostName(_configurationManager.Configuration.ServerHostName);

                    bool hasChanged = currentIpAddresses.Any(currentIpAddress => wolConfig.HostIpAddresses.All(x => x != currentIpAddress));

                    if (!hasChanged) {
                        hasChanged = wolConfig.HostIpAddresses.Any(hostIpAddress => currentIpAddresses.All(x => x != hostIpAddress));
                    }

                    if (hasChanged) {
                        wolConfig.HostMacAddresses =
                            await NetworkUtils.ResolveMacAddressesForHostName(_configurationManager.Configuration.ServerHostName);
                        wolConfig.HostIpAddresses = currentIpAddresses;

                        //Always add system info MAC address in case we are in a WAN setting
                        if (!wolConfig.HostMacAddresses.Contains(systemInfo.MacAddress)) {
                            wolConfig.HostMacAddresses.Add(systemInfo.MacAddress);
                        }

                        _configurationManager.SaveConfiguration();
                    }
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error attempting to configure WOL.", ex);
                }
            }

            //Try and wait for WOL if its configured
            if (!foundServer && _configurationManager.Configuration.WakeOnLanConfiguration.HostMacAddresses.Count > 0) {
                for (int i = 0; i < _configurationManager.Configuration.WakeOnLanConfiguration.WakeAttempts; i++) {
                    try {
                        systemInfo = await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);
                    }
                    catch (Exception) { }

                    if (systemInfo != null) {
                        foundServer = true;
                        break;
                    }
                }
            }

            //Try and find server
            if (!foundServer) {
                try {
                    IPEndPoint address = await new ServerLocator().FindServer(500, CancellationToken.None).ConfigureAwait(false);

                    string[] parts = address.ToString().Split(':');

                    _apiClient.ChangeServerLocation(parts[0], address.Port);

                    await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);

                    foundServer = true;
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error attempting to locate server.", ex);
                }
            }

            return foundServer;
        }

        public async Task SendWakeOnLanCommand()
        {
            const int payloadSize = 102;
            WolConfiguration wolConfig = _configurationManager.Configuration.WakeOnLanConfiguration;

            if (wolConfig == null) {
                return;
            }

            _logger.Log(LogSeverity.Info, String.Format("Sending Wake on LAN signal to {0}", _configurationManager.Configuration.ServerHostName));

            //Send magic packets to each address
            foreach (string macAddress in wolConfig.HostMacAddresses) {
                byte[] macBytes = PhysicalAddress.Parse(macAddress).GetAddressBytes();

                _logger.Log(LogSeverity.Debug, String.Format("Sending magic packet to {0}", macAddress));

                //Construct magic packet
                var payload = new byte[payloadSize];
                Buffer.BlockCopy(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0, payload, 0, 6);

                for (int i = 1; i < 17; i++) {
                    Buffer.BlockCopy(macBytes, 0, payload, 6*i, 6);
                }

                //Send packet LAN
                using (var udp = new UdpClient()) {
                    try {
                        udp.Connect(IPAddress.Broadcast, wolConfig.Port);
                        await udp.SendAsync(payload, payloadSize);
                    }
                    catch (Exception ex) {
                        _logger.Error(String.Format("Magic packet send failed: {0}", ex.Message));
                    }
                }

                //Send packet WAN
                using (var udp = new UdpClient()) {
                    try {
                        udp.Connect(_configurationManager.Configuration.ServerHostName, wolConfig.Port);
                        await udp.SendAsync(payload, payloadSize);
                    }
                    catch (Exception ex) {
                        _logger.Error(String.Format("Magic packet send failed: {0}", ex.Message));
                    }
                }
            }
        }
    }
}