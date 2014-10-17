using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Implementations.Configuration;
using MediaBrowser.UI.StartupWizard;
using Microsoft.Win32;
using System;
using System.Diagnostics;
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

            var updateArchive = Path.Combine(appPaths.TempUpdatePath, "MBTheater" + ".zip");

            if (File.Exists(updateArchive))
            {
                // Update is there - execute update
                try
                {
                    new ApplicationUpdater().UpdateApplication(appPaths, updateArchive,
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
            var win = new MainWindow(_logger, _appHost.PlaybackManager, _appHost.ImageManager,
                _appHost, _appHost.PresentationManager, _appHost.UserInputManager, _appHost.TheaterConfigurationManager,
                _appHost.NavigationService, _appHost.ScreensaverManager, _appHost.ConnectionManager);

            var config = _appHost.TheaterConfigurationManager.Configuration;

            // Restore window position/size
            if (config.WindowState.HasValue)
            {
                double left = 0;
                double top = 0;

                // Set left
                if (config.WindowLeft.HasValue)
                {
                    win.WindowStartupLocation = WindowStartupLocation.Manual;
                    win.Left = left = Math.Max(config.WindowLeft.Value, SystemParameters.VirtualScreenLeft);
                }

                // Set top
                if (config.WindowTop.HasValue)
                {
                    win.WindowStartupLocation = WindowStartupLocation.Manual;
                    win.Top = top = Math.Max(config.WindowTop.Value, SystemParameters.VirtualScreenLeft);
                }

                // Set width
                if (config.WindowWidth.HasValue && config.WindowWidth.Value > 0)
                {
                    win.Width = Math.Min(config.WindowWidth.Value, SystemParameters.VirtualScreenWidth - left + SystemParameters.VirtualScreenLeft);
                }

                // Set height
                if (config.WindowHeight.HasValue && config.WindowHeight.Value > 0)
                {
                    win.Height = Math.Min(config.WindowHeight.Value, SystemParameters.VirtualScreenHeight - top + SystemParameters.VirtualScreenTop);
                }

                // Set window state
                win.WindowState = config.WindowState.Value;
            }
            else
            {
                //Set these so we don't generate exceptions later on. This also fixes the issue where the first run hidden window size problem.
                if (double.IsNaN(win.Width))
                    win.Width = System.Windows.SystemParameters.FullPrimaryScreenWidth * .75;
                if (double.IsNaN(win.Height))
                    win.Height = System.Windows.SystemParameters.FullPrimaryScreenHeight * .75;

                if (double.IsNaN(win.Top))
                    win.Top = 0;
                if (double.IsNaN(win.Left))
                    win.Left = 0;

                win.WindowState = WindowState.Normal;
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

            _hiddenWindowThread = new Thread(() => ShowHiddenWindow(formWidth, formHeight, formTop, formLeft, state));
            _hiddenWindowThread.SetApartmentState(ApartmentState.MTA);
            _hiddenWindowThread.IsBackground = true;
            _hiddenWindowThread.Priority = ThreadPriority.AboveNormal;
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

        private void ShowHiddenWindow(int? width, int? height, int? top, int? left, System.Windows.Forms.FormWindowState windowState)
        {
            HiddenWindow = new HiddenForm();
            HiddenWindow.Load += HiddenWindow_Load;
            HiddenWindow.Activated += HiddenWindow_Activated;

            if (left.HasValue || top.HasValue)
            {
                HiddenWindow.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            }

            if (top.HasValue)
            {
                HiddenWindow.Top = top.Value;
            }
            if (left.HasValue)
            {
                HiddenWindow.Left = left.Value;
            }
            if (width.HasValue)
            {
                HiddenWindow.Width = width.Value;
            }
            if (height.HasValue)
            {
                HiddenWindow.Height = height.Value;
            }

            HiddenWindow.WindowState = windowState;

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

        private void HiddenWindow_Activated(object sender, EventArgs e)
        {
            _logger.Debug("HiddenWindow_Activated");
            _appHost.PresentationManager.EnsureApplicationWindowHasFocus();
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
            _appHost.PresentationManager.ShowModalLoadingAnimation();
            var cancellationToken = CancellationToken.None;

            var connectionResult = await _appHost.ConnectionManager.Connect(cancellationToken);

            _appHost.PresentationManager.HideModalLoadingAnimation();

            NavigateFromConnectionResult(connectionResult);
        }

        public async void NavigateFromConnectionResult(ConnectionResult result)
        {
            // Startup wizard
            if (result.State == ConnectionState.Unavailable)
            {
                await Dispatcher.InvokeAsync(async () => await _appHost.NavigationService.Navigate(new StartupWizardPage(_appHost.NavigationService, _appHost.ConnectionManager, _appHost.PresentationManager, _logger)));
            }

            else if (result.State == ConnectionState.ServerSelection)
            {
                await Dispatcher.InvokeAsync(async () => await _appHost.NavigationService.Navigate(new ServerSelectionPage(_appHost.ConnectionManager, _appHost.PresentationManager, result.Servers, _appHost.NavigationService, _logger)));
            }

            else if (result.State == ConnectionState.ServerSignIn)
            {
                //await Dispatcher.InvokeAsync(async () => await _appHost.NavigationService.Navigate(new ServerSelectionPage(_appHost.ConnectionManager, _appHost.PresentationManager, result.Servers, _appHost.NavigationService, _logger)));
                await _appHost.NavigationService.NavigateToLoginPage();
            }

            else if (result.State == ConnectionState.SignedIn)
            {
                await _appHost.SessionManager.ValidateSavedLogin(result);
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
