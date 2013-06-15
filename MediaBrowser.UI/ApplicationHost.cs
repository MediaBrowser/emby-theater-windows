using MediaBrowser.ApiInteraction;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Implementations;
using MediaBrowser.Common.Implementations.ScheduledTasks;
using MediaBrowser.Common.IO;
using MediaBrowser.IsoMounter;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Updates;
using MediaBrowser.Plugins.DefaultTheme;
using MediaBrowser.Theater.ExternalPlayer;
using MediaBrowser.Theater.Implementations.Configuration;
using MediaBrowser.Theater.Implementations.Playback;
using MediaBrowser.Theater.Implementations.Presentation;
using MediaBrowser.Theater.Implementations.Session;
using MediaBrowser.Theater.Implementations.Theming;
using MediaBrowser.Theater.Implementations.UserInput;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Vlc;
using MediaBrowser.UI.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
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
        public IApplicationWindow ApplicationWindow { get; private set; }
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

            ApplicationWindow = new TheaterApplicationWindow(Logger);
            RegisterSingleInstance(ApplicationWindow);

            RegisterSingleInstance(ApplicationPaths);

            RegisterSingleInstance(TheaterConfigurationManager);

            RegisterSingleInstance<IIsoManager>(new PismoIsoManager(Logger));

            ThemeManager = new ThemeManager();
            RegisterSingleInstance(ThemeManager);

            NavigationService = new NavigationService(ThemeManager, () => PlaybackManager, ApiClient);
            RegisterSingleInstance(NavigationService);

            PlaybackManager = new PlaybackManager(TheaterConfigurationManager, Logger, ApiClient, NavigationService, ApplicationWindow);
            RegisterSingleInstance(PlaybackManager);

            ImageManager = new ImageManager(ApiClient, ApplicationPaths);
            RegisterSingleInstance(ImageManager);

            SessionManager = new SessionManager(NavigationService, ApiClient, Logger);
            RegisterSingleInstance(SessionManager);

            RegisterSingleInstance(ApiClient);

            UserInputManager = new UserInputManager();
            RegisterSingleInstance(UserInputManager);

            RegisterSingleInstance<IHiddenWindow>(new AppHiddenWIndow());
        }

        /// <summary>
        /// Finds the parts.
        /// </summary>
        protected override void FindParts()
        {
            base.FindParts();

            ThemeManager.AddParts(GetExports<ITheme>());

            PlaybackManager.AddParts(GetExports<IMediaPlayer>());
        }

        /// <summary>
        /// Disposes the current ApiClient and creates a new one
        /// </summary>
        private void ReloadApiClient()
        {
            var logger = LogManager.GetLogger("ApiClient");

            ApiClient = new ApiClient(logger, new AsyncHttpClient(logger, new WebRequestHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate)

            }), TheaterConfigurationManager.Configuration.ServerHostName, TheaterConfigurationManager.Configuration.ServerApiPort, "Media Browser Theater", Environment.MachineName, Environment.MachineName)
            {
                JsonSerializer = JsonSerializer
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
        /// Checks for update.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task{CheckForUpdateResult}.</returns>
        public override Task<CheckForUpdateResult> CheckForApplicationUpdate(CancellationToken cancellationToken, IProgress<double> progress)
        {
            return Task.FromResult(new CheckForUpdateResult { });
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

            // Vlc assembly
            yield return typeof(NVlcPlayer).Assembly;

            // External player assembly
            yield return typeof(GenericExternalPlayer).Assembly;
            
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

        protected override IConfigurationManager GetConfigurationManager()
        {
            return new ConfigurationManager(ApplicationPaths, LogManager, XmlSerializer);
        }
    }
}