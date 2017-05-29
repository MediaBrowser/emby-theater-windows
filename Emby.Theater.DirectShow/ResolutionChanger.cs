using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DirectShowLib.Utils;
using System.Windows.Forms;

namespace Emby.Theater.DirectShow
{
    public class Display
    {
        static Display _instance = new Display();
        List<Resolution> _modes = null;
        List<DEVMODE> _resCombo = null;

        public static Display Instance
        {
            get
            {
                return _instance;
            }
        }

        public List<DEVMODE> GetDeviceModes()
        {
            if (_resCombo == null)
            {
                _resCombo = new List<DEVMODE>();
                int i = 0;

                DEVMODE dm = new DEVMODE();
                dm.dmDeviceName = new String(new char[32]);
                dm.dmFormName = new String(new char[32]);
                dm.dmSize = (short)Marshal.SizeOf(dm);

                while (0 != NativeMethods.EnumDisplaySettings(null, i, ref dm))
                {
                    if (dm.dmBitsPerPel == 32)
                    {
                        _resCombo.Add(dm);
                    }
                    i++;
                }
            }

            return _resCombo;
        }

        public List<Resolution> GetResolutions()
        {
            if (_modes == null)
            {
                _modes = new List<Resolution>();
                List<string> addedModes = new List<string>();
                int i = 0;

                DEVMODE dm = new DEVMODE();
                dm.dmDeviceName = new String(new char[32]);
                dm.dmFormName = new String(new char[32]);
                dm.dmSize = (short)Marshal.SizeOf(dm);

                while (0 != NativeMethods.EnumDisplaySettings(null, i, ref dm))
                {
                    if (dm.dmBitsPerPel == 32 || dm.dmBitsPerPel == 16)
                    {
                        Resolution r = new Resolution(dm.dmPelsWidth, dm.dmPelsHeight, dm.dmDisplayFrequency, (dm.dmDisplayFlags & NativeMethods.DM_INTERLACED) == NativeMethods.DM_INTERLACED, dm.dmDisplayFixedOutput, dm.dmBitsPerPel);
                        if (!addedModes.Contains(r.ToString()))
                        {
                            _modes.Add(r);
                            addedModes.Add(r.ToString());
                        }
                    }
                    i++;
                }
            }

            return _modes;
        }

        public static Screen GetScreenFromHandle(IntPtr handle)
        {
            return Screen.FromHandle(handle);
        }

        public static Resolution GetCurrentResolution(IntPtr windowHandle)
        {
            DEVMODE cDm = new DEVMODE();
            //cDm.dmDeviceName = new String(new char[32]);
            //cDm.dmFormName = new String(new char[32]);
            cDm.dmSize = (short)Marshal.SizeOf(cDm);

            NativeMethods.EnumDisplaySettings(GetScreenFromHandle(windowHandle).DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref cDm);

            Resolution res = new Resolution(cDm.dmPelsWidth, cDm.dmPelsHeight, cDm.dmDisplayFrequency, ((cDm.dmDisplayFlags & NativeMethods.DM_INTERLACED) == NativeMethods.DM_INTERLACED), cDm.dmDisplayFixedOutput, cDm.dmBitsPerPel);

            return res;
        }

        public static bool ChangeResolution(IntPtr windowHandle, Resolution res, bool permanent)
        {
            int i = 0;

            DEVMODE cDm = new DEVMODE();
            //cDm.dmDeviceName = new String(new char[32]);
            //cDm.dmFormName = new String(new char[32]);
            cDm.dmSize = (short)Marshal.SizeOf(cDm);
            //FileLogger.Log("DEVMODE Size: {0}", Marshal.SizeOf(cDm));
            Screen hostScreen = GetScreenFromHandle(windowHandle);

            NativeMethods.EnumDisplaySettings(hostScreen.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref cDm);

            if (cDm.dmBitsPerPel == res.PixelDepth
                    && cDm.dmPelsWidth == res.Width
                    && cDm.dmPelsHeight == res.Height
                    && cDm.dmDisplayFrequency == res.Rate
                    && ((cDm.dmDisplayFlags & NativeMethods.DM_INTERLACED) == NativeMethods.DM_INTERLACED) == res.Interlaced
                    && cDm.dmDisplayFixedOutput == res.FixedOutput)
            {
                return false;
            }

            DEVMODE dm = new DEVMODE();
            //dm.dmDeviceName = new String(new char[32]);
            //dm.dmFormName = new String(new char[32]);
            dm.dmSize = (short)Marshal.SizeOf(dm);

            while (0 != NativeMethods.EnumDisplaySettings(hostScreen.DeviceName, i, ref dm))
            {
                if (dm.dmBitsPerPel == res.PixelDepth
                    && dm.dmPelsWidth == res.Width
                    && dm.dmPelsHeight == res.Height
                    && dm.dmDisplayFrequency == res.Rate
                    && ((dm.dmDisplayFlags & NativeMethods.DM_INTERLACED) == NativeMethods.DM_INTERLACED) == res.Interlaced
                    && dm.dmDisplayFixedOutput == res.FixedOutput)
                {
                    DISP_CHANGE iRet = NativeMethods.ChangeDisplaySettingsEx(hostScreen.DeviceName, ref dm, IntPtr.Zero, CDS.Test, IntPtr.Zero);

                    if (iRet == DISP_CHANGE.Successful)
                    {
                        if (permanent)
                            iRet = NativeMethods.ChangeDisplaySettingsEx(hostScreen.DeviceName, ref dm, IntPtr.Zero, CDS.UpdateRegistry, IntPtr.Zero);
                        else
                        {
                            iRet = NativeMethods.ChangeDisplaySettingsEx(hostScreen.DeviceName, ref dm, IntPtr.Zero, CDS.Dynamic, IntPtr.Zero);
                            //IntPtr nRes = IntPtr.Zero;
                            //try
                            //{
                            //    nRes = Marshal.AllocHGlobal(Marshal.SizeOf(dm));
                            //    Marshal.StructureToPtr(dm, nRes, true);
                            //    iRet = NativeMethods.ChangeDisplaySettings(nRes, CDS.Dynamic);
                            //}
                            //finally
                            //{
                            //    if (nRes != IntPtr.Zero)
                            //        Marshal.FreeHGlobal(nRes);
                            //}
                        }
                    }

                    return iRet == DISP_CHANGE.Successful;
                }

                i++;
            }
            return false;
        }

        public static void RevertResolution()
        {
            NativeMethods.ChangeDisplaySettings(IntPtr.Zero, CDS.Dynamic);
        }

        private Display() { }
    }

    [Serializable()]
    public class Resolution
    {
        int _width = 0;
        int _height = 0;
        int _rate = 0;
        //int _output = 0;
        bool _interlaced = false;
        int _fixedOutput = 0;
        int _pixelDepth = 32;

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public int Rate
        {
            get
            {
                return _rate;
            }
            set
            {
                _rate = value;
            }
        }

        public bool Interlaced
        {
            get
            {
                return _interlaced;
            }
            set
            {
                _interlaced = value;
            }
        }

        public int FixedOutput
        {
            get
            {
                return _fixedOutput;
            }
            set
            {
                _fixedOutput = value;
            }
        }

        public int PixelDepth
        {
            get
            {
                return _pixelDepth;
            }
            set
            {
                _pixelDepth = value;
            }
        }

        public Resolution()
        {

        }

        public Resolution(int width, int height, int rate, bool interlaced, int fixedOutput, int pixelDepth)
        {
            _height = height;
            _width = width;
            _rate = rate;
            _interlaced = interlaced;
            _fixedOutput = fixedOutput;
            _pixelDepth = pixelDepth;
        }

        public Resolution(string text)
        {
            Match m = Regex.Match(text, @"(?<width>\d+)x(?<height>\d+)(?<i>\w)\s(?:(?<bits>\d\d)-bit\s)?@(?<rate>\d+)Hz\s-\s(?<output>\w+)");
            if (m.Success)
            {
                _height = Convert.ToInt32(m.Groups["height"].Value);
                _width = Convert.ToInt32(m.Groups["width"].Value);
                _rate = Convert.ToInt32(m.Groups["rate"].Value);
                _interlaced = m.Groups["i"].Value == "i" ? true : false;
                if (!string.IsNullOrEmpty(m.Groups["bits"].Value))
                    _pixelDepth = Convert.ToInt32(m.Groups["bits"].Value);

                DisplayFixedOutput fo = (DisplayFixedOutput)Enum.Parse(typeof(DisplayFixedOutput), m.Groups["output"].Value);
                _fixedOutput = (int)fo;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}x{1}{3} {5}-bit @{2}Hz - {4}", _width, _height, _rate, _interlaced ? "i" : "p", ((DisplayFixedOutput)_fixedOutput), _pixelDepth);
        }
    }
}
