using System.Diagnostics;
using MediaBrowser.ApiInteraction;
using MediaBrowser.Common.Constants;
using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Common.Implementations.Updates;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Implementations.Configuration;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.System;
using MediaBrowser.UI.StartupWizard;
using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The single instance mutex
        /// </summary>
        private static Mutex _singleInstanceMutex;

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger _logger;

        private readonly ILogManager _logManager;

        /// <summary>
        /// Gets or sets the composition root.
        /// </summary>
        /// <value>The composition root.</value>
        private ApplicationHost _appHost;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static App Instance
        {
            get
            {
                return Current as App;
            }
        }

        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        public MainWindow ApplicationWindow { get; private set; }

        /// <summary>
        /// Gets the hidden window.
        /// </summary>
        /// <value>The hidden window.</value>
        internal HiddenForm HiddenWindow { get; set; }

        /// <summary>
        /// The _app paths
        /// </summary>
        private readonly ApplicationPaths _appPaths;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            bool createdNew;

            _singleInstanceMutex = new Mutex(true, @"Local\" + typeof(App).Assembly.GetName().Name, out createdNew);

            if (!createdNew)
            {
                _singleInstanceMutex = null;
                return;
            }

            var appPath = Process.GetCurrentProcess().MainModule.FileName;

            // Look for the existence of an update archive
            var appPaths = new ApplicationPaths(appPath);
            var logManager = new NlogManager(appPaths.LogDirectoryPath, "theater");
            logManager.ReloadLogger(LogSeverity.Debug);

            var updateArchive = Path.Combine(appPaths.TempUpdatePath, Constants.MbTheaterPkgName + ".zip");

            if (File.Exists(updateArchive))
            {
                // Update is there - execute update
                try
                {
                    new ApplicationUpdater().UpdateApplication(MBApplication.MBTheater, appPaths, updateArchive,
                        logManager.GetLogger("ApplicationUpdater"), string.Empty);

                    // And just let the app exit so it can update
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Error attempting to update application.\n\n{0}\n\n{1}",
                        e.GetType().Name, e.Message));
                }
            }

            var application = new App(appPaths, logManager);

            application.Run();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App(ApplicationPaths appPaths, ILogManager logManager)
        {
            _appPaths = appPaths;
            _logManager = logManager;

            InitializeComponent();
        }

        private Thread _hiddenWindowThread;

        /// <summary>
        /// Shows the application window.
        /// </summary>
        private void ShowApplicationWindow()
        {
            var win = new MainWindow(_logger, _appHost.PlaybackManager, _appHost.ApiClient, _appHost.ImageManager,
                _appHost, _appHost.PresentationManager, _appHost.UserInputManager, _appHost.TheaterConfigurationManager,
                _appHost.NavigationService);

            var config = _appHost.TheaterConfigurationManager.Configuration;

            System.Windows.Forms.FormStartPosition? startPosition = null;

            // Restore window position/size
            if (config.WindowState.HasValue)
            {
                // Set window state
                win.WindowState = config.WindowState.Value;

                // Set position if not maximized
                if (config.WindowState.Value != WindowState.Maximized)
                {
                    double left = 0;
                    double top = 0;

                    // Set left
                    if (config.WindowLeft.HasValue)
                    {
                        win.WindowStartupLocation = WindowStartupLocation.Manual;
                        startPosition = System.Windows.Forms.FormStartPosition.Manual;
                        win.Left = left = Math.Max(config.WindowLeft.Value, 0);
                    }

                    // Set top
                    if (config.WindowTop.HasValue)
                    {
                        win.WindowStartupLocation = WindowStartupLocation.Manual;
                        startPosition = System.Windows.Forms.FormStartPosition.Manual;
                        win.Top = top = Math.Max(config.WindowTop.Value, 0);
                    }

                    // Set width
                    if (config.WindowWidth.HasValue)
                    {
                        win.Width = Math.Min(config.WindowWidth.Value, SystemParameters.VirtualScreenWidth - left);
                    }

                    // Set height
                    if (config.WindowHeight.HasValue)
                    {
                        win.Height = Math.Min(config.WindowHeight.Value, SystemParameters.VirtualScreenHeight - top);
                    }
                }
            }

            ApplicationWindow = win;

            win.Loaded += win_Loaded;
            win.LocationChanged += win_LocationChanged;
            win.StateChanged += win_StateChanged;
            win.SizeChanged += win_SizeChanged;
            win.Closing += win_Closing;

            int? formWidth = null;
            int? formHeight = null;
            int? formLeft = null;
            int? formTop = null;

            try
            {
                formWidth = Convert.ToInt32(ApplicationWindow.Width);
                formHeight = Convert.ToInt32(ApplicationWindow.Height);
            }
            catch (OverflowException)
            {
                formWidth = null;
                formHeight = null;
            }
            try
            {
                formTop = Convert.ToInt32(ApplicationWindow.Top);
                formLeft = Convert.ToInt32(ApplicationWindow.Left);
            }
            catch (OverflowException)
            {
                formLeft = null;
                formTop = null;
            }

            var state = GetWindowsFormState(ApplicationWindow.WindowState);

            _hiddenWindowThread = new Thread(() => ShowHiddenWindow(formWidth, formHeight, formTop, formLeft, startPosition, state));
            _hiddenWindowThread.SetApartmentState(ApartmentState.STA);
            _hiddenWindowThread.IsBackground = true;
            _hiddenWindowThread.Start();
        }

        void win_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (HiddenWindow != null)
            {
                InvokeOnHiddenWindow(() => HiddenWindow.Close());
            }
        }

        async void win_Loaded(object sender, RoutedEventArgs e)
        {
            _appHost.StartEntryPoints();

            await LoadInitialPresentation().ConfigureAwait(false);

            ApplicationWindow.Loaded -= win_Loaded;
        }

        private void ShowHiddenWindow(int? width, int? height, int? top, int? left, System.Windows.Forms.FormStartPosition? startPosition, System.Windows.Forms.FormWindowState windowState)
        {
            HiddenWindow = new HiddenForm();
            HiddenWindow.Load += HiddenWindow_Load;

            if (width.HasValue)
            {
                HiddenWindow.Width = width.Value;
            }
            if (height.HasValue)
            {
                HiddenWindow.Height = height.Value;
            }
            if (top.HasValue)
            {
                HiddenWindow.Top = top.Value;
            }
            if (left.HasValue)
            {
                HiddenWindow.Left = left.Value;
            }

            HiddenWindow.WindowState = windowState;

            if (startPosition.HasValue)
            {
                HiddenWindow.StartPosition = startPosition.Value;
            }

            HiddenWindow.Show();

            System.Windows.Threading.Dispatcher.Run();
        }

        void HiddenWindow_Load(object sender, EventArgs e)
        {
            // Hide this from ALT-TAB
            //var handle = HiddenWindow.Handle;
            //var exStyle = (int)GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            //exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            //SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            var handle = HiddenWindow.Handle;

            ApplicationWindow.Dispatcher.InvokeAsync(() =>
            {
                new WindowInteropHelper(ApplicationWindow).Owner = handle;

                ApplicationWindow.Show();
            });
        }

        void win_LocationChanged(object sender, EventArgs e)
        {
            var top = ApplicationWindow.Top;
            var left = ApplicationWindow.Left;

            if (double.IsNaN(top) || double.IsNaN(left))
            {
                return;
            }

            InvokeOnHiddenWindow(() =>
            {
                HiddenWindow.Top = Convert.ToInt32(top);
                HiddenWindow.Left = Convert.ToInt32(left);
            });
        }

        void win_StateChanged(object sender, EventArgs e)
        {
            var state = GetWindowsFormState(ApplicationWindow.WindowState);

            ApplicationWindow.ShowInTaskbar = state == System.Windows.Forms.FormWindowState.Minimized;

            InvokeOnHiddenWindow(() =>
            {
                if (state == System.Windows.Forms.FormWindowState.Minimized)
                {
                    HiddenWindow.Hide();
                }
                else
                {
                    HiddenWindow.Show();
                    HiddenWindow.WindowState = state;
                }
            });
        }

        void win_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = ApplicationWindow.Width;
            var height = ApplicationWindow.Height;

            if (double.IsNaN(width) || double.IsNaN(height))
            {
                return;
            }

            InvokeOnHiddenWindow(() =>
            {
                HiddenWindow.Width = Convert.ToInt32(width);
                HiddenWindow.Height = Convert.ToInt32(height);
            });
        }

        private void InvokeOnHiddenWindow(Action action)
        {
            if (HiddenWindow.InvokeRequired)
            {
                HiddenWindow.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private System.Windows.Forms.FormWindowState GetWindowsFormState(WindowState state)
        {
            switch (state)
            {
                case WindowState.Maximized:
                    return System.Windows.Forms.FormWindowState.Maximized;
                case WindowState.Minimized:
                    return System.Windows.Forms.FormWindowState.Minimized;
            }
            return System.Windows.Forms.FormWindowState.Normal;
        }

        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion

        /// <summary>
        /// Loads the kernel.
        /// </summary>
        protected async void LoadApplication()
        {
            try
            {
                _appHost = new ApplicationHost(_appPaths, _logManager);

                _logger = _appHost.LogManager.GetLogger("App");

                await _appHost.Init(new Progress<double>());

                LoadListBoxItemResourceFile();

                // Load default theme
                await _appHost.ThemeManager.LoadDefaultTheme();

                _appHost.TheaterConfigurationManager.ConfigurationUpdated += TheaterConfigurationManager_ConfigurationUpdated;

                ShowApplicationWindow();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error launching application", ex);

                MessageBox.Show("There was an error launching Media Browser: " + ex.Message);

                // Shutdown the app with an error code
                Shutdown(1);
            }
        }

        void TheaterConfigurationManager_ConfigurationUpdated(object sender, EventArgs e)
        {
            if (_enableHighQualityImageScaling != _appHost.TheaterConfigurationManager.Configuration.EnableHighQualityImageScaling)
            {
                ApplicationWindow.Dispatcher.InvokeAsync(LoadListBoxItemResourceFile);
            }
        }

        private ResourceDictionary _listBoxItemResource;
        private bool _enableHighQualityImageScaling;
        private void LoadListBoxItemResourceFile()
        {
            if (_listBoxItemResource != null)
            {
                _appHost.PresentationManager.RemoveResourceDictionary(_listBoxItemResource);
            }

            _enableHighQualityImageScaling = _appHost.TheaterConfigurationManager.Configuration.EnableHighQualityImageScaling;
            var filename = _enableHighQualityImageScaling ? "ListBoxItemsHighQuality" : "ListBoxItems";

            _listBoxItemResource = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Resources/" + filename + ".xaml", UriKind.Absolute)
            };

            _appHost.PresentationManager.AddResourceDictionary(_listBoxItemResource);
        }

        /// <summary>
        /// Loads the initial presentation.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task LoadInitialPresentation()
        {
            var foundServer = false;

            SystemInfo systemInfo = null;

            try
            {
                systemInfo = await _appHost.ApiClient.GetSystemInfoAsync().ConfigureAwait(false);

                foundServer = true;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error connecting to server using saved connection information. Host: {0}, Port {1}", ex, _appHost.ApiClient.ServerHostName, _appHost.ApiClient.ServerApiPort);
            }

            if (!foundServer)
            {
                try
                {
                    var address = await new ServerLocator().FindServer(500, CancellationToken.None).ConfigureAwait(false);

                    var parts = address.ToString().Split(':');

                    _appHost.ApiClient.ChangeServerLocation(parts[0], address.Port);

                    systemInfo = await _appHost.ApiClient.GetSystemInfoAsync().ConfigureAwait(false);

                    foundServer = true;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error attempting to locate server.", ex);
                }
            }

            var mediaFilters = _appHost.MediaFilters;

            if (!foundServer || !AreRequiredMediaFiltersInstalled(mediaFilters))
            {
                // Show connection wizard
                await Dispatcher.InvokeAsync(async () => await _appHost.NavigationService.Navigate(new StartupWizardPage(_appHost.NavigationService, _appHost.TheaterConfigurationManager, _appHost.ApiClient, _appHost.PresentationManager, _logger, mediaFilters)));
                return;
            }

            //Do final login
            await Dispatcher.InvokeAsync(async () => await Login());
        }

        private async Task Login()
        {
            //Check for auto-login credientials
            var config = _appHost.TheaterConfigurationManager.Configuration;
            try
            {
                if (config.AutoLoginConfiguration.UserName != null && config.AutoLoginConfiguration.UserPasswordHash != null)
                {
                    //Attempt password login
                    await _appHost.SessionManager.LoginWithHash(config.AutoLoginConfiguration.UserName, config.AutoLoginConfiguration.UserPasswordHash, true);
                    return;
                }
                else if (config.AutoLoginConfiguration.UserName != null)
                {
                    //Attempt passwordless login
                    await _appHost.SessionManager.Login(config.AutoLoginConfiguration.UserName, string.Empty, true);
                    return;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                //Login failed, redirect to login page and clear the auto-login
                _logger.ErrorException("Auto-login failed", ex, config.AutoLoginConfiguration.UserName);

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

            await _appHost.NavigationService.NavigateToLoginPage();
        }

        private bool AreRequiredMediaFiltersInstalled(IMediaFilters mediaFilters)
        {
            try
            {
                return mediaFilters.IsLavSplitterInstalled() && mediaFilters.IsLavAudioInstalled() && mediaFilters.IsLavVideoInstalled() && mediaFilters.IsXyVsFilterInstalled();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            LoadApplication();

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs" /> instance containing the event data.</param>
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            LogUnhandledException(exception);

            MessageBox.Show("Unhandled exception: " + exception.Message);
        }

        private void LogUnhandledException(Exception ex)
        {
            _logger.ErrorException("UnhandledException", ex);

            var path = Path.Combine(_appPaths.LogDirectoryPath, "crash_" + Guid.NewGuid() + ".txt");

            var builder = LogHelper.GetLogMessage(ex);

            File.WriteAllText(path, builder.ToString());
        }

        /// <summary>
        /// Handles the SessionEnding event of the SystemEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SessionEndingEventArgs" /> instance containing the event data.</param>
        void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            // Try to shut down gracefully
            Shutdown();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            var win = ApplicationWindow;

            if (win != null)
            {
                // Save window position
                var config = _appHost.TheaterConfigurationManager.Configuration;
                config.WindowState = win.WindowState;
                config.WindowTop = win.Top;
                config.WindowLeft = win.Left;
                config.WindowWidth = win.Width;
                config.WindowHeight = win.Height;
                _appHost.TheaterConfigurationManager.SaveConfiguration();
            }

            ReleaseMutex();

            base.OnExit(e);

            _appHost.Dispose();
        }

        /// <summary>
        /// Releases the mutex.
        /// </summary>
        private void ReleaseMutex()
        {
            if (_singleInstanceMutex == null)
            {
                return;
            }

            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Close();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        public void Restart()
        {
            Dispatcher.Invoke(ReleaseMutex);

            _appHost.Dispose();

            System.Windows.Forms.Application.Restart();

            Dispatcher.Invoke(Shutdown);
        }

    }
}
