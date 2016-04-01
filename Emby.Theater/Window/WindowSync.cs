using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaBrowser.Model.Logging;
using Microsoft.Win32;

namespace Emby.Theater.Window
{
    public class WindowSync
    {
        private readonly Form _form;
        private readonly IntPtr _windowHandle;
        private readonly ILogger _logger;

        private System.Threading.Timer _syncTimer;

        public WindowSync(Form form, IntPtr windowHandle, ILogger logger)
        {
            _form = form;
            _windowHandle = windowHandle;
            _logger = logger;

            _form.Invoke(new MethodInvoker(() =>
            {
                _form.ShowInTaskbar = true;
                NativeWindowMethods.SetWindowLong(_windowHandle, -8, _form.Handle);
                // Until the electron window starts reporting window changes, use a timer to keep them in sync
                //_syncTimer = new System.Threading.Timer(OnTimerCallback, null, 10, 10);
            }));

            OnTimerCallback(null);
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            RestoreMaximizeIfNeeded();
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                RestoreMaximizeIfNeeded();
            }
        }

        public void OnElectronWindowSizeChanged()
        {
            // Now that the electron window is reporting changes, this timer is no longer needed
            var timer = _syncTimer;
            if (timer != null)
            {
                timer.Dispose();
                _syncTimer = null;
            }

            SyncWindowSize(false);
        }

        public void OnElectronWindowStateChanged(string newWindowState)
        {
            // Now that the electron window is reporting changes, this timer is no longer needed
            var timer = _syncTimer;
            if (timer != null)
            {
                timer.Dispose();
                _syncTimer = null;
            }

            SyncWindowState(newWindowState);
        }

        private void OnTimerCallback(object state)
        {
            SyncAllStates();
        }

        private async void RestoreMaximizeIfNeeded()
        {
            await Task.Delay(5000);

            FormWindowState state = FormWindowState.Normal;
            _form.InvokeIfRequired(() =>
            {
                state = _form.WindowState;
            });

            if (state == FormWindowState.Maximized)
            {
                SyncWindowState("Maximized");
            }
        }

        private void SyncAllStates()
        {
            var placement = NativeWindowMethods.GetPlacement(_windowHandle);

            switch (placement.showCmd)
            {
                case ShowWindowCommands.Maximized:
                    SyncWindowState("Maximized");
                    break;
                case ShowWindowCommands.Minimized:
                    SyncWindowState("Minimized");
                    break;
                case ShowWindowCommands.Normal:
                    SyncWindowState("Normal");
                    break;
            }

            SyncWindowSize(true);
        }

        private void SyncWindowSize(bool log)
        {
            try
            {
                RECT rect = new RECT();
                NativeWindowMethods.GetWindowRect(_windowHandle, ref rect);

                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                if (log)
                {
                    _logger.Info("SyncWindowSize Top={0} Left={1} Width={2} Height={3}", rect.Top, rect.Left, width, height);
                }

                _form.InvokeIfRequired(() =>
                {
                    if (_form.WindowState == FormWindowState.Normal)
                    {
                        _form.Top = rect.Top;
                        _form.Left = rect.Left;
                        _form.Width = rect.Right - rect.Left;
                        _form.Height = rect.Bottom - rect.Top;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error syncing window positions", ex);
            }
        }

        private void SyncWindowState(string newWindowState)
        {
            _logger.Info("Setting window state to {0}", newWindowState);

            try
            {
                FormWindowState newState;
                bool fullscreen = false;
                if (string.Equals(newWindowState, "fullscreen", StringComparison.OrdinalIgnoreCase))
                {
                    newState = FormWindowState.Maximized;
                    fullscreen = true;
                }
                else if (string.Equals(newWindowState, "maximized", StringComparison.OrdinalIgnoreCase))
                {
                    newState = FormWindowState.Maximized;
                    fullscreen = true;
                }
                else if (string.Equals(newWindowState, "minimized", StringComparison.OrdinalIgnoreCase))
                {
                    newState = FormWindowState.Minimized;
                }
                else
                {
                    newState = FormWindowState.Normal;
                }

                _form.InvokeIfRequired(() =>
                {
                    _form.WindowState = newState;
                });

                if (fullscreen)
                {
                    NativeWindowMethods.SetWindowPos(_windowHandle, -1, _form.Left, _form.Top, _form.Width, _form.Height, 0);
                    //var placement = new WINDOWPLACEMENT();
                    //placement.showCmd = ShowWindowCommands.Maximized;
                    //placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    //NativeWindowMethods.SetWindowPlacement(_windowHandle, ref placement);
                }
                else if (newState == FormWindowState.Maximized)
                {
                    //NativeWindowMethods.SetWindowPos(_windowHandle, -1, _form.Left, _form.Top, _form.Width, _form.Height, 0);
                    var placement = new WINDOWPLACEMENT();
                    placement.showCmd = ShowWindowCommands.Maximized;
                    placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    NativeWindowMethods.SetWindowPlacement(_windowHandle, ref placement);
                }
                else if (newState == FormWindowState.Normal)
                {
                    var placement = new WINDOWPLACEMENT();
                    placement.showCmd = ShowWindowCommands.Normal;
                    NativeWindowMethods.SetWindowPos(_windowHandle, -2, _form.Left, _form.Top, _form.Width, _form.Height, 0);
                    //NativeWindowMethods.SetWindowPlacement(_windowHandle, ref placement);
                }

                if (newState != FormWindowState.Minimized)
                {
                    NativeWindowMethods.SetForegroundWindow(_windowHandle);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error syncing window positions", ex);
            }
        }
    }
}
