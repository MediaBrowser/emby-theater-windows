using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DirectShow;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace MediaBrowser.Theater.DefaultTheme
{
    public struct ShowPageEvent
    {
        public IViewModel ViewModel { get; set; }
    }

    public struct ShowNotificationEvent
    {
        public IViewModel ViewModel { get; set; }
    }

    public class WindowManager
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly IInternalPlayerWindowManager _internalPlayerWindowManager;
        private readonly ILogger _logger;
        private readonly INavigator _navigator;
        private PopupWindow _currentPopup;

        private MainWindow _mainWindow;
        private IntPtr _mainWindowHandle;

        public WindowManager(INavigator navigator, IInternalPlayerWindowManager internalPlayerWindowManager, ILogManager logManager, ITheaterApplicationHost appHost)
        {
            _navigator = navigator;
            _internalPlayerWindowManager = internalPlayerWindowManager;
            _appHost = appHost;
            _logger = logManager.GetLogger("WindowManager");
        }

        public MainWindow MainWindow
        {
            get { return _mainWindow; }
        }

        public FrameworkElement ActiveWindow
        {
            get { return _currentPopup as FrameworkElement ?? _mainWindow; }
        }

        public event Action<Window> MainWindowLoaded;

        protected virtual void OnMainWindowLoaded(Window obj)
        {
            Action<Window> handler = MainWindowLoaded;
            if (handler != null) {
                handler(obj);
            }
        }

        private void FocusMainWindow()
        {
            var rootVm = _mainWindow.DataContext as RootViewModel;
            if (rootVm != null) {
                rootVm.IsInFocus = true;
            }
        }

        private void UnfocusMainWindow()
        {
            var rootVm = _mainWindow.DataContext as RootViewModel;
            if (rootVm != null) {
                rootVm.IsInFocus = false;
            }
        }

        public async Task ClosePopup()
        {
            if (_currentPopup != null) {
                var context = _currentPopup.DataContext as IViewModel;
                if (context != null) {
                    await context.Close();
                }

                _currentPopup.Close();
                _currentPopup = null;
            }
        }

        public async Task ShowPopup(IViewModel contents)
        {
            await ClosePopup();

            _currentPopup = new PopupWindow(_navigator) {
                DataContext = contents
            };

            EventHandler windowClosed = null;
            windowClosed = (sender, args) => {
                FocusMainWindow();
                _currentPopup.Closed -= windowClosed;
            };

            _currentPopup.Closed += windowClosed;

            UnfocusMainWindow();

            _currentPopup.ShowModal(_mainWindow, _appHost.UserInputManager);
        }

        public MainWindow CreateMainWindow(PluginConfiguration config, IViewModel viewModel)
        {
            var window = new MainWindow {
                DataContext = viewModel,
            };

            FormStartPosition? startPosition = null;

            // Restore window position/size
            if (config.WindowState.HasValue) {
                double left = 0;
                double top = 0;

                // Set left
                if (config.WindowLeft.HasValue) {
                    startPosition = FormStartPosition.Manual;
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = left = Math.Max(config.WindowLeft.Value, 0);
                }

                // Set top
                if (config.WindowTop.HasValue) {
                    startPosition = FormStartPosition.Manual;
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Top = top = Math.Max(config.WindowTop.Value, 0);
                }

                // Set width
                if (config.WindowWidth.HasValue && config.WindowWidth.Value > 0) {
                    window.Width = Math.Min(config.WindowWidth.Value, SystemParameters.VirtualScreenWidth - left);
                }

                // Set height
                if (config.WindowHeight.HasValue && config.WindowHeight.Value > 0) {
                    window.Height = Math.Min(config.WindowHeight.Value, SystemParameters.VirtualScreenHeight - top);
                }

                // set window state
                window.WindowState = config.WindowState.Value;
            } else {
                // set first startup size and state
                if (double.IsNaN(window.Width)) {
                    window.Width = SystemParameters.VirtualScreenWidth*0.75;
                }

                if (double.IsNaN(window.Height)) {
                    window.Height = SystemParameters.VirtualScreenHeight*0.75;
                }

                if (double.IsNaN(window.Top)) {
                    window.Top = 0;
                }

                if (double.IsNaN(window.Left)) {
                    window.Left = 0;
                }

                window.WindowState = WindowState.Normal;
            }

            window.ShowInTaskbar = window.WindowState == WindowState.Minimized;

            _mainWindow = window;
            _mainWindowHandle = new WindowInteropHelper(_mainWindow).Handle;

            CreateInternalPlayerWindow(startPosition);

            OnMainWindowLoaded(window);

            return window;
        }

        private void CreateInternalPlayerWindow(FormStartPosition? startPosition)
        {
            int? formWidth = null;
            int? formHeight = null;
            int? formLeft = null;
            int? formTop = null;

            try {
                formWidth = Convert.ToInt32(_mainWindow.Width);
                formHeight = Convert.ToInt32(_mainWindow.Height);
            }
            catch (OverflowException) {
                formWidth = null;
                formHeight = null;
            }
            try {
                formTop = Convert.ToInt32(_mainWindow.Top);
                formLeft = Convert.ToInt32(_mainWindow.Left);
            }
            catch (OverflowException) {
                formLeft = null;
                formTop = null;
            }

            FormWindowState state = GetWindowsFormState(_mainWindow.WindowState);
            
            var internalPlayerWindowThread = new Thread(() => ShowHiddenWindow(formWidth, formHeight, formTop, formLeft, startPosition, state));
            internalPlayerWindowThread.Name = "Internal Player Window";
            internalPlayerWindowThread.SetApartmentState(ApartmentState.MTA);
            internalPlayerWindowThread.IsBackground = true;
            internalPlayerWindowThread.Priority = ThreadPriority.AboveNormal;
            internalPlayerWindowThread.Start();
        }

        private FormWindowState GetWindowsFormState(WindowState state)
        {
            switch (state) {
                case WindowState.Maximized:
                    return FormWindowState.Maximized;
                case WindowState.Minimized:
                    return FormWindowState.Minimized;
            }

            return FormWindowState.Normal;
        }

        private void ShowHiddenWindow(int? width, int? height, int? top, int? left, FormStartPosition? startPosition, FormWindowState windowState)
        {
            var playerWindow = new InternalPlayerWindow();
            playerWindow.Load += HiddenWindow_Load;
            playerWindow.Activated += HiddenWindow_Activated;

            if (startPosition.HasValue) {
                playerWindow.StartPosition = startPosition.Value;
            }

            var dpiScale = GetSystemDpiFactor();

            if (width.HasValue) {
                playerWindow.Width = (int)(width.Value * dpiScale);
            }
            if (height.HasValue) {
                playerWindow.Height = (int)(height.Value * dpiScale);
            }
            if (top.HasValue) {
                playerWindow.Top = (int)(top.Value * dpiScale);
            }
            if (left.HasValue) {
                playerWindow.Left = (int)(left.Value * dpiScale);
            }

            playerWindow.WindowState = windowState;

            _mainWindow.Loaded += (s, e) => {
                MovePlayerWindow(playerWindow);
                UpdatePlayerWindowSize(playerWindow);
                UpdatePlayerWindowState(playerWindow);
            };

            _mainWindow.LocationChanged += (s, e) => MovePlayerWindow(playerWindow);
            _mainWindow.StateChanged += (s, e) => UpdatePlayerWindowState(playerWindow);
            _mainWindow.SizeChanged += (s, e) => UpdatePlayerWindowSize(playerWindow);
            _mainWindow.Closing += (s, e) => ClosePlayerWindow(playerWindow);

            _internalPlayerWindowManager.Window = playerWindow;

            playerWindow.Show();

            Dispatcher.Run();
        }

        private static double GetSystemDpiFactor()
        {
            var dpi = (int) Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "AppliedDPI", 96);
            return dpi/96.0;
        }


        private void MovePlayerWindow(InternalPlayerWindow playerWindow)
        {
            double top = _mainWindow.Top;
            double left = _mainWindow.Left;

            if (double.IsNaN(top) || double.IsNaN(left)) {
                return;
            }

            var dpiScale = GetSystemDpiFactor();

            InvokeOnWindow(playerWindow, () => {
                playerWindow.Top = Convert.ToInt32(top * dpiScale);
                playerWindow.Left = Convert.ToInt32(left * dpiScale);
            });
        }

        private void UpdatePlayerWindowState(InternalPlayerWindow playerWindow)
        {
            FormWindowState state = GetWindowsFormState(_mainWindow.WindowState);

            _mainWindow.ShowInTaskbar = state == FormWindowState.Minimized;

            InvokeOnWindow(playerWindow, () => {
                if (state == FormWindowState.Minimized) {
                    playerWindow.Hide();
                } else {
                    playerWindow.Show();
                    playerWindow.WindowState = state;
                }
            });
        }

        private void UpdatePlayerWindowSize(InternalPlayerWindow playerWindow)
        {
            double width = _mainWindow.Width;
            double height = _mainWindow.Height;

            if (double.IsNaN(width) || double.IsNaN(height)) {
                return;
            }

            var dpiScale = GetSystemDpiFactor();

            InvokeOnWindow(playerWindow, () => {
                playerWindow.Width = Convert.ToInt32(width * dpiScale);
                playerWindow.Height = Convert.ToInt32(height * dpiScale);
            });
        }

        private void ClosePlayerWindow(InternalPlayerWindow playerWindow)
        {
            InvokeOnWindow(playerWindow, playerWindow.Close);
        }

        private void InvokeOnWindow(Form form, Action action)
        {
            if (form.InvokeRequired) {
                form.Invoke(action);
            } else {
                action();
            }
        }

        private void HiddenWindow_Load(object sender, EventArgs e)
        {
            // Hide this from ALT-TAB
            //var handle = HiddenWindow.Handle;
            //var exStyle = (int)GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            //exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            //SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            var window = sender as Form;
            if (window == null) {
                return;
            }

            IntPtr handle = window.Handle;

            _mainWindow.Dispatcher.InvokeAsync(() => {
                new WindowInteropHelper(_mainWindow).Owner = handle;
                _mainWindow.Show();
            });
        }

        private void HiddenWindow_Activated(object sender, EventArgs e)
        {
            _logger.Debug("HiddenWindow_Activated");
            EnsureApplicationWindowHasFocus();
        }

        public void SaveWindowPosition(PluginConfiguration config)
        {
            if (_mainWindow != null) {
                // Save window position
                config.WindowState = _mainWindow.WindowState;
                config.WindowTop = _mainWindow.Top;
                config.WindowLeft = _mainWindow.Left;
                config.WindowWidth = _mainWindow.Width;
                config.WindowHeight = _mainWindow.Height;
            }
        }

        public void EnsureApplicationWindowHasFocus()
        {
            IntPtr focused = Interop.GetForegroundWindow();
            if (_mainWindowHandle != focused) {
                Interop.SetForegroundWindow(_mainWindowHandle);
            }
        }

        public class Interop
        {
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
        }
    }

    public class Presenter
        : IPresenter
    {
        private readonly IEventBus<PageLoadedEvent> _pageLoadedEvent;
        private readonly IEventBus<ShowNotificationEvent> _showNotificationEvent;
        private readonly IEventBus<ShowPageEvent> _showPageEvent;

        private readonly WindowManager _windowManager;
        private IntPtr _mainWindowHandle;

        private IViewModel _currentPage;

        public Presenter(IEventAggregator events, WindowManager windowManager)
        {
            _showPageEvent = events.Get<ShowPageEvent>();
            _showNotificationEvent = events.Get<ShowNotificationEvent>();
            _pageLoadedEvent = events.Get<PageLoadedEvent>();
            _windowManager = windowManager;
        }

        public event Action<Window> MainWindowLoaded
        {
            add { _windowManager.MainWindowLoaded += value; }
            remove { _windowManager.MainWindowLoaded -= value; }
        }

        public Window MainApplicationWindow
        {
            get { return _windowManager.MainWindow; }
        }

        public Window ActiveWindow
        {
            get { return _windowManager.ActiveWindow as Window; }
        }

        public IntPtr MainApplicationWindowHandle
        {
            get
            {
                if (_mainWindowHandle == IntPtr.Zero && MainApplicationWindow != null) {
                    MainApplicationWindow.Dispatcher.Invoke(() => _mainWindowHandle = new WindowInteropHelper(MainApplicationWindow).Handle);
                }

                return _mainWindowHandle;
            }
        }

        public void EnsureApplicationWindowHasFocus()
        {
            _windowManager.EnsureApplicationWindowHasFocus();
        }

        public FrameworkElement GetFocusedElement()
        {
            return Keyboard.FocusedElement as FrameworkElement ?? FocusManager.GetFocusedElement(_windowManager.ActiveWindow) as FrameworkElement;
        }

        public async Task ShowPage(IViewModel contents)
        {
            await _windowManager.ClosePopup();

            _currentPage = contents;
            await _showPageEvent.Publish(new ShowPageEvent { ViewModel = contents });
            await _pageLoadedEvent.Publish(new PageLoadedEvent { ViewModel = contents });
        }

        public async Task ShowPopup(IViewModel contents)
        {
            await _windowManager.ShowPopup(contents);
            await _pageLoadedEvent.Publish(new PageLoadedEvent { ViewModel = contents });
        }

        public Task ShowNotification(IViewModel contents)
        {
            return _showNotificationEvent.Publish(new ShowNotificationEvent { ViewModel = contents });
        }

        public MessageBoxResult ShowMessage(MessageBoxInfo messageBoxInfo)
        {
            //todo message box
            return MessageBoxResult.Cancel;
        }

        public IViewModel CurrentPage {
            get { return _currentPage; }
        }
    }
}