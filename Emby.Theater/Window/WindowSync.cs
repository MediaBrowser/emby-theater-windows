using System;
using System.Windows.Forms;
using MediaBrowser.Model.Logging;

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

            SyncWindowSize();
        }

        public void OnElectronWindowStateChanged(FormWindowState newWindowState)
        {
            // Now that the electron window is reporting changes, this timer is no longer needed
            var timer = _syncTimer;
            if (timer != null)
            {
                timer.Dispose();
                _syncTimer = null;
            }

            _logger.Info("Setting window state to {0}", newWindowState.ToString());

            SyncWindowState(newWindowState);
        }

        private void OnTimerCallback(object state)
        {
            var placement = NativeWindowMethods.GetPlacement(_windowHandle);

            switch (placement.showCmd)
            {
                case ShowWindowCommands.Maximized:
                    SyncWindowState(FormWindowState.Maximized);
                    break;
                case ShowWindowCommands.Minimized:
                    SyncWindowState(FormWindowState.Minimized);
                    break;
                case ShowWindowCommands.Normal:
                    SyncWindowState(FormWindowState.Normal);
                    break;
            }

            SyncWindowSize();
        }

        private void SyncWindowSize()
        {
            try
            {
                RECT rect = new RECT();
                NativeWindowMethods.GetWindowRect(_windowHandle, ref rect);

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

        private void SyncWindowState(FormWindowState newWindowState)
        {
            try
            {
                _form.InvokeIfRequired(() =>
                {
                    _form.WindowState = newWindowState;
                });

                if (newWindowState == FormWindowState.Maximized)
                {
                    NativeWindowMethods.SetWindowPos(_windowHandle, -1, _form.Left, _form.Top, _form.Width, _form.Height, 0);
                }
                else if (newWindowState == FormWindowState.Normal)
                {
                    NativeWindowMethods.SetWindowPos(_windowHandle, -2, _form.Left, _form.Top, _form.Width, _form.Height, 0);
                }

                NativeWindowMethods.SetForegroundWindow(_windowHandle);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error syncing window positions", ex);
            }
        }
    }
}
