// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.Runtime.InteropServices;

namespace Mpdn.PlayerExtensions.GitHub.DisplayChangerNativeMethods
{
    [Flags]
    internal enum DM
    {
        Orientation = 0x1,
        PaperSize = 0x2,
        PaperLength = 0x4,
        PaperWidth = 0x8,
        Scale = 0x10,
        Position = 0x20,
        NUP = 0x40,
        DisplayOrientation = 0x80,
        Copies = 0x100,
        DefaultSource = 0x200,
        PrintQuality = 0x400,
        Color = 0x800,
        Duplex = 0x1000,
        YResolution = 0x2000,
        TTOption = 0x4000,
        Collate = 0x8000,
        FormName = 0x10000,
        LogPixels = 0x20000,
        BitsPerPixel = 0x40000,
        PelsWidth = 0x80000,
        PelsHeight = 0x100000,
        DisplayFlags = 0x200000,
        DisplayFrequency = 0x400000,
        ICMMethod = 0x800000,
        ICMIntent = 0x1000000,
        MediaType = 0x2000000,
        DitherType = 0x4000000,
        PanningWidth = 0x8000000,
        PanningHeight = 0x10000000,
        DisplayFixedOutput = 0x20000000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;

        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;

        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    };

    public class NativeMethods
    {
        // PInvoke declaration for EnumDisplaySettings Win32 API

        // constants
        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        public const int SPI_SETNONCLIENTMETRICS = 42;
        public const int CDS_UPDATEREGISTRY = 1;

        public const int WM_DISPLAYCHANGE = 0x7E;

        public const int ENUM_CURRENT_SETTINGS = -1;

        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_BADDUALVIEW = -6;
        public const int DISP_CHANGE_BADFLAGS = -4;
        public const int DISP_CHANGE_BADMODE = -2;
        public const int DISP_CHANGE_BADPARAM = -5;
        public const int DISP_CHANGE_FAILED = -1;
        public const int DISP_CHANGE_NOTUPDATED = -3;
        public const int DISP_CHANGE_RESTART = 1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        // PInvoke declaration for ChangeDisplaySettings Win32 API
        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd,
            int dwflags, IntPtr lParam);

        // helper for creating an initialized DEVMODE structure
        public static DEVMODE CreateDevmode(string deviceName)
        {
            var dm = new DEVMODE
            {
                dmDeviceName = new String(deviceName.ToCharArray()),
                dmFormName = new String(new char[32]),
                dmSize = (short) Marshal.SizeOf(typeof (DEVMODE))
            };
            return dm;
        }
    }
}
