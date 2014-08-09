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
using MediaBrowser.Theater.Api.Networking;
using MediaBrowser.Theater.Api.Session;

namespace MediaBrowser.Theater.Api.System
{
    public class ServerConnectionManager
        : IServerConnectionManager
    {
        private readonly IApiClient _apiClient;
        private readonly ITheaterApplicationHost _appHost;
        private readonly ISessionManager _sessionManager;
        private readonly ITheaterConfigurationManager _configurationManager;
        private readonly ILogger _logger;

        public ServerConnectionManager(ILogManager logManager, ITheaterConfigurationManager configurationManager, IApiClient apiClient, ITheaterApplicationHost appHost, ISessionManager sessionManager)
        {
            _logger = logManager.GetLogger("ServerManager");
            _configurationManager = configurationManager;
            _apiClient = apiClient;
            _appHost = appHost;
            _sessionManager = sessionManager;
        }

        public async Task<PublicSystemInfo> AttemptServerConnection()
        {
            //Try and send WOL now to give system time to wake
            await SendWakeOnLanCommand();

            try {
                return await _apiClient.GetPublicSystemInfoAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error connecting to server using saved connection information. Address: {0}", ex, _apiClient.ServerAddress);
            }

            //Try and wait for WOL if its configured
            if (_configurationManager.Configuration.WakeOnLanConfiguration.HostMacAddresses.Count > 0) {
                for (int i = 0; i < _configurationManager.Configuration.WakeOnLanConfiguration.WakeAttempts; i++) {
                    try {
                        return await _apiClient.GetPublicSystemInfoAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception) { }
                }
            }

            //Try and find server
            try {
                var serverInfo = await new ServerLocator().FindServer(500, CancellationToken.None).ConfigureAwait(false);

                _apiClient.ChangeServerLocation(serverInfo.Address);

                return await _apiClient.GetPublicSystemInfoAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error attempting to locate server.", ex);
            }

            return null;
        }

        public async Task<bool> AttemptAutoLogin(PublicSystemInfo systemInfo)
        {
            //Check for auto-login credientials
            var config = _appHost.TheaterConfigurationManager.Configuration;

            try
            {
                if (systemInfo != null && string.Equals(systemInfo.Id, config.AutoLoginConfiguration.ServerId))
                {
                    await _sessionManager.ValidateSavedLogin(config.AutoLoginConfiguration);
                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                //Login failed, redirect to login page and clear the auto-login
                _logger.ErrorException("Auto-login failed", ex);

                config.AutoLoginConfiguration = new AutoLoginConfiguration();
                _appHost.TheaterConfigurationManager.SaveConfiguration();
            }
            catch (FormatException ex)
            {
                //Login failed, redirect to login page and clear the auto-login
                _logger.ErrorException("Auto-login password hash corrupt", ex);

                config.AutoLoginConfiguration = new AutoLoginConfiguration();
                _appHost.TheaterConfigurationManager.SaveConfiguration();
            }

            return false;
        }

        public async Task SendWakeOnLanCommand()
        {
            const int payloadSize = 102;
            WolConfiguration wolConfig = _configurationManager.Configuration.WakeOnLanConfiguration;

            if (wolConfig == null) {
                return;
            }

            _logger.Log(LogSeverity.Info, String.Format("Sending Wake on LAN signal to {0}", _configurationManager.Configuration.ServerAddress));

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

                var hostname = _configurationManager.Configuration.WakeOnLanConfiguration.HostName;

                if (!string.IsNullOrEmpty(hostname)) {
                    //Send packet WAN
                    using (var udp = new UdpClient()) {
                        try {
                            udp.Connect(hostname, wolConfig.Port);
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
}