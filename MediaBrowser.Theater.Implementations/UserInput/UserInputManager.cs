using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Gma.UserActivityMonitor;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using System.Runtime.InteropServices;
using WindowsForms = System.Windows.Forms;
using WindowsInput = System.Windows.Input;

namespace MediaBrowser.Theater.Implementations.UserInput
{
    /// <summary>
    /// Class UserInputManager
    /// </summary>
    public class UserInputManager : IUserInputManager
    {
        /// <summary>
        /// Occurs when [key down]., gloabl across all apps
        /// </summary>
        public event WindowsForms.KeyEventHandler GlobalKeyDown
        {
            add
            {
                HookManager.KeyDown += value;
            }
            remove
            {
                HookManager.KeyDown -= value;
            }
        }

        public event WindowsForms.KeyPressEventHandler GlobalKeyPress
        {
            add
            {
                HookManager.KeyPress += value;
            }
            remove
            {
                HookManager.KeyPress -= value;
            }
        }

        /// <summary>
        /// Occurs when [mouse move].
        /// </summary>
        public event WindowsForms.MouseEventHandler GlobalMouseMove
        {
            add
            {
                HookManager.MouseMove += value;
            }
            remove
            {
                HookManager.MouseMove -= value;
            }
        }

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public DateTime GetLastInputTime()
        {
            var lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            GetLastInputInfo(ref lastInputInfo);

            return DateTime.Now.AddMilliseconds(-(Environment.TickCount - lastInputInfo.dwTime));
        }

        /// <summary>
        /// Occurs when [key down]. Local to MBT
        /// </summary>
        private event WindowsInput.KeyEventHandler _keyDown;
        public event WindowsInput.KeyEventHandler KeyDown
        {
            add
            {
                _keyDown += value;
            }
            remove
            {
                _keyDown -= value;
            }
        }

        /// <summary>
        /// Occurs when [key down]. Local to MBT
        /// </summary>
        private event WindowsInput.MouseEventHandler _mouseMove;
        public event WindowsInput.MouseEventHandler MouseMove
        {
            add
            {
                _mouseMove += value;
            }
            remove
            {
                _mouseMove -= value;
            }
        }

        /// <summary>
        /// Occurs when an AppCommand is sent to MBT Main window. Local to MBT
        /// </summary>
        private event AppCommandEventHandler _appCommand;
        public event AppCommandEventHandler AppCommand
        {
            add
            {
                _appCommand += value;
            }
            remove
            {
                _appCommand -= value;
            }
        }


        private readonly IPresentationManager _presenationManager;
        private readonly ILogger _logger;
        private readonly INavigationService _navigationService;
        private readonly IHiddenWindow _hiddenWindow;

        public UserInputManager(IPresentationManager presenationManager, INavigationService navigationService, IHiddenWindow hiddenWindow, ILogManager logManager)
        {
            _presenationManager = presenationManager;
            _navigationService = navigationService;
            _hiddenWindow = hiddenWindow;

            _presenationManager.WindowLoaded += MainWindowLoaded;
            _logger = logManager.GetLogger(GetType().Name);
        }

        internal void MainWindowLoaded(object sender, EventArgs eventArgs)
        {
            var window = _presenationManager.Window;

            window.Dispatcher.InvokeAsync(() =>
            {
                var source = (HwndSource)PresentationSource.FromVisual(window);
                if (source != null)
                    source.AddHook(WndProc);
            });

            _navigationService.Navigated += _nav_Navigated;
            // subscribe and manager key down events from the application window
            // note, the hiddenform will also route keydown and mousemove event though here
            _presenationManager.Window.KeyDown += Window_KeyDown;
            _presenationManager.Window.MouseMove += Window_MouseMove;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_APPCOMMAND = 0x319;

            if (msg == WM_APPCOMMAND)
            {
                if (_appCommand != null)
                {
                    var appCommandEventArgs = new AppCommandEventArgs { Cmd = (lParam.ToInt32() / 65536) };
                 
                    _appCommand.Invoke(null, appCommandEventArgs);
                    handled = appCommandEventArgs.Handled;
                }
            }

            return IntPtr.Zero;
        }

        void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NewPage is IFullscreenVideoPage)
            {
                AddirectPlayWindowHook();
            }
            else
            {
                RemoveDirectPlayWindowHook();
            }
        }

        private void AddirectPlayWindowHook()
        {
            // can't subscribe directly to Windw_Keydow - have to map a form event to a wpf window event
           _hiddenWindow.KeyDown -= directPlayWindow_KeyDown; // 
           _hiddenWindow.KeyDown += directPlayWindow_KeyDown;

           _hiddenWindow.MouseMove -= directPlayWindow_MouseMove;
           _hiddenWindow.MouseMove += directPlayWindow_MouseMove;
        }
        
        private void RemoveDirectPlayWindowHook()
        {
            _hiddenWindow.KeyDown -= directPlayWindow_KeyDown;
            _hiddenWindow.MouseMove -= directPlayWindow_MouseMove;
        }

        /// <summary>
        /// Responds to key presses inside a wpf window
        /// </summary>
        void Window_KeyDown(object sender, WindowsInput.KeyEventArgs e)
        {
            _logger.Debug("Window_KeyDown {0}", e.Key);
            if (_keyDown != null)
            {
                _keyDown.Invoke(null, e);
            }
        }

        /// <summary>
        /// Responds to key presses inside a wpf window
        /// </summary>
        void Window_MouseMove(object sender, WindowsInput.MouseEventArgs e)
        {
            //_logger.Debug("Window_MouseMove");
            if (_mouseMove != null)
            {
                _mouseMove.Invoke(null, e);
            }
        }

        /// <summary>
        /// Route a KeyDown event via the input Manager
        /// </summary>
        public void OnKeyDown(WindowsInput.KeyEventArgs e)
        {
             _logger.Debug("OnKeyDown {0}", e.Key);
             Window_KeyDown(null, e);
        }

        /// <summary>
        /// Route a KeyDown event via the input Manager
        /// </summary>
        public void OnMouseMove(WindowsInput.MouseEventArgs e)
        {
            //_logger.Debug("OnMouseMove {0}");
            Window_MouseMove(null, e);
        }
  
        /// <summary>
        /// Responds to key down in hiddenwindow 
        /// </summary>
        //
        void directPlayWindow_KeyDown(object sender, WindowsForms.KeyEventArgs e)
        {
            if (_keyDown != null)
            {
                _logger.Debug("directPlayWindow_KeyDown {0}", e.KeyCode);
                // map from System.Windows.Forms to System.Windows.Input key event
                var window = _presenationManager.Window;
                
                window.Dispatcher.InvokeAsync(() =>
                {
                    var source = (HwndSource)PresentationSource.FromVisual(window);
                    if (source != null)
                    {
                        var key = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
                        var keyEventArg = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key);

                        _keyDown.Invoke(null, keyEventArg);
                        e.Handled = keyEventArg.Handled;
                    }
                });
            }
        }

        /// <summary>
        /// Responds to key down in hiddenwindow 
        /// </summary>
        //
        void directPlayWindow_MouseMove(object sender, WindowsForms.MouseEventArgs e)
        {
            if (_mouseMove != null)
            {
                //_logger.Debug("directPlayWindow_MouseMove");
                // map from System.Windows.Forms to System.Windows.Input mouse move event
                  var window = _presenationManager.Window.Dispatcher.InvokeAsync(() =>
                  {
                    var mouseEventArgs = new MouseButtonEventArgs(WindowsInput.InputManager.Current.PrimaryMouseDevice,
                        0,
                        ConvertMouseButton(e.Button));
                    _mouseMove.Invoke(null, mouseEventArgs);
                  });
            }
        }


        private WindowsInput.MouseButton ConvertMouseButton(WindowsForms.MouseButtons button)
        {
            switch (button)
            {
                case WindowsForms.MouseButtons.Left:
                    return WindowsInput.MouseButton.Left;
                case WindowsForms.MouseButtons.Right:
                    return WindowsInput.MouseButton.Right;
                case WindowsForms.MouseButtons.Middle:
                    return WindowsInput.MouseButton.Middle;
                case WindowsForms.MouseButtons.XButton1:
                    return WindowsInput.MouseButton.XButton1;
                case WindowsForms.MouseButtons.XButton2:
                    return WindowsInput.MouseButton.XButton2;
                // can't map none & can't access MouseButtonEventArgs.ButtonState
                // todo - use reflection to set button sate, join new MouseButtonEventArgs call & and this method 
                case WindowsForms.MouseButtons.None:
                    return WindowsInput.MouseButton.XButton2; 
            }
            throw new InvalidOperationException();
        }
    }
}
