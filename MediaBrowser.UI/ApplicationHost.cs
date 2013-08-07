using MediaBrowser.ApiInteraction;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Constants;
using MediaBrowser.Common.Implementations;
using MediaBrowser.Common.Implementations.ScheduledTasks;
using MediaBrowser.Common.IO;
using MediaBrowser.IsoMounter;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Updates;
using MediaBrowser.Plugins.DefaultTheme;
using MediaBrowser.Theater.Core.Login;
using MediaBrowser.Theater.DirectShow;
using MediaBrowser.Theater.Implementations.Configuration;
using MediaBrowser.Theater.Implementations.Playback;
using MediaBrowser.Theater.Implementations.Presentation;
using MediaBrowser.Theater.Implementations.Session;
using MediaBrowser.Theater.Implementations.Theming;
using MediaBrowser.Theater.Implementations.UserInput;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.Playback;
using MediaBrowser.UI.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Class CompositionRoot
    /// </summary>
    internal class ApplicationHost : BaseApplicationHost<ApplicationPaths>
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public IApiClient ApiClient { get; private set; }

        public IThemeManager ThemeManager { get; private set; }
        public IPlaybackManager PlaybackManager { get; private set; }
        public IImageManager ImageManager { get; private set; }
        public INavigationService NavigationService { get; private set; }
        public ISessionManager SessionManager { get; private set; }
        public IPresentationManager PresentationManager { get; private set; }
        public IUserInputManager UserInputManager { get; private set; }

        public ConfigurationManager TheaterConfigurationManager
        {
            get { return (ConfigurationManager)ConfigurationManager; }
        }

        protected override string LogFilePrefixName
        {
            get { return "mbt"; }
        }

        public override async Task Init()
        {
            await base.Init().ConfigureAwait(false);

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

        /// <summary>
        /// The full path to our startmenu shortcut
        /// </summary>
        protected override string ProductShortcutPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Media Browser 3", "Media Browser Theater.lnk"); }
        }

        /// <summary>
        /// Registers resources that classes will depend on
        /// </summary>
        protected override async Task RegisterResources()
        {
            ReloadApiClient();

            await base.RegisterResources().ConfigureAwait(false);

            ThemeManager = new ThemeManager();
            RegisterSingleInstance(ThemeManager);

            PresentationManager = new TheaterApplicationWindow(Logger, ThemeManager);
            RegisterSingleInstance(PresentationManager);

            RegisterSingleInstance(ApplicationPaths);

            RegisterSingleInstance<ITheaterConfigurationManager>(TheaterConfigurationManager);

            RegisterSingleInstance<IIsoManager>(new PismoIsoManager(Logger));

            ImageManager = new ImageManager(ApiClient, ApplicationPaths, TheaterConfigurationManager);
            RegisterSingleInstance(ImageManager);

            UserInputManager = new UserInputManager();
            RegisterSingleInstance(UserInputManager);

            NavigationService = new NavigationService(ThemeManager, () => PlaybackManager, ApiClient, PresentationManager, TheaterConfigurationManager, () => SessionManager, this, InstallationManager, ImageManager, Logger, UserInputManager);
            RegisterSingleInstance(NavigationService);

            PlaybackManager = new PlaybackManager(TheaterConfigurationManager, Logger, ApiClient, NavigationService, PresentationManager);
            RegisterSingleInstance(PlaybackManager);

            SessionManager = new SessionManager(NavigationService, ApiClient, Logger, ThemeManager, TheaterConfigurationManager, PlaybackManager);
            RegisterSingleInstance(SessionManager);

            RegisterSingleInstance(ApiClient);

            RegisterSingleInstance<IHiddenWindow>(new AppHiddenWIndow());
        }

        /// <summary>
        /// Finds the parts.
        /// </summary>
        protected override void FindParts()
        {
            base.FindParts();

            BasePage.Logger = Logger;

            ThemeManager.AddParts(GetExports<ITheme>());
            PresentationManager.AddParts(GetExports<ITheaterApp>(), GetExports<ISettingsPage>(), GetExports<IHomePage>());

            PlaybackManager.AddParts(GetExports<IMediaPlayer>());
        }

        /// <summary>
        /// Disposes the current ApiClient and creates a new one
        /// </summary>
        private void ReloadApiClient()
        {
            var logger = LogManager.GetLogger("ApiClient");

            ApiClient = new ApiClient(logger, TheaterConfigurationManager.Configuration.ServerHostName, TheaterConfigurationManager.Configuration.ServerApiPort, "Media Browser Theater", Environment.MachineName, Environment.MachineName, ApplicationVersion.ToString())
            {
                JsonSerializer = JsonSerializer,
                ImageQuality = TheaterConfigurationManager.Configuration.DownloadCompressedImages
                                                       ? 90
                                                       : 100
            };
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        public override void Restart()
        {
            App.Instance.Restart();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can self update.
        /// </summary>
        /// <value><c>true</c> if this instance can self update; otherwise, <c>false</c>.</value>
        public override bool CanSelfUpdate
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the composable part assemblies.
        /// </summary>
        /// <returns>IEnumerable{Assembly}.</returns>
        protected override IEnumerable<Assembly> GetComposablePartAssemblies()
        {
            // Gets all plugin assemblies by first reading all bytes of the .dll and calling Assembly.Load against that
            // This will prevent the .dll file from getting locked, and allow us to replace it when needed
            foreach (var pluginAssembly in Directory
                .EnumerateFiles(ApplicationPaths.PluginsPath, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(LoadAssembly).Where(a => a != null))
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

            // Default theme assembly
            yield return typeof(DefaultTheme).Assembly;
        }

        /// <summary>
        /// Shuts down.
        /// </summary>
        public override void Shutdown()
        {
            App.Instance.Shutdown();
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

        public override async Task<CheckForUpdateResult> CheckForApplicationUpdate(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var availablePackages = await PackageManager.GetAvailablePackagesWithoutRegistrationInfo(CancellationToken.None).ConfigureAwait(false);
            var version = InstallationManager.GetLatestCompatibleVersion(availablePackages, Constants.MbTheaterPkgName, ConfigurationManager.CommonConfiguration.SystemUpdateLevel);

            return version != null ? new CheckForUpdateResult { AvailableVersion = version.version, IsUpdateAvailable = version.version > ApplicationVersion, Package = version } :
                       new CheckForUpdateResult { AvailableVersion = ApplicationVersion, IsUpdateAvailable = false };
        }
    }
}