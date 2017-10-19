using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ListResolutions
{

    class Program
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string deviceName, ref DEVMODE devMode, IntPtr hwnd, int flags, IntPtr lParam);

        const int ENUM_CURRENT_SETTINGS = -1;
        const int ENUM_REGISTRY_SETTINGS = -2;
        const int CDS_TEST = 0x02;
        const int CDS_UPDATEREGISTRY = 0x01;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
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
        }

        static void Main(string[] args)
        {
            DEVMODE vDevMode = new DEVMODE();
            DEVMODE vDevMode_current = new DEVMODE();

            string action = "";
            string mon_name = "";
            string mon_freq_request = "";

            if (args.Length > 0)
            {
                action = args[0];
                mon_name = args[1];

                if (args.Length == 3) mon_freq_request = args[2];
            }

            EnumDisplaySettings(mon_name, ENUM_CURRENT_SETTINGS, ref vDevMode_current);

            switch (action)
            {
                case "current":
                    Console.Write("Current Refresh Rate -: " + vDevMode_current.dmDisplayFrequency.ToString());
                    break;
                case "clean":
                    Console.Write("Current Refresh Rate -: " + vDevMode_current.dmDisplayFrequency.ToString());
                    break;
                case "change":
                    bool found_match = false;
                    foreach (string frequency in mon_freq_request.Split(';'))
                    {
                        int i = 0;
                        while (EnumDisplaySettings(mon_name, i, ref vDevMode))
                        {
                            if (vDevMode.dmPelsWidth == vDevMode_current.dmPelsWidth && vDevMode.dmBitsPerPel == vDevMode_current.dmBitsPerPel && vDevMode.dmPelsHeight == vDevMode_current.dmPelsHeight && frequency == vDevMode.dmDisplayFrequency.ToString())
                            {
                                int result = ChangeDisplaySettingsEx(mon_name, ref vDevMode, IntPtr.Zero, CDS_TEST, IntPtr.Zero);
                                if (result == 0)
                                {
                                    ChangeDisplaySettingsEx(mon_name, ref vDevMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                                    EnumDisplaySettings(mon_name, ENUM_CURRENT_SETTINGS, ref vDevMode_current);
                                    Console.Write("Current Refresh Rate -: " + vDevMode_current.dmDisplayFrequency.ToString());
                                    found_match = true;
                                    break;
                                }
                            }
                            i++;
                        }
                        if (found_match) break;
                    }

                    if (!found_match) Console.Write("Current Refresh Rate -: " + vDevMode_current.dmDisplayFrequency.ToString());
                    break;
                case "list_all":
                    int h = 0;
                    while (EnumDisplaySettings(mon_name, h, ref vDevMode))
                    {
                        Console.WriteLine(vDevMode.dmDeviceName + " Width:{0} Height:{1} Frequency:{2} BitDepth:{3}", vDevMode.dmPelsWidth, vDevMode.dmPelsHeight, vDevMode.dmDisplayFrequency, vDevMode.dmBitsPerPel);
                        h++;
                    }
                    break;
                case "list_possible":
                    string holding = "";
                    int j = 0;
                    while (EnumDisplaySettings(mon_name, j, ref vDevMode))
                    {
                        if (vDevMode.dmPelsWidth == vDevMode_current.dmPelsWidth && vDevMode.dmBitsPerPel == vDevMode_current.dmBitsPerPel && vDevMode.dmPelsHeight == vDevMode_current.dmPelsHeight)
                        {
                            if (holding == "") holding = vDevMode.dmDisplayFrequency.ToString();
                            else holding += ";" + vDevMode.dmDisplayFrequency.ToString();
                        }
                        j++;
                    }
                    Console.Write(holding);
                    break;
                default:
                    break;
            }
            //Console.ReadLine(); //Pause
        }

    }

}