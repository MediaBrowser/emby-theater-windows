using MediaBrowser.ApiInteraction;
using MediaBrowser.Common.Constants;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Implementations.Updates;
using MediaBrowser.Common.IO;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Implementations.Configuration;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.UI.Controller;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.Pages;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaBrowser.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets or sets the clock timer.
        /// </summary>
        /// <value>The clock timer.</value>
        private Timer ClockTimer { get; set; }

        /// <summary>
        /// The single instance mutex
        /// </summary>
        private static Mutex _singleInstanceMutex;

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the name of the uninstaller file.
        /// </summary>
        /// <value>The name of the uninstaller file.</value>
        protected string UninstallerFileName
        {
            get { return "MediaBrowser.UI.Uninstall.exe"; }
        }

        /// <summary>
        /// Gets or sets the composition root.
        /// </summary>
        /// <value>The composition root.</value>
        protected ApplicationHost CompositionRoot { get; set; }

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
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public IApiClient ApiClient
        {
            get { return CompositionRoot.ApiClient; }
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
        public HiddenWindow HiddenWindow { get; private set; }

        /// <summary>
        /// The _current user
        /// </summary>
        private UserDto _currentUser;
        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public UserDto CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                _currentUser = value;

                if (ApiClient != null)
                {
                    ApiClient.CurrentUserId = value == null ? null : value.Id;
                }

                OnPropertyChanged("CurrentUser");
            }
        }

        /// <summary>
        /// The _current time
        /// </summary>
        private DateTime _currentTime = DateTime.Now;
        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <value>The current time.</value>
        public DateTime CurrentTime
        {
            get
            {
                return _currentTime;
            }
            private set
            {
                _currentTime = value;
                OnPropertyChanged("CurrentTime");
            }
        }

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

            // Look for the existence of an update archive
            var appPaths = new ApplicationPaths();

            var updateArchive = Path.Combine(appPaths.TempUpdatePath, Constants.MbTheaterPkgName + ".zip");

            if (File.Exists(updateArchive))
            {
                // Update is there - execute update
                try
                {
                    new ApplicationUpdater().UpdateApplication(MBApplication.MBTheater, appPaths, updateArchive);

                    // And just let the app exit so it can update
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Error attempting to update application.\n\n{0}\n\n{1}", e.GetType().Name, e.Message));
                }
            }

            var application = new App();

            application.Run();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Instantiates the main window.
        /// </summary>
        /// <returns>Window.</returns>
        protected Window InstantiateMainWindow()
        {
            HiddenWindow = new HiddenWindow(CompositionRoot);

            return HiddenWindow;
        }

        /// <summary>
        /// Shows the application window.
        /// </summary>
        private void ShowApplicationWindow()
        {
            var win = new MainWindow(Logger);

            var config = CompositionRoot.UIConfigurationManager.Configuration;

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
                        win.Left = left = Math.Max(config.WindowLeft.Value, 0);
                    }

                    // Set top
                    if (config.WindowTop.HasValue)
                    {
                        win.WindowStartupLocation = WindowStartupLocation.Manual;
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

            win.LocationChanged += ApplicationWindow_LocationChanged;
            win.StateChanged += ApplicationWindow_LocationChanged;
            win.SizeChanged += ApplicationWindow_LocationChanged;

            ApplicationWindow = win;

            ApplicationWindow.Show();

            ApplicationWindow.Owner = HiddenWindow;

            SyncHiddenWindowLocation();
        }

        /// <summary>
        /// Handles the LocationChanged event of the ApplicationWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void ApplicationWindow_LocationChanged(object sender, EventArgs e)
        {
            SyncHiddenWindowLocation();
        }

        /// <summary>
        /// Syncs the hidden window location.
        /// </summary>
        public void SyncHiddenWindowLocation()
        {
            HiddenWindow.Width = ApplicationWindow.Width;
            HiddenWindow.Height = ApplicationWindow.Height;
            HiddenWindow.Top = ApplicationWindow.Top;
            HiddenWindow.Left = ApplicationWindow.Left;
            HiddenWindow.WindowState = ApplicationWindow.WindowState;

            ApplicationWindow.Activate();
        }

        /// <summary>
        /// Loads the kernel.
        /// </summary>
        protected async void LoadKernel()
        {
            try
            {
                CompositionRoot = new ApplicationHost();

                Logger = CompositionRoot.LogManager.GetLogger("App");

                await CompositionRoot.Init();

                OnKernelLoaded();

                InstantiateMainWindow().Show();

                ShowApplicationWindow();

                await LoadInitialPresentation().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error launching application", ex);

                MessageBox.Show("There was an error launching Media Browser: " + ex.Message);

                // Shutdown the app with an error code
                Shutdown(1);
            }
        }

        /// <summary>
        /// Loads the initial presentation.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task LoadInitialPresentation()
        {
            var foundServer = false;

            SystemInfo systemInfo;

            try
            {
                systemInfo = await ApiClient.GetSystemInfoAsync().ConfigureAwait(false);

                foundServer = true;
            }
            catch (HttpException ex)
            {
                Logger.ErrorException("Error connecting to server using saved connection information. Host: {0}, Port {1}", ex, ApiClient.ServerHostName, ApiClient.ServerApiPort);
            }

            if (!foundServer)
            {
                try
                {
                    var address = await new ServerLocator().FindServer().ConfigureAwait(false);

                    var parts = address.ToString().Split(':');

                    ApiClient.ServerHostName = parts[0];
                    ApiClient.ServerApiPort = address.Port;

                    systemInfo = await ApiClient.GetSystemInfoAsync().ConfigureAwait(false);

                    foundServer = true;
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error attempting to locate server.", ex);
                }
            }

            if (!foundServer)
            {
                // Show connection wizard
            }
            else
            {
                // Open web socket

                await LogoutUser();
            }
        }

        /// <summary>
        /// Called when [kernel loaded].
        /// </summary>
        /// <returns>Task.</returns>
        protected void OnKernelLoaded()
        {
            // Update every 10 seconds
            ClockTimer = new Timer(ClockTimerCallback, null, 0, 10000);

            CompositionRoot.ThemeManager.SetCurrentTheme(CompositionRoot.ThemeManager.Themes.First());

            foreach (var resource in CompositionRoot.ThemeManager.CurrentTheme.GetGlobalResources())
            {
                Resources.MergedDictionaries.Add(resource);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            LoadKernel();

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

            Logger.ErrorException("UnhandledException", exception);

            MessageBox.Show("Unhandled exception: " + exception.Message);
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="info">The info.</param>
        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                try
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error in event handler", ex);
                }
            }
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
                var config = CompositionRoot.UIConfigurationManager.Configuration;
                config.WindowState = win.WindowState;
                config.WindowTop = win.Top;
                config.WindowLeft = win.Left;
                config.WindowWidth = win.Width;
                config.WindowHeight = win.Height;
                CompositionRoot.UIConfigurationManager.SaveConfiguration();
            }

            ReleaseMutex();

            base.OnExit(e);

            CompositionRoot.Dispose();
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
        /// Clocks the timer callback.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        private void ClockTimerCallback(object stateInfo)
        {
            CurrentTime = DateTime.Now;
        }

        /// <summary>
        /// Logouts the user.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task LogoutUser()
        {
            CurrentUser = null;

            await Dispatcher.InvokeAsync(() => Navigate(CompositionRoot.ThemeManager.CurrentTheme.GetLoginPage()));
        }

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        public void Navigate(Page page)
        {
            _remoteImageCache = new FileSystemRepository(Path.Combine(CompositionRoot.UIConfigurationManager.ApplicationPaths.CachePath, "remote-images"));

            ApplicationWindow.Navigate(page);
        }

        /// <summary>
        /// Navigates to settings page.
        /// </summary>
        public void NavigateToSettingsPage()
        {
            Navigate(new SettingsPage());
        }

        /// <summary>
        /// Navigates to internal player page.
        /// </summary>
        public void NavigateToInternalPlayerPage()
        {
            Navigate(CompositionRoot.ThemeManager.CurrentTheme.GetInternalPlayerPage());
        }

        /// <summary>
        /// Navigates to image viewer.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <param name="caption">The caption.</param>
        public void OpenImageViewer(Uri imageUrl, string caption)
        {
            var tuple = new Tuple<Uri, string>(imageUrl, caption);

            OpenImageViewer(new[] { tuple });
        }

        /// <summary>
        /// Navigates to image viewer.
        /// </summary>
        /// <param name="images">The images.</param>
        public void OpenImageViewer(IEnumerable<Tuple<Uri, string>> images)
        {
            new ImageViewerWindow(images).ShowModal(ApplicationWindow);
        }

        /// <summary>
        /// Navigates to item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void NavigateToItem(BaseItemDto item)
        {
            if (item.IsRoot)
            {
                NavigateToHomePage();
            }
            else
            {
                Navigate(CompositionRoot.ThemeManager.CurrentTheme.GetItemPage(item.Id, item.Type, item.Name, item.IsFolder));
            }
        }

        /// <summary>
        /// Navigates to home page.
        /// </summary>
        public void NavigateToHomePage()
        {
            Navigate(CompositionRoot.ThemeManager.CurrentTheme.GetHomePage());
        }

        /// <summary>
        /// Shows a notification message that will disappear on it's own
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="icon">The icon.</param>
        public void ShowNotificationMessage(string text, string caption = null, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            ApplicationWindow.ShowModalMessage(text, caption: caption, icon: icon);
        }

        /// <summary>
        /// Shows a notification message that will disappear on it's own
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="icon">The icon.</param>
        public void ShowNotificationMessage(UIElement text, string caption = null, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            ApplicationWindow.ShowModalMessage(text, caption: caption, icon: icon);
        }

        /// <summary>
        /// Shows a modal message box and asynchronously returns a MessageBoxResult
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>MessageBoxResult.</returns>
        public MessageBoxResult ShowModalMessage(string text, string caption = null, MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return ApplicationWindow.ShowModalMessage(text, caption: caption, button: button, icon: icon);
        }

        /// <summary>
        /// Shows a modal message box and asynchronously returns a MessageBoxResult
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>MessageBoxResult.</returns>
        public MessageBoxResult ShowModalMessage(UIElement text, string caption = null, MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return ApplicationWindow.ShowModalMessage(text, caption: caption, button: button, icon: icon);
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        public void ShowErrorMessage(string message, string caption = null)
        {
            caption = caption ?? "Error";
            ShowModalMessage(message, caption: caption, button: MessageBoxButton.OK, icon: MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows the default error message.
        /// </summary>
        public void ShowDefaultErrorMessage()
        {
            ShowErrorMessage("There was an error processing the request", "Error");
        }

        /// <summary>
        /// The _remote image cache
        /// </summary>
        private FileSystemRepository _remoteImageCache;

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{Image}.</returns>
        public async Task<Image> GetRemoteImageAsync(string url)
        {
            var bitmap = await GetRemoteBitmapAsync(url);

            return new Image { Source = bitmap };
        }

        /// <summary>
        /// The _locks
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _imageFileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Gets the lock.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.Object.</returns>
        private SemaphoreSlim GetImageFileLock(string filename)
        {
            return _imageFileLocks.GetOrAdd(filename, key => new SemaphoreSlim(1, 1));
        }

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        /// <exception cref="System.ArgumentNullException">url</exception>
        public async Task<BitmapImage> GetRemoteBitmapAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            var cachePath = _remoteImageCache.GetResourcePath(url.GetMD5().ToString());

            if (File.Exists(cachePath))
            {
                return GetCachedBitmapImage(cachePath);
            }

            return await Task.Run(async () =>
            {
                var semaphore = GetImageFileLock(cachePath);
                await semaphore.WaitAsync().ConfigureAwait(false);

                // Look in the cache again
                if (File.Exists(cachePath))
                {
                    semaphore.Release();

                    return GetCachedBitmapImage(cachePath);
                }

                try
                {
                    using (var httpStream = await ApiClient.GetImageStreamAsync(url))
                    {
                        return await GetBitmapImageAsync(httpStream, cachePath);
                    }
                }
                finally
                {
                    semaphore.Release();
                }

            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the image async.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>Task{BitmapImage}.</returns>
        private async Task<BitmapImage> GetBitmapImageAsync(Stream sourceStream, string cachePath)
        {
            using (var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.Read, StreamDefaults.DefaultFileStreamBufferSize, true))
            {
                await sourceStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return GetCachedBitmapImage(cachePath);
        }

        /// <summary>
        /// Gets the cached bitmap image.
        /// </summary>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>BitmapImage.</returns>
        private BitmapImage GetCachedBitmapImage(string cachePath)
        {
            var bitmapImage = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                CacheOption = BitmapCacheOption.Default,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            RenderOptions.SetBitmapScalingMode(bitmapImage, BitmapScalingMode.Fant);

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(cachePath);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        public void Restart()
        {
            Dispatcher.Invoke(ReleaseMutex);

            CompositionRoot.Dispose();

            System.Windows.Forms.Application.Restart();

            Dispatcher.Invoke(Shutdown);
        }

        /// <summary>
        /// Gets the bitmap image.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>BitmapImage.</returns>
        /// <exception cref="System.ArgumentNullException">uri</exception>
        public BitmapImage GetBitmapImage(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("uri");
            }

            return GetBitmapImage(new Uri(uri));
        }

        /// <summary>
        /// Gets the bitmap image.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>BitmapImage.</returns>
        /// <exception cref="System.ArgumentNullException">uri</exception>
        public BitmapImage GetBitmapImage(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var bitmap = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                CacheOption = BitmapCacheOption.OnDemand,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();

            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.Fant);
            return bitmap;
        }
    }
}
