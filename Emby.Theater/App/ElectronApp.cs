using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.System;
using System.Runtime.InteropServices;

namespace Emby.Theater.App
{
    public class ElectronApp : IDisposable
    {
        private Process _process;
        private ILogger _logger;
        private Func<IHttpClient> _httpClient;
        private IApplicationPaths _appPaths;
        private IEnvironmentInfo _environmentInfo;

        public ElectronApp(ILogger logger, IApplicationPaths appPaths, Func<IHttpClient> httpClient, IEnvironmentInfo environment)
        {
            _logger = logger;
            _appPaths = appPaths;
            _httpClient = httpClient;
            _environmentInfo = environment;
            StartProcess();
        }

        public void Dispose()
        {
            CloseProcess();
            GC.SuppressFinalize(this);
        }

        private void StartProcess()
        {
            var appDirectoryPath = Path.GetDirectoryName(Program.ApplicationPath);

            var is64BitElectron = Program.Is64BitElectron;

            var architecture = is64BitElectron ? "x64" : "x86";
            var archPath = Path.Combine(appDirectoryPath, architecture);
            var electronExePath = Path.Combine(archPath, "electron", "electron.exe");
            var electronAppPath = Path.Combine(appDirectoryPath, "electronapp");
            var mpvExePath = Path.Combine(archPath, "mpv", "mpv.exe");

            var dataPath = Path.Combine(_appPaths.DataPath, "electron");

            var cecPath = Path.Combine(Path.GetDirectoryName(Program.ApplicationPath), "cec");
            if (is64BitElectron)
            {
                cecPath = Path.Combine(cecPath, "cec-client.x64.exe");
            }
            else
            {
                cecPath = Path.Combine(cecPath, "cec-client.exe");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,

                    FileName = electronExePath,
                    Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", electronAppPath, dataPath, cecPath, mpvExePath)
                },

                EnableRaisingEvents = true,
            };

            _logger.Info("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            process.Exited += Process_Exited;

            process.Start();

            //process.WaitForInputIdle(3000);

            while (process.MainWindowHandle.Equals(IntPtr.Zero))
            {
                var task = Task.Delay(50);
                Task.WaitAll(task);
            }

            try
            {
                Task.WaitAll(Task.Delay(2000));
                Win32.SendMessage(process.MainWindowHandle.ToInt32(), Win32.WM_LBUTTONDOWN, 0x00000001, 0x1E5025B);
            }
            catch
            {

            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Program.Exit();
        }

        private void CloseProcess()
        {
            var process = _process;

            if (process == null)
            {
                return;
            }

            _logger.Info("Closing electron");

            using (process)
            {
                try
                {
                    var url = "http://127.0.0.1:8024/exit";
                    using (_httpClient().Post(url, new Dictionary<string, string>(), CancellationToken.None).Result)
                    {

                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error attempting to close electron", ex);
                }

                var exited = false;

                try
                {
                    _logger.Info("electron WaitForExit");
                    exited = process.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error in WaitForExit", ex);
                }

                if (exited)
                {
                    _logger.Info("electron exited");
                }
                else
                {
                    try
                    {
                        _logger.Info("electron Kill");
                        process.Kill();
                    }
                    catch
                    {

                    }
                }
            }

            _process = null;
        }
    }

//    public class ClickOnPointTool
//    {

//        [DllImport("user32.dll")]
//        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

//        [DllImport("user32.dll")]
//        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

//#pragma warning disable 649
//        internal struct INPUT
//        {
//            public UInt32 Type;
//            public MOUSEKEYBDHARDWAREINPUT Data;
//        }

//        [StructLayout(LayoutKind.Explicit)]
//        internal struct MOUSEKEYBDHARDWAREINPUT
//        {
//            [FieldOffset(0)]
//            public MOUSEINPUT Mouse;
//        }

//        internal struct MOUSEINPUT
//        {
//            public Int32 X;
//            public Int32 Y;
//            public UInt32 MouseData;
//            public UInt32 Flags;
//            public UInt32 Time;
//            public IntPtr ExtraInfo;
//        }

//#pragma warning restore 649
//        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
//        {
//            /// get screen coordinates
//            ClientToScreen(wndHandle, ref clientPoint);

//            var inputMouseDown = new INPUT();
//            inputMouseDown.Type = 0; /// input type mouse
//            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

//            var inputMouseUp = new INPUT();
//            inputMouseUp.Type = 0; /// input type mouse
//            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

//            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
//            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
//        }

//    }

    /// <summary>
    /// Summary description for Win32.
    /// </summary>
    public class Win32
    {
        // The WM_COMMAND message is sent when the user selects a command item from 
        // a menu, when a control sends a notification message to its parent window, 
        // or when an accelerator keystroke is translated.
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_COMMAND = 0x111;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;

        // The FindWindow function retrieves a handle to the top-level window whose
        // class name and window name match the specified strings.
        // This function does not search child windows.
        // This function does not perform a case-sensitive search.
        [DllImport("User32.dll")]
        public static extern int FindWindow(string strClassName, string strWindowName);

        // The FindWindowEx function retrieves a handle to a window whose class name 
        // and window name match the specified strings.
        // The function searches child windows, beginning with the one following the
        // specified child window.
        // This function does not perform a case-sensitive search.
        [DllImport("User32.dll")]
        public static extern int FindWindowEx(
            int hwndParent,
            int hwndChildAfter,
            string strClassName,
            string strWindowName);


        // The SendMessage function sends the specified message to a window or windows. 
        // It calls the window procedure for the specified window and does not return
        // until the window procedure has processed the message. 
        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(
            int hWnd,               // handle to destination window
            int Msg,                // message
            int wParam,             // first message parameter
            [MarshalAs(UnmanagedType.LPStr)] string lParam); // second message parameter

        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(
            int hWnd,               // handle to destination window
            int Msg,                // message
            int wParam,             // first message parameter
            int lParam);            // second message parameter
    }
}
