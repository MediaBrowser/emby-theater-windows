using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using MediaBrowser.ApiInteraction;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Constants;
using MediaBrowser.Common.Implementations;
using MediaBrowser.Common.Implementations.ScheduledTasks;
using MediaBrowser.Common.Net;
using MediaBrowser.IsoMounter;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Updates;
using MediaBrowser.Plugins.DefaultTheme;
using MediaBrowser.Theater.Core.Login;
using MediaBrowser.Theater.DirectShow;
using MediaBrowser.Theater.Implementations.Commands;
using MediaBrowser.Theater.Implementations.Configuration;
using MediaBrowser.Theater.Implementations.Playback;
using MediaBrowser.Theater.Implementations.Presentation;
using MediaBrowser.Theater.Implementations.Session;
using MediaBrowser.Theater.Implementations.System;
using MediaBrowser.Theater.Implementations.Theming;
using MediaBrowser.Theater.Implementations.UserInput;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Commands;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.System;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Playback;
using MediaBrowser.UI.EntryPoints;
using MediaBrowser.UI.Implementations;
using MediaBrowser.UI.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Class CompositionRoot
    /// </summary>
    internal class ApplicationHost : BaseApplicationHost<ApplicationPaths>, ITheaterApplicationHost
    {
        public ApplicationHost(ApplicationPaths applicationPaths, ILogManager logManager)
            : base(applicationPaths, logManager)
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public IApiClient ApiClient { get; private set; }

        internal ApiWebSocket ApiWebSocket { get; set; }

        public IThemeManager ThemeManager { get; private set; }
        public IPlaybackManager PlaybackManager { get; private set; }
        public IScreensaverManager ScreensaverManager { get; private set; }
        public IImageManager ImageManager { get; private set; }
        public INavigationService NavigationService { get; private set; }
        public ISessionManager SessionManager { get; private set; }
        public IPresentationManager PresentationManager { get; private set; }
        public IUserInputManager UserInputManager { get; private set; }
        public ICommandManager CommandManager { get; private set; }
        public IMediaFilters MediaFilters { get; private set; }

        public ConfigurationManager TheaterConfigurationManager
        {
            get { return (ConfigurationManager)ConfigurationManager; }
        }

        public override async Task Init(IProgress<double> progress)
        {
            await base.Init(progress).ConfigureAwait(false);

            // For now until the ui has it's own startup wizard
            if (IsFirstRun)
            {
                ConfigurationManager.CommonConfiguration.IsStartupWizardCompleted = true;
                ConfigurationManager.SaveConfiguration();
            }

            await RunStartupTasks().ConfigureAwait(false);

            Logger.Info("Core startup complete");
        }

        public void StartEntryPoints()
        {
            Parallel.ForEach(GetExports<IStartupEntryPoint>(), entryPoint =>
            {
                try
                {
                    entryPoint.Run();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error in {0}", ex, entryPoint.GetType().Name);
                }
            });
        }

        public override bool CanSelfRestart
        {
            get { return true; }
        }

        /// <summary>
        /// Registers resources that classes will depend on
        /// </summary>
        protected override async Task RegisterResources(IProgress<double> progress)
        {
            ReloadApiClient();

            await base.RegisterResources(progress).ConfigureAwait(false);

            RegisterSingleInstance<ITheaterApplicationHost>(this);

            MediaFilters = new MediaFilters(HttpClient, Logger);
            RegisterSingleInstance(MediaFilters);

            ThemeManager = new ThemeManager(() => PresentationManager, Logger);
            RegisterSingleInstance(ThemeManager);

            PresentationManager = new TheaterApplicationWindow(Logger, ThemeManager, ApiClient, () => SessionManager, TheaterConfigurationManager);
            RegisterSingleInstance(PresentationManager);

            RegisterSingleInstance(ApplicationPaths);

            RegisterSingleInstance<ITheaterConfigurationManager>(TheaterConfigurationManager);
         
            var hiddenWindow = new AppHiddenWIndow();

            ImageManager = new ImageManager(ApiClient, ApplicationPaths, TheaterConfigurationManager);
            RegisterSingleInstance(ImageManager);

            NavigationService = new NavigationService(ThemeManager, () => PlaybackManager, ApiClient, PresentationManager, TheaterConfigurationManager, () => SessionManager, this, InstallationManager, ImageManager, Logger, () => UserInputManager, ApiWebSocket, hiddenWindow);
            RegisterSingleInstance(NavigationService);

            UserInputManager = new UserInputManager(PresentationManager, NavigationService, hiddenWindow, LogManager);
            RegisterSingleInstance(UserInputManager);

            PlaybackManager = new PlaybackManager(TheaterConfigurationManager, Logger, ApiClient, NavigationService, PresentationManager);
            RegisterSingleInstance(PlaybackManager);

            CommandManager = new CommandManager(PresentationManager, PlaybackManager, NavigationService, UserInputManager, LogManager);
            RegisterSingleInstance(CommandManager);

            SessionManager = new SessionManager(NavigationService, ApiClient, Logger, ThemeManager, TheaterConfigurationManager, PlaybackManager);
            RegisterSingleInstance(SessionManager);

            ScreensaverManager = new ScreensaverManager(UserInputManager, PresentationManager, PlaybackManager, SessionManager, ApiClient, TheaterConfigurationManager, LogManager, ApiWebSocket);
            RegisterSingleInstance(ScreensaverManager);

            RegisterSingleInstance(ApiClient);

            RegisterSingleInstance<IHiddenWindow>(hiddenWindow);

            RegisterSingleInstance<IServerEvents>(ApiWebSocket);
        }

        /// <summary>
        /// Finds the parts.
        /// </summary>
        protected override void FindParts()
        {
            base.FindParts();

            ThemeManager.AddParts(GetExports<ITheme>());
            PresentationManager.AddParts(GetExports<IAppFactory>(), GetExports<ISettingsPage>(), GetExports<IHomePageInfo>());
            PlaybackManager.AddParts(GetExports<IMediaPlayer>());
            ScreensaverManager.AddParts(GetExports<IScreensaverFactory>());
        }

        protected override INetworkManager CreateNetworkManager()
        {
            return new NetworkManager();
        }

        /// <summary>
        /// Disposes the current ApiClient and creates a new one
        /// </summary>
        private void ReloadApiClient()
        {
            var logger = LogManager.GetLogger("ApiClient");

            var apiClient = new ApiClient(new HttpWebRequestClient(logger), logger, TheaterConfigurationManager.Configuration.ServerHostName, TheaterConfigurationManager.Configuration.ServerApiPort, "Media Browser Theater", Environment.MachineName, Environment.MachineName, ApplicationVersion.ToString())
            {
                JsonSerializer = JsonSerializer,
                ImageQuality = TheaterConfigurationManager.Configuration.DownloadCompressedImages
                                                       ? 90
                                                       : 100
            };

            ApiClient = apiClient;

            logger = LogManager.GetLogger("ApiWebSocket");

            // WebSocketEntry point will handle figuring out the port and connecting
            ApiWebSocket = new ApiWebSocket(logger, JsonSerializer, apiClient.ServerHostName, 0,
                                  ApiClient.DeviceId, apiClient.ApplicationVersion,
                                  apiClient.ClientName, apiClient.DeviceName, () => ClientWebSocketFactory.CreateWebSocket(logger));

            apiClient.WebSocketConnection = ApiWebSocket;
        }

        public override Task Restart()
        {
            PlaybackManager.StopAllPlayback();

            return Task.Run(() => App.Instance.Dispatcher.Invoke(() => App.Instance.Restart()));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can self update.
        /// </summary>
        /// <value><c>true</c> if this instance can self update; otherwise, <c>false</c>.</value>
        public override bool CanSelfUpdate
        {
            get
            {
#if DEBUG
                return false;
#endif
                return true;
            }
        }

        /// <summary>
        /// Gets the composable part assemblies.
        /// </summary>
        /// <returns>IEnumerable{Assembly}.</returns>
        protected override IEnumerable<Assembly> GetComposablePartAssemblies()
        {
            // Gets all plugin assemblies by first reading all bytes of the .dll and calling Assembly.Load against that
            // This will prevent the .dll file from getting locked, and allow us to replace it when needed
            foreach (var pluginAssembly in GetPluginAssemblies())
            {
                yield return pluginAssembly;
            }

            // Include composable parts in the Model assembly 
            yield return typeof(SystemInfo).Assembly;

            // Include composable parts in the Common assembly 
            yield return typeof(IApplicationPaths).Assembly;

            // Common implementations
            yield return typeof(TaskManager).Assembly;

            // Include composable parts in the running assembly
            yield return GetType().Assembly;

            // DirectShow assembly
            yield return typeof(InternalDirectShowPlayer).Assembly;

            // Presentation player assembly
            yield return typeof(GenericExternalPlayer).Assembly;

            // Core assembly
            yield return typeof(LoginPage).Assembly;

            yield return typeof(PismoIsoManager).Assembly;

            // Default theme assembly
            yield return typeof(DefaultTheme).Assembly;
        }

        /// <summary>
        /// Gets the plugin assemblies.
        /// </summary>
        /// <returns>IEnumerable{Assembly}.</returns>
        private IEnumerable<Assembly> GetPluginAssemblies()
        {
            try
            {
                return Directory.EnumerateFiles(ApplicationPaths.PluginsPath, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(LoadAssembly)
                    .Where(a => a != null)
                    .ToList();
            }
            catch (DirectoryNotFoundException)
            {
                return new List<Assembly>();
            }
        }

        public override Task Shutdown()
        {
            PlaybackManager.StopAllPlayback();

            return Task.Run(() => App.Instance.Dispatcher.Invoke(() => App.Instance.Shutdown()));
        }

        protected override void OnConfigurationUpdated(object sender, EventArgs e)
        {
            base.OnConfigurationUpdated(sender, e);

            ((ApiClient)ApiClient).ImageQuality = TheaterConfigurationManager.Configuration.DownloadCompressedImages
                                                       ? 90
                                                       : 100;
        }

        protected override IConfigurationManager GetConfigurationManager()
        {
            return new ConfigurationManager(ApplicationPaths, LogManager, XmlSerializer);
        }

        /// <summary>
        /// Checks for update.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task{CheckForUpdateResult}.</returns>
        public override async Task<CheckForUpdateResult> CheckForApplicationUpdate(CancellationToken cancellationToken,
                                                                    IProgress<double> progress)
        {
            var serverInfo = await ApiClient.GetSystemInfoAsync(cancellationToken).ConfigureAwait(false);

            var availablePackages = await InstallationManager.GetAvailablePackagesWithoutRegistrationInfo(cancellationToken).ConfigureAwait(false);

            var serverVersion = new Version(serverInfo.Version);

            var version = InstallationManager.GetLatestCompatibleVersion(availablePackages, "MBTheater", null, serverVersion, ConfigurationManager.CommonConfiguration.SystemUpdateLevel);

            var versionObject = version == null || string.IsNullOrWhiteSpace(version.versionStr) ? null : new Version(version.versionStr);
            
            return versionObject != null ? new CheckForUpdateResult { AvailableVersion = versionObject.ToString(), IsUpdateAvailable = versionObject > ApplicationVersion, Package = version } :
                       new CheckForUpdateResult { AvailableVersion = ApplicationVersion.ToString(), IsUpdateAvailable = false };
        }

        /// <summary>
        /// Updates the application.
        /// </summary>
        /// <param name="package">The package that contains the update</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task.</returns>
        public override async Task UpdateApplication(PackageVersionInfo package, CancellationToken cancellationToken, IProgress<double> progress)
        {
            await InstallationManager.InstallPackage(package, progress, cancellationToken).ConfigureAwait(false);

            OnApplicationUpdated(package);
        }

        protected override void ConfigureAutoRunAtStartup(bool autorun)
        {
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Media Browser 3", "Media Browser Theater.lnk");

            if (autorun)
            {
                // Copy our shortut into the startup folder for this user
                File.Copy(shortcutPath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileName(shortcutPath) ?? "MediaBrowserTheaterStartup.lnk"), true);
            }
            else
            {
                // Remove our shortcut from the startup folder for this user
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileName(shortcutPath) ?? "MediaBrowserTheaterStartup.lnk"));
            }
        }

        public void ShutdownSystem()
        {
            PlaybackManager.StopAllPlayback();
            Process.Start("shutdown", "/s /t 0");
        }

        public void RebootSystem()
        {
            PlaybackManager.StopAllPlayback();
            Process.Start("shutdown", "/r /t 0");
        }

        public void SetSystemToSleep()
        {
            PlaybackManager.StopAllPlayback();
            Application.SetSuspendState(PowerState.Suspend, false, false);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            // ReSharper disable once CSharpWarnings::CS4014
            SendWolCommand();
        }

        public async Task SendWolCommand()
        {
            const int payloadSize = 102;
            var wolConfig = TheaterConfigurationManager.Configuration.WolConfiguration;

            if (wolConfig == null)
            {
                return;
            }

            Logger.Log(LogSeverity.Info, String.Format("Sending Wake on LAN signal to {0}", TheaterConfigurationManager.Configuration.ServerHostName));

            //Send magic packets to each address
            foreach (var macAddress in wolConfig.HostMacAddresses)
            {
                var macBytes = PhysicalAddress.Parse(macAddress).GetAddressBytes();

                Logger.Log(LogSeverity.Debug, String.Format("Sending magic packet to {0}", macAddress));

                //Construct magic packet
                var payload = new byte[payloadSize];
                Buffer.BlockCopy(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0, payload, 0, 6);

                for (var i = 1; i < 17; i++)
                    Buffer.BlockCopy(macBytes, 0, payload, 6 * i, 6);

                //Send packet LAN
                using (var udp = new UdpClient())
                {
                    try
                    {
                        udp.Connect(IPAddress.Broadcast, wolConfig.Port);
                        await udp.SendAsync(payload, payloadSize);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(String.Format("Magic packet send failed: {0}", ex.Message));
                    }
                }

                //Send packet WAN
                using (var udp = new UdpClient())
                {
                    try
                    {
                        udp.Connect(TheaterConfigurationManager.Configuration.ServerHostName, wolConfig.Port);
                        await udp.SendAsync(payload, payloadSize);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(String.Format("Magic packet send failed: {0}", ex.Message));
                    }
                }
            }
        }

        public override string Name
        {
            get { return "Media Browser Theater"; }
        }

        public override bool IsRunningAsService
        {
            get { return false; }
        }
    }
}