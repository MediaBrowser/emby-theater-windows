using MediaBrowser.ApiInteraction;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Implementations;
using MediaBrowser.Common.Implementations.ScheduledTasks;
using MediaBrowser.Common.IO;
using MediaBrowser.IsoMounter;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Updates;
using MediaBrowser.UI.Configuration;
using MediaBrowser.UI.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class ApplicationHost : BaseApplicationHost<UIApplicationPaths>
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public ApiClient ApiClient { get; private set; }

        public UIConfigurationManager UIConfigurationManager
        {
            get { return (UIConfigurationManager)ConfigurationManager; }
        }
        
        protected override string LogFilePrefixName
        {
            get { return "MBT"; }
        }

        protected UIKernel UIKernel { get; set; }

        public override async Task Init()
        {
            await base.Init().ConfigureAwait(false);

            UIKernel.Init();

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
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),"Media Browser 3", "Media Browser Theater.lnk"); }
        }

        /// <summary>
        /// Registers resources that classes will depend on
        /// </summary>
        protected override async Task RegisterResources()
        {
            UIKernel = new UIKernel(this);

            ReloadApiClient();

            await base.RegisterResources().ConfigureAwait(false);

            RegisterSingleInstance(ApplicationPaths);
            RegisterSingleInstance(UIKernel);

            RegisterSingleInstance(UIConfigurationManager);

            RegisterSingleInstance<IIsoManager>(new PismoIsoManager(Logger));
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

            }), UIConfigurationManager.Configuration.ServerHostName, UIConfigurationManager.Configuration.ServerApiPort, "Media Browser Theater", Environment.MachineName, Environment.MachineName)
            {
                SerializationFormat = SerializationFormats.Json,
                JsonSerializer = JsonSerializer,
                ProtobufSerializer = ProtobufSerializer
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
            return Task.FromResult(new CheckForUpdateResult{ });
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

            var runningDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var corePluginDirectory = Path.Combine(runningDirectory, "CorePlugins");

            // This will prevent the .dll file from getting locked, and allow us to replace it when needed
            foreach (var pluginAssembly in Directory
                .EnumerateFiles(corePluginDirectory, "*.dll", SearchOption.TopDirectoryOnly)
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
            return new UIConfigurationManager(ApplicationPaths, LogManager, XmlSerializer);
        }
    }
}