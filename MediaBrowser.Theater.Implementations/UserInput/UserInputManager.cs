using Gma.UserActivityMonitor;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MediaBrowser.Theater.Implementations.UserInput
{
    /// <summary>
    /// Class UserInputManager
    /// </summary>
    public class UserInputManager : IUserInputManager
    {
        /// <summary>
        /// Occurs when [key down].
        /// </summary>
        public event KeyEventHandler KeyDown
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

        /// <summary>
        /// Occurs when [mouse move].
        /// </summary>
        public event MouseEventHandler MouseMove
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
    }
}
