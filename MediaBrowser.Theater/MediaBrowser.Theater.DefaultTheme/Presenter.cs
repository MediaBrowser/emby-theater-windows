using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DirectShow;

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
        private readonly INavigator _navigator;
        private readonly IInternalPlayerWindowManager _internalPlayerWindowManager;
        private readonly ILogger _logger;

        private MainWindow _mainWindow;
        private IntPtr _mainWindowHandle;
        private PopupWindow _currentPopup;

        public WindowManager(INavigator navigator, IInternalPlayerWindowManager internalPlayerWindowManager, ILogManager logManager)
        {
            _navigator = navigator;
            _internalPlayerWindowManager = internalPlayerWindowManager;
            _logger = logManager.GetLogger("WindowManager");
        }

        public MainWindow MainWindow
        {
            get { return _mainWindow; }
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

            _currentPopup.ShowModal(_mainWindow);
        }

        public MainWindow CreateMainWindow(PluginConfiguration config, IViewModel viewModel)
        {
            var window = new MainWindow {
                DataContext = viewModel,
            };

            FormStartPosition? startPosition = null;

            // Restore window position/size
            if (config.WindowState.HasValue) {
                // Set window state
                window.WindowState = config.WindowState.Value;

                // Set position if not maximized
                if (config.WindowState.Value != WindowState.Maximized) {
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
                    if (config.WindowWidth.HasValue) {
                        window.Width = Math.Min(config.WindowWidth.Value, SystemParameters.VirtualScreenWidth - left);
                    }

                    // Set height
                    if (config.WindowHeight.HasValue) {
                        window.Height = Math.Min(config.WindowHeight.Value, SystemParameters.VirtualScreenHeight - top);
                    }
                }
            }

            window.ShowInTaskbar = window.WindowState == WindowState.Minimized;

            _mainWindow = window;
            _mainWindowHandle = new WindowInteropHelper(_mainWindow).Handle;

            CreateInternalPlayerWindow(startPosition);

            return window;
        }

        private void CreateInternalPlayerWindow(FormStartPosition? startPosition)
        {
            int? formWidth = null;
            int? formHeight = null;
            int? formLeft = null;
            int? formTop = null;

            try
            {
                formWidth = Convert.ToInt32(_mainWindow.Width);
                formHeight = Convert.ToInt32(_mainWindow.Height);
            }
            catch (OverflowException)
            {
                formWidth = null;
                formHeight = null;
            }
            try
            {
                formTop = Convert.ToInt32(_mainWindow.Top);
                formLeft = Convert.ToInt32(_mainWindow.Left);
            }
            catch (OverflowException)
            {
                formLeft = null;
                formTop = null;
            }

            var state = GetWindowsFormState(_mainWindow.WindowState);

            var internalPlayerWindowThreade = new Thread(() => ShowHiddenWindow(formWidth, formHeight, formTop, formLeft, startPosition, state));
            internalPlayerWindowThreade.SetApartmentState(ApartmentState.STA);
            internalPlayerWindowThreade.IsBackground = true;
            internalPlayerWindowThreade.Start();
        }

        private FormWindowState GetWindowsFormState(WindowState state)
        {
            switch (state)
            {
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

            if (width.HasValue) {
                playerWindow.Width = width.Value;
            }
            if (height.HasValue) {
                playerWindow.Height = height.Value;
            }
            if (top.HasValue) {
                playerWindow.Top = top.Value;
            }
            if (left.HasValue) {
                playerWindow.Left = left.Value;
            }

            playerWindow.WindowState = windowState;

            if (startPosition.HasValue) {
                playerWindow.StartPosition = startPosition.Value;
            }

            _mainWindow.LocationChanged += (s, e) => MovePlayerWindow(playerWindow);
            _mainWindow.StateChanged += (s, e) => UpdatePlayerWindowState(playerWindow);
            _mainWindow.SizeChanged += (s, e) => UpdatePlayerWindowSize(playerWindow);
            _mainWindow.Closing += (s, e) => ClosePlayerWindow(playerWindow);

            _internalPlayerWindowManager.Window = playerWindow;

            playerWindow.Show();

            System.Windows.Threading.Dispatcher.Run();
        }

        private void MovePlayerWindow(InternalPlayerWindow playerWindow)
        {
            var top = _mainWindow.Top;
            var left = _mainWindow.Left;

            if (double.IsNaN(top) || double.IsNaN(left)) {
                return;
            }

            InvokeOnWindow(playerWindow, () => {
                playerWindow.Top = Convert.ToInt32(top);
                playerWindow.Left = Convert.ToInt32(left);
            });
        }

        private void UpdatePlayerWindowState(InternalPlayerWindow playerWindow)
        {
            var state = GetWindowsFormState(_mainWindow.WindowState);

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
            var width = _mainWindow.Width;
            var height = _mainWindow.Height;

            if (double.IsNaN(width) || double.IsNaN(height))
            {
                return;
            }

            InvokeOnWindow(playerWindow, () => {
                playerWindow.Width = Convert.ToInt32(width);
                playerWindow.Height = Convert.ToInt32(height);
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

        void HiddenWindow_Load(object sender, EventArgs e)
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

            var handle = window.Handle;

            _mainWindow.Dispatcher.InvokeAsync(() =>
            {
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
        private readonly IEventBus<ShowNotificationEvent> _showNotificationEvent;
        private readonly IEventBus<ShowPageEvent> _showPageEvent;

        private readonly WindowManager _windowManager;
        private IntPtr _mainWindowHandle;
        
        public Presenter(IEventAggregator events, WindowManager windowManager)
        {
            _showPageEvent = events.Get<ShowPageEvent>();
            _showNotificationEvent = events.Get<ShowNotificationEvent>();
            _windowManager = windowManager;
        }

        public Window MainApplicationWindow { get { return _windowManager.MainWindow; } }

        public IntPtr MainApplicationWindowHandle
        {
            get
            {
                if (_mainWindowHandle == IntPtr.Zero && MainApplicationWindow != null) {
                    _mainWindowHandle = new WindowInteropHelper(MainApplicationWindow).Handle;
                }

                return _mainWindowHandle;
            }
        }

        public async Task ShowPage(IViewModel contents)
        {
            await _windowManager.ClosePopup();
            await _showPageEvent.Publish(new ShowPageEvent { ViewModel = contents });
        }

        public Task ShowPopup(IViewModel contents)
        {
            return _windowManager.ShowPopup(contents);
        }

        public Task ShowNotification(IViewModel contents)
        {
            return _showNotificationEvent.Publish(new ShowNotificationEvent { ViewModel = contents });
        }
    }
}