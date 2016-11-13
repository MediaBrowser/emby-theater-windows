using Emby.Theater.Configuration;
using Emby.Theater.Network;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Implementations;
using MediaBrowser.Common.Implementations.IO;
using MediaBrowser.Common.Implementations.ScheduledTasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Updates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaBrowser.Common.Implementations.Updates;
using MediaBrowser.Model.IO;

namespace Emby.Theater.App
{
    /// <summary>
    /// Class CompositionRoot
    /// </summary>
    public class ApplicationHost : BaseApplicationHost<ApplicationPaths>
    {
        public ApplicationHost(ApplicationPaths applicationPaths, ILogManager logManager)
            : base(applicationPaths, logManager, new WindowsFileSystem(new PatternsLogger(logManager.GetLogger("Logger"))))
        {
        }

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

        public override Version ApplicationVersion
        {
            get { return GetType().Assembly.GetName().Version; }
        }

        public IZipClient GetZipClient()
        {
            return ZipClient;
        }

        public IHttpClient GetHttpClient()
        {
            return HttpClient;
        }

        public IIsoManager GetIsoManager()
        {
            return IsoManager;
        }

        protected override INetworkManager CreateNetworkManager(ILogger logger)
        {
            return new NetworkManager(logger);
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

        private async Task BeforeShutdown()
        {
            //PlaybackManager.StopAllPlayback();
        }

        public override async Task Restart()
        {
            await BeforeShutdown();

            //await Task.Run(() => App.Instance.Dispatcher.Invoke(() => App.Instance.Restart()));
        }

        public override async Task Shutdown()
        {
            await BeforeShutdown();

            Program.Exit();
        }

        protected override IConfigurationManager GetConfigurationManager()
        {
            return new ConfigurationManager(ApplicationPaths, LogManager, XmlSerializer, FileSystemManager);
        }

        /// <summary>
        /// Checks for update.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task{CheckForUpdateResult}.</returns>
        public override Task<CheckForUpdateResult> CheckForApplicationUpdate(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var updateLevel = ConfigurationManager.CommonConfiguration.SystemUpdateLevel;
            return new GithubUpdater(HttpClient, JsonSerializer)
                .CheckForUpdateResult("MediaBrowser", "Emby.Theater.Windows", ApplicationVersion, updateLevel, "emby.theater.zip", "emby.theater", "emby.theater.zip", TimeSpan.FromTicks(0), cancellationToken);
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
            await InstallationManager.InstallPackage(package, false, progress, cancellationToken).ConfigureAwait(false);

            OnApplicationUpdated(package);
        }

        protected override void ConfigureAutoRunAtStartup(bool autorun)
        {
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Emby", "Emby Theater.lnk");

            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            if (autorun)
            {
                // Copy our shortut into the startup folder for this user
                File.Copy(shortcutPath, Path.Combine(startupPath, Path.GetFileName(shortcutPath)), true);
            }
            else
            {
                // Remove our shortcut from the startup folder for this user
                File.Delete(Path.Combine(startupPath, Path.GetFileName(shortcutPath)));
            }
        }

        public void ShutdownSystem()
        {
            //PlaybackManager.StopAllPlayback();
            Process.Start("shutdown", "/s /t 0");
        }

        public void RebootSystem()
        {
            //PlaybackManager.StopAllPlayback();
            Process.Start("shutdown", "/r /t 0");
        }

        public void SetSystemToSleep()
        {
            //PlaybackManager.StopAllPlayback();
            Application.SetSuspendState(PowerState.Suspend, false, false);
        }

        public override string Name
        {
            get { return "Emby Theater"; }
        }

        public override bool IsRunningAsService
        {
            get { return false; }
        }
    }
}
