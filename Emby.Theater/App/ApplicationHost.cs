using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Events;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Common.Progress;
using MediaBrowser.Common.Security;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Cryptography;
using MediaBrowser.Model.Diagnostics;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.News;
using MediaBrowser.Model.Reflection;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using MediaBrowser.Model.Social;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Model.Text;
using MediaBrowser.Model.Threading;
using MediaBrowser.Model.Updates;
using MediaBrowser.Model.Xml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emby.Theater.App;
using Emby.Theater.Archiving;
using Emby.Theater.Configuration;
using Emby.Theater.Cryptography;
using Emby.Theater.IO;
using Emby.Theater.Net;
using Emby.Theater.Networking;
using Emby.Theater.ScheduledTasks;
using Emby.Theater.Serialization;
using Emby.Theater.Updates;
using ServiceStack;

namespace Emby.Theater.App
{
    /// <summary>
    /// Class CompositionRoot
    /// </summary>
    public class ApplicationHost : IApplicationHost, IDisposable
    {
        public bool CanSelfRestart
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can self update.
        /// </summary>
        /// <value><c>true</c> if this instance can self update; otherwise, <c>false</c>.</value>
        public bool CanSelfUpdate
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
        /// Occurs when [has pending restart changed].
        /// </summary>
        public event EventHandler HasPendingRestartChanged;

        /// <summary>
        /// Occurs when [application updated].
        /// </summary>
        public event EventHandler<GenericEventArgs<PackageVersionInfo>> ApplicationUpdated;

        /// <summary>
        /// Gets or sets a value indicating whether this instance has changes that require the entire application to restart.
        /// </summary>
        /// <value><c>true</c> if this instance has pending application restart; otherwise, <c>false</c>.</value>
        public bool HasPendingRestart { get; private set; }

        public bool IsShuttingDown { get; private set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        /// <value>The plugins.</value>
        public IPlugin[] Plugins { get; protected set; }

        /// <summary>
        /// Gets or sets the log manager.
        /// </summary>
        /// <value>The log manager.</value>
        public ILogManager LogManager { get; protected set; }

        /// <summary>
        /// Gets the application paths.
        /// </summary>
        /// <value>The application paths.</value>
        protected ApplicationPaths ApplicationPaths { get; set; }

        /// <summary>
        /// Gets assemblies that failed to load
        /// </summary>
        /// <value>The failed assemblies.</value>
        public List<string> FailedAssemblies { get; protected set; }

        /// <summary>
        /// Gets all concrete types.
        /// </summary>
        /// <value>All concrete types.</value>
        public Type[] AllConcreteTypes { get; protected set; }

        /// <summary>
        /// The disposable parts
        /// </summary>
        protected readonly List<IDisposable> DisposableParts = new List<IDisposable>();

        /// <summary>
        /// Gets a value indicating whether this instance is first run.
        /// </summary>
        /// <value><c>true</c> if this instance is first run; otherwise, <c>false</c>.</value>
        public bool IsFirstRun { get; private set; }

        /// <summary>
        /// Gets the configuration manager.
        /// </summary>
        /// <value>The configuration manager.</value>
        public IConfigurationManager ConfigurationManager { get; set; }

        public IFileSystem FileSystemManager { get; set; }

        public IEnvironmentInfo EnvironmentInfo { get; set; }

        private IBlurayExaminer BlurayExaminer { get; set; }

        public PackageVersionClass SystemUpdateLevel
        {
            get
            {

#if BETA
                return PackageVersionClass.Beta;
#endif
                return PackageVersionClass.Release;
            }
        }

        public virtual string OperatingSystemDisplayName
        {
            get { return EnvironmentInfo.OperatingSystemName; }
        }

        /// <summary>
        /// The container
        /// </summary>
        protected readonly SimpleInjector.Container Container = new SimpleInjector.Container();

        protected ISystemEvents SystemEvents { get; set; }
        public IMemoryStreamFactory MemoryStreamFactory { get; set; }

        /// <summary>
        /// Gets the configuration manager.
        /// </summary>
        /// <returns>IConfigurationManager.</returns>
        protected IConfigurationManager GetConfigurationManager()
        {
            return new ConfigurationManager(ApplicationPaths, LogManager, XmlSerializer, FileSystemManager);
        }

        /// <summary>
        /// Gets or sets the installation manager.
        /// </summary>
        /// <value>The installation manager.</value>
        protected IInstallationManager InstallationManager { get; private set; }

        /// <summary>
        /// Gets or sets the zip client.
        /// </summary>
        /// <value>The zip client.</value>
        protected IZipClient ZipClient { get; private set; }

        protected readonly StartupOptions StartupOptions;
        protected readonly string ReleaseAssetFilename;

        internal IPowerManagement PowerManagement { get; private set; }

        protected IProcessFactory ProcessFactory { get; private set; }
        protected ITimerFactory TimerFactory { get; private set; }
        public ICryptoProvider CryptographyProvider = new CryptographyProvider();
        protected readonly IXmlSerializer XmlSerializer;

        public ISocketFactory SocketFactory { get; private set; }
        protected ITaskManager TaskManager { get; private set; }
        public IHttpClient HttpClient { get; private set; }
        public INetworkManager NetworkManager { get; set; }
        public IJsonSerializer JsonSerializer { get; private set; }
        protected IIsoManager IsoManager { get; private set; }
        public ITextEncoding TextEncoding { get; private set; }

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

        protected INetworkManager CreateNetworkManager(ILogger logger)
        {
            return new NetworkManager(logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationHost" /> class.
        /// </summary>
        public ApplicationHost(ApplicationPaths applicationPaths,
            ILogManager logManager,
            StartupOptions options,
            IFileSystem fileSystem,
            IPowerManagement powerManagement,
            string releaseAssetFilename,
            IEnvironmentInfo environmentInfo,
            ISystemEvents systemEvents,
            INetworkManager networkManager)
        {
            // hack alert, until common can target .net core
            BaseExtensions.CryptographyProvider = CryptographyProvider;

            XmlSerializer = new MyXmlSerializer(fileSystem, logManager.GetLogger("XmlSerializer"));

            NetworkManager = networkManager;
            EnvironmentInfo = environmentInfo;
            SystemEvents = systemEvents;
            MemoryStreamFactory = new MemoryStreamProvider();

            FailedAssemblies = new List<string>();

            ApplicationPaths = applicationPaths;
            LogManager = logManager;
            FileSystemManager = fileSystem;

            ConfigurationManager = GetConfigurationManager();

            // Initialize this early in case the -v command line option is used
            Logger = LogManager.GetLogger("App");

            StartupOptions = options;
            ReleaseAssetFilename = releaseAssetFilename;
            PowerManagement = powerManagement;

            SetBaseExceptionMessage();
        }

        private Version _version;
        /// <summary>
        /// Gets the current application version
        /// </summary>
        /// <value>The application version.</value>
        public Version ApplicationVersion
        {
            get
            {
                return _version ?? (_version = GetAssembly(GetType()).GetName().Version);
            }
        }

        private DeviceId _deviceId;
        public string SystemId
        {
            get
            {
                if (_deviceId == null)
                {
                    _deviceId = new DeviceId(ApplicationPaths, LogManager.GetLogger("SystemId"), FileSystemManager);
                }

                return _deviceId.Value;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return "Emby Theater";
            }
        }

        private Assembly GetAssembly(Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public virtual bool SupportsAutoRunAtStartup
        {
            get
            {
                return EnvironmentInfo.OperatingSystem == MediaBrowser.Model.System.OperatingSystem.Windows;
            }
        }

        /// <summary>
        /// Creates an instance of type and resolves all constructor dependancies
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        public object CreateInstance(Type type)
        {
            try
            {
                return Container.GetInstance(type);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error creating {0}", ex, type.FullName);

                throw;
            }
        }

        /// <summary>
        /// Creates the instance safe.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        protected object CreateInstanceSafe(Type type)
        {
            try
            {
                return Container.GetInstance(type);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error creating {0}", ex, type.FullName);
                // Don't blow up in release mode
                return null;
            }
        }

        /// <summary>
        /// Registers the specified obj.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="manageLifetime">if set to <c>true</c> [manage lifetime].</param>
        protected void RegisterSingleInstance<T>(T obj, bool manageLifetime = true)
            where T : class
        {
            Container.RegisterSingleton(obj);

            if (manageLifetime)
            {
                var disposable = obj as IDisposable;

                if (disposable != null)
                {
                    DisposableParts.Add(disposable);
                }
            }
        }

        /// <summary>
        /// Registers the single instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The func.</param>
        protected void RegisterSingleInstance<T>(Func<T> func)
            where T : class
        {
            Container.RegisterSingleton(func);
        }

        /// <summary>
        /// Resolves this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>``0.</returns>
        public T Resolve<T>()
        {
            return (T)Container.GetRegistration(typeof(T), true).GetInstance();
        }

        /// <summary>
        /// Resolves this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>``0.</returns>
        public T TryResolve<T>()
        {
            var result = Container.GetRegistration(typeof(T), false);

            if (result == null)
            {
                return default(T);
            }
            return (T)result.GetInstance();
        }

        /// <summary>
        /// Loads the assembly.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>Assembly.</returns>
        protected Assembly LoadAssembly(string file)
        {
            try
            {
                return Assembly.Load(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                FailedAssemblies.Add(file);
                Logger.ErrorException("Error loading assembly {0}", ex, file);
                return null;
            }
        }

        /// <summary>
        /// Gets the export types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IEnumerable{Type}.</returns>
        public IEnumerable<Type> GetExportTypes<T>()
        {
            var currentType = typeof(T);

            return AllConcreteTypes.Where(currentType.IsAssignableFrom);
        }

        /// <summary>
        /// Gets the exports.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="manageLiftime">if set to <c>true</c> [manage liftime].</param>
        /// <returns>IEnumerable{``0}.</returns>
        public IEnumerable<T> GetExports<T>(bool manageLiftime = true)
        {
            var parts = GetExportTypes<T>()
                .Select(CreateInstanceSafe)
                .Where(i => i != null)
                .Cast<T>()
                .ToList();

            if (manageLiftime)
            {
                lock (DisposableParts)
                {
                    DisposableParts.AddRange(parts.OfType<IDisposable>());
                }
            }

            return parts;
        }

        private void SetBaseExceptionMessage()
        {
            var builder = GetBaseExceptionMessage(ApplicationPaths);

            builder.Insert(0, string.Format("Version: {0}{1}", ApplicationVersion, Environment.NewLine));
            builder.Insert(0, "*** Error Report ***" + Environment.NewLine);

            LogManager.ExceptionMessagePrefix = builder.ToString();
        }

        /// <summary>
        /// Runs the startup tasks.
        /// </summary>
        public async Task RunStartupTasks()
        {
            Resolve<ITaskManager>().AddTasks(GetExports<IScheduledTask>(false));

            ConfigureAutorun();

            Logger.Info("Core startup complete");

            LogManager.RemoveConsoleOutput();
        }

        /// <summary>
        /// Configures the autorun.
        /// </summary>
        private void ConfigureAutorun()
        {
            try
            {
                ConfigureAutoRunAtStartup(ConfigurationManager.CommonConfiguration.RunAtStartup);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error configuring autorun", ex);
            }
        }

        private IJsonSerializer CreateJsonSerializer()
        {
            try
            {
                // https://github.com/ServiceStack/ServiceStack/blob/master/tests/ServiceStack.WebHost.IntegrationTests/Web.config#L4
                Licensing.RegisterLicense("1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6UHVNTVRPclhvT2ZIbjQ5MG5LZE1mUTd5RUMzQnBucTFEbTE3TDczVEF4QUNMT1FhNXJMOWkzVjFGL2ZkVTE3Q2pDNENqTkQyUktRWmhvUVBhYTBiekJGUUZ3ZE5aZHFDYm9hL3lydGlwUHI5K1JsaTBYbzNsUC85cjVJNHE5QVhldDN6QkE4aTlvdldrdTgyTk1relY2eis2dFFqTThYN2lmc0JveHgycFdjPSxFeHBpcnk6MjAxMy0wMS0wMX0=");
            }
            catch
            {
                // Failing under mono
            }

            return new JsonSerializer(FileSystemManager, LogManager.GetLogger("JsonSerializer"));
        }

        public async Task Init(IProgress<double> progress)
        {
            progress.Report(1);

            JsonSerializer = CreateJsonSerializer();

            OnLoggerLoaded(true);
            LogManager.LoggerLoaded += (s, e) => OnLoggerLoaded(false);

            IsFirstRun = !ConfigurationManager.CommonConfiguration.IsStartupWizardCompleted;
            progress.Report(2);

            LogManager.LogSeverity = ConfigurationManager.CommonConfiguration.EnableDebugLevelLogging
                ? LogSeverity.Debug
                : LogSeverity.Info;

            progress.Report(3);

            DiscoverTypes();
            progress.Report(14);

            SetHttpLimit();
            progress.Report(15);

            var innerProgress = new ActionableProgress<double>();
            innerProgress.RegisterAction(p => progress.Report(.8 * p + 15));

            await RegisterResources(innerProgress).ConfigureAwait(false);

            FindParts();
            progress.Report(95);

            progress.Report(100);
        }

        protected virtual void OnLoggerLoaded(bool isFirstLoad)
        {
            Logger.Info("Application version: {0}", ApplicationVersion);

            if (!isFirstLoad)
            {
                LogEnvironmentInfo(Logger, ApplicationPaths, false);
            }

            // Put the app config in the log for troubleshooting purposes
            Logger.LogMultiline("Application configuration:", LogSeverity.Info, new StringBuilder(JsonSerializer.SerializeToString(ConfigurationManager.CommonConfiguration)));

            if (Plugins != null)
            {
                var pluginBuilder = new StringBuilder();

                foreach (var plugin in Plugins)
                {
                    pluginBuilder.AppendLine(string.Format("{0} {1}", plugin.Name, plugin.Version));
                }

                Logger.LogMultiline("Plugins:", LogSeverity.Info, pluginBuilder);
            }
        }

        /// <summary>
        /// Registers resources that classes will depend on
        /// </summary>
        protected async Task RegisterResources(IProgress<double> progress)
        {
            RegisterSingleInstance(ConfigurationManager);
            RegisterSingleInstance<IApplicationHost>(this);

            RegisterSingleInstance<IApplicationPaths>(ApplicationPaths);

            RegisterSingleInstance(JsonSerializer);
            RegisterSingleInstance(MemoryStreamFactory);
            RegisterSingleInstance(SystemEvents);

            RegisterSingleInstance(LogManager, false);
            RegisterSingleInstance(Logger);

            RegisterSingleInstance(EnvironmentInfo);

            RegisterSingleInstance(FileSystemManager);

            HttpClient = new HttpClientManager.HttpClientManager(ApplicationPaths, LogManager.GetLogger("HttpClient"), FileSystemManager, MemoryStreamFactory, GetDefaultUserAgent);
            RegisterSingleInstance(HttpClient);

            RegisterSingleInstance(NetworkManager);

            TaskManager = new TaskManager(ApplicationPaths, JsonSerializer, LogManager.GetLogger("TaskManager"), FileSystemManager, SystemEvents);
            RegisterSingleInstance(TaskManager);

            RegisterSingleInstance(XmlSerializer);

            RegisterSingleInstance(CryptographyProvider);

            SocketFactory = new SocketFactory(LogManager.GetLogger("SocketFactory"));
            RegisterSingleInstance(SocketFactory);

            //RegisterSingleInstance(PowerManagement);

            InstallationManager = new InstallationManager(LogManager.GetLogger("InstallationManager"), this, ApplicationPaths, HttpClient, JsonSerializer, ConfigurationManager, FileSystemManager, CryptographyProvider, PackageRuntime);
            RegisterSingleInstance(InstallationManager);

            ZipClient = new ZipClient(FileSystemManager);
            RegisterSingleInstance(ZipClient);

            progress.Report(100);
        }

        public virtual string PackageRuntime
        {
            get
            {
                return "netframework";
            }
        }

        public static void LogEnvironmentInfo(ILogger logger, IApplicationPaths appPaths, bool isStartup)
        {
            logger.LogMultiline("Emby", LogSeverity.Info, GetBaseExceptionMessage(appPaths));
        }

        protected static StringBuilder GetBaseExceptionMessage(IApplicationPaths appPaths)
        {
            var builder = new StringBuilder();

            builder.AppendLine(string.Format("Command line: {0}", string.Join(" ", Environment.GetCommandLineArgs())));

            builder.AppendLine(string.Format("Operating system: {0}", Environment.OSVersion));
            builder.AppendLine(string.Format("64-Bit OS: {0}", Environment.Is64BitOperatingSystem));
            builder.AppendLine(string.Format("64-Bit Process: {0}", Environment.Is64BitProcess));
            builder.AppendLine(string.Format("User Interactive: {0}", Environment.UserInteractive));

            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                {
                    builder.AppendLine("Mono: " + displayName.Invoke(null, null));
                }
            }

            builder.AppendLine(string.Format("Processor count: {0}", Environment.ProcessorCount));
            builder.AppendLine(string.Format("Program data path: {0}", appPaths.ProgramDataPath));
            builder.AppendLine(string.Format("Application directory: {0}", appPaths.ProgramSystemPath));

            return builder;
        }

        private void SetHttpLimit()
        {
            try
            {
                // Increase the max http request limit
                ServicePointManager.DefaultConnectionLimit = Math.Max(96, ServicePointManager.DefaultConnectionLimit);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error setting http limit", ex);
            }
        }

        private string GetDefaultUserAgent()
        {
            var name = FormatAttribute(Name);

            return name + "/" + ApplicationVersion;
        }

        private string FormatAttribute(string str)
        {
            var arr = str.ToCharArray();

            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c)
                                                  || char.IsWhiteSpace(c))));

            var result = new string(arr);

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "Emby";
            }

            return result;
        }

        /// <summary>
        /// Finds the parts.
        /// </summary>
        protected void FindParts()
        {
            ConfigurationManager.AddParts(GetExports<IConfigurationFactory>());
            Plugins = new IPlugin[] { };
        }

        /// <summary>
        /// Discovers the types.
        /// </summary>
        protected void DiscoverTypes()
        {
            FailedAssemblies.Clear();

            var assemblies = GetComposablePartAssemblies().ToList();

            foreach (var assembly in assemblies)
            {
                Logger.Info("Loading {0}", assembly.FullName);
            }

            AllConcreteTypes = assemblies
                .SelectMany(GetTypes)
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && !t.IsGenericType)
                .ToArray();
        }

        /// <summary>
        /// Gets a list of types within an assembly
        /// This will handle situations that would normally throw an exception - such as a type within the assembly that depends on some other non-existant reference
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>IEnumerable{Type}.</returns>
        /// <exception cref="System.ArgumentNullException">assembly</exception>
        protected List<Type> GetTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                return new List<Type>();
            }

            try
            {
                // This null checking really shouldn't be needed but adding it due to some
                // unhandled exceptions in mono 5.0 that are a little hard to hunt down
                var types = assembly.GetTypes() ?? new Type[] { };
                return types.Where(t => t != null).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.LoaderExceptions != null)
                {
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        if (loaderException != null)
                        {
                            Logger.Error("LoaderException: " + loaderException.Message);
                        }
                    }
                }

                // If it fails we can still get a list of the Types it was able to resolve
                var types = ex.Types ?? new Type[] { };
                return types.Where(t => t != null).ToList();
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error loading types from assembly", ex);

                return new List<Type>();
            }
        }

        /// <summary>
        /// Notifies that the kernel that a change has been made that requires a restart
        /// </summary>
        public void NotifyPendingRestart()
        {
            Logger.Info("App needs to be restarted.");

            var changed = !HasPendingRestart;

            HasPendingRestart = true;

            if (changed)
            {
                EventHelper.QueueEventIfNotNull(HasPendingRestartChanged, this, EventArgs.Empty, Logger);
            }
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        public void Restart()
        {
            if (!CanSelfRestart)
            {
                throw new PlatformNotSupportedException("The server is unable to self-restart. Please restart manually.");
            }

            if (IsShuttingDown)
            {
                return;
            }

            IsShuttingDown = true;
        }

        /// <summary>
        /// Gets the composable part assemblies.
        /// </summary>
        /// <returns>IEnumerable{Assembly}.</returns>
        protected IEnumerable<Assembly> GetComposablePartAssemblies()
        {
            // Include composable parts in the Model assembly 
            yield return typeof(SystemInfo).Assembly;

            // Include composable parts in the Common assembly 
            yield return typeof(IApplicationPaths).Assembly;

            // Include composable parts in the running assembly
            yield return GetType().Assembly;
        }

        /// <summary>
        /// Shuts down.
        /// </summary>
        public async Task Shutdown()
        {
            if (IsShuttingDown)
            {
                return;
            }

            IsShuttingDown = true;

            Program.Exit();
        }

        public event EventHandler HasUpdateAvailableChanged;

        private bool _hasUpdateAvailable;
        public bool HasUpdateAvailable
        {
            get { return _hasUpdateAvailable; }
            set
            {
                var fireEvent = value && !_hasUpdateAvailable;

                _hasUpdateAvailable = value;

                if (fireEvent)
                {
                    EventHelper.FireEventIfNotNull(HasUpdateAvailableChanged, this, EventArgs.Empty, Logger);
                }
            }
        }

        /// <summary>
        /// Removes the plugin.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public void RemovePlugin(IPlugin plugin)
        {
            var list = Plugins.ToList();
            list.Remove(plugin);
            Plugins = list.ToArray();
        }

        /// <summary>
        /// Checks for update.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task{CheckForUpdateResult}.</returns>
        public Task<CheckForUpdateResult> CheckForApplicationUpdate(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var updateLevel = SystemUpdateLevel;
            return new GithubUpdater(HttpClient, JsonSerializer)
                .CheckForUpdateResult("MediaBrowser", "Emby.Theater.Windows", ApplicationVersion, updateLevel, Program.UpdatePackageName, "emby.theater", "emby.theater.zip", TimeSpan.FromTicks(0), cancellationToken);
        }

        /// <summary>
        /// Updates the application.
        /// </summary>
        /// <param name="package">The package that contains the update</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task.</returns>
        public async Task UpdateApplication(PackageVersionInfo package, CancellationToken cancellationToken, IProgress<double> progress)
        {
            await InstallationManager.InstallPackage(package, false, progress, cancellationToken).ConfigureAwait(false);

            OnApplicationUpdated(package);
        }

        protected void ConfigureAutoRunAtStartup(bool autorun)
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

        public void LaunchUrl(string url)
        {
            if (EnvironmentInfo.OperatingSystem != MediaBrowser.Model.System.OperatingSystem.Windows &&
                EnvironmentInfo.OperatingSystem != MediaBrowser.Model.System.OperatingSystem.OSX)
            {
                throw new NotImplementedException();
            }

            var process = ProcessFactory.Create(new ProcessOptions
            {
                FileName = url,
                //EnableRaisingEvents = true,
                UseShellExecute = true,
                ErrorDialog = false
            });

            process.Exited += ProcessExited;

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error launching url: {0}", url);
                Logger.ErrorException("Error launching url: {0}", ex, url);

                throw;
            }
        }

        private static void ProcessExited(object sender, EventArgs e)
        {
            ((IProcess)sender).Dispose();
        }

        public virtual void EnableLoopback(string appName)
        {
        }

        /// <summary>
        /// Called when [application updated].
        /// </summary>
        /// <param name="package">The package.</param>
        protected void OnApplicationUpdated(PackageVersionInfo package)
        {
            Logger.Info("Application has been updated to version {0}", package.versionStr);

            EventHelper.FireEventIfNotNull(ApplicationUpdated, this, new GenericEventArgs<PackageVersionInfo>
            {
                Argument = package

            }, Logger);

            NotifyPendingRestart();
        }

        private bool _disposed;
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                var type = GetType();

                LogManager.AddConsoleOutput();
                Logger.Info("Disposing " + type.Name);

                var parts = DisposableParts.Distinct().Where(i => i.GetType() != type).ToList();
                DisposableParts.Clear();

                foreach (var part in parts)
                {
                    Logger.Info("Disposing " + part.GetType().Name);

                    try
                    {
                        part.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException("Error disposing {0}", ex, part.GetType().Name);
                    }
                }
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
    }
}
