using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DirectShowLib;
using Mpdn.PlayerExtensions.GitHub.DisplayChangerNativeMethods;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class DisplayChanger : PlayerExtension<DisplayChangerSettings, DisplayChangerConfigDialog>
    {
        private Screen m_RestoreScreen;
        private int m_RestoreFrequency;

        private readonly List<int> m_AllRefreshRates = new List<int>();

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("9C1BBA5B-B956-43E1-9A91-58B72571EF82"),
                    Name = "Display Changer",
                    Description = "Changes display refresh rate based on video"
                };
            }
        }

        protected override string ConfigFileName
        {
            get { return "Example.DisplayChanger"; }
        }

        public override void Initialize()
        {
            base.Initialize();

            PlayerControl.PlayerStateChanged += PlayerStateChanged;
            PlayerControl.FormClosed += FormClosed;

            foreach (var screen in Screen.AllScreens)
            {
                m_AllRefreshRates.Add(GetRefreshRate(screen));
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            PlayerControl.PlayerStateChanged -= PlayerStateChanged;
            PlayerControl.FormClosed -= FormClosed;
        }

        public override IList<Verb> Verbs
        {
            get { return new Verb[0]; }
        }

        private void FormClosed(object sender, EventArgs eventArgs)
        {
            RestoreSettings();

            if (Settings.RestoreOnExit)
            {
                for (int i = 0; i < m_AllRefreshRates.Count; i++)
                {
                    var rate = m_AllRefreshRates[i];
                    ChangeRefreshRate(Screen.AllScreens[i], rate);
                }
            }
        }

        private void PlayerStateChanged(object sender, PlayerStateEventArgs e)
        {
            if (e.NewState == PlayerState.Closed)
            {
                RestoreSettings();
                return;
            }

            if (e.OldState != PlayerState.Closed)
                return;

            if (!Activated())
                return;

            m_RestoreFrequency = 0;

            var screen = Screen.FromControl(PlayerControl.VideoPanel);

            var frequencies = GetFrequencies(screen);

            if (!frequencies.Any())
                return;

            var timePerFrame = PlayerControl.VideoInfo.AvgTimePerFrame;
            if (Math.Abs(timePerFrame) < 1)
                return;

            var fps = Math.Round(1000000 / timePerFrame, 3);
            if (Math.Abs(fps) < 1) 
                return;

            // Find the highest frequency that matches fps
            var frequenciesDescending = frequencies.OrderByDescending(f => f).ToArray();
            var frequency = frequenciesDescending.FirstOrDefault(f => MatchFrequencies(f, fps));
            if (frequency == 0)
            {
                // Exact match (23 for 23.976) not found
                // Find the closest one that matches fps (e.g. 24 for 23.976)
                frequency = frequenciesDescending.FirstOrDefault(f => MatchRoundedFrequencies(f, fps));
            }
            if (frequency == 0)
            {
                // Still couldn't find a match
                if (!Settings.HighestRate)
                    return;

                // Use the highest rate available
                frequency = frequenciesDescending.First();
            }
            
            ChangeRefreshRate(screen, frequency);
        }

        private static bool MatchRoundedFrequencies(int f, double fps)
        {
            return f % (int) Math.Round(fps) == 0;
        }

        private static bool MatchFrequencies(int f, double fps)
        {
            var multiples = Math.Round(f/fps);
            fps *= multiples;

            return f % (int) fps == 0;
        }

        private bool Activated()
        {
            if (!Settings.Activate)
                return false;

            if (!Settings.Restricted) 
                return true;

            // Parse video types
            var videoTypes = Settings.VideoTypes.ToLowerInvariant().Split(' ');
            return videoTypes.Any(VideoTypeMatches);
        }

        private bool VideoTypeMatches(string vt)
        {
            var regexValidate = new Regex(@"^(w{1}\d+)?(h{1}\d+)?((i|p){1}\d+)?$");
            if (regexValidate.Matches(vt).Count == 0)
                return false;

            var vid = PlayerControl.VideoInfo.BmiHeader;

            var regexWidth = new Regex(@"w(\d+)");
            var widthMatch = regexWidth.Match(vt).Groups[1];
            var width = widthMatch.Success ? int.Parse(widthMatch.Value) : vid.Width;

            var regexHeight = new Regex(@"h(\d+)");
            var heightMatch = regexHeight.Match(vt).Groups[1];
            var height = heightMatch.Success ? int.Parse(heightMatch.Value) : vid.Height;

            var vidIsInterlaced = PlayerControl.VideoInfo.InterlaceFlags.HasFlag(AMInterlace.IsInterlaced);
            var vidFps = 1000000/(int) PlayerControl.VideoInfo.AvgTimePerFrame;

            bool interlaced = vidIsInterlaced;
            int frameRate = vidFps;

            var regexFrameRate = new Regex(@"(i|p)(\d+)");
            var frameRateMatch = regexFrameRate.Match(vt);
            if (frameRateMatch.Success)
            {
                interlaced = frameRateMatch.Groups[1].Value == "i";
                frameRate = int.Parse(frameRateMatch.Groups[2].Value);
            }

            return width == vid.Width && height == vid.Height && interlaced == vidIsInterlaced && frameRate == vidFps;
        }

        private void RestoreSettings()
        {
            if (m_RestoreFrequency == 0)
                return;

            if (Settings.Restore)
            {
                ChangeRefreshRate(m_RestoreScreen, m_RestoreFrequency);
            }
            m_RestoreFrequency = 0;
        }

        private void ChangeRefreshRate(Screen screen, int frequency)
        {
            bool wasFullScreen = false;
            if (PlayerControl.InFullScreenMode)
            {
                wasFullScreen = true;
                // We can't change frequency in exclusive mode
                PlayerControl.GoWindowed();
            }

            var dm = NativeMethods.CreateDevmode(screen.DeviceName);
            if (GetSettings(ref dm, screen.DeviceName) == 0)
                return;

            if (dm.dmDisplayFrequency == frequency)
                return;

            var oldFreq = dm.dmDisplayFrequency;
            m_RestoreScreen = screen;

            dm.dmFields = (int) DM.DisplayFrequency;
            dm.dmDisplayFrequency = frequency;
            dm.dmDeviceName = screen.DeviceName;
            bool continuePlaying = false;
            if (PlayerControl.PlayerState == PlayerState.Playing)
            {
                continuePlaying = true;
                PlayerControl.PauseMedia(false);
            }
            if (ChangeSettings(dm))
            {
                m_RestoreFrequency = oldFreq;
            }
            if (continuePlaying)
            {
                PlayerControl.PlayMedia(false);
            }
            if (wasFullScreen)
            {
                PlayerControl.GoFullScreen();
            }
        }

        private static IList<int> GetFrequencies(Screen screen)
        {
            var frequencies = new List<int>();
            int index = 0;
            while (true)
            {
                var dm = NativeMethods.CreateDevmode(screen.DeviceName);
                if (GetSettings(ref dm, screen.DeviceName, index++) == 0)
                    break;

                if (dm.dmBitsPerPel != 32)
                    continue;

                if (dm.dmDisplayFlags != 0) // Only want progressive modes (1: DM_GRAYSCALE, 2: DM_INTERLACED)
                    continue;

                if (dm.dmPelsWidth != screen.Bounds.Width)
                    continue;

                if (dm.dmPelsHeight != screen.Bounds.Height)
                    continue;

                frequencies.Add(dm.dmDisplayFrequency);
            }

            return frequencies;
        }

        private static int GetRefreshRate(Screen screen)
        {
            var dm = NativeMethods.CreateDevmode(screen.DeviceName);
            return GetSettings(ref dm, screen.DeviceName) == 0 ? 0 : dm.dmDisplayFrequency;
        }

        private static bool ChangeSettings(DEVMODE dm)
        {
            if (NativeMethods.ChangeDisplaySettingsEx(dm.dmDeviceName, ref dm, IntPtr.Zero,
                    NativeMethods.CDS_UPDATEREGISTRY, IntPtr.Zero) == NativeMethods.DISP_CHANGE_SUCCESSFUL)
            {
                NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, NativeMethods.WM_DISPLAYCHANGE,
                    new IntPtr(NativeMethods.SPI_SETNONCLIENTMETRICS), IntPtr.Zero);
                return true;
            }
            Trace.Write("Failed to change display refresh rate");
            return false;
        }

        private static int GetSettings(ref DEVMODE dm, string deviceName)
        {
            // helper to obtain current settings
            return GetSettings(ref dm, deviceName, NativeMethods.ENUM_CURRENT_SETTINGS);
        }

        private static int GetSettings(ref DEVMODE dm, string deviceName, int iModeNum)
        {
            // helper to wrap EnumDisplaySettings Win32 API
            return NativeMethods.EnumDisplaySettings(deviceName, iModeNum, ref dm);
        }
    }

    public class DisplayChangerSettings
    {
        public DisplayChangerSettings()
        {
            Activate = false;
            Restore = false;
            RestoreOnExit = false;
            HighestRate = false;
            Restricted = false;
        }

        public bool Activate { get; set; }
        public bool Restore { get; set; }
        public bool RestoreOnExit { get; set; }
        public bool HighestRate { get; set; }
        public bool Restricted { get; set; }
        public string VideoTypes { get; set; }
    }
}
