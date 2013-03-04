using MediaBrowser.ApiInteraction;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Implementations;
using MediaBrowser.Common.Implementations.HttpServer;
using MediaBrowser.Common.Implementations.ScheduledTasks;
using MediaBrowser.Common.IO;
using MediaBrowser.Common.Kernel;
using MediaBrowser.IsoMounter;
using MediaBrowser.Model.Connectivity;
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

        /// <summary>
        /// Registers resources that classes will depend on
        /// </summary>
        protected override async Task RegisterResources()
        {
            ReloadApiClient();

            try
            {
                await new PluginUpdater(this, Logger, ApplicationPaths, ApiClient).UpdatePlugins().ConfigureAwait(false);
            }
            catch (HttpException ex)
            {
                Logger.ErrorException("Error updating plugins from the server", ex);
            }
            
            await base.RegisterResources().ConfigureAwait(false);

            RegisterSingleInstance<UIApplicationPaths>(ApplicationPaths);

            RegisterSingleInstance(UIConfigurationManager);

            RegisterSingleInstance<IIsoManager>(new PismoIsoManager(Logger));
            RegisterSingleInstance(ServerFactory.CreateServer(this, ProtobufSerializer, Logger, "Media Browser Theater", "index.html"), false);
        }

        /// <summary>
        /// Disposes the current ApiClient and creates a new one
        /// </summary>
        private void ReloadApiClient()
        {
            ApiClient = new ApiClient(Logger, new AsyncHttpClient(new WebRequestHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate)
            }))
            {
                ServerHostName = UIConfigurationManager.Configuration.ServerHostName,
                ServerApiPort = UIConfigurationManager.Configuration.ServerApiPort,
                ClientType = ClientType.Pc,
                DeviceName = Environment.MachineName,
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
            yield return typeof(IKernel).Assembly;

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

        /// <summary>
        /// Updates the application.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task.</returns>
        public override Task UpdateApplication(PackageVersionInfo package, CancellationToken cancellationToken, IProgress<double> progress)
        {
            return Task.Run(() => { });
        }

        protected override IConfigurationManager GetConfigurationManager()
        {
            return new UIConfigurationManager(ApplicationPaths, LogManager, XmlSerializer);
        }

        protected override IKernel GetKernel()
        {
            return new UIKernel(this, LogManager, ConfigurationManager);
        }
    }
}