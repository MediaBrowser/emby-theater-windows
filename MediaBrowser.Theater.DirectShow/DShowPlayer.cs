using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;
using System.IO;
using DirectShowLib;
using DirectShowLib.Dvd;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.UI.Playback.DirectShow
{
    [ComImport, Guid("E1A8B82A-32CE-4B0D-BE0D-AA68C772E423")]
    public class MadVR
    {
    }

    [ComImport, Guid("EE30215D-164F-4A92-A4EB-9D4C13390F9F")]
    internal class LAVVideo
    {
    }

    [ComImport, Guid("E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491")]
    internal class LAVAudio
    {
    }

    [ComImport, Guid("79376820-07D0-11CF-A24D-0020AFD79767")]
    internal class DefaultAudioRenderer
    {
    }

    [ComImport, Guid("9DC15360-914C-46B8-B9DF-BFE67FD36C6A")]
    internal class ReclockAudioRenderer
    {
    }

    internal enum MenuMode
    {
        No,
        Buttons,
        Still
    }

    public enum PlaybackState
    {
        Running,
        Paused,
        Stopped,
        Closed,
        Menu
    }

    class DShowPlayer : Form, IDisposable
    {
        #region Plugin Variables
        private Thread ScanBluRayThread = null;
        private Thread DurationThread;
        private Thread SeekThread;
        private bool DurationActive = false;
        private bool SeekActive = false;
        private bool ShowTime = false;


        public bool CommandFromTaskBar = false;
        private double CurrentTimePosition = 0;
        private double TotalTime = 0;


        private int ScreenWidth = 0;
        private int ScreenHeight = 0;
        private double ScaleRatio = 0;

        private int OSDPosy = 0;
        IntPtr MeediosWindowHandle;
        private int posyTB = 0;
        public BaseItemDto Item;
        private MovieInfo movieInfo = new MovieInfo();

        private int TotalNumberOfChapters = 0;

        private bool DiscIsDVD = false;
        private bool DiscIsBluRay = false;
        public bool HasVideo = false;
        public Uri m_sourceUri;
        private string DriveLetter;

        #region TransportBar
        private IntPtr iPlayFocusTransportBar;
        private IntPtr iPauseFocusTransportBar;
        private IntPtr iRRFocusPlayTransportBar;
        private IntPtr iRRFocusPauseTransportBar;
        private IntPtr iFFFocusPlayTransportBar;
        private IntPtr iFFFocusPauseTransportBar;
        private IntPtr iNextFocusPlayTransportBar;
        private IntPtr iNextFocusPauseTransportBar;
        private IntPtr iPreviousFocusPlayTransportBar;
        private IntPtr iPreviousFocusPauseTransportBar;
        private IntPtr iNextFocusChapterTransportBar;
        private IntPtr iPreviousFocusChapterTransportBar;
        private IntPtr iDiscMenuFocusPlayTransportBar;
        private IntPtr iDiscMenuFocusPauseTransportBar;
        private IntPtr iSettingsMenuFocusPlayTransportBar;
        private IntPtr iSettingsMenuFocusPauseTransportBar;
        private IntPtr iSettingsMenuFocusTransportBar;
        private IntPtr iText;
        private IntPtr iMovieInfoImage;

        private int iBallHeight;
        private int iBallWidth;

        private bool iSettingsMenuFocusTransportBarIsShowing;
        private bool iPlayFocusTransportBarIsShowing = false;
        private bool iPauseFocusTransportBarIsShowing = false;
        private bool iRRFocusPlayTransportBarIsShowing = false;
        private bool iRRFocusPauseTransportBarIsShowing = false;
        private bool iFFFocusPlayTransportBarIsShowing = false;
        private bool iFFFocusPauseTransportBarIsShowing = false;
        private bool iNextFocusPlayTransportBarIsShowing = false;
        private bool iNextFocusPauseTransportBarIsShowing = false;
        private bool iPreviousFocusPlayTransportBarIsShowing = false;
        private bool iPreviousFocusPauseTransportBarIsShowing = false;
        private bool iNextFocusChapterTransportBarIsShowing = false;
        private bool iPreviousFocusChapterTransportBarIsShowing = false;
        private bool iDiscMenuFocusPlayTransportBarIsShowing = false;
        private bool iDiscMenuFocusPauseTransportBarIsShowing = false;
        private bool iSettingsMenuFocusPlayTransportBarIsShowing = false;
        private bool iSettingsMenuFocusPauseTransportBarIsShowing = false;
        private bool iMovieInfoImageIsShowing = false;
        private bool iSeekIsShowing = false;
        #endregion

        #region Menu Images
        private Image PlaylistBackgroundImage;
        private IntPtr Playlists;
        private bool PlaylistsIsShowing;
        private int PlaylistHeight;
        private int PlaylistWidth;
        private int PlaylistStart = -1;
        private int SelectedPlaylistNumber = -1;

        //Main Menu
        private IntPtr PlayerMenu;
        private bool PlayerMenuIsShowing;
        private int PlayerMenuStart = -1;
        private int SelectedPlayerMenuNumber = -1;
        private string SelectedMenuText = "";

        //Audio Menu
        private IntPtr AudioMenu;
        private bool AudioMenuIsShowing;
        private int AudioMenuStart = -1;
        private int SelectedAudioMenuNumber = -1;
        private string SelectedAudioMenuText = "";
        private int SelectedAudioTrackNumber = 0;

        //Subtitle Menu
        private IntPtr SubtitleMenu;
        private bool SubtitleMenuIsShowing;
        private int SubtitleMenuStart = -1;
        private int SelectedSubtitleMenuNumber = -1;
        private string SelectedSubtitleMenuText = "";
        private int SelectedSubtitleTrackNumber = 0;

        //Chapter Menu
        private IntPtr ChapterMenu;
        private bool ChapterMenuIsShowing;
        private int ChapterMenuStart = -1;
        private int SelectedChapterMenuNumber = -1;
        private string SelectedChapterMenuText = "";
        private int SelectedChapterTrackNumber = 0;

        #endregion

        #region Transport Icons
        private IntPtr iPlayIcon;
        private IntPtr iPauseIcon;
        private IntPtr iRewindIcon;
        private IntPtr iFastforwardIcon;
        private IntPtr iNextIcon;
        private IntPtr iPreviousIcon;
        private IntPtr Time;

        private Image FastForwardImage;
        private Image RewindImage;
        private Image TransportBallImage;

        private int ImagePriority = 5;
        private int TimePriority = 7;
        private int InfoBarTextPriority = 6;
        private int IconTextPriority = 10;
        private int TimeTextHeight = 0;

        #endregion

        #endregion

        #region DirectShow Variables

        public PlaybackState m_state = PlaybackState.Closed;
        private MadVR madvr = null;
        private LAVAudio lavaudio = null;
        private LAVVideo lavvideo = null;
        private DefaultAudioRenderer defaultAudioRenderer = null;
        private ReclockAudioRenderer reclockAudioRenderer = null;
        private DirectShowLib.IVideoWindow videoWindow;
        private DirectShowLib.IBasicVideo basicVideo = null;
        private IBasicVideo2 basicVideo2 = null;
        private DirectShowLib.IBaseFilter sourceFilter = null;
        private DirectShowLib.IBaseFilter dvdNav;
        private IAMExtendedSeeking eSeek = null;
        private DirectShowLib.IGraphBuilder m_graph = null;
        private DirectShowLib.IFilterGraph2 filterGraph = null;
        private DirectShowLib.IMediaControl m_pControl = null;
        private DirectShowLib.IMediaSeeking m_pSeek = null;
        private AMSeekingSeekingCapabilities m_seekCaps;
        private DirectShowLib.IMediaPosition mediaPosition = null;
        private DirectShowLib.IMediaEventEx mediaEventEx = null;
        private const int WMGraphNotify = 0x0400 + 13;
        private Guid subtitleSelector, audioSelector;
        protected Dictionary<int, string> audioTracks = new Dictionary<int, string>();
        protected Dictionary<int, string> subtitles = new Dictionary<int, string>();
        public Dictionary<int, string> Subtitles { get { return subtitles; } }
        public Dictionary<int, string> AudioTracks { get { return audioTracks; } }
        protected int subtitle;
        protected int audioTrack;

        private void HandleGraphEvent()
        {
            int hr = 0;
            EventCode evCode;
            IntPtr evParam1, evParam2;

            // Make sure that we don't access the media event interface
            // after it has already been released.
            if (mediaEventEx == null)
                return;

            // Process all queued events
            while (mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
            {
                // Free memory associated with callback, since we're not using it
                hr = mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

                // If this is the end of the clip, close
                if (evCode == EventCode.Complete)
                {

                    CloseMe();
                    //DsLong pos = new DsLong(0);
                    //// Reset to first frame of movie
                    //hr = this.m_pSeek.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning,
                    //  null, AMSeekingSeekingFlags.NoPositioning);
                    //if (hr < 0)
                    //{
                    //    // Some custom filters (like the Windows CE MIDI filter)
                    //    // may not implement seeking interfaces (IMediaSeeking)
                    //    // to allow seeking to the start.  In that case, just stop
                    //    // and restart for the same effect.  This should not be
                    //    // necessary in most cases.
                    //    hr = this.m_pControl.Stop();
                    //    hr = this.m_pControl.Run();
                    //}
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //case WMGraphNotify:
                //    {
                //        HandleGraphEvent();
                //        break;
                //    }
                case WM_DVD_EVENT:
                    {
                        IntPtr p1, p2;
                        int hr = 0;
                        EventCode code;
                        do
                        {
                            hr = mediaEventEx.GetEvent(out code, out p1, out p2, 0);
                            if (code != 0 && code != EventCode.DvdCurrentHmsfTime)
                            {
                            }
                            if (hr < 0)
                            {
                                break;
                            }

                            switch (code)
                            {
                                case EventCode.Complete:
                                    break;
                                case EventCode.Paused:
                                    m_state = PlaybackState.Paused;
                                    break;
                                case EventCode.DvdDomainChange:
                                    if (BeforeSendAsync != null)
                                        BeforeSendAsync(this, null);
                                    domain = (DvdDomain)p1;
                                    switch (domain)
                                    {
                                        case DvdDomain.FirstPlay:
                                            m_state = PlaybackState.Running;
                                            menuMode = MenuMode.No;

                                            break;
                                        case DvdDomain.VideoManagerMenu:
                                        case DvdDomain.VideoTitleSetMenu:
                                            m_state = PlaybackState.Menu;
                                            Subtitles.Clear();
                                            AudioTracks.Clear();
                                            //UpdateTrackAmount();
                                            break;
                                        case DvdDomain.Title:
                                            m_state = PlaybackState.Running;
                                            m_dvdInfo.GetCurrentDomain(out domain);
                                            menuMode = MenuMode.No;
                                            if (domain == DvdDomain.Title)
                                                UpdateTitleInfo(CurrentTitle);
                                            break;
                                        case DvdDomain.Stop:
                                            m_state = PlaybackState.Paused;
                                            break;
                                    }
                                    if (AfterSendAsync != null)
                                        AfterSendAsync(this, null);
                                    break;
                                case EventCode.DvdTitleChange:
                                    CurrentTitle = p1.ToInt32();
                                    if (BeforeSendAsync != null)
                                        BeforeSendAsync(this, null);
                                    if (domain == DvdDomain.Title)
                                    {
                                        audioTrack = subtitle = -1;
                                        UpdateTitleInfo(CurrentTitle);
                                    }
                                    if (AfterSendAsync != null)
                                        AfterSendAsync(this, null);
                                    if (TrackChanged != null)
                                        TrackChanged(this, EventArgs.Empty);
                                    break;
                                case EventCode.DvdChapterStart:
                                    CurrentChapter = p1.ToInt32();
                                    m_state = PlaybackState.Running;
                                    menuMode = MenuMode.No;
                                    if (TrackChanged != null)
                                        TrackChanged(this, EventArgs.Empty);
                                    break;
                                case EventCode.DvdAudioStreamChange:
                                    audioTrack = p1.ToInt32();
                                    if (TrackChanged != null)
                                        TrackChanged(this, EventArgs.Empty);
                                    break;
                                case EventCode.DvdSubPicictureStreamChange:
                                    if (m_state == PlaybackState.Menu)
                                    {
                                        menuMode = MenuMode.Buttons;
                                        break;
                                    }
                                    if (!subtitlesDisabled)
                                        subtitle = p1.ToInt32();
                                    else
                                        subtitle = -1;

                                    if (TrackChanged != null)
                                        TrackChanged(this, EventArgs.Empty);
                                    break;
                                case EventCode.DvdAngleChange:
                                    Angles = p1.ToInt32();
                                    CurrentAngle = p2.ToInt32();
                                    if (TrackChanged != null)
                                        TrackChanged(this, EventArgs.Empty);
                                    break;
                                case EventCode.DvdButtonChange:
                                    buttons = p1.ToInt32();
                                    selectedButton = p2.ToInt32();
                                    break;
                                case EventCode.DvdValidUopsChange:
                                    break;
                                case EventCode.DvdStillOn:
                                    break;
                                case EventCode.DvdStillOff:
                                    break;
                                case EventCode.DvdCurrentTime:
                                    break;
                                case EventCode.DvdError:
                                    if (DiskError != null)
                                        DiskError(this, null);
                                    break;
                                case EventCode.DvdWarning:
                                    break;
                                case EventCode.DvdChapterAutoStop:
                                    break;
                                case EventCode.DvdNoFpPgc:
                                    break;
                                case EventCode.DvdPlaybackRateChange:
                                    break;
                                case EventCode.DvdParentalLevelChange:
                                    break;
                                case EventCode.DvdPlaybackStopped:
                                    break;
                                case EventCode.DvdAnglesAvailable:
                                    if (BeforeSendAsync != null)
                                        BeforeSendAsync(this, null);
                                    if (p1.ToInt32() == 1)
                                    {
                                        int angles, currentAngle;
                                        m_dvdInfo.GetCurrentAngle(out angles, out currentAngle);
                                        Angles = angles;
                                        CurrentAngle = currentAngle;
                                    }
                                    else
                                    {
                                        CurrentAngle = Angles = 1;
                                    }
                                    if (AfterSendAsync != null)
                                        AfterSendAsync(this, null);
                                    break;
                                case EventCode.DvdPlayPeriodAutoStop:
                                    break;
                                case EventCode.DvdButtonAutoActivated:
                                    break;
                                case EventCode.DvdCmdStart:
                                    break;
                                case EventCode.DvdCmdEnd:
                                    IDvdCmd dvdCommand;
                                    if (BeforeSendAsync != null)
                                        BeforeSendAsync(this, null);
                                    hr = m_dvdInfo.GetCmdFromEvent(p1, out dvdCommand);
                                    for (int i = 0; i < commands.Count; i++)
                                    {
                                        if (commands[i].Command == dvdCommand)
                                        {
                                            // Handle the command (i need to save more info about the command, so that i can figure out what the command was for)
                                            switch (commands[i].Type)
                                            {
                                                case Command.RootMenu:
                                                    m_state = PlaybackState.Menu;
                                                    break;
                                                case Command.Seek:
                                                case Command.ChangeChapter:
                                                    DvdPlaybackLocation2 location;
                                                    hr = m_dvdInfo.GetCurrentLocation(out location);
                                                    DsError.ThrowExceptionForHR(hr);

                                                    CurrentChapter = location.ChapterNum;
                                                    CurrentTitle = location.TitleNum;
                                                    position = new TimeSpan(location.TimeCode.bHours, location.TimeCode.bMinutes, location.TimeCode.bSeconds);

                                                    m_state = PlaybackState.Running;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            Marshal.ReleaseComObject(commands[i].Command);
                                            commands.RemoveAt(i--);
                                        }
                                    }
                                    Marshal.ReleaseComObject(dvdCommand);
                                    if (AfterSendAsync != null)
                                        AfterSendAsync(this, null);
                                    break;
                                case EventCode.DvdDiscEjected:
                                    if (DiskEjected != null)
                                        DiskEjected(this, null);
                                    break;
                                case EventCode.DvdDiscInserted:
                                    break;
                                case EventCode.DvdCurrentHmsfTime:
                                    byte[] ati = BitConverter.GetBytes(p1.ToInt32());
                                    position = new TimeSpan(ati[0], ati[1], ati[2]);

                                    break;
                                case EventCode.DvdKaraokeMode:
                                    break;
                            }

                            hr = mediaEventEx.FreeEventParams(code, p1, p2);
                        }
                        while (hr == 0);
                        break;
                    }

            }

            // Pass this message to the video window for notification of system changes
            if (videoWindow != null)
                videoWindow.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);

            base.WndProc(ref m);
        }

        private double currentPlaybackRate = 1.0;
        private double SeekSpeed = 1.0;
        private IntPtr MB3PlayingWindowHandle;
        #endregion

        #region Taskbar

        public PlaybackState State()
        {
            return m_state;
        }

        private void Play()
        {
            Play(MB3PlayingWindowHandle);
        }

        public void Play(IntPtr windowHandle)
        {
            MB3PlayingWindowHandle = windowHandle;
            currentPlaybackRate = 1;
            SeekSpeed = 1;
            if (HasVideo == false)
            {
                SetupGraph();
                m_state = PlaybackState.Stopped;
                //return;
            }

            if (m_state == PlaybackState.Paused || m_state == PlaybackState.Stopped || SeekActive == true)
            {
                if (SeekActive == true)
                {
                    SeekActive = false;
                }
                m_pControl.Run();
                m_state = PlaybackState.Running;
                if (iSeekIsShowing == true)
                {
                    MadvrInterface.ClearMadVrBitmap("FFIconText", madvr);
                    MadvrInterface.ClearMadVrBitmap("RRIconText", madvr);
                    iSeekIsShowing = false;
                }

                if (iPlayFocusTransportBarIsShowing == true
                 || iPauseFocusTransportBarIsShowing == true
                 || iRRFocusPlayTransportBarIsShowing == true
                 || iRRFocusPauseTransportBarIsShowing == true
                 || iFFFocusPlayTransportBarIsShowing == true
                 || iFFFocusPauseTransportBarIsShowing == true
                 || iNextFocusPlayTransportBarIsShowing == true
                 || iNextFocusPauseTransportBarIsShowing == true
                 || iPreviousFocusPlayTransportBarIsShowing == true
                 || iPreviousFocusPauseTransportBarIsShowing == true)
                {
                    //PlayToPauseTransportBar();
                    ClearTransportBar();
                    MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    iPauseFocusTransportBarIsShowing = true;
                }
                else
                {
                    MadvrInterface.ShowMadVrBitmap("PlayIcon", iPlayIcon, 0, 0, 3000, ImagePriority, madvr);
                }
            }
        }

        public void Pause()
        {
            if (m_state == PlaybackState.Closed)
            {
                m_state = PlaybackState.Stopped;
                Play();
            }
            else if (m_state == PlaybackState.Paused || SeekActive == true)
            {
                Play();
            }
            else
            {
                m_pControl.Pause();
                m_state = PlaybackState.Paused;

                currentPlaybackRate = 1;
                SeekSpeed = 1;
                if (iSeekIsShowing == true)
                {
                    MadvrInterface.ClearMadVrBitmap("FFIconText", madvr);
                    MadvrInterface.ClearMadVrBitmap("RRIconText", madvr);
                    iSeekIsShowing = false;
                }

                if (iPlayFocusTransportBarIsShowing == true
                 || iPauseFocusTransportBarIsShowing == true
                 || iRRFocusPlayTransportBarIsShowing == true
                 || iRRFocusPauseTransportBarIsShowing == true
                 || iFFFocusPlayTransportBarIsShowing == true
                 || iFFFocusPauseTransportBarIsShowing == true
                 || iNextFocusPlayTransportBarIsShowing == true
                 || iNextFocusPauseTransportBarIsShowing == true
                 || iPreviousFocusPlayTransportBarIsShowing == true
                 || iPreviousFocusPauseTransportBarIsShowing == true)
                {
                    //PauseToPlayTransportBar();
                    ClearTransportBar();
                    MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    iPlayFocusTransportBarIsShowing = true;
                }
                else
                {
                    MadvrInterface.ShowMadVrBitmap("Pause Icon", iPauseIcon, 0, 0, 3000, ImagePriority, madvr);
                }
            }
        }

        public void StopMedia()
        {
            currentPlaybackRate = 1;
            SeekSpeed = 1;
            if (SeekActive == true)
            {
                SeekThread.Join();
                SeekActive = false;
            }
            if (m_state == PlaybackState.Running || m_state == PlaybackState.Paused)
            {
                m_pControl.Stop();
                m_state = PlaybackState.Stopped;
            }
        }

        public void FastForward()
        {
            if (SeekActive == true)
            {
                SeekThread.Suspend();
            }
            if (currentPlaybackRate == 1)
            {
                SeekSpeed = 5;
            }
            else if (currentPlaybackRate == 5)
            {
                SeekSpeed = 10;
            }
            else if (currentPlaybackRate == 10)
            {
                SeekSpeed = 15;
            }
            else if (currentPlaybackRate > 0 && currentPlaybackRate < 240)
            {
                SeekSpeed = currentPlaybackRate * 2;
            }
            else
            {
                currentPlaybackRate = 1;
                SeekSpeed = 1;
                Play();
                return;
            }


            Font font = new Font("Segoe Condensed", 12);
            CreateTextonIcon(SeekSpeed.ToString(), FastForwardImage, font, Color.Black, Color.Transparent);
            font.Dispose();
            MadvrInterface.ClearMadVrBitmap("FFIconText", madvr);
            MadvrInterface.ClearMadVrBitmap("RRIconText", madvr);
            MadvrInterface.ShowMadVrBitmap("FFIconText", iFastforwardIcon, 0, 0, 0, IconTextPriority, madvr);
            iSeekIsShowing = true;
            currentPlaybackRate = SeekSpeed;

            if (iPlayFocusTransportBarIsShowing == true
             || iPauseFocusTransportBarIsShowing == true
             || iRRFocusPlayTransportBarIsShowing == true
             || iRRFocusPauseTransportBarIsShowing == true
             || iFFFocusPlayTransportBarIsShowing == true
             || iFFFocusPauseTransportBarIsShowing == true
             || iNextFocusPlayTransportBarIsShowing == true
             || iNextFocusPauseTransportBarIsShowing == true
             || iPreviousFocusPlayTransportBarIsShowing == true
             || iPreviousFocusPauseTransportBarIsShowing == true)
            {
                //m_state = PlaybackState.Paused;
                PauseToPlayTransportBar();
            }

            if (SeekActive == true)
            {
                SeekThread.Resume();

            }
            if (SeekActive == false)
            {
                ThreadStart threadDelegate = SeekRelative;
                SeekThread = new Thread(threadDelegate);
                SeekThread.Start();
                SeekActive = true;
            }

            m_state = PlaybackState.Running;
        }

        public void Rewind()
        {
            if (SeekActive == true)
            {
                SeekThread.Suspend();
            }

            if (currentPlaybackRate == 1)
            {
                SeekSpeed = -5;
            }
            else if (currentPlaybackRate == -5)
            {
                SeekSpeed = -10;
            }
            else if (currentPlaybackRate == -10)
            {
                SeekSpeed = -15;
            }
            else if (currentPlaybackRate < 0 && currentPlaybackRate >= -240)
            {
                SeekSpeed = currentPlaybackRate * 2;
            }
            else if (SeekActive == true)
            {
                currentPlaybackRate = 1;
                SeekSpeed = 1;
                Play();
                return;
            }
            else
            {
                SeekSpeed = -15;
            }

            Font font = new Font("Segoe Condensed", 12);
            CreateTextonIcon(SeekSpeed.ToString(), RewindImage, font, Color.Black, Color.Transparent);
            font.Dispose();
            MadvrInterface.ClearMadVrBitmap("FFIconText", madvr);
            MadvrInterface.ClearMadVrBitmap("RRIconText", madvr);
            MadvrInterface.ShowMadVrBitmap("RRIconText", iFastforwardIcon, 0, 0, 0, IconTextPriority, madvr);
            iSeekIsShowing = true;
            currentPlaybackRate = SeekSpeed;

            if (iPlayFocusTransportBarIsShowing == true
             || iPauseFocusTransportBarIsShowing == true
             || iRRFocusPlayTransportBarIsShowing == true
             || iRRFocusPauseTransportBarIsShowing == true
             || iFFFocusPlayTransportBarIsShowing == true
             || iFFFocusPauseTransportBarIsShowing == true
             || iNextFocusPlayTransportBarIsShowing == true
             || iNextFocusPauseTransportBarIsShowing == true
             || iPreviousFocusPlayTransportBarIsShowing == true
             || iPreviousFocusPauseTransportBarIsShowing == true)
            {
                //m_state = PlaybackState.Paused;
                PauseToPlayTransportBar();
            }

            if (SeekActive == true)
            {
                SeekThread.Resume();
            }
            if (SeekActive == false)
            {
                ThreadStart threadDelegate = SeekRelative;
                SeekThread = new Thread(threadDelegate);
                SeekThread.Start();
                SeekActive = true;
            }

            m_state = PlaybackState.Running;
        }

        public void NextChapter()
        {
            currentPlaybackRate = 1;
            SeekSpeed = 1;
            if (iSeekIsShowing == true)
            {
                MadvrInterface.ClearMadVrBitmap("FFIconText", madvr);
                MadvrInterface.ClearMadVrBitmap("RRIconText", madvr);
                iSeekIsShowing = false;
            }
            if (SeekActive == true)
                SeekActive = false;

            GoToNextChapter(true);
        }

        public void PreviousChapter()
        {
            currentPlaybackRate = 1;
            SeekSpeed = 1;
            if (iSeekIsShowing == true)
            {
                MadvrInterface.ClearMadVrBitmap("FFIconText", madvr);
                MadvrInterface.ClearMadVrBitmap("RRIconText", madvr);
                iSeekIsShowing = false;
            }
            if (SeekActive == true)
                SeekActive = false;
            GoToPreviousChapter(true);
        }

        private struct ShowtimeObjectArguments
        {
            public MadVR madvr;
            public ShowtimeObjectArguments(MadVR p1)
            {
                madvr = p1;
            }
        }

        private void StartDurationThread()
        {
            ShowTime = true;
            if (DurationActive == false)
            {
                ShowtimeObjectArguments ShowtimeArguments = new ShowtimeObjectArguments(madvr);
                ParameterizedThreadStart paramthreadstart = ShowDuration;
                DurationThread = new Thread(paramthreadstart);
                DurationThread.Start(ShowtimeArguments);
                DurationActive = true;
            }
        }

        public void ShowDuration(Object obj)
        {
            ShowtimeObjectArguments arguments = (ShowtimeObjectArguments)obj;
            MadVR mvr = arguments.madvr;

            TimeSpan tPosition;
            TimeSpan tDuration;
            string cDuration = "";
            string cPosition = "";
            double position = 0;
            int bmpWidth = (int)(1920 * ScaleRatio);
            int bmpHeight = (int)(190 * ScaleRatio);

            while (ShowTime == true)
            {
                //if (m_state != PlaybackState.Paused)
                {
                    if (DiscIsDVD == true)
                    {
                        position = DvdCurrentTime();
                        TotalTime = DvdDuration();
                    }
                    else if (DiscIsDVD == false)
                    {
                        if (CurrentTimePosition == 0)
                        {
                            position = CurrentPosition();
                            TotalTime = Duration();
                        }
                        else
                        {
                            position = CurrentTimePosition;
                            TotalTime = Duration();
                        }
                    }

                    if (position != 0 && TotalTime != 0)
                    {
                        tPosition = TimeSpan.FromSeconds(position);
                        cPosition = tPosition.Hours + ":" + tPosition.Minutes.ToString().PadLeft(2, '0') + ":" + tPosition.Seconds.ToString().PadLeft(2, '0');

                        tDuration = TimeSpan.FromSeconds(TotalTime);
                        cDuration = tDuration.Hours + ":" + tDuration.Minutes.ToString().PadLeft(2, '0') + ":" + tDuration.Seconds.ToString().PadLeft(2, '0');

                        DrawTimeOnTransportBar(cPosition, cDuration, Color.White, Color.Transparent, bmpWidth, bmpHeight);
                        if (Time != IntPtr.Zero)
                        {
                            MadvrInterface.ShowMadVrBitmap("transportTime", Time, 0, posyTB, 0, TimePriority, mvr);
                            DeleteObject(Time);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }

            MadvrInterface.ClearMadVrBitmap("transportTime", mvr);
            DurationActive = false;
            // Release the current GDI bitmap
            DeleteObject(Time);
        }

        public double Duration()
        {
            if (m_state != PlaybackState.Closed)
            {
                double m_dDuration;
                mediaPosition.get_Duration(out m_dDuration);
                return m_dDuration;
            }
            return 0.0d;
        }

        public double CurrentPosition()
        {
            double dTime;
            mediaPosition.get_CurrentPosition(out dTime);
            return dTime;
        }

        #endregion
        
        public void CloseMe()
        {
            if (videoWindow != null)
            {
                StopMedia();

                videoWindow.put_Visible(OABool.False);
                videoWindow.put_Owner(IntPtr.Zero);
                videoWindow.SetWindowPosition(0, 0, 0, 0);
                videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.Visible | WindowStyle.ClipSiblings);
                Dispose();
            }
        }

        public double PercentComplete()
        {
            double pcomplete = 0;
            if (DiscIsDVD == false)
            {
                pcomplete = (CurrentPosition() / Duration()) * 100;
                pcomplete = Math.Round(pcomplete, 0);
            }
            else if (DiscIsDVD == true)
            {
                pcomplete = (DvdCurrentTime() / DvdDuration()) * 100;
                pcomplete = Math.Round(pcomplete, 0);
            }
            return pcomplete;
        }


        #region DVD Code

        //DVD interfaces
        private IDvdControl2 m_dvdControl;
        private IDvdInfo2 m_dvdInfo;
        private IDvdCmd m_dvdCmd;
        private DvdMenuAttributes dma;
        private DvdTitleAttributes dta = null;
        private DvdHMSFTimeCode _DVDCurrentPlaybackTime;
        private MenuMode menuMode = MenuMode.No;
        private Thread DvdDomainThread;
        private bool DvdDomainThreadActive = false;

        public const int E_DVDOperationInhibited = unchecked((int)0x80040276);
        private const int WM_DVD_EVENT = 0x00008002;

        public static string BookMarkFile { get; set; }
        private List<DvdCommand> commands = new List<DvdCommand>();
        private bool subtitlesDisabled = false;
        private int selectedButton, buttons;
        private DvdDomain domain;
        private TimeSpan duration;
        private TimeSpan position;

        public int CurrentTitle { get; set; }
        public int CurrentChapter { get; set; }
        public int Titles { get; set; }
        public int Chapters { get; set; }
        public int CurrentAngle { get; set; }
        public int Angles { get; set; }
        public event EventHandler TitleChanged;
        public event EventHandler TrackChanged;
        public event EventHandler BeforeSendAsync;
        public event EventHandler AfterSendAsync;
        public event EventHandler DiskError;
        public event EventHandler DiskEjected;


        private readonly Guid DVD_SUBPICTURE_TYPE = new Guid("{E06D802D-DB46-11CF-B4D1-00805F6CBBEA}");

        private static TimeSpan ToTimeSpan(double timeStamp)
        {
            return new TimeSpan(0, 0, 0, 0, (int)(timeStamp * 1000.0f));
        }

        private static double ToDouble(DvdHMSFTimeCode timeCode)
        {
            Double result = timeCode.bHours * 3600d;
            result += (timeCode.bMinutes * 60d);
            result += timeCode.bSeconds;
            return result;
        }

        private static DvdHMSFTimeCode ToTimeCode(TimeSpan newTime)
        {
            int hours = newTime.Hours;
            int minutes = newTime.Minutes;
            int seconds = newTime.Seconds;
            DvdHMSFTimeCode timeCode = new DvdHMSFTimeCode
            {
                bHours = (byte)(hours & 0xff),
                bMinutes = (byte)(minutes & 0xff),
                bSeconds = (byte)(seconds & 0xff),
                bFrames = 0
            };
            return timeCode;
        }

        private void DVDSeek()
        {
            m_pControl.Pause();
            DvdPlaybackLocation2 loc;
            m_dvdInfo.GetCurrentLocation(out loc);
            _DVDCurrentPlaybackTime = loc.TimeCode;
            Double DoubleDvdTimespan = ToDouble(_DVDCurrentPlaybackTime);
            TimeSpan DvdTimespan = ToTimeSpan(DoubleDvdTimespan);
            double totalSeconds = DvdTimespan.TotalSeconds;
            totalSeconds = totalSeconds + (SeekSpeed * .5);

            TimeSpan NewDVDTimespan = new TimeSpan(0, 0, Convert.ToInt16(totalSeconds));
            DvdHMSFTimeCode newDVDTimeCode = ToTimeCode(NewDVDTimespan);
            m_dvdControl.PlayAtTime(newDVDTimeCode, DvdCmdFlags.None, out m_dvdCmd);
            m_pControl.Run();
        }

        private double DvdCurrentTime()
        {
            DvdPlaybackLocation2 loc;
            m_dvdInfo.GetCurrentLocation(out loc);
            _DVDCurrentPlaybackTime = loc.TimeCode;
            Double DoubleDvdTimespan = ToDouble(_DVDCurrentPlaybackTime);
            return DoubleDvdTimespan;
        }

        private double DvdDuration()
        {
            DvdHMSFTimeCode totaltime = new DvdHMSFTimeCode();
            DvdTimeCodeFlags ulTimeCodeFlags;
            m_dvdInfo.GetTotalTitleTime(totaltime, out ulTimeCodeFlags);
            double _duration = new TimeSpan(totaltime.bHours, totaltime.bMinutes, totaltime.bSeconds).TotalSeconds;
            return _duration;
        }

        private DvdPlaybackLocation2 DvdCurrentLocation()
        {
            DvdPlaybackLocation2 dpLocation = new DvdPlaybackLocation2();
            m_dvdInfo.GetCurrentLocation(out dpLocation);
            return dpLocation;
        }

        private int DvdCurrentChapter()
        {
            DvdPlaybackLocation2 dpLocation = DvdCurrentLocation();
            return dpLocation.ChapterNum;
        }

        private int DvdTotalChapters()
        {
            int TotalChapters = 0;
            DvdPlaybackLocation2 dpLocation = DvdCurrentLocation();
            int CurrentTitle = dpLocation.TitleNum;
            m_dvdInfo.GetNumberOfChapters(CurrentTitle, out TotalChapters);
            return TotalChapters;
        }

        #region DVD Menu Navigation

        /// <summary>
        /// This function should be called when ever the mouse is moved
        /// so that the DVD graph can navigate the DVD menu
        /// </summary>
        /// <param name="x">X coordinate of the mouse</param>
        /// <param name="y">Y coordinate of the mouse</param>
        public void MouseMove(int x, int y)
        {
            if (m_dvdControl != null && menuMode == MenuMode.Buttons)
                m_dvdControl.SelectAtPosition(new Point(x, y));
        }

        /// <summary>
        /// This function should be called when the left mouse button is clicked to 
        /// allow DVD navigation
        /// </summary>
        /// <param name="x">X Coordinate of the mouse</param>
        /// <param name="y">Y coordinate of the mouse</param>
        public void MouseDown(int x, int y)
        {
            if (m_dvdControl != null && menuMode == MenuMode.Buttons)
                m_dvdControl.ActivateAtPosition(new Point(x, y));
        }

        /// <summary>
        /// Should be called if eith left, right, up or down is pressed to
        /// allow DVD navigation
        /// </summary>
        /// <param name="relativeButton">The relative direction to move</param>
        public void SelectButton(DvdRelativeButton relativeButton)
        {
            if (m_dvdControl != null && menuMode == MenuMode.Buttons)
            {
                m_dvdControl.SelectRelativeButton(relativeButton);
            }
        }

        /// <summary>
        /// Should be called to activate the selected button
        /// </summary>
        public void ActivateButton()
        {
            if (m_dvdControl != null)
            {
                if (menuMode == MenuMode.Buttons)
                {
                    m_dvdControl.ActivateButton();
                }
                else
                {
                    m_dvdControl.StillOff();
                }
            }
        }

        /// <summary>
        /// Shows the DVD menu
        /// </summary>
        public void ShowMenu(DvdMenuId menu)
        {
            if (DiscIsDVD == true)
            {
                if (m_dvdControl != null)
                {
                    int hr = m_dvdControl.ShowMenu(menu, DvdCmdFlags.Block, out m_dvdCmd);
                    DsError.ThrowExceptionForHR(hr);
                    if (hr == 0)
                    {
                        StartDvdDomainThread();
                        menuMode = MenuMode.Buttons;
                    }
                }
            }
        }

        /// <summary>
        /// Shows the DVD root Menu
        /// </summary>
        public void ShowRootMenu()
        {
            ShowMenu(DvdMenuId.Root);
        }

        #endregion

        private class DvdCommand
        {
            internal IDvdCmd Command { get; set; }
            internal Command Type { get; set; }
        }

        private enum Command { RootMenu, Seek, ChangeChapter, TrackChange }

        private void UpdateTitleInfo(int title)
        {
            int hr;
            DvdTitleAttributes attributes = new DvdTitleAttributes();
            DvdMenuAttributes menuAttributes;
            DvdHMSFTimeCode time = new DvdHMSFTimeCode();
            DvdTimeCodeFlags codeflags;
            hr = m_dvdInfo.GetTotalTitleTime(time, out codeflags);
            DsError.ThrowExceptionForHR(hr);

            duration = new TimeSpan(time.bHours, time.bMinutes, time.bSeconds);

            hr = m_dvdInfo.GetTitleAttributes(title, out menuAttributes, attributes);
            DsError.ThrowExceptionForHR(hr);
            int chapters;
            hr = m_dvdInfo.GetNumberOfChapters(title, out chapters);
            DsError.ThrowExceptionForHR(hr);
            Chapters = chapters;

            audioTracks.Clear();
            subtitles.Clear();

            for (int i = 0; i < attributes.ulNumberOfAudioStreams; i++)
            {
                bool enabled;
                m_dvdInfo.IsAudioStreamEnabled(i, out enabled);
                if (!enabled)
                    continue;
                string audioTrackName;
                try
                {
                    CultureInfo language = new CultureInfo(attributes.AudioAttributes[i].Language);
                    audioTrackName = Regex.Replace(language.EnglishName, @"\(.*\)", "") + "(" +
                                     attributes.AudioAttributes[i].bNumberOfChannels + "ch)";
                }
                catch
                {
                    audioTrackName = "Unknown " + "(" +
                                     attributes.AudioAttributes[i].bNumberOfChannels + "ch)";
                }
                if (attributes.AudioAttributes[i].LanguageExtension == DvdAudioLangExt.DirectorComments1)
                {
                    audioTrackName += " Director's Commentary";
                }
                else if (attributes.AudioAttributes[i].LanguageExtension == DvdAudioLangExt.DirectorComments2)
                {
                    audioTrackName += " Director's Commentary 2";
                }
                audioTracks.Add(i, audioTrackName);

            }

            subtitles.Add(-1, "Subtitles off");
            for (int i = 0; i < attributes.ulNumberOfSubpictureStreams; i++)
            {
                bool enabled;
                m_dvdInfo.IsSubpictureStreamEnabled(i, out enabled);
                if (!enabled)
                    continue;

                string subtitleName;
                try
                {
                    CultureInfo language = new CultureInfo(attributes.SubpictureAttributes[i].Language);
                    subtitleName = Regex.Replace(language.EnglishName, @"\(.*\)", "");
                }
                catch
                {
                    subtitleName = "Unknown";
                }
                subtitles.Add(i, subtitleName);
            }
            int subTitleStreams;
            m_dvdInfo.GetCurrentSubpicture(out subTitleStreams, out subtitle, out subtitlesDisabled);
            if (subtitlesDisabled)
                subtitle = -1;

            if (TitleChanged != null)
            {
                TitleChanged(this, null);
            }

            //UpdateTrackAmount();
        }

        private void StartDvdDomainThread()
        {
            if (DiscIsDVD == true)
            {
                if (DvdDomainThreadActive == false)
                {
                    DvdDomainThreadActive = true;
                    ThreadStart threadDelegate = GetCurrentDvdDomain;
                    DvdDomainThread = new Thread(threadDelegate);
                    DvdDomainThread.Start();
                }
            }
        }

        private void GetCurrentDvdDomain()
        {
            int i = 0;
            while (DvdDomainThreadActive == true)
            {
                m_dvdInfo.GetCurrentDomain(out domain);
                //Madvr_VideoPlayer.Plugin.WriteLog("DVD Domain: " + domain);
                if (domain == DvdDomain.Title)
                {
                    i++;
                    m_state = PlaybackState.Running;
                    menuMode = MenuMode.No;
                    if (i > 120 * 5)
                    {
                        DvdDomainThreadActive = false;
                        break;
                    }
                    //UpdateTitleInfo(CurrentTitle);
                }
                else if (domain == DvdDomain.FirstPlay)
                {
                    i++;
                    m_state = PlaybackState.Running;
                    menuMode = MenuMode.No;
                    if (i > 120 * 5)
                    {
                        DvdDomainThreadActive = false;
                        break;
                    }
                    //UpdateTitleInfo(CurrentTitle);
                }
                else if (domain == DvdDomain.Stop)
                {
                    i++;
                    m_state = PlaybackState.Paused;
                    menuMode = MenuMode.No;
                    if (i > 120 * 5)
                    {
                        DvdDomainThreadActive = false;
                        break;
                    }
                    //UpdateTitleInfo(CurrentTitle);
                }
                else if (domain == DvdDomain.VideoManagerMenu)
                {
                    i = 0;
                    m_state = PlaybackState.Menu;
                    menuMode = MenuMode.Buttons;
                    //UpdateTitleInfo(CurrentTitle);
                }
                else if (domain == DvdDomain.VideoTitleSetMenu)
                {
                    i = 0;
                    m_state = PlaybackState.Menu;
                    menuMode = MenuMode.Buttons;
                    //UpdateTitleInfo(CurrentTitle);
                }
                Thread.Sleep(500);
            }
        }

        #endregion

        #region Seeking Code
        public bool CanSeek()
        {
            const AMSeekingSeekingCapabilities caps =
                AMSeekingSeekingCapabilities.CanSeekAbsolute |
                AMSeekingSeekingCapabilities.CanGetDuration;

            return ((m_seekCaps & caps) == caps);
        }

        public void GetStopTime(out long pDuration)
        {
            if (m_pSeek == null)
            {
                throw new COMException("No seek pointer");
            }

            int hr = m_pSeek.GetStopPosition(out pDuration);
            
            // If we cannot get the stop time, try to get the duration.
            //if (Failed(hr))
            //{
            //    hr = m_pSeek.GetDuration(out pDuration);
            //}
            DsError.ThrowExceptionForHR(hr);
        }

        public void GetCurrentPosition(out long pTimeNow)
        {
            if (m_pSeek == null)
            {
                throw new COMException("No seek pointer");
            }

            int hr = m_pSeek.GetCurrentPosition(out pTimeNow);
            DsError.ThrowExceptionForHR(hr);
        }

        //Used for Fastforward and Rewind
        private void SeekRelative()
        {
            if (m_state != PlaybackState.Closed)
            {
                if (m_pControl != null && mediaPosition != null)
                {
                    CurrentTimePosition = 0;
                    double dTime = 0;

                    if (DiscIsDVD == false)
                    {
                        mediaPosition.get_CurrentPosition(out CurrentTimePosition);
                        dTime = CurrentTimePosition + (SeekSpeed * .5);
                    }
                    m_pControl.Pause();
                    while (SeekSpeed != 1)
                    {
                        if (DiscIsDVD == false)
                        {
                            //double dCurTime = 0;
                            //double dTime;
                            //mediaPosition.get_CurrentPosition(out dCurTime);

                            dTime = dTime + (SeekSpeed * .5);
                            CurrentTimePosition = dTime;

                            if (dTime < 0.0d)
                            {
                                dTime = 0.0d;
                                Play();
                                return;
                            }
                            if (dTime < Duration())
                            {
                                //m_pControl.Pause();
                                mediaPosition.put_CurrentPosition(dTime);
                                //m_pControl.Run();
                            }
                        }
                        else if (DiscIsDVD == true)
                        {
                            DVDSeek();
                        }
                        Thread.Sleep(500);
                    }
                    CurrentTimePosition = 0;
                    m_pControl.Run();
                }
            }
        }

        //Use for "Go To" code
        private void SeekAbsolute(double dTime)
        {
            if (m_state != PlaybackState.Closed)
            {
                if (m_pControl != null && mediaPosition != null)
                {
                    if (dTime < 0.0d)
                    {
                        dTime = 0.0d;
                    }
                    if (dTime < Duration())
                    {
                        if (m_state != PlaybackState.Paused)
                        {
                            m_pControl.Pause();
                            mediaPosition.put_CurrentPosition(dTime);
                            m_pControl.Run();
                        }
                        else
                        {
                            mediaPosition.put_CurrentPosition(dTime);
                        }
                    }
                }
            }
        }

        private void SeekRelativePercentage(int iPercentage)
        {
            if (m_state != PlaybackState.Closed)
            {
                if (m_pControl != null && mediaPosition != null)
                {
                    double dCurrentPos;
                    mediaPosition.get_CurrentPosition(out dCurrentPos);
                    double dDuration = Duration();

                    double fCurPercent = (dCurrentPos / Duration()) * 100.0d;
                    double fOnePercent = Duration() / 100.0d;
                    fCurPercent = fCurPercent + (double)iPercentage;
                    fCurPercent *= fOnePercent;
                    if (fCurPercent < 0.0d)
                    {
                        fCurPercent = 0.0d;
                    }
                    if (fCurPercent < Duration())
                    {
                        mediaPosition.put_CurrentPosition(fCurPercent);
                    }
                }
            }
        }

        private void SeekAsolutePercentage(int iPercentage)
        {
            if (m_state != PlaybackState.Closed)
            {
                if (m_pControl != null && mediaPosition != null)
                {
                    if (iPercentage < 0)
                    {
                        iPercentage = 0;
                    }
                    if (iPercentage >= 100)
                    {
                        iPercentage = 100;
                    }
                    double fPercent = Duration() / 100.0f;
                    fPercent *= (double)iPercentage;
                    mediaPosition.put_CurrentPosition(fPercent);
                }
            }
        }

        private int GetCurrentChapter()
        {
            if (eSeek == null)
            {
                eSeek = (IAMExtendedSeeking)sourceFilter;
            }
            int CurrentChapter;
            eSeek.get_CurrentMarker(out CurrentChapter);
            return CurrentChapter;
        }

        private int GetTotalChapters()
        {
            return 1;
            if (DiscIsDVD == false)
            {
                if (eSeek == null)
                {
                    eSeek = (IAMExtendedSeeking)sourceFilter;
                }
                eSeek.get_MarkerCount(out TotalNumberOfChapters);
            }
            else if (DiscIsDVD == true)
            {
                TotalNumberOfChapters = DvdTotalChapters();
            }
            return TotalNumberOfChapters;
        }

        private void GoToNextChapter(bool GoToChapter)
        {
            StartDurationThread();

            if (DiscIsDVD == true)
            {
                int CurrentChapter = DvdCurrentChapter();
                int TotalNumberOfChapters = DvdTotalChapters();
                if ((CurrentChapter < TotalNumberOfChapters && GoToChapter == true) || GoToChapter == false)
                {
                    int NextChapter = CurrentChapter + 1;
                    if (GoToChapter == false)
                    {
                        NextChapter = CurrentChapter;
                    }

                    string Text = "Chapter " + NextChapter + " of " + TotalNumberOfChapters;
                    DrawInfoBarText(Text, Color.White, Color.Transparent);
                    iNextFocusChapterTransportBarIsShowing = true;

                    MadvrInterface.ShowMadVrBitmap("iNextFocusChapterTransportBar", iNextFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);

                    if (GoToChapter == true)
                    {
                        m_dvdControl.PlayNextChapter(DvdCmdFlags.None, out m_dvdCmd);
                    }

                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    StartTimer(5000);
                }
            }
            else if (DiscIsDVD == false)
            {
                if (eSeek == null)
                {
                    eSeek = (IAMExtendedSeeking)sourceFilter;
                }
                else
                {
                    int CurrentChapter;
                    eSeek.get_CurrentMarker(out CurrentChapter);
                    if (CurrentChapter < TotalNumberOfChapters || (GoToChapter == false))
                    {
                        int NextChapter = CurrentChapter + 1;
                        if (GoToChapter == false)
                        {
                            NextChapter = CurrentChapter;
                        }
                        double NextChapterTime;
                        eSeek.GetMarkerTime(NextChapter, out NextChapterTime);

                        string Text = "Chapter " + NextChapter + " of " + TotalNumberOfChapters;
                        DrawInfoBarText(Text, Color.White, Color.Transparent);
                        iNextFocusChapterTransportBarIsShowing = true;

                        MadvrInterface.ShowMadVrBitmap("iNextFocusChapterTransportBar", iNextFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                        MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);

                        if (GoToChapter == true)
                        {
                            SeekAbsolute(NextChapterTime);
                        }

                        if (timer != null)
                        {
                            timer.Stop();
                        }
                        StartTimer(5000);
                    }
                }
            }
            return;
        }

        private void GoToPreviousChapter(bool GoToChapter)
        {
            StartDurationThread();

            if (DiscIsDVD == true)
            {
                int CurrentChapter = DvdCurrentChapter();
                int TotalNumberOfChapters = DvdTotalChapters();
                if ((CurrentChapter > 1 && GoToChapter == true) || (GoToChapter == false))
                {
                    int PreviousChapter = CurrentChapter - 1;
                    if (GoToChapter == false)
                    {
                        PreviousChapter = CurrentChapter;
                    }

                    string Text = "Chapter " + PreviousChapter + " of " + TotalNumberOfChapters;

                    DrawInfoBarText(Text, Color.White, Color.Transparent);
                    iPreviousFocusChapterTransportBarIsShowing = true;

                    MadvrInterface.ShowMadVrBitmap("iPreviousFocusChapterTransportBar", iPreviousFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);

                    if (GoToChapter == true)
                    {
                        m_dvdControl.PlayPrevChapter(DvdCmdFlags.None, out m_dvdCmd);
                    }

                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    StartTimer(5000);
                }
            }
            else if (DiscIsDVD == false)
            {
                if (eSeek == null)
                {
                    eSeek = (IAMExtendedSeeking)sourceFilter;
                }
                int CurrentChapter;
                eSeek.get_CurrentMarker(out CurrentChapter);

                if (CurrentChapter > 1 || (GoToChapter == false))
                {
                    int PreviousChapter = CurrentChapter - 1;
                    if (GoToChapter == false)
                    {
                        PreviousChapter = CurrentChapter;
                    }
                    double PreviousChapterTime;
                    eSeek.GetMarkerTime(PreviousChapter, out PreviousChapterTime);


                    string Text = "Chapter " + PreviousChapter + " of " + TotalNumberOfChapters;
                    DrawInfoBarText(Text, Color.White, Color.Transparent);
                    iPreviousFocusChapterTransportBarIsShowing = true;

                    MadvrInterface.ShowMadVrBitmap("iPreviousFocusChapterTransportBar", iPreviousFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);

                    if (GoToChapter == true)
                    {
                        SeekAbsolute(PreviousChapterTime);
                    }

                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    StartTimer(5000);
                }
            }
            return;
        }

        private void DisplayCurrentChapterStatus()
        {
            if (iNextFocusChapterTransportBarIsShowing == false
                && iPreviousFocusChapterTransportBarIsShowing == false)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                }
                StartTimer(5000);
            }
        }

        #endregion

        #region Menu

        public void ExecuteCommand(string Command)
        {
            if (HasVideo == true)
            {
                #region down
                if (Command.ToLower() == "down")
                {
                    if (menuMode != MenuMode.Buttons)
                    {
                        if (m_state == PlaybackState.Running)
                        {
                            if (PlaylistsIsShowing == true)
                            {
                                ShowPlaylistMenu(movieInfo, "Next");
                                return;
                            }
                            else if (PlayerMenuIsShowing == true)
                            {
                                ShowPlayerMenu("Next");
                                return;
                            }
                            else if (AudioMenuIsShowing == true)
                            {
                                ShowAudioMenu("Next");
                                return;
                            }
                            else if (SubtitleMenuIsShowing == true)
                            {
                                ShowSubtitleMenu("Next");
                                return;
                            }
                            else if (ChapterMenuIsShowing == true)
                            {
                                ShowChapterMenu("Next");
                                return;
                            }
                            else if (GoToTimeMenuIsShowing == true)
                            {
                                GoToTimeMenuIsShowing = false;
                                GoToDigitOne = null;
                                GoToDigitTwo = null;
                                GoToDigitThree = null;
                                GoToDigitFour = null;
                                GoToDigitFive = null;
                                GoToDigitSix = null;
                                GoToText = null;
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                                iPauseFocusTransportBarIsShowing = true;
                                StartDurationThread();
                                return;
                            }

                            else if (iPlayFocusTransportBarIsShowing == false
                               && iPauseFocusTransportBarIsShowing == false
                               && iRRFocusPlayTransportBarIsShowing == false
                               && iRRFocusPauseTransportBarIsShowing == false
                               && iFFFocusPlayTransportBarIsShowing == false
                               && iFFFocusPauseTransportBarIsShowing == false
                               && iNextFocusPlayTransportBarIsShowing == false
                               && iNextFocusPauseTransportBarIsShowing == false
                               && iNextFocusChapterTransportBarIsShowing == false
                               && iPreviousFocusPlayTransportBarIsShowing == false
                               && iPreviousFocusPauseTransportBarIsShowing == false
                               && iPreviousFocusChapterTransportBarIsShowing == false
                               && iMovieInfoImageIsShowing == false)
                            {
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                                iPauseFocusTransportBarIsShowing = true;
                                StartDurationThread();
                                return;
                            }
                            else if (iMovieInfoImageIsShowing == false)
                            {
                                ShowTime = false;
                                MadvrInterface.ClearMadVrBitmap("transportTime", madvr);
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("iMovieInfoImage", iMovieInfoImage, 0, OSDPosy, 0, ImagePriority + 1, madvr);
                                iMovieInfoImageIsShowing = true;
                                return;
                            }
                            else
                            {
                                ClearTransportBar();
                                return;
                            }
                        }
                        else if (m_state == PlaybackState.Paused || m_state == PlaybackState.Stopped)
                        {
                            if (PlaylistsIsShowing == true)
                            {
                                ShowPlaylistMenu(movieInfo, "Next");
                                return;
                            }
                            else if (PlayerMenuIsShowing == true)
                            {
                                ShowPlayerMenu("Next");
                                return;
                            }
                            else if (AudioMenuIsShowing == true)
                            {
                                ShowAudioMenu("Next");
                                return;
                            }
                            else if (SubtitleMenuIsShowing == true)
                            {
                                ShowSubtitleMenu("Next");
                                return;
                            }
                            else if (ChapterMenuIsShowing == true)
                            {
                                ShowChapterMenu("Next");
                                return;
                            }
                            else if (GoToTimeMenuIsShowing == true)
                            {
                                GoToTimeMenuIsShowing = false;
                                GoToDigitOne = null;
                                GoToDigitTwo = null;
                                GoToDigitThree = null;
                                GoToDigitFour = null;
                                GoToDigitFive = null;
                                GoToDigitSix = null;
                                GoToText = null;
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                                iPlayFocusTransportBarIsShowing = true;
                                StartDurationThread();
                                return;
                            }
                            else if (iPlayFocusTransportBarIsShowing == false
                               && iPauseFocusTransportBarIsShowing == false
                               && iRRFocusPlayTransportBarIsShowing == false
                               && iRRFocusPauseTransportBarIsShowing == false
                               && iFFFocusPlayTransportBarIsShowing == false
                               && iFFFocusPauseTransportBarIsShowing == false
                               && iNextFocusPlayTransportBarIsShowing == false
                               && iNextFocusPauseTransportBarIsShowing == false
                               && iNextFocusChapterTransportBarIsShowing == false
                               && iPreviousFocusPlayTransportBarIsShowing == false
                               && iPreviousFocusPauseTransportBarIsShowing == false
                               && iPreviousFocusChapterTransportBarIsShowing == false
                               && iMovieInfoImageIsShowing == false)
                            {
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                                iPlayFocusTransportBarIsShowing = true;
                                StartDurationThread();
                                return;
                            }
                            else if (iMovieInfoImageIsShowing == false)
                            {
                                ShowTime = false;
                                MadvrInterface.ClearMadVrBitmap("transportTime", madvr);
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("iMovieInfoImage", iMovieInfoImage, 0, OSDPosy, 0, ImagePriority + 1, madvr);
                                iMovieInfoImageIsShowing = true;
                                return;
                            }
                            else
                            {
                                ShowTime = false;
                                ClearTransportBar();
                                return;
                            }
                        }
                    }
                    else if (menuMode == MenuMode.Buttons)
                    {
                        SelectButton(DvdRelativeButton.Lower);
                    }

                }
                #endregion

                #region up
                else if (Command.ToLower() == "up")
                {
                    if (menuMode != MenuMode.Buttons)
                    {
                        if (PlaylistsIsShowing == true)
                        {
                            ShowPlaylistMenu(movieInfo, "Previous");
                            return;
                        }
                        else if (PlayerMenuIsShowing == true)
                        {
                            ShowPlayerMenu("Previous");
                            return;
                        }
                        else if (AudioMenuIsShowing == true)
                        {
                            ShowAudioMenu("Previous");
                            return;
                        }
                        else if (SubtitleMenuIsShowing == true)
                        {
                            ShowSubtitleMenu("Previous");
                            return;
                        }
                        else if (ChapterMenuIsShowing == true)
                        {
                            ShowChapterMenu("Previous");
                            return;
                        }
                        else if (GoToTimeMenuIsShowing == true)
                        {
                            GoToTimeMenuIsShowing = false;
                            GoToDigitOne = null;
                            GoToDigitTwo = null;
                            GoToDigitThree = null;
                            GoToDigitFour = null;
                            GoToDigitFive = null;
                            GoToDigitSix = null;
                            GoToText = null;
                            ShowTime = false;
                            MadvrInterface.ClearMadVrBitmap("transportTime", madvr);
                            ClearTransportBar();
                            return;
                        }
                        else
                        {
                            ShowTime = false;
                            MadvrInterface.ClearMadVrBitmap("transportTime", madvr);
                            ClearTransportBar();
                            return;
                        }
                    }
                    else if (menuMode == MenuMode.Buttons)
                    {
                        SelectButton(DvdRelativeButton.Upper);
                    }
                }
                #endregion

                #region left
                if (Command.ToLower() == "left")
                {
                    if (menuMode != MenuMode.Buttons)
                    {
                        if (PlayerMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;
                            return;
                        }
                        else if (PlaylistsIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("Playlists", madvr);
                            PlaylistsIsShowing = false;
                            SelectedPlaylistNumber = -1;
                            ShowPlayerMenu("Show");
                            return;
                        }
                        else if (AudioMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("AudioMenu", madvr);
                            AudioMenuIsShowing = false;
                            SelectedAudioMenuNumber = -1;
                            ShowPlayerMenu("Show");
                            return;
                        }
                        else if (SubtitleMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("SubtitleMenu", madvr);
                            SubtitleMenuIsShowing = false;
                            SelectedSubtitleMenuNumber = -1;
                            ShowPlayerMenu("Show");
                            return;
                        }
                        else if (ChapterMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("ChapterMenu", madvr);
                            ChapterMenuIsShowing = false;
                            SelectedChapterMenuNumber = -1;
                            ShowPlayerMenu("Show");
                            return;
                        }
                        else if (iPlayFocusTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iRRFocusPlayTransportBar", iRRFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iRRFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iRRFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPreviousFocusPlayTransportBar", iPreviousFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPreviousFocusPlayTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iPreviousFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iDiscMenuFocusPlayTransportBar", iDiscMenuFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iDiscMenuFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iDiscMenuFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusPlayTransportBar", iSettingsMenuFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iSettingsMenuFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iSettingsMenuFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iNextFocusPlayTransportBar", iNextFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iNextFocusPlayTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iNextFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iFFFocusPlayTransportBar", iFFFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iFFFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iNextFocusChapterTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPreviousFocusChapterTransportBar", iPreviousFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                            iPreviousFocusChapterTransportBarIsShowing = true;
                            return;
                        }
                        else if (iPreviousFocusChapterTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iNextFocusChapterTransportBar", iNextFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                            iNextFocusChapterTransportBarIsShowing = true;
                            return;
                        }
                        else if (iFFFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPlayFocusTransportBarIsShowing = true;
                            return;
                        }
                        else if (iPauseFocusTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iRRFocus Pause TransportBar", iRRFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iRRFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iRRFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPreviousFocus Pause TransportBar", iPreviousFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPreviousFocusPauseTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iPreviousFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iDiscMenuFocus Pause TransportBar", iDiscMenuFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iDiscMenuFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iDiscMenuFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocus Pause TransportBar", iSettingsMenuFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iSettingsMenuFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iSettingsMenuFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iNextFocus Pause TransportBar", iNextFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iNextFocusPauseTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iNextFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iFFFocus Pause TransportBar", iFFFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iFFFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iFFFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPauseFocusTransportBarIsShowing = true;
                            return;
                        }
                    }
                    else if (menuMode == MenuMode.Buttons)
                    {
                        SelectButton(DvdRelativeButton.Left);
                    }


                }
                #endregion

                #region right
                if (Command.ToLower() == "right")
                {
                    if (menuMode != MenuMode.Buttons)
                    {
                        if (PlayerMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;
                            return;
                        }
                        else if (PlaylistsIsShowing == true)
                        {
                            MadvrInterface.ShowMadVrBitmap("PlayerMenu", PlayerMenu, 0, 0, 0, TimePriority + 1, madvr);
                            PlayerMenuIsShowing = true;
                            MadvrInterface.ClearMadVrBitmap("Playlists", madvr);
                            PlaylistsIsShowing = false;
                            SelectedPlaylistNumber = -1;
                            return;
                        }
                        else if (AudioMenuIsShowing == true)
                        {
                            MadvrInterface.ShowMadVrBitmap("PlayerMenu", PlayerMenu, 0, 0, 0, TimePriority + 1, madvr);
                            PlayerMenuIsShowing = true;
                            MadvrInterface.ClearMadVrBitmap("AudioMenu", madvr);
                            AudioMenuIsShowing = false;
                            SelectedAudioMenuNumber = -1;
                            return;
                        }
                        else if (SubtitleMenuIsShowing == true)
                        {
                            MadvrInterface.ShowMadVrBitmap("PlayerMenu", PlayerMenu, 0, 0, 0, TimePriority + 1, madvr);
                            PlayerMenuIsShowing = true;
                            MadvrInterface.ClearMadVrBitmap("SubtitleMenu", madvr);
                            SubtitleMenuIsShowing = false;
                            SelectedSubtitleMenuNumber = -1;
                            return;
                        }
                        else if (ChapterMenuIsShowing == true)
                        {
                            MadvrInterface.ShowMadVrBitmap("PlayerMenu", PlayerMenu, 0, 0, 0, TimePriority + 1, madvr);
                            PlayerMenuIsShowing = true;
                            MadvrInterface.ClearMadVrBitmap("ChapterMenu", madvr);
                            ChapterMenuIsShowing = false;
                            SelectedChapterMenuNumber = -1;
                            return;
                        }
                        else if (iPlayFocusTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iFFFocusPlayTransportBar", iFFFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iFFFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iFFFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iNextFocusPlayTransportBar", iNextFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iNextFocusPlayTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iNextFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusPlayTransportBar", iSettingsMenuFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iSettingsMenuFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iSettingsMenuFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iDiscMenuFocusPlayTransportBar", iDiscMenuFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iDiscMenuFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iDiscMenuFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPreviousFocusPlayTransportBar", iPreviousFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPreviousFocusPlayTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iPreviousFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iRRFocusPlayTransportBar", iRRFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iRRFocusPlayTransportBarIsShowing = true;
                            return;
                        }
                        else if (iPreviousFocusChapterTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iNextFocusChapterTransportBar", iNextFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                            iNextFocusChapterTransportBarIsShowing = true;
                            return;
                        }
                        else if (iNextFocusChapterTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPreviousFocusChapterTransportBar", iPreviousFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                            iPreviousFocusChapterTransportBarIsShowing = true;
                            return;
                        }
                        else if (iRRFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPlayFocusTransportBarIsShowing = true;
                            return;
                        }

                        else if (iPauseFocusTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iFFFocus Pause TransportBar", iFFFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iFFFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iFFFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iNextFocus Pause TransportBar", iNextFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iNextFocusPauseTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iNextFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocus Pause TransportBar", iSettingsMenuFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iSettingsMenuFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iSettingsMenuFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iDiscMenuFocus Pause TransportBar", iDiscMenuFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iDiscMenuFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iDiscMenuFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iPreviousFocus Pause TransportBar", iPreviousFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPreviousFocusPauseTransportBarIsShowing = true;
                            DisplayCurrentChapterStatus();
                            return;
                        }
                        else if (iPreviousFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("iRRFocus Pause TransportBar", iRRFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iRRFocusPauseTransportBarIsShowing = true;
                            return;
                        }
                        else if (iRRFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            ClearTransportBar();
                            MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                            iPauseFocusTransportBarIsShowing = true;
                            return;
                        }
                    }
                    else if (menuMode == MenuMode.Buttons)
                    {
                        SelectButton(DvdRelativeButton.Right);
                    }
                }

                #endregion

                #region enter or select
                if ((Command.ToLower() == "select" || Command.ToLower() == "enter"))
                {
                    if (menuMode != MenuMode.Buttons)
                    {
                        if (PlaylistsIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("Playlists", madvr);
                            PlaylistsIsShowing = false;
                            SelectedPlaylistNumber = -1;

                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;

                            //Uri path;
                            //string DiscPath = DriveLetter + ":\\BDMV\\PLAYLIST";
                            //string FileName = Path.Combine(DiscPath, SelectedPlaylist.Name);
                            //if (Uri.TryCreate(FileName, UriKind.RelativeOrAbsolute, out path))
                            //{
                            //    m_sourceUri = path;
                            //    HasVideo = false;
                            //    Play();
                            //}
                            return;
                        }
                        else if (AudioMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("AudioMenu", madvr);
                            AudioMenuIsShowing = false;
                            SelectedAudioMenuNumber = -1;

                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;

                            SetAudioTrack();
                            return;
                        }
                        else if (SubtitleMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("SubtitleMenu", madvr);
                            SubtitleMenuIsShowing = false;
                            SelectedSubtitleMenuNumber = -1;

                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;

                            SetSubtitleTrack();
                            return;
                        }
                        else if (ChapterMenuIsShowing == true)
                        {
                            MadvrInterface.ClearMadVrBitmap("ChapterMenu", madvr);
                            ChapterMenuIsShowing = false;
                            SelectedChapterMenuNumber = -1;

                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;

                            SetChapterTrack();
                            return;
                        }
                        else if (GoToTimeMenuIsShowing == true)
                        {
                            ShowGoToTimeMenu("Enter");
                            MadvrInterface.ClearMadVrBitmap("iSettingsMenuFocusTransportBar", madvr);
                            MadvrInterface.ClearMadVrBitmap("ChapterText", madvr);
                            GoToTimeMenuIsShowing = false;

                            MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                            PlayerMenuIsShowing = false;
                            SelectedPlayerMenuNumber = -1;

                            if (m_state == PlaybackState.Running)
                            {
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                                iPauseFocusTransportBarIsShowing = true;
                                StartDurationThread();
                            }
                            else if (m_state == PlaybackState.Paused || m_state == PlaybackState.Stopped)
                            {
                                ClearTransportBar();
                                MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                                iPlayFocusTransportBarIsShowing = true;
                                StartDurationThread();
                            }
                            return;
                        }

                        else if (iPlayFocusTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            Play();
                            return;
                        }
                        else if (iRRFocusPlayTransportBarIsShowing == true
                            || iRRFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            Rewind();
                            return;
                        }
                        else if (iFFFocusPlayTransportBarIsShowing == true
                            || iFFFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            FastForward();
                            return;
                        }
                        else if (iNextFocusPlayTransportBarIsShowing == true
                            || iNextFocusChapterTransportBarIsShowing == true
                            || iNextFocusPauseTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            NextChapter();
                            return;
                        }
                        else if (iPauseFocusTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            Pause();
                            return;
                        }
                        else if (iPreviousFocusPauseTransportBarIsShowing == true
                            || iPreviousFocusChapterTransportBarIsShowing == true
                            || iPreviousFocusPlayTransportBarIsShowing == true)
                        {
                            CommandFromTaskBar = true;
                            PreviousChapter();
                            return;
                        }
                        else if (iDiscMenuFocusPlayTransportBarIsShowing == true
                            || iDiscMenuFocusPauseTransportBarIsShowing == true)
                        {
                            if (DiscIsDVD == true)
                            {
                                CommandFromTaskBar = true;
                                ShowRootMenu();
                                ShowTime = false;
                                DurationThread.Join();
                                ClearTransportBar();
                            }
                            return;
                        }
                        else if (iSettingsMenuFocusPlayTransportBarIsShowing == true
                            || iSettingsMenuFocusPauseTransportBarIsShowing == true
                            || PlayerMenuIsShowing == true)
                        {
                            if (PlayerMenuIsShowing == false)
                            {
                                CommandFromTaskBar = true;
                                ShowPlayerMenu("Show");
                                return;
                            }

                            if (PlayerMenuIsShowing == true)
                            {
                                if (SelectedMenuText == "Playlists")
                                {
                                    PlayerMenuIsShowing = false;
                                    ShowPlaylistMenu(movieInfo, "Show");
                                    return;
                                }
                                else if (SelectedMenuText == "Chapters")
                                {
                                    ShowChapterMenu("Show");
                                    MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                                    PlayerMenuIsShowing = false;
                                    return;
                                }
                                else if (SelectedMenuText == "Go To Time")
                                {
                                    ShowGoToTimeMenu("Show");
                                    MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                                    PlayerMenuIsShowing = false;
                                    return;
                                }
                                else if (SelectedMenuText == "Audio")
                                {
                                    ShowAudioMenu("Show");
                                    MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                                    PlayerMenuIsShowing = false;
                                    return;
                                }
                                else if (SelectedMenuText == "Video")
                                {
                                    return;
                                }
                                else if (SelectedMenuText == "Subtitles")
                                {
                                    ShowSubtitleMenu("Show");
                                    MadvrInterface.ClearMadVrBitmap("PlayerMenu", madvr);
                                    PlayerMenuIsShowing = false;
                                    return;
                                }
                                else if (SelectedMenuText == "Settings")
                                {
                                    SetExclusiveMode(false);
                                    return;
                                }
                                else if (SelectedMenuText == "Track Information")
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else if (menuMode == MenuMode.Buttons)
                    {
                        ActivateButton();
                    }
                }
                #endregion

                #region back or exit or stop
                if (Command.ToLower() == "back" || Command.ToLower() == "exit" || Command.ToLower() == "stop" || Command.ToLower() == "delete")
                {
                    if (Command.ToLower() == "back" || Command.ToLower() == "delete")
                    {
                        if (GoToTimeMenuIsShowing == true)
                        {
                            ShowGoToTimeMenu(Command);
                            return;
                        }
                        else
                        {
                            CloseMe();
                            return;
                        }
                    }
                    else if (Command.ToLower() == "back" || Command.ToLower() == "exit" || Command.ToLower() == "stop")
                    {
                        CloseMe();
                        return;
                    }
                }
                #endregion

                #region Number
                if (ToolBox.StringFunctions.IsNumeric(Command))
                {
                    if (GoToTimeMenuIsShowing == true)
                    {
                        ShowGoToTimeMenu(Command);
                    }
                    return;
                }
                #endregion

            }
        }

        public void ShowPlayerMenu(string NextorPrevious)
        {
            List<string> MenuListNames = new List<string>();
            if (DiscIsBluRay == true)
            {
                MenuListNames.Add("Playlists");
            }
            GetTotalChapters();
            if (TotalNumberOfChapters > 0)
            {
                MenuListNames.Add("Chapters");
            }
            MenuListNames.Add("Go To Time");
            MenuListNames.Add("Audio");
            MenuListNames.Add("Video");
            //  Zoom
            //  Aspect Ratio
            //  Change Monitor
            MenuListNames.Add("Subtitles");
            MenuListNames.Add("Settings");
            //  Full Screen Exclusive Mode (Madvr)
            //  Hardware Acceleration (LAV)
            MenuListNames.Add("Track Information");

            if (SelectedPlayerMenuNumber == -1)
            {
                SelectedMenuText = MenuListNames[0];
                SelectedPlayerMenuNumber = 0;
                PlayerMenuStart = 0;
            }
            else if (NextorPrevious != "Next" && NextorPrevious != "Previous")
            {
                SelectedMenuText = MenuListNames[SelectedPlayerMenuNumber];
            }
            else if (NextorPrevious == "Next")
            {
                if (SelectedPlayerMenuNumber < MenuListNames.Count - 1)
                {
                    SelectedPlayerMenuNumber++;
                }
                if (SelectedPlayerMenuNumber > PlayerMenuStart + 7)
                {
                    PlayerMenuStart = SelectedPlayerMenuNumber - 7;
                }
                if (SelectedPlayerMenuNumber < MenuListNames.Count)
                {
                    SelectedMenuText = MenuListNames[SelectedPlayerMenuNumber];
                }
            }
            else if (NextorPrevious == "Previous")
            {
                SelectedPlayerMenuNumber--;
                if (SelectedPlayerMenuNumber < PlayerMenuStart)
                {
                    PlayerMenuStart = SelectedPlayerMenuNumber;
                }
                if (SelectedPlayerMenuNumber > -1)
                {
                    SelectedMenuText = MenuListNames[SelectedPlayerMenuNumber];
                }
            }

            DrawPlayerMenu("Player Menu:", MenuListNames, SelectedMenuText, PlayerMenuStart);
            MadvrInterface.ShowMadVrBitmap("PlayerMenu", PlayerMenu, 0, 0, 0, TimePriority + 1, madvr);
            PlayerMenuIsShowing = true;
        }

        public void ShowPlaylistMenu(MovieInfo media, string NextorPrevious)
        {
            ////Setup Popup Menu
            //List<string> pListNames = new List<string>();
            //List<string> pListNames2 = new List<string>();
            //string MainTitle = GetPlaylist();
            //foreach (BDInfo.TSPlaylistFile p in ValidPlaylists)
            //{
            //    if (p.Name.ToLower() == MainTitle.ToLower())
            //    {
            //        pListNames.Add("Main Title");
            //        MainTitlePlaylist = p;
            //        break;
            //    }
            //}

            //foreach (BDInfo.TSPlaylistFile p in ValidPlaylists)
            //{
            //    if (MovieRunTime(p) != "0" && MovieRunTime(p) != "1")
            //    {
            //        if (!string.IsNullOrEmpty(media.runtime))
            //        {
            //            int PlaylistRuntime = Convert.ToInt16(MovieRunTime(p));
            //            int ImdbRuntime = Convert.ToInt16(media.runtime);
            //            if (PlaylistRuntime > ImdbRuntime * .75)
            //            {
            //                pListNames2.Add(p.Name.Remove(p.Name.IndexOf(".")) + " - " + MovieRunTime(p).PadLeft(3, ' ') + " minutes");
            //            }
            //        }
            //        else
            //        {
            //            pListNames2.Add(p.Name.Remove(p.Name.IndexOf(".")) + " - " + MovieRunTime(p).PadLeft(3, ' ') + " minutes");
            //        }
            //    }
            //}
            //pListNames2.Sort();

            //foreach (string p in pListNames2)
            //{
            //    pListNames.Add(p);
            //}

            //string SelectedPlayListText = "";
            //if (SelectedPlaylistNumber == -1)
            //{
            //    SelectedPlayListText = pListNames[0];
            //    SelectedPlaylistNumber = 0;
            //    PlaylistStart = 0;
            //    string pText = SelectedPlayListText.Substring(0, 5);
            //    if (MainTitlePlaylist != null && MainTitlePlaylist.Name.StartsWith(pText))
            //    {
            //        SelectedPlaylist = MainTitlePlaylist;
            //    }
            //    else
            //    {
            //        foreach (BDInfo.TSPlaylistFile p in ValidPlaylists)
            //        {
            //            if (p.Name.StartsWith(pText))
            //            {
            //                SelectedPlaylist = p;
            //                break;
            //            }
            //        }
            //    }
            //}
            //else if (NextorPrevious == "Next")
            //{
            //    if (SelectedPlaylistNumber < pListNames.Count - 1)
            //    {
            //        SelectedPlaylistNumber++;
            //    }
            //    if (SelectedPlaylistNumber > PlaylistStart + 7)
            //    {
            //        PlaylistStart = SelectedPlaylistNumber - 7;
            //    }
            //    if (SelectedPlaylistNumber < pListNames.Count)
            //    {
            //        SelectedPlayListText = pListNames[SelectedPlaylistNumber];
            //        string pText = SelectedPlayListText.Substring(0, 5);
            //        if (MainTitlePlaylist != null && MainTitlePlaylist.Name.StartsWith(pText))
            //        {
            //            SelectedPlaylist = MainTitlePlaylist;
            //        }
            //        else
            //        {
            //            foreach (BDInfo.TSPlaylistFile p in ValidPlaylists)
            //            {
            //                if (p.Name.StartsWith(pText))
            //                {
            //                    SelectedPlaylist = p;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
            //else if (NextorPrevious == "Previous")
            //{
            //    SelectedPlaylistNumber--;
            //    if (SelectedPlaylistNumber < PlaylistStart)
            //    {
            //        PlaylistStart = SelectedPlaylistNumber;
            //    }
            //    if (SelectedPlaylistNumber > -1)
            //    {
            //        SelectedPlayListText = pListNames[SelectedPlaylistNumber];
            //        string pText = SelectedPlayListText.Substring(0, 5);
            //        if (MainTitlePlaylist != null && MainTitlePlaylist.Name.StartsWith(pText))
            //        {
            //            SelectedPlaylist = MainTitlePlaylist;
            //        }
            //        else
            //        {
            //            foreach (BDInfo.TSPlaylistFile p in ValidPlaylists)
            //            {
            //                if (p.Name.StartsWith(pText))
            //                {
            //                    SelectedPlaylist = p;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            //int bmpWidth = (int)(690 * ScaleRatio);
            //int bmpHeight = (int)(1080 * ScaleRatio);
            //DrawPlaylist(pListNames, SelectedPlayListText, PlaylistStart, Color.White, Color.Transparent, bmpWidth, bmpHeight);

            //MadvrInterface.ShowMadVrBitmap("Playlists", Playlists, 0, 0, 0, TimePriority + 1, madvr);
            //PlaylistsIsShowing = true;
        }

        public void ShowChapterMenu(string NextorPrevious)
        {
            GetTrackInfo();

            List<string> MenuListNames = new List<string>();
            GetTotalChapters();
            for (int i = 1; i < TotalNumberOfChapters + 1; i++)
            {
                MenuListNames.Add("Chapter " + i);
            }

            if (SelectedChapterMenuNumber == -1)
            {
                SelectedChapterMenuText = MenuListNames[0];
                SelectedChapterMenuNumber = 0;
                ChapterMenuStart = 0;
                SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
            }
            else if (NextorPrevious != "Next" && NextorPrevious != "Previous")
            {
                SelectedChapterMenuText = MenuListNames[SelectedChapterMenuNumber];
                SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
            }
            else if (NextorPrevious == "Next")
            {
                if (SelectedChapterMenuNumber < MenuListNames.Count - 1)
                {
                    SelectedChapterMenuNumber++;
                    SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
                }
                if (SelectedChapterMenuNumber > ChapterMenuStart + 7)
                {
                    ChapterMenuStart = SelectedChapterMenuNumber - 7;
                    SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
                }
                if (SelectedChapterMenuNumber < MenuListNames.Count)
                {
                    SelectedChapterMenuText = MenuListNames[SelectedChapterMenuNumber];
                    SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
                }
            }
            else if (NextorPrevious == "Previous")
            {
                SelectedChapterMenuNumber--;
                if (SelectedChapterMenuNumber < ChapterMenuStart)
                {
                    ChapterMenuStart = SelectedChapterMenuNumber;
                    SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
                }
                if (SelectedChapterMenuNumber > -1)
                {
                    SelectedChapterMenuText = MenuListNames[SelectedChapterMenuNumber];
                    SelectedChapterTrackNumber = SelectedChapterMenuNumber + 1;
                }
            }

            DrawChapterMenu("Chapters:", MenuListNames, SelectedChapterMenuText, ChapterMenuStart);
            MadvrInterface.ShowMadVrBitmap("ChapterMenu", ChapterMenu, 0, 0, 0, TimePriority + 1, madvr);
            ChapterMenuIsShowing = true;
        }

        private string GoToDigitOne;
        private string GoToDigitTwo;
        private string GoToDigitThree;
        private string GoToDigitFour;
        private string GoToDigitFive;
        private string GoToDigitSix;
        private string GoToText;
        public bool GoToTimeMenuIsShowing = false;

        public void ShowGoToTimeMenu(string Input)
        {
            if (Input == "Show")
            {
                GoToTimeMenuIsShowing = true;
                GoToText = "Go To:  __:__:__";
                DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                iSettingsMenuFocusTransportBarIsShowing = true;
                MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
            }
            else if (Input == "Enter")
            {
                string hours = GoToText.Substring(8, 2).Replace("_", "0");
                string minutes = GoToText.Substring(11, 2).Replace("_", "0");
                string seconds = GoToText.Substring(14, 2).Replace("_", "0");

                TimeSpan ts = new TimeSpan(Convert.ToInt16(hours), Convert.ToInt16(minutes), Convert.ToInt16(seconds));
                Double result = ts.Hours * 3600d;
                result += (ts.Minutes * 60d);
                result += ts.Seconds;
                SeekAbsolute(result);

                GoToTimeMenuIsShowing = false;
                GoToDigitOne = null;
                GoToDigitTwo = null;
                GoToDigitThree = null;
                GoToDigitFour = null;
                GoToDigitFive = null;
                GoToDigitSix = null;
                GoToText = null;
                return;
            }
            else if (Input.ToLower() == "back")
            {
                if (!string.IsNullOrEmpty(GoToDigitSix))
                {
                    GoToDigitSix = null;
                    GoToText = "Go To:  _" + GoToDigitOne + ":" + GoToDigitTwo + GoToDigitThree + ":" + GoToDigitFour + GoToDigitFive;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (!string.IsNullOrEmpty(GoToDigitFive))
                {
                    GoToDigitFive = null;
                    GoToText = "Go To:  __:" + GoToDigitOne + GoToDigitTwo + ":" + GoToDigitThree + GoToDigitFour;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (!string.IsNullOrEmpty(GoToDigitFour))
                {
                    GoToDigitFour = null;
                    GoToText = "Go To:  __:_" + GoToDigitOne + ":" + GoToDigitTwo + GoToDigitThree;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (!string.IsNullOrEmpty(GoToDigitThree))
                {
                    GoToDigitThree = null;
                    GoToText = "Go To:  __:__:" + GoToDigitOne + GoToDigitTwo;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (!string.IsNullOrEmpty(GoToDigitTwo))
                {
                    GoToDigitTwo = null;
                    GoToText = "Go To:  __:__:_" + GoToDigitOne;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (!string.IsNullOrEmpty(GoToDigitOne))
                {
                    GoToDigitOne = null;
                    GoToText = "Go To:  __:__:__";
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
            }
            else if (ToolBox.StringFunctions.IsNumeric(Input))
            {
                if (string.IsNullOrEmpty(GoToDigitOne))
                {
                    GoToDigitOne = Input;
                    GoToText = "Go To:  __:__:_" + GoToDigitOne;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (string.IsNullOrEmpty(GoToDigitTwo))
                {
                    GoToDigitTwo = Input;
                    GoToText = "Go To:  __:__:" + GoToDigitOne + GoToDigitTwo;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (string.IsNullOrEmpty(GoToDigitThree))
                {
                    GoToDigitThree = Input;
                    GoToText = "Go To:  __:_" + GoToDigitOne + ":" + GoToDigitTwo + GoToDigitThree;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (string.IsNullOrEmpty(GoToDigitFour))
                {
                    GoToDigitFour = Input;
                    GoToText = "Go To:  __:" + GoToDigitOne + GoToDigitTwo + ":" + GoToDigitThree + GoToDigitFour;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (string.IsNullOrEmpty(GoToDigitFive))
                {
                    GoToDigitFive = Input;
                    GoToText = "Go To:  _" + GoToDigitOne + ":" + GoToDigitTwo + GoToDigitThree + ":" + GoToDigitFour + GoToDigitFive;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
                else if (string.IsNullOrEmpty(GoToDigitSix))
                {
                    GoToDigitSix = Input;
                    GoToText = "Go To:  " + GoToDigitOne + GoToDigitTwo + ":" + GoToDigitThree + GoToDigitFour + ":" + GoToDigitFive + GoToDigitSix;
                    DrawInfoBarText(GoToText, Color.White, Color.Transparent);
                    iSettingsMenuFocusTransportBarIsShowing = true;
                    MadvrInterface.ShowMadVrBitmap("iSettingsMenuFocusTransportBar", iSettingsMenuFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                }
            }
        }

        public void ShowAudioMenu(string NextorPrevious)
        {
            GetTrackInfo();

            List<string> MenuListNames = new List<string>();
            foreach (KeyValuePair<int, string> audiotrack in audioTracks)
            {
                string TrackNumber = audiotrack.Key.ToString() + ": ";

                int start = audiotrack.Value.LastIndexOf(':') + 1;
                int end = audiotrack.Value.IndexOf('[');

                //Get Language
                string TrackLanguage = audiotrack.Value.Substring(start, end - start).Trim();

                //Get Codec
                start = audiotrack.Value.IndexOf('(') + 1;
                end = audiotrack.Value.IndexOf(',');
                string TrackCodec = audiotrack.Value.Substring(start, end - start).Trim();
                TrackCodec = TrackCodec.Replace("truehd", "Dolby TrueHD");
                TrackCodec = TrackCodec.Replace("ac3", "Dolby Digital");
                TrackCodec = TrackCodec.Replace("dts-hd ma", "DTS-HD Master Audio");
                TrackCodec = TrackCodec.Replace("dts", "DTS");
                TrackCodec = TrackCodec.Replace("pcm_bluray", "PCM");
                TrackCodec = TrackCodec.Replace("Apple Alias Data Handler", "Apple");
                TrackCodec = TrackCodec.Replace("aac", "AAC");

                //Get Number of Channels
                start = audiotrack.Value.IndexOf("Hz,") + 4;
                end = audiotrack.Value.LastIndexOf(',');
                string TrackNumberOfChannels = audiotrack.Value.Substring(start, 3).Trim();
                TrackNumberOfChannels = TrackNumberOfChannels.Replace("ste", "Stereo");

                string mText = (TrackNumber + TrackLanguage + " (" + TrackCodec + ", " + TrackNumberOfChannels + ")").Trim();

                MenuListNames.Add(mText);
            }

            if (SelectedAudioMenuNumber == -1)
            {
                SelectedAudioMenuText = MenuListNames[0];
                SelectedAudioMenuNumber = 0;
                AudioMenuStart = 0;
                SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[0].Substring(0, 1));
            }
            else if (NextorPrevious != "Next" && NextorPrevious != "Previous")
            {
                SelectedAudioMenuText = MenuListNames[SelectedAudioMenuNumber];
                SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[SelectedAudioMenuNumber].Substring(0, 1));
            }
            else if (NextorPrevious == "Next")
            {
                if (SelectedAudioMenuNumber < MenuListNames.Count - 1)
                {
                    SelectedAudioMenuNumber++;
                    SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[SelectedAudioMenuNumber].Substring(0, 1));
                }
                if (SelectedAudioMenuNumber > AudioMenuStart + 7)
                {
                    AudioMenuStart = SelectedAudioMenuNumber - 7;
                    SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[SelectedAudioMenuNumber].Substring(0, 1));
                }
                if (SelectedAudioMenuNumber < MenuListNames.Count)
                {
                    SelectedAudioMenuText = MenuListNames[SelectedAudioMenuNumber];
                    SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[SelectedAudioMenuNumber].Substring(0, 1));

                }
            }
            else if (NextorPrevious == "Previous")
            {
                SelectedAudioMenuNumber--;
                if (SelectedAudioMenuNumber < AudioMenuStart)
                {
                    AudioMenuStart = SelectedAudioMenuNumber;
                    SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[SelectedAudioMenuNumber].Substring(0, 1));
                }
                if (SelectedAudioMenuNumber > -1)
                {
                    SelectedAudioMenuText = MenuListNames[SelectedAudioMenuNumber];
                    SelectedAudioTrackNumber = Convert.ToInt16(MenuListNames[SelectedAudioMenuNumber].Substring(0, 1));
                }
            }

            DrawAudioMenu("Audio Tracks:", MenuListNames, SelectedAudioMenuText, AudioMenuStart);
            MadvrInterface.ShowMadVrBitmap("AudioMenu", AudioMenu, 0, 0, 0, TimePriority + 1, madvr);
            AudioMenuIsShowing = true;
        }

        public void ShowSubtitleMenu(string NextorPrevious)
        {
            GetTrackInfo();

            List<string> MenuListNames = new List<string>();
            foreach (KeyValuePair<int, string> Subtitletrack in Subtitles)
            {
                string TrackNumber = Subtitletrack.Key.ToString() + ": ";

                //Get Language
                string TrackLanguage = Subtitletrack.Value.Remove(0, 2).Trim();
                TrackLanguage = TrackLanguage.Replace("pgs", "PGS");
                TrackLanguage = TrackLanguage.Replace("auto", "Auto");
                TrackLanguage = TrackLanguage.Replace("subtitles", "Subtitles");
                if (TrackLanguage.Contains("["))
                {
                    string start = TrackLanguage.Remove(TrackLanguage.IndexOf("[") - 1).Trim();
                    string end = TrackLanguage.Remove(0, TrackLanguage.IndexOf("]") + 1).Trim();
                    TrackLanguage = start + " " + end;
                }

                string mText = (TrackNumber + TrackLanguage).Trim();

                MenuListNames.Add(mText);
            }

            if (SelectedSubtitleMenuNumber == -1)
            {
                SelectedSubtitleMenuText = MenuListNames[0];
                SelectedSubtitleMenuNumber = 0;
                SubtitleMenuStart = 0;
                SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[0].Substring(0, 1));
            }
            else if (NextorPrevious != "Next" && NextorPrevious != "Previous")
            {
                SelectedSubtitleMenuText = MenuListNames[SelectedSubtitleMenuNumber];
                SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[SelectedSubtitleMenuNumber].Substring(0, 1));
            }
            else if (NextorPrevious == "Next")
            {
                if (SelectedSubtitleMenuNumber < MenuListNames.Count - 1)
                {
                    SelectedSubtitleMenuNumber++;
                    SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[SelectedSubtitleMenuNumber].Substring(0, 1));
                }
                if (SelectedSubtitleMenuNumber > SubtitleMenuStart + 7)
                {
                    SubtitleMenuStart = SelectedSubtitleMenuNumber - 7;
                    SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[SelectedSubtitleMenuNumber].Substring(0, 1));
                }
                if (SelectedSubtitleMenuNumber < MenuListNames.Count)
                {
                    SelectedSubtitleMenuText = MenuListNames[SelectedSubtitleMenuNumber];
                    SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[SelectedSubtitleMenuNumber].Substring(0, 1));

                }
            }
            else if (NextorPrevious == "Previous")
            {
                SelectedSubtitleMenuNumber--;
                if (SelectedSubtitleMenuNumber < SubtitleMenuStart)
                {
                    SubtitleMenuStart = SelectedSubtitleMenuNumber;
                    SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[SelectedSubtitleMenuNumber].Substring(0, 1));
                }
                if (SelectedSubtitleMenuNumber > -1)
                {
                    SelectedSubtitleMenuText = MenuListNames[SelectedSubtitleMenuNumber];
                    SelectedSubtitleTrackNumber = Convert.ToInt16(MenuListNames[SelectedSubtitleMenuNumber].Substring(0, 1));
                }
            }

            DrawSubtitleMenu("Subtitles:", MenuListNames, SelectedSubtitleMenuText, SubtitleMenuStart);
            MadvrInterface.ShowMadVrBitmap("SubtitleMenu", SubtitleMenu, 0, 0, 0, TimePriority + 1, madvr);
            SubtitleMenuIsShowing = true;
        }

        #region Madvr Settings

        public void SetExclusiveMode(bool enable)
        {
            bool exclusiveMode = MadvrInterface.InExclusiveMode(madvr);

            if (exclusiveMode == true)
            {
                if (enable == true)
                {
                    return;
                }
                else
                {
                    MadvrInterface.EnableExclusiveMode(false, madvr);
                }
            }
            else if (exclusiveMode == false)
            {
                if (enable == true)
                {
                    MadvrInterface.EnableExclusiveMode(true, madvr);
                }
                else
                {
                    return;
                }
            }
        }

        #endregion

        #region LAV Settings
        
        #endregion
        
        protected void GetTrackInfo()
        {
            int hr;
            SortedDictionary<int, string> grp2Subs = new SortedDictionary<int, string>(), vobsubSubs = new SortedDictionary<int, string>();
            Guid vobsubSelector = Guid.Empty, grp2Selector = Guid.Empty;
            int grp2CurrentSub = 0, vobsubCurrentSub = 0;

            if (m_graph == null)
                throw new ArgumentNullException("graphBuilder");

            Subtitles.Clear();
            AudioTracks.Clear();

            IEnumFilters enumFilters;
            hr = m_graph.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                DirectShowLib.IBaseFilter[] filters = new DirectShowLib.IBaseFilter[1];

                while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    FilterInfo filterInfo;

                    hr = filters[0].QueryFilterInfo(out filterInfo);
                    DsError.ThrowExceptionForHR(hr);
                    Guid cl;
                    filters[0].GetClassID(out cl);

                    if (hr == 0)
                    {
                        if (filterInfo.pGraph != null)
                            Marshal.ReleaseComObject(filterInfo.pGraph);

                        IAMStreamSelect iss = filters[0] as IAMStreamSelect;

                        if (iss != null)
                        {
                            int count;
                            hr = iss.Count(out count);
                            DsError.ThrowExceptionForHR(hr);
                            for (int i = 0; i < count; i++)
                            {
                                AMMediaType type;
                                AMStreamSelectInfoFlags flags;
                                int plcid, pwdGrp; // language
                                String pzname;
                                //Thread t = Thread.CurrentThread;
                                object ppobject, ppunk;
                                hr = iss.Info(i, out type, out flags, out plcid, out pwdGrp, out pzname, out ppobject, out ppunk);
                                DsError.ThrowExceptionForHR(hr);
                                if (ppobject != null)
                                    Marshal.ReleaseComObject(ppobject);

                                if (type != null)
                                {
                                    DsUtils.FreeAMMediaType(type);
                                }

                                if (ppunk != null)
                                    Marshal.ReleaseComObject(ppunk);

                                if (pwdGrp == 2)
                                {
                                    if (grp2Selector == Guid.Empty)
                                        filters[0].GetClassID(out grp2Selector);
                                    if ((AMStreamSelectInfoFlags.Enabled & flags) == AMStreamSelectInfoFlags.Enabled)
                                    {
                                        grp2CurrentSub = i;
                                    }
                                    grp2Subs.Add(i, pzname);
                                    //Madvr_VideoPlayer.Plugin.WriteLog("Subtitle track: " + i + ": " + pzname);
                                }

                                if (pwdGrp == 1)
                                {
                                    if (audioSelector == Guid.Empty)
                                        filters[0].GetClassID(out audioSelector);
                                    if ((AMStreamSelectInfoFlags.Enabled & flags) == AMStreamSelectInfoFlags.Enabled)
                                    {
                                        audioTrack = i;
                                    }
                                    AudioTracks.Add(i, pzname);
                                    //Madvr_VideoPlayer.Plugin.WriteLog("Audio track: " + i + ": " + pzname);
                                }


                                if (pwdGrp == 6590033)
                                {
                                    if (vobsubSelector == Guid.Empty)
                                        filters[0].GetClassID(out vobsubSelector);
                                    if ((AMStreamSelectInfoFlags.Enabled & flags) == AMStreamSelectInfoFlags.Enabled)
                                    {
                                        vobsubCurrentSub = i;
                                    }
                                    vobsubSubs.Add(i, pzname);
                                    //Madvr_VideoPlayer.Plugin.WriteLog("VOB subs: " + i + ": " + pzname);
                                }
                            }
                        }
                    }

                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }
            if (grp2Subs.Count > 0)
            {
                foreach (KeyValuePair<int, string> sub in grp2Subs)
                {
                    Subtitles.Add(sub.Key, sub.Value);
                }
                subtitle = grp2CurrentSub;
                subtitleSelector = grp2Selector;
            }
            else if (vobsubSubs.Count > 0)
            {
                foreach (KeyValuePair<int, string> sub in vobsubSubs)
                {
                    Subtitles.Add(sub.Key, sub.Value);
                }
                subtitle = vobsubCurrentSub;
                subtitleSelector = vobsubSelector;
            }
        }

        public void SetAudioTrack()
        {
            int hr;

            IEnumFilters enumFilters;
            hr = m_graph.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                DirectShowLib.IBaseFilter[] filters = new DirectShowLib.IBaseFilter[1];

                while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    FilterInfo filterInfo;

                    hr = filters[0].QueryFilterInfo(out filterInfo);
                    DsError.ThrowExceptionForHR(hr);

                    if (hr == 0)
                    {
                        if (filterInfo.pGraph != null)
                            Marshal.ReleaseComObject(filterInfo.pGraph);

                        Guid clid;
                        filters[0].GetClassID(out clid);
                        if (clid != audioSelector)
                            continue;

                        IAMStreamSelect iss = filters[0] as IAMStreamSelect;

                        if (iss != null)
                        {
                            if (DiscIsDVD == true)
                            {
                                IDvdCmd icmd;
                                hr = m_dvdControl.SelectAudioStream(SelectedAudioTrackNumber, DvdCmdFlags.Flush | DvdCmdFlags.SendEvents, out icmd);
                                if (hr != E_DVDOperationInhibited)
                                    DsError.ThrowExceptionForHR(hr);
                            }
                            else
                            {
                                iss.Enable(SelectedAudioTrackNumber, AMStreamSelectEnableFlags.Enable);
                                audioTrack = SelectedAudioTrackNumber;
                            }
                        }
                    }
                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }
        }

        public void SetSubtitleTrack()
        {
            int hr;
            IEnumFilters enumFilters;
            hr = m_graph.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                DirectShowLib.IBaseFilter[] filters = new DirectShowLib.IBaseFilter[1];

                while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    FilterInfo filterInfo;

                    hr = filters[0].QueryFilterInfo(out filterInfo);
                    DsError.ThrowExceptionForHR(hr);

                    if (hr == 0)
                    {
                        if (filterInfo.pGraph != null)
                            Marshal.ReleaseComObject(filterInfo.pGraph);

                        Guid clid;
                        filters[0].GetClassID(out clid);
                        if (clid != subtitleSelector)
                            continue;

                        IAMStreamSelect iss = filters[0] as IAMStreamSelect;

                        if (iss != null)
                        {
                            if (DiscIsDVD == true)
                            {
                                IDvdCmd icmd;
                                hr = m_dvdControl.SelectSubpictureStream(SelectedSubtitleTrackNumber, DvdCmdFlags.Flush | DvdCmdFlags.SendEvents, out icmd);
                                if (hr != E_DVDOperationInhibited)
                                    DsError.ThrowExceptionForHR(hr);
                            }
                            else
                            {
                                iss.Enable(SelectedSubtitleTrackNumber, AMStreamSelectEnableFlags.Enable);
                                subtitle = SelectedSubtitleTrackNumber;
                            }
                        }
                    }
                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }
        }

        public void SetChapterTrack()
        {
            try
            {
                StartDurationThread();

                if (DiscIsDVD == true)
                {
                    int CurrentChapter = DvdCurrentChapter();
                    int TotalNumberOfChapters = DvdTotalChapters();

                    int NextChapter = SelectedChapterTrackNumber;

                    string Text = "Chapter " + NextChapter + " of " + TotalNumberOfChapters;
                    DrawInfoBarText(Text, Color.White, Color.Transparent);
                    iNextFocusChapterTransportBarIsShowing = true;

                    MadvrInterface.ShowMadVrBitmap("iNextFocusChapterTransportBar", iNextFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);
                    m_dvdControl.PlayNextChapter(DvdCmdFlags.None, out m_dvdCmd);


                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    StartTimer(5000);

                }
                else if (DiscIsDVD == false)
                {
                    if (eSeek == null)
                    {
                        eSeek = (IAMExtendedSeeking)sourceFilter;
                    }
                    else
                    {
                        int CurrentChapter;
                        eSeek.get_CurrentMarker(out CurrentChapter);
                        int NextChapter = SelectedChapterTrackNumber;
                        double NextChapterTime;
                        eSeek.GetMarkerTime(NextChapter, out NextChapterTime);

                        string Text = "Chapter " + NextChapter + " of " + TotalNumberOfChapters;
                        DrawInfoBarText(Text, Color.White, Color.Transparent);
                        iNextFocusChapterTransportBarIsShowing = true;

                        MadvrInterface.ShowMadVrBitmap("iNextFocusChapterTransportBar", iNextFocusChapterTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                        MadvrInterface.ShowMadVrBitmap("ChapterText", iText, 0, posyTB, 0, InfoBarTextPriority, madvr);

                        SeekAbsolute(NextChapterTime);

                        if (timer != null)
                        {
                            timer.Stop();
                        }
                        StartTimer(5000);
                    }
                }
            }
            catch (Exception e)
            {
            }
            return;

        }

        #endregion

        #region Graphics

        private void SetImages()
        {
                #region Transport Icons
                string TransportPath = "Icons\\Transport";

                string TransportBarPath = Path.Combine(TransportPath, "osd-back.png");
                Image TransportBarImage = Image.FromFile(TransportBarPath);

                string PlayPath = Path.Combine(TransportPath, "play-nofo.png");
                Image PlayImage = Image.FromFile(PlayPath);
                Bitmap PlayBitmap = new Bitmap(PlayImage);
                iPlayIcon = PlayBitmap.GetHbitmap();

                string PlayFocusPath = Path.Combine(TransportPath, "play-fo.png");
                Image PlayFocusImage = Image.FromFile(PlayFocusPath);

                string PausePath = Path.Combine(TransportPath, "pause-nofo.png");
                Image PauseImage = Image.FromFile(PausePath);
                Bitmap PauseBitmap = new Bitmap(PauseImage);
                iPauseIcon = PauseBitmap.GetHbitmap();

                string PauseFocusPath = Path.Combine(TransportPath, "pause-fo.png");
                Image PauseFocusImage = Image.FromFile(PauseFocusPath);

                string RewindPath = Path.Combine(TransportPath, "rewind-nofo.png");
                RewindImage = Image.FromFile(RewindPath);
                Bitmap RewindBitmap = new Bitmap(RewindImage);
                iRewindIcon = RewindBitmap.GetHbitmap();

                string RewindFocusPath = Path.Combine(TransportPath, "rewind-fo.png");
                Image RewindFocusImage = Image.FromFile(RewindFocusPath);

                string FFPath = Path.Combine(TransportPath, "fast-forward-nofo.png");
                FastForwardImage = Image.FromFile(FFPath);
                Bitmap FastforwardBitmap = new Bitmap(FastForwardImage);
                iFastforwardIcon = FastforwardBitmap.GetHbitmap();

                string FFFocusPath = Path.Combine(TransportPath, "fast-forward-fo.png");
                Image FastForwardFocusImage = Image.FromFile(FFFocusPath);

                string NextPath = Path.Combine(TransportPath, "next-nofo.png");
                Image NextImage = Image.FromFile(NextPath);
                Bitmap NextBitmap = new Bitmap(NextImage);
                iNextIcon = NextBitmap.GetHbitmap();

                string NextFocusPath = Path.Combine(TransportPath, "next-fo.png");
                Image NextFocusImage = Image.FromFile(NextFocusPath);

                string PreviousPath = Path.Combine(TransportPath, "previous-nofo.png");
                Image PreviousImage = Image.FromFile(PreviousPath);
                Bitmap PreviousBitmap = new Bitmap(PreviousImage);
                iPreviousIcon = PreviousBitmap.GetHbitmap();

                string PreviousFocusPath = Path.Combine(TransportPath, "previous-fo.png");
                Image PreviousFocusImage = Image.FromFile(PreviousFocusPath);

                string DiscMenuPath = Path.Combine(TransportPath, "dvd-nofo.png");
                Image DiscMenuImage = Image.FromFile(DiscMenuPath);

                string DiscMenuFocusPath = Path.Combine(TransportPath, "dvd-fo.png");
                Image DiscMenuFocusImage = Image.FromFile(DiscMenuFocusPath);

                string VideoSettingsPath = Path.Combine(TransportPath, "video-settings-nofo.png");
                Image VideoSettingsImage = Image.FromFile(VideoSettingsPath);

                string VideoSettingsFocusPath = Path.Combine(TransportPath, "video-settings-fo.png");
                Image VideoSettingsFocusImage = Image.FromFile(VideoSettingsFocusPath);


                #endregion

                #region TransportBar

                iPlayFocusTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PlayFocusImage, FastForwardImage, NextImage, VideoSettingsImage);
                iPauseFocusTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PauseFocusImage, FastForwardImage, NextImage, VideoSettingsImage);
                iRRFocusPauseTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindFocusImage, PauseImage, FastForwardImage, NextImage, VideoSettingsImage);
                iRRFocusPlayTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindFocusImage, PlayImage, FastForwardImage, NextImage, VideoSettingsImage);
                iFFFocusPauseTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PauseImage, FastForwardFocusImage, NextImage, VideoSettingsImage);
                iFFFocusPlayTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PlayImage, FastForwardFocusImage, NextImage, VideoSettingsImage);
                iNextFocusPauseTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PauseImage, FastForwardImage, NextFocusImage, VideoSettingsImage);
                iNextFocusPlayTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PlayImage, FastForwardImage, NextFocusImage, VideoSettingsImage);
                iPreviousFocusPauseTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousFocusImage, RewindImage, PauseImage, FastForwardImage, NextImage, VideoSettingsImage);
                iPreviousFocusPlayTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousFocusImage, RewindImage, PlayImage, FastForwardImage, NextImage, VideoSettingsImage);
                iDiscMenuFocusPlayTransportBar = CreateTransportBar(TransportBarImage, DiscMenuFocusImage, PreviousImage, RewindImage, PlayImage, FastForwardImage, NextImage, VideoSettingsImage);
                iDiscMenuFocusPauseTransportBar = CreateTransportBar(TransportBarImage, DiscMenuFocusImage, PreviousImage, RewindImage, PauseImage, FastForwardImage, NextImage, VideoSettingsImage);
                iSettingsMenuFocusPlayTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PlayImage, FastForwardImage, NextImage, VideoSettingsFocusImage);
                iSettingsMenuFocusPauseTransportBar = CreateTransportBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PauseImage, FastForwardImage, NextImage, VideoSettingsFocusImage);
                iSettingsMenuFocusTransportBar = CreateSettingsMenuBar(TransportBarImage, DiscMenuImage, PreviousImage, RewindImage, PauseImage, FastForwardImage, NextImage, VideoSettingsFocusImage);

                iNextFocusChapterTransportBar = CreateChapterTransportBar(TransportBarImage, PreviousImage, RewindImage, PlayImage, FastForwardImage, NextFocusImage);
                iPreviousFocusChapterTransportBar = CreateChapterTransportBar(TransportBarImage, PreviousFocusImage, RewindImage, PlayImage, FastForwardImage, NextImage);

                #endregion

                #region Dispose of Transport Images
                if (TransportBarImage != null)
                    TransportBarImage.Dispose();
                if (PlayImage != null)
                    PlayImage.Dispose();
                if (PlayFocusImage != null)
                    PlayFocusImage.Dispose();
                if (PauseImage != null)
                    PauseImage.Dispose();
                if (PauseFocusImage != null)
                    PauseFocusImage.Dispose();
                if (RewindFocusImage != null)
                    RewindFocusImage.Dispose();
                if (FastForwardFocusImage != null)
                    FastForwardFocusImage.Dispose();
                if (NextImage != null)
                    NextImage.Dispose();
                if (NextFocusImage != null)
                    NextFocusImage.Dispose();
                if (PreviousImage != null)
                    PreviousImage.Dispose();
                if (PreviousFocusImage != null)
                    PreviousFocusImage.Dispose();
                if (DiscMenuImage != null)
                    DiscMenuImage.Dispose();
                if (DiscMenuFocusImage != null)
                    DiscMenuFocusImage.Dispose();
                if (VideoSettingsImage != null)
                    VideoSettingsImage.Dispose();
                if (VideoSettingsFocusImage != null)
                    VideoSettingsFocusImage.Dispose();


                #endregion

                #region Set Movie Info
                //System.Collections.IList keylist = Item.Keys;
                //System.Collections.IList valuelist = Item.Values;
                

                #region Original video items in.
                //for (int i = 0; i < keylist.Count; i++)
                //{
                //    //Madvr_VideoPlayer.Plugin.WriteLog(keylist[i].ToString() + ": " + valuelist[i].ToString());
                //    if (!string.IsNullOrEmpty(valuelist[i].ToString()))
                //    {
                //        switch (keylist[i].ToString().ToLower())
                //        {
                //            case "actors":
                //                string[] ActorsList = valuelist[i].ToString().Split('|');
                //                foreach (string a in ActorsList)
                //                {
                //                    if (!string.IsNullOrEmpty(a))
                //                    {
                //                        movieInfo.Actors.Add(a);
                //                    }
                //                }
                //                break;

                //            case "audio_channels":
                //                movieInfo.audio_channels = valuelist[i].ToString();
                //                break;

                //            case "audio_codec":
                //                movieInfo.audio_codec = valuelist[i].ToString();
                //                break;

                //            case "awards":
                //                string[] AwardsList = valuelist[i].ToString().Split('|');
                //                foreach (string a in AwardsList)
                //                {
                //                    if (!string.IsNullOrEmpty(a))
                //                    {
                //                        movieInfo.Awards.Add(a);
                //                    }
                //                }
                //                break;

                //            case "director":
                //                movieInfo.Director = valuelist[i].ToString();
                //                break;

                //            case "genre":
                //                string[] GenreList = valuelist[i].ToString().Split('|');
                //                foreach (string a in GenreList)
                //                {
                //                    if (!string.IsNullOrEmpty(a))
                //                    {
                //                        movieInfo.Genre.Add(a);
                //                    }
                //                }
                //                break;

                //            case "image":
                //                if (!valuelist[i].ToString().ToLower().EndsWith("ifo"))
                //                {
                //                    movieInfo.image = valuelist[i].ToString();
                //                }
                //                break;

                //            case "path":
                //                if (valuelist[i].ToString().ToLower().EndsWith("ifo"))
                //                {
                //                    movieInfo.Media_Type = "dvd";
                //                }
                //                break;

                //            case "media_type":
                //                movieInfo.Media_Type = valuelist[i].ToString();
                //                break;

                //            case "id":
                //                movieInfo.MeediosId = valuelist[i].ToString();
                //                break;

                //            case "mpaarating":
                //                movieInfo.MPAARating = valuelist[i].ToString();
                //                break;

                //            case "overview":
                //                movieInfo.Overview = valuelist[i].ToString();
                //                break;

                //            case "tagline":
                //                movieInfo.Tagline = valuelist[i].ToString();
                //                break;

                //            case "title":
                //                movieInfo.Title = valuelist[i].ToString();
                //                break;

                //            case "video_codec":
                //                movieInfo.video_codec = valuelist[i].ToString();
                //                break;

                //            case "year":
                //                movieInfo.Year = valuelist[i].ToString();
                //                break;

                //            case "studio":
                //                string[] StudioList = valuelist[i].ToString().Split('|');
                //                foreach (string a in StudioList)
                //                {
                //                    if (!string.IsNullOrEmpty(a))
                //                    {
                //                        movieInfo.Studio.Add(a);
                //                    }
                //                }
                //                break;
                //            case "runtime":
                //                movieInfo.runtime = valuelist[i].ToString();
                //                break;
                //        }
                //    }
                //}
                #endregion

                #endregion


                #region Set Movie Info Images

                string OsdInfoPath = "Icons\\Media";
                string OsdImagePath = Path.Combine(OsdInfoPath, "osd-back-info.png");
                Image bp = Image.FromFile(OsdImagePath);
                int OsdOriginalHeight = bp.Height;
                int OsdOriginalWidth = bp.Width;

                Image PosterImage = null;
                Image PosterThumb = null;
                int TitlePosX = 120;
                if (!string.IsNullOrEmpty(movieInfo.image))
                {
                    if (movieInfo.image.ToLower().StartsWith("http"))
                    {
                        HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(movieInfo.image);
                        myRequest.Method = "GET";
                        HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                        PosterImage = Image.FromStream(myResponse.GetResponseStream());
                        myResponse.Close();
                    }
                    else
                    {
                        PosterImage = Image.FromFile(movieInfo.image);
                    }
                    PosterThumb = ScaleImage(PosterImage, PosterImage.Width, OsdOriginalHeight - 80);
                    bp = MergeImage(bp, PosterThumb, 75, 120);
                    TitlePosX = 120 + PosterThumb.Width - 33;
                }

                Image title = null;
                if (!string.IsNullOrEmpty(movieInfo.Title))
                {
                    Font titlefont = new Font("Segoe Condensed", 40, FontStyle.Bold);
                    title = DrawText(movieInfo.Title, titlefont, Color.White, Color.Transparent);
                    titlefont.Dispose();
                    bp = MergeImage(bp, title, 85, TitlePosX);
                }

                string detailsText = "";
                //Add Year
                if (!string.IsNullOrEmpty(movieInfo.Year))
                {
                    detailsText = movieInfo.Year + " • ";
                }

                //Add Rating
                string mpaaRating = "";
                if (!string.IsNullOrEmpty(movieInfo.MPAARating))
                {
                    if (movieInfo.MPAARating.ToLower().Contains("usa"))
                    {
                        mpaaRating = movieInfo.MPAARating.Remove(0, 4).Trim();
                    }
                    else
                    {
                        mpaaRating = movieInfo.MPAARating.Trim();
                    }
                    if (!string.IsNullOrEmpty(mpaaRating))
                    {
                        detailsText = detailsText + mpaaRating + " • ";
                    }
                }

                //Add Genre
                if (movieInfo.Genre.Count > 0)
                {
                    StringBuilder Genrebuilder = new StringBuilder();
                    foreach (string s in movieInfo.Genre)
                    {
                        Genrebuilder.Append(s).Append(" / ");
                    }
                    string genre = Genrebuilder.ToString().Trim();
                    if (genre.EndsWith("/"))
                    {
                        int slashPosition = genre.LastIndexOf("/");
                        genre = genre.Remove(slashPosition).Trim();
                    }
                    if (!string.IsNullOrEmpty(genre))
                    {
                        detailsText = detailsText + genre + " • ";
                    }
                }

                //Add Studio
                if (movieInfo.Studio.Count > 0)
                {
                    StringBuilder Studiobuilder = new StringBuilder();
                    foreach (string s in movieInfo.Studio)
                    {
                        Studiobuilder.Append(s).Append(" / ");
                    }
                    string studio = Studiobuilder.ToString().Trim();
                    if (studio.EndsWith("/"))
                    {
                        int slashPosition = studio.LastIndexOf("/");
                        studio = studio.Remove(slashPosition).Trim();
                    }
                    if (!string.IsNullOrEmpty(studio))
                    {
                        detailsText = detailsText + studio;
                    }
                }

                detailsText.Trim();
                if (detailsText.EndsWith("•"))
                {
                    int dotPosition = detailsText.LastIndexOf("•");
                    detailsText = detailsText.Remove(dotPosition).Trim();
                }
                if (detailsText.Length > 110)
                {
                    detailsText = detailsText.Substring(0, 110);
                }
                if (detailsText.EndsWith("/"))
                {
                    int dotPosition = detailsText.LastIndexOf("/");
                    detailsText = detailsText.Remove(dotPosition).Trim();
                }

                Image details = null;
                if (!string.IsNullOrEmpty(detailsText))
                {
                    Font detailsfont = new Font("Segoe Condensed", 30, FontStyle.Regular);
                    details = DrawText(detailsText, detailsfont, Color.White, Color.Transparent);
                    detailsfont.Dispose();
                    bp = MergeImage(bp, details, 135, TitlePosX);
                }

                Image overview = null;
                if (!string.IsNullOrEmpty(movieInfo.Overview))
                {
                    Font overviewfont = new Font("Segoe Condensed", 30, FontStyle.Regular);

                    string OverviewLine1 = "";
                    string OverviewLine2 = "";
                    string OverviewLine3 = "";

                    if (movieInfo.Overview.Length > 110)
                    {
                        OverviewLine1 = movieInfo.Overview.Substring(0, 110);
                        int i = OverviewLine1.LastIndexOf(" ");
                        OverviewLine1 = OverviewLine1.Remove(i);
                        overview = DrawText(OverviewLine1, overviewfont, Color.Gray, Color.Transparent);
                        bp = MergeImage(bp, overview, 180, TitlePosX);

                        OverviewLine2 = movieInfo.Overview.Substring(i, movieInfo.Overview.Length - i).Trim();
                        if (OverviewLine2.Length > 110)
                        {
                            OverviewLine2 = OverviewLine2.Substring(0, 110);
                            i = OverviewLine2.LastIndexOf(" ");
                            OverviewLine2 = OverviewLine2.Remove(i);
                        }
                        overview = DrawText(OverviewLine2, overviewfont, Color.Gray, Color.Transparent);
                        bp = MergeImage(bp, overview, 215, TitlePosX);

                        if (movieInfo.Overview.Length > 220)
                        {
                            int start = OverviewLine1.Length + 1 + OverviewLine2.Length + 1;
                            OverviewLine3 = movieInfo.Overview.Substring(start, movieInfo.Overview.Length - start).Trim();
                            if (OverviewLine3.Length > 110)
                            {
                                OverviewLine3 = OverviewLine3.Substring(0, 110);
                                i = OverviewLine3.LastIndexOf(" ");
                                OverviewLine3 = OverviewLine3.Remove(i);
                            }
                            overview = DrawText(OverviewLine3, overviewfont, Color.Gray, Color.Transparent);
                            bp = MergeImage(bp, overview, 250, TitlePosX);
                        }
                    }
                    else
                    {
                        overview = DrawText(movieInfo.Overview, overviewfont, Color.Gray, Color.Transparent);
                        bp = MergeImage(bp, overview, 175, TitlePosX);
                    }
                    overviewfont.Dispose();
                }


                //Add media info
                string MediaInfoPath = "Icons\\MediaInfo";

                #region Disc Type
                Image MTImage = null;
                if (!string.IsNullOrEmpty(movieInfo.Media_Type))
                {
                    string MTImagePath = "";
                    if (movieInfo.Media_Type.ToLower() == "bluray")
                    {
                        MTImagePath = Path.Combine(MediaInfoPath, "br.png");
                        MTImage = Image.FromFile(MTImagePath);
                        MTImage = ScaleImage(MTImage, 155, 105);

                        int posx = OsdOriginalWidth - 75 - MTImage.Width;
                        bp = MergeImage(bp, MTImage, 70, posx);
                    }
                    else if (movieInfo.Media_Type.ToLower() == "dvd")
                    {
                        MTImagePath = Path.Combine(MediaInfoPath, "dvd.png");
                        MTImage = Image.FromFile(MTImagePath);
                        MTImage = ScaleImage(MTImage, 155, 105);

                        int posx = OsdOriginalWidth - 75 - MTImage.Width;
                        bp = MergeImage(bp, MTImage, 70, posx);
                    }
                }
                #endregion

                #region Audio Type
                string AudioImagePath = "";
                Image AudioImage = null;
                if (!string.IsNullOrEmpty(movieInfo.audio_codec))
                {
                    if (movieInfo.audio_codec.ToLower().Contains("dts-hd"))
                    {
                        AudioImagePath = Path.Combine(MediaInfoPath, "dtshd.png");
                    }
                    else if (movieInfo.audio_codec.ToLower().Contains("truehd"))
                    {
                        AudioImagePath = Path.Combine(MediaInfoPath, "truehd.png");
                    }
                    else if (movieInfo.audio_codec.ToLower().Contains("dts"))
                    {
                        AudioImagePath = Path.Combine(MediaInfoPath, "dts.png");
                    }
                    else if (movieInfo.audio_codec.ToLower().Contains("ac3"))
                    {
                        AudioImagePath = Path.Combine(MediaInfoPath, "digital.png");
                    }
                    else if (movieInfo.audio_codec.ToLower().Contains("pcm"))
                    {
                        AudioImagePath = Path.Combine(MediaInfoPath, "pcm.png");
                    }
                    else if (movieInfo.audio_codec.ToLower().Contains("flac"))
                    {
                        AudioImagePath = Path.Combine(MediaInfoPath, "flac.png");
                    }

                    if (!string.IsNullOrEmpty(AudioImagePath))
                    {
                        AudioImage = Image.FromFile(AudioImagePath);
                        AudioImage = ScaleImage(AudioImage, 161, 109);

                        int posx;
                        if (MTImage != null)
                        {
                            posx = OsdOriginalWidth - 40 - MTImage.Width - AudioImage.Width;
                        }
                        else
                        {
                            posx = OsdOriginalWidth - 40 - AudioImage.Width;
                        }
                        bp = MergeImage(bp, AudioImage, 70, posx);
                    }
                }
                #endregion

                if (bp != null)
                {
                    bp = ScaleImage(bp, ScreenWidth, OsdOriginalHeight);
                }

                if (OSDPosy == 0)
                {
                    if (bp != null)
                    {
                        OSDPosy = ScreenHeight - bp.Height;
                    }
                }
                Bitmap MovieInfoBitmap = null;
                if (bp != null)
                {
                    MovieInfoBitmap = new Bitmap(bp);
                    iMovieInfoImage = MovieInfoBitmap.GetHbitmap();
                }

                if (PosterImage != null)
                    PosterImage.Dispose();
                if (PosterThumb != null)
                    PosterThumb.Dispose();
                if (title != null)
                    title.Dispose();
                if (details != null)
                    details.Dispose();
                if (overview != null)
                    overview.Dispose();
                if (MTImage != null)
                    MTImage.Dispose();
                if (AudioImage != null)
                    AudioImage.Dispose();
                if (bp != null)
                    bp.Dispose();
                if (MovieInfoBitmap != null)
                    MovieInfoBitmap.Dispose();

                #endregion
            
                SetProgressOrbImage();
                SetPlaylistBackgroundImage();
        }

        private void PlayToPauseTransportBar()
        {
            if (iPlayFocusTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("Pause FocusTransportBar", iPauseFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iPauseFocusTransportBarIsShowing = true;
                return;
            }
            else if (iRRFocusPlayTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iRRFocus Pause TransportBar", iRRFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iRRFocusPauseTransportBarIsShowing = true;
                return;
            }
            else if (iPreviousFocusPlayTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iPreviousFocus Pause TransportBar", iPreviousFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iPreviousFocusPauseTransportBarIsShowing = true;
                return;
            }
            else if (iFFFocusPlayTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iFFFocus Pause TransportBar", iFFFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iFFFocusPauseTransportBarIsShowing = true;
                return;
            }
            else if (iNextFocusPlayTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iNextFocus Pause TransportBar", iNextFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iNextFocusPauseTransportBarIsShowing = true;
                return;
            }
        }

        private void PauseToPlayTransportBar()
        {
            if (iPauseFocusTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iPlayFocusTransportBar", iPlayFocusTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iPlayFocusTransportBarIsShowing = true;
                return;
            }
            else if (iRRFocusPauseTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iRRFocusPlayTransportBar", iRRFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iRRFocusPlayTransportBarIsShowing = true;
                return;
            }
            else if (iPreviousFocusPauseTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iPreviousFocusPlayTransportBar", iPreviousFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iPreviousFocusPlayTransportBarIsShowing = true;
                return;
            }
            else if (iFFFocusPauseTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iFFFocusPlayTransportBar", iFFFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iFFFocusPlayTransportBarIsShowing = true;
                return;
            }
            else if (iNextFocusPauseTransportBarIsShowing == true)
            {
                ClearTransportBar();
                MadvrInterface.ShowMadVrBitmap("iNextFocusPlayTransportBar", iNextFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                iNextFocusPlayTransportBarIsShowing = true;
                return;
            }
        }

        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            if (ScaleRatio == 0)
            {
                ScaleRatio = ratio;
            }

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            Bitmap ScaledBitmap = new Bitmap(newWidth, newHeight);
            Graphics myGraphic2 = Graphics.FromImage(ScaledBitmap);
            myGraphic2.DrawImage(image, 0, 0, newWidth, newHeight);
            myGraphic2.Dispose();
            image.Dispose();
            return ScaledBitmap;
        }

        private static Image MergeImage(Image BaseImage, Image Image1, Image Image2, Image Image3, Image Image4, Image Image5, Image Image6, Image Image7)
        {
            int img1Width = Image1.Width;
            int img1Height = Image1.Height;

            int img2Width = Image2.Width;
            int img2Height = Image2.Height;

            int img3Width = Image3.Width;
            int img3Height = Image3.Height;

            int img4Width = Image4.Width;
            int img4Height = Image4.Height;

            int img5Width = Image5.Width;
            int img5Height = Image5.Height;

            int img6Width = Image6.Width;
            int img6Height = Image6.Height;

            int img7Width = Image7.Width;
            int img7Height = Image7.Height;

            Image m = BaseImage;
            Graphics myGraphic = Graphics.FromImage(m);

            int posy = (m.Height / 2) - (img1Height / 2);
            int i4X = (m.Width / 2) - (img4Width / 2);

            int i3X = i4X - img3Width;
            int i2X = i3X - img2Width;
            int i1X = i2X - img1Width;

            int i5X = i4X + img5Width;
            int i6X = i5X + img6Width;
            int i7X = i6X + img7Width;

            myGraphic.DrawImageUnscaled(Image1, i1X, posy);
            myGraphic.DrawImageUnscaled(Image2, i2X, posy);
            myGraphic.DrawImageUnscaled(Image3, i3X, posy);
            myGraphic.DrawImageUnscaled(Image4, i4X, posy);
            myGraphic.DrawImageUnscaled(Image5, i5X, posy);
            myGraphic.DrawImageUnscaled(Image6, i6X, posy);
            myGraphic.DrawImageUnscaled(Image7, i7X, posy);
            myGraphic.Dispose();
            return m;
        }

        private Image MergeSettingsMenuImage(Image BaseImage, Image Image1, Image Image2, Image Image3, Image Image4, Image Image5, Image Image6, Image Image7)
        {
            int img1Width = Image1.Width;
            int img1Height = Image1.Height;

            int img2Width = Image2.Width;
            int img2Height = Image2.Height;

            int img3Width = Image3.Width;
            int img3Height = Image3.Height;

            int img4Width = Image4.Width;
            int img4Height = Image4.Height;

            int img5Width = Image5.Width;
            int img5Height = Image5.Height;

            int img6Width = Image6.Width;
            int img6Height = Image6.Height;

            int img7Width = Image7.Width;
            int img7Height = Image7.Height;

            Image m = BaseImage;
            Graphics myGraphic = Graphics.FromImage(m);

            int posy = (m.Height / 2) - (img1Height / 2);
            int i4X = (m.Width / 2) - (img4Width / 2);

            int i3X = i4X - img3Width;
            int i2X = i3X - img2Width;
            int i1X = i2X - img1Width;

            int i5X = i4X + img5Width;
            int i6X = i5X + img6Width;
            int i7X = i6X + img7Width;

            myGraphic.DrawImageUnscaled(Image7, i7X, posy);
            myGraphic.Dispose();
            return m;
        }

        private static Image MergeChapterImage(Image BaseImage, Image Image1, Image Image2, Image Image3, Image Image4, Image Image5)
        {
            int img1Width = Image1.Width;
            int img1Height = Image1.Height;

            int img2Width = Image2.Width;
            int img2Height = Image2.Height;

            int img3Width = Image3.Width;
            int img3Height = Image3.Height;

            int img4Width = Image4.Width;
            int img4Height = Image4.Height;

            int img5Width = Image5.Width;
            int img5Height = Image5.Height;

            Image m = BaseImage;
            Graphics myGraphic = Graphics.FromImage(m);

            int posy = (m.Height / 2) - (img1Height / 2);
            int i3X = (m.Width / 2) - (img3Width / 2);
            int i2X = i3X - img2Width;
            int i1X = i2X - img1Width;
            int i4X = i3X + img4Width;
            int i5X = i4X + img5Width;

            myGraphic.DrawImageUnscaled(Image1, i1X, posy);
            myGraphic.DrawImageUnscaled(Image5, i5X, posy);
            myGraphic.Dispose();
            return m;
        }

        private static Image MergeIconImage(Image BaseImage, Image TextImage)
        {
            int img1Width = TextImage.Width;
            int img1Height = TextImage.Height;

            Image m = BaseImage;
            Graphics myGraphic = Graphics.FromImage(m);

            int posy = (m.Height / 2) - (img1Height / 2);
            int posx = (m.Width / 2) - ((img1Width / 2) + 5);

            myGraphic.DrawImageUnscaled(TextImage, posx, posy);
            myGraphic.Dispose();
            return m;
        }

        private static Image MergeImage(Image BaseImage, Image TitleImage, int posy, int posx)
        {
            Graphics myGraphic = Graphics.FromImage(BaseImage);
            myGraphic.DrawImageUnscaled(TitleImage, posx, posy);
            myGraphic.Dispose();
            return BaseImage;
        }

        private IntPtr CreateTransportBar(Image BaseImage, Image Image1, Image Image2, Image Image3, Image Image4, Image Image5, Image Image6, Image Image7)
        {
            Bitmap i0 = new Bitmap(BaseImage);
            Bitmap i1 = new Bitmap(Image1);
            Bitmap i2 = new Bitmap(Image2);
            Bitmap i3 = new Bitmap(Image3);
            Bitmap i4 = new Bitmap(Image4);
            Bitmap i5 = new Bitmap(Image5);
            Bitmap i6 = new Bitmap(Image6);
            Bitmap i7 = new Bitmap(Image7);

            Image tb = MergeImage(i0, i1, i2, i3, i4, i5, i6, i7);
            tb = ScaleImage(tb, ScreenWidth, BaseImage.Height);

            if (posyTB == 0)
            {
                posyTB = ScreenHeight - tb.Height;
            }
            Bitmap temp = new Bitmap(tb);
            IntPtr image = temp.GetHbitmap();
            tb.Dispose();
            i0.Dispose();
            i1.Dispose();
            i2.Dispose();
            i3.Dispose();
            i4.Dispose();
            i5.Dispose();
            i6.Dispose();
            i7.Dispose();
            //totalTimeImage.Dispose();
            temp.Dispose();
            return image;
        }

        private IntPtr CreateSettingsMenuBar(Image BaseImage, Image Image1, Image Image2, Image Image3, Image Image4, Image Image5, Image Image6, Image Image7)
        {
            Bitmap i0 = new Bitmap(BaseImage);
            Bitmap i1 = new Bitmap(Image1);
            Bitmap i2 = new Bitmap(Image2);
            Bitmap i3 = new Bitmap(Image3);
            Bitmap i4 = new Bitmap(Image4);
            Bitmap i5 = new Bitmap(Image5);
            Bitmap i6 = new Bitmap(Image6);
            Bitmap i7 = new Bitmap(Image7);

            Image tb = ScaleImage(i0, ScreenWidth, BaseImage.Height);

            if (posyTB == 0)
            {
                posyTB = ScreenHeight - tb.Height;
            }
            Bitmap temp = new Bitmap(tb);
            IntPtr image = temp.GetHbitmap();
            tb.Dispose();
            i0.Dispose();
            i1.Dispose();
            i2.Dispose();
            i3.Dispose();
            i4.Dispose();
            i5.Dispose();
            i6.Dispose();
            i7.Dispose();
            temp.Dispose();
            return image;
        }

        private IntPtr CreateChapterTransportBar(Image BaseImage, Image Image1, Image Image2, Image Image3, Image Image4, Image Image5)
        {
            Bitmap i0 = new Bitmap(BaseImage);
            Bitmap i1 = new Bitmap(Image1);
            Bitmap i2 = new Bitmap(Image2);
            Bitmap i3 = new Bitmap(Image3);
            Bitmap i4 = new Bitmap(Image4);
            Bitmap i5 = new Bitmap(Image5);

            Image tb = MergeChapterImage(i0, i1, i2, i3, i4, i5);
            tb = ScaleImage(tb, ScreenWidth, BaseImage.Height);

            if (posyTB == 0)
            {
                posyTB = ScreenHeight - tb.Height;
            }
            Bitmap temp = new Bitmap(tb);
            IntPtr image = temp.GetHbitmap();
            tb.Dispose();
            i0.Dispose();
            i1.Dispose();
            i2.Dispose();
            i3.Dispose();
            i4.Dispose();
            i5.Dispose();
            temp.Dispose();
            //totalTimeImage.Dispose();
            return image;
        }

        private static Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);
            SizeF textSize = drawing.MeasureString(text, font);
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);
            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);
            textBrush.Dispose();
            drawing.Dispose();
            return img;
        }

        private void DrawInfoBarText(String text, Color textColor, Color backColor)
        {
            DeleteObject(iText);

            Bitmap img2 = new Bitmap(1920, 190);
            Graphics drawing2 = Graphics.FromImage(img2);

            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize, FontStyle.Bold);
            SizeF textSize = drawing2.MeasureString(text, font);
            Brush textBrush = new SolidBrush(textColor);

            int Posx = (int)((1920 / 2) * ScaleRatio) - (Convert.ToInt16(textSize.Width) / 2);
            int Posy = (int)((190 / 2) * ScaleRatio) - (Convert.ToInt16(textSize.Height) / 2);
            drawing2.Clear(backColor);
            drawing2.DrawString(text, font, textBrush, Posx, Posy);
            textBrush.Dispose();
            drawing2.Dispose();
            font.Dispose();

            iText = img2.GetHbitmap();
            img2.Dispose();
        }

        private void ReleasePlayerImages()
        {
            if (FastForwardImage != null)
                FastForwardImage.Dispose();
            if (RewindImage != null)
                RewindImage.Dispose();
            if (TransportBallImage != null)
                TransportBallImage.Dispose();
            if (PlaylistBackgroundImage != null)
                PlaylistBackgroundImage.Dispose();

            DeleteObject(Time);
            DeleteObject(iPlayIcon);
            DeleteObject(iPauseIcon);
            DeleteObject(iRewindIcon);
            DeleteObject(iFastforwardIcon);
            DeleteObject(iNextIcon);
            DeleteObject(iPreviousIcon);
            DeleteObject(iPlayFocusTransportBar);
            DeleteObject(iPauseFocusTransportBar);
            DeleteObject(iRRFocusPlayTransportBar);
            DeleteObject(iRRFocusPauseTransportBar);
            DeleteObject(iFFFocusPlayTransportBar);
            DeleteObject(iFFFocusPauseTransportBar);
            DeleteObject(iNextFocusPlayTransportBar);
            DeleteObject(iNextFocusPauseTransportBar);
            DeleteObject(iPreviousFocusPlayTransportBar);
            DeleteObject(iPreviousFocusPauseTransportBar);
            DeleteObject(iNextFocusChapterTransportBar);
            DeleteObject(iPreviousFocusChapterTransportBar);
            DeleteObject(iText);
            DeleteObject(Time);
            DeleteObject(iMovieInfoImage);
            DeleteObject(Playlists);
            DeleteObject(iSettingsMenuFocusTransportBar);
            DeleteObject(iSettingsMenuFocusPauseTransportBar);
            DeleteObject(iSettingsMenuFocusPlayTransportBar);

            DeleteObject(PlayerMenu);
            DeleteObject(AudioMenu);
            DeleteObject(SubtitleMenu);
            DeleteObject(ChapterMenu);
        }

        private void CreateTextonIcon(string text, Image icon, Font font, Color textColor, Color backColor)
        {
            DeleteObject(iFastforwardIcon);

            Bitmap i0 = new Bitmap(icon);
            Image ImageText = DrawText(text, font, textColor, backColor);
            Image tb = MergeIconImage(i0, ImageText);

            i0 = new Bitmap(tb);
            iFastforwardIcon = i0.GetHbitmap();
            i0.Dispose();
            ImageText.Dispose();
            tb.Dispose();
        }

        private void ClearTransportBar()
        {
            if (iPlayFocusTransportBarIsShowing == true
               || iPauseFocusTransportBarIsShowing == true
               || iRRFocusPlayTransportBarIsShowing == true
               || iRRFocusPauseTransportBarIsShowing == true
               || iFFFocusPlayTransportBarIsShowing == true
               || iFFFocusPauseTransportBarIsShowing == true
               || iNextFocusPlayTransportBarIsShowing == true
               || iNextFocusPauseTransportBarIsShowing == true
               || iNextFocusChapterTransportBarIsShowing == true
               || iPreviousFocusPlayTransportBarIsShowing == true
               || iPreviousFocusPauseTransportBarIsShowing == true
               || iPreviousFocusChapterTransportBarIsShowing == true
               || iMovieInfoImageIsShowing == true
               || iDiscMenuFocusPlayTransportBarIsShowing == true
               || iSettingsMenuFocusPlayTransportBarIsShowing == true
               || iDiscMenuFocusPauseTransportBarIsShowing == true
               || iSettingsMenuFocusPauseTransportBarIsShowing == true
               || PlaylistsIsShowing == true
               || iSettingsMenuFocusTransportBarIsShowing == true)
            {
                MadvrInterface.ClearMadVrBitmap("iPlayFocusTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("Pause FocusTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iRRFocusPlayTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iRRFocus Pause TransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iFFFocusPlayTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iFFFocus Pause TransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iNextFocusPlayTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iNextFocus Pause TransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iNextFocusChapterTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iPreviousFocusChapterTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iPreviousFocusPlayTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iPreviousFocus Pause TransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iMovieInfoImage", madvr);
                MadvrInterface.ClearMadVrBitmap("ChapterText", madvr);
                MadvrInterface.ClearMadVrBitmap("iDiscMenuFocusPlayTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iSettingsMenuFocusPlayTransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iDiscMenuFocus Pause TransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("iSettingsMenuFocus Pause TransportBar", madvr);
                MadvrInterface.ClearMadVrBitmap("Playlists", madvr);
                MadvrInterface.ClearMadVrBitmap("iSettingsMenuFocusTransportBar", madvr);

                iPlayFocusTransportBarIsShowing = false;
                iPauseFocusTransportBarIsShowing = false;
                iRRFocusPlayTransportBarIsShowing = false;
                iRRFocusPauseTransportBarIsShowing = false;
                iFFFocusPlayTransportBarIsShowing = false;
                iFFFocusPauseTransportBarIsShowing = false;
                iNextFocusPlayTransportBarIsShowing = false;
                iNextFocusPauseTransportBarIsShowing = false;
                iNextFocusChapterTransportBarIsShowing = false;
                iPreviousFocusChapterTransportBarIsShowing = false;
                iPreviousFocusPlayTransportBarIsShowing = false;
                iPreviousFocusPauseTransportBarIsShowing = false;
                iMovieInfoImageIsShowing = false;
                iDiscMenuFocusPlayTransportBarIsShowing = false;
                iDiscMenuFocusPauseTransportBarIsShowing = false;
                iSettingsMenuFocusPlayTransportBarIsShowing = false;
                iSettingsMenuFocusPauseTransportBarIsShowing = false;
                PlaylistsIsShowing = false;
                iSettingsMenuFocusTransportBarIsShowing = false;
            }
        }

        private void DrawTimeOnTransportBar(String text, String durationText, Color textColor, Color backColor, int bmpWidth, int bmpHeight)
        {
            // Release the current GDI bitmap
            DeleteObject(Time);

            Bitmap curTimeImage = new Bitmap(bmpWidth, bmpHeight);
            Graphics myGraphic = Graphics.FromImage(curTimeImage);

            Brush textBrush = new SolidBrush(Color.White);
            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize);

            if (TimeTextHeight == 0)
            {
                SizeF textSize = myGraphic.MeasureString(text, font);
                TimeTextHeight = Convert.ToInt16(textSize.Height);
            }

            int TimePosx = (int)(125 * ScaleRatio);
            int TimePosy = (bmpHeight / 2) - ((int)(TimeTextHeight) / 2);
            myGraphic.Clear(backColor);
            myGraphic.DrawString(text, font, textBrush, TimePosx, TimePosy);

            //Add Duration Time
            SizeF durationSize = myGraphic.MeasureString(durationText, font);
            int DurationPosx = (bmpWidth - (int)(125 * ScaleRatio)) - (int)(durationSize.Width);
            myGraphic.DrawString(durationText, font, textBrush, DurationPosx, TimePosy);


            textBrush.Dispose();
            font.Dispose();

            //add progress ball
            double progressLength = 1720 - iBallWidth;
            int progressPosition = Convert.ToInt16(progressLength * (PercentComplete() / 100)) + 100;
            int Posx = (int)(progressPosition * ScaleRatio);
            int Posy = ((int)(57 * ScaleRatio) - (iBallHeight / 2));

            myGraphic.DrawImageUnscaled(TransportBallImage, Posx, Posy);
            Time = curTimeImage.GetHbitmap();

            curTimeImage.Dispose();
            myGraphic.Dispose();
        }

        private Image SetProgressOrbImage()
        {
            string TransportPath = "Icons\\Transport";
            string TransportBallPath = Path.Combine(TransportPath, "orb_nofo.png");
            TransportBallImage = Image.FromFile(TransportBallPath);
            int scaleWidth = Convert.ToInt16(TransportBallImage.Width * ScaleRatio);
            int scaleHeight = Convert.ToInt16(TransportBallImage.Height * ScaleRatio);
            TransportBallImage = ScaleImage(TransportBallImage, scaleWidth, scaleHeight);

            iBallHeight = TransportBallImage.Height;
            iBallWidth = TransportBallImage.Width;
            return TransportBallImage;
        }

        private Image SetPlaylistBackgroundImage()
        {
            string PlaylistBackgroundPath = "Icons\\Media";
            PlaylistBackgroundPath = Path.Combine(PlaylistBackgroundPath, "views_panel.png");
            PlaylistBackgroundImage = Image.FromFile(PlaylistBackgroundPath);
            int scaleWidth = Convert.ToInt16(PlaylistBackgroundImage.Width * ScaleRatio);
            int scaleHeight = Convert.ToInt16(PlaylistBackgroundImage.Height * ScaleRatio);
            PlaylistBackgroundImage = ScaleImage(PlaylistBackgroundImage, scaleWidth, scaleHeight);
            PlaylistHeight = PlaylistBackgroundImage.Height;
            PlaylistWidth = PlaylistBackgroundImage.Width;
            return PlaylistBackgroundImage;
        }

        private void DrawPlaylist(List<string> pListNames, string SelectedPlaylist, int PlaylistStart, Color textColor, Color backColor, int bmpWidth, int bmpHeight)
        {
            // Release the current GDI bitmap
            DeleteObject(Playlists);

            Bitmap curImage = new Bitmap(PlaylistBackgroundImage);
            Graphics myGraphic = Graphics.FromImage(curImage);

            Brush titleBrush = new SolidBrush(Color.White);
            Brush textBrush = new SolidBrush(Color.Gray);
            Color SelectedTextColor = Color.FromArgb(12, 108, 150);
            Brush SelectedTextBrush = new SolidBrush(SelectedTextColor);
            float titleFontSize = (float)(24 * ScaleRatio);
            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize);
            Font TitleFont = new Font("Segoe Condensed", titleFontSize);

            int PlaylistCount = 8 + PlaylistStart;
            if (pListNames.Count < 8)
            {
                PlaylistCount = pListNames.Count;
            }

            string title = "Select a Playlist:";
            SizeF titleSize = myGraphic.MeasureString(title, TitleFont);
            int TitleHeight = Convert.ToInt16(titleSize.Height);
            int TitleWidth = Convert.ToInt16(titleSize.Width);
            int TitlePosx = (PlaylistWidth / 2) - (TitleWidth / 2);
            int TitlePosy = (int)(250 * ScaleRatio) - TitleHeight;
            myGraphic.DrawString(title, TitleFont, titleBrush, TitlePosx, TitlePosy);

            int Row = 267;
            for (int i = PlaylistStart; i < PlaylistCount; i++)
            {
                string playlist = pListNames[i];
                SizeF textSize = myGraphic.MeasureString(playlist, font);
                int TextHeight = Convert.ToInt16(textSize.Height);
                int TextWidth = Convert.ToInt16(textSize.Width);
                int TimePosx = (PlaylistWidth / 2) - (TextWidth / 2);
                int TimePosy = (int)(Row * ScaleRatio) + (TextHeight / 2);
                if (playlist == SelectedPlaylist)
                {
                    myGraphic.DrawString(playlist, font, SelectedTextBrush, TimePosx, TimePosy);
                }
                else
                {
                    myGraphic.DrawString(playlist, font, textBrush, TimePosx, TimePosy);
                }
                Row = Row + 68;
            }
            titleBrush.Dispose();
            textBrush.Dispose();
            SelectedTextBrush.Dispose();
            TitleFont.Dispose();
            font.Dispose();
            Playlists = curImage.GetHbitmap();
            curImage.Dispose();
            myGraphic.Dispose();
        }

        private void DrawPlayerMenu(string MenuTitle, List<string> pListNames, string SelectedPlaylist, int PlaylistStart)
        {
            DeleteObject(PlayerMenu);

            Bitmap curImage = new Bitmap(PlaylistBackgroundImage);
            Graphics myGraphic = Graphics.FromImage(curImage);

            Brush titleBrush = new SolidBrush(Color.White);
            Brush textBrush = new SolidBrush(Color.Gray);
            Color SelectedTextColor = Color.FromArgb(12, 108, 150);  //Blue
            Brush SelectedTextBrush = new SolidBrush(SelectedTextColor);
            float titleFontSize = (float)(24 * ScaleRatio);
            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize);
            Font TitleFont = new Font("Segoe Condensed", titleFontSize);

            int PlaylistCount = 8 + PlaylistStart;
            if (pListNames.Count < 8)
            {
                PlaylistCount = pListNames.Count;
            }

            string title = MenuTitle;
            SizeF titleSize = myGraphic.MeasureString(title, TitleFont);
            int TitleHeight = Convert.ToInt16(titleSize.Height);
            int TitleWidth = Convert.ToInt16(titleSize.Width);
            int TitlePosx = (PlaylistWidth / 2) - (TitleWidth / 2);
            int TitlePosy = (int)(250 * ScaleRatio) - TitleHeight;
            myGraphic.DrawString(title, TitleFont, titleBrush, TitlePosx, TitlePosy);

            int Row = 267;
            for (int i = PlaylistStart; i < PlaylistCount; i++)
            {
                string playlist = pListNames[i];
                SizeF textSize = myGraphic.MeasureString(playlist, font);
                int TextHeight = Convert.ToInt16(textSize.Height);
                int TextWidth = Convert.ToInt16(textSize.Width);
                int TimePosx = (PlaylistWidth / 2) - (TextWidth / 2);
                int TimePosy = (int)(Row * ScaleRatio) + (TextHeight / 2);
                if (playlist == SelectedPlaylist)
                {
                    myGraphic.DrawString(playlist, font, SelectedTextBrush, TimePosx, TimePosy);
                }
                else
                {
                    myGraphic.DrawString(playlist, font, textBrush, TimePosx, TimePosy);
                }
                Row = Row + 68;
            }

            titleBrush.Dispose();
            textBrush.Dispose();
            SelectedTextBrush.Dispose();
            TitleFont.Dispose();
            font.Dispose();
            PlayerMenu = curImage.GetHbitmap();
            curImage.Dispose();
            myGraphic.Dispose();
        }

        private void DrawAudioMenu(string MenuTitle, List<string> pListNames, string SelectedPlaylist, int PlaylistStart)
        {
            DeleteObject(AudioMenu);

            Bitmap curImage = new Bitmap(PlaylistBackgroundImage);
            Graphics myGraphic = Graphics.FromImage(curImage);

            Brush titleBrush = new SolidBrush(Color.White);
            Brush textBrush = new SolidBrush(Color.Gray);
            Color SelectedTextColor = Color.FromArgb(12, 108, 150);  //Blue
            Brush SelectedTextBrush = new SolidBrush(SelectedTextColor);
            float titleFontSize = (float)(24 * ScaleRatio);
            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize);
            Font TitleFont = new Font("Segoe Condensed", titleFontSize);

            int PlaylistCount = 8 + PlaylistStart;
            if (pListNames.Count < 8)
            {
                PlaylistCount = pListNames.Count;
            }

            string title = MenuTitle;
            SizeF titleSize = myGraphic.MeasureString(title, TitleFont);
            int TitleHeight = Convert.ToInt16(titleSize.Height);
            int TitleWidth = Convert.ToInt16(titleSize.Width);
            int TitlePosx = (PlaylistWidth / 2) - (TitleWidth / 2);
            int TitlePosy = (int)(250 * ScaleRatio) - TitleHeight;
            myGraphic.DrawString(title, TitleFont, titleBrush, TitlePosx, TitlePosy);

            int Row = 267;
            for (int i = PlaylistStart; i < PlaylistCount; i++)
            {
                string playlist = pListNames[i];
                SizeF textSize = myGraphic.MeasureString(playlist, font);
                int TextHeight = Convert.ToInt16(textSize.Height);
                int TextWidth = Convert.ToInt16(textSize.Width);
                int TimePosx = (int)(120 * ScaleRatio);  // (PlaylistWidth / 2) - (TextWidth / 2);
                int TimePosy = (int)(Row * ScaleRatio) + (TextHeight / 2);
                if (playlist == SelectedPlaylist)
                {
                    myGraphic.DrawString(playlist, font, SelectedTextBrush, TimePosx, TimePosy);
                }
                else
                {
                    myGraphic.DrawString(playlist, font, textBrush, TimePosx, TimePosy);
                }
                Row = Row + 68;
            }

            titleBrush.Dispose();
            textBrush.Dispose();
            SelectedTextBrush.Dispose();
            TitleFont.Dispose();
            font.Dispose();
            AudioMenu = curImage.GetHbitmap();
            curImage.Dispose();
            myGraphic.Dispose();
        }

        private void DrawSubtitleMenu(string MenuTitle, List<string> pListNames, string SelectedPlaylist, int PlaylistStart)
        {
            DeleteObject(SubtitleMenu);

            Bitmap curImage = new Bitmap(PlaylistBackgroundImage);
            Graphics myGraphic = Graphics.FromImage(curImage);

            Brush titleBrush = new SolidBrush(Color.White);
            Brush textBrush = new SolidBrush(Color.Gray);
            Color SelectedTextColor = Color.FromArgb(12, 108, 150);  //Blue
            Brush SelectedTextBrush = new SolidBrush(SelectedTextColor);
            float titleFontSize = (float)(24 * ScaleRatio);
            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize);
            Font TitleFont = new Font("Segoe Condensed", titleFontSize);

            int PlaylistCount = 8 + PlaylistStart;
            if (pListNames.Count < 8)
            {
                PlaylistCount = pListNames.Count;
            }

            string title = MenuTitle;
            SizeF titleSize = myGraphic.MeasureString(title, TitleFont);
            int TitleHeight = Convert.ToInt16(titleSize.Height);
            int TitleWidth = Convert.ToInt16(titleSize.Width);
            int TitlePosx = (PlaylistWidth / 2) - (TitleWidth / 2);
            int TitlePosy = (int)(250 * ScaleRatio) - TitleHeight;
            myGraphic.DrawString(title, TitleFont, titleBrush, TitlePosx, TitlePosy);

            int Row = 267;
            for (int i = PlaylistStart; i < PlaylistCount; i++)
            {
                string playlist = pListNames[i];
                SizeF textSize = myGraphic.MeasureString(playlist, font);
                int TextHeight = Convert.ToInt16(textSize.Height);
                int TextWidth = Convert.ToInt16(textSize.Width);
                int TimePosx = (int)(120 * ScaleRatio);  // (PlaylistWidth / 2) - (TextWidth / 2);
                int TimePosy = (int)(Row * ScaleRatio) + (TextHeight / 2);
                if (playlist == SelectedPlaylist)
                {
                    myGraphic.DrawString(playlist, font, SelectedTextBrush, TimePosx, TimePosy);
                }
                else
                {
                    myGraphic.DrawString(playlist, font, textBrush, TimePosx, TimePosy);
                }
                Row = Row + 68;
            }

            titleBrush.Dispose();
            textBrush.Dispose();
            SelectedTextBrush.Dispose();
            TitleFont.Dispose();
            font.Dispose();
            SubtitleMenu = curImage.GetHbitmap();
            curImage.Dispose();
            myGraphic.Dispose();
        }

        private void DrawChapterMenu(string MenuTitle, List<string> pListNames, string SelectedPlaylist, int PlaylistStart)
        {
            DeleteObject(ChapterMenu);

            Bitmap curImage = new Bitmap(PlaylistBackgroundImage);
            Graphics myGraphic = Graphics.FromImage(curImage);

            Brush titleBrush = new SolidBrush(Color.White);
            Brush textBrush = new SolidBrush(Color.Gray);
            Color SelectedTextColor = Color.FromArgb(12, 108, 150);  //Blue
            Brush SelectedTextBrush = new SolidBrush(SelectedTextColor);
            float titleFontSize = (float)(24 * ScaleRatio);
            float fontSize = (float)(20 * ScaleRatio);
            Font font = new Font("Segoe Condensed", fontSize);
            Font TitleFont = new Font("Segoe Condensed", titleFontSize);

            int PlaylistCount = 8 + PlaylistStart;
            if (pListNames.Count < 8)
            {
                PlaylistCount = pListNames.Count;
            }

            string title = MenuTitle;
            SizeF titleSize = myGraphic.MeasureString(title, TitleFont);
            int TitleHeight = Convert.ToInt16(titleSize.Height);
            int TitleWidth = Convert.ToInt16(titleSize.Width);
            int TitlePosx = (PlaylistWidth / 2) - (TitleWidth / 2);
            int TitlePosy = (int)(250 * ScaleRatio) - TitleHeight;
            myGraphic.DrawString(title, TitleFont, titleBrush, TitlePosx, TitlePosy);

            int Row = 267;
            for (int i = PlaylistStart; i < PlaylistCount; i++)
            {
                string playlist = pListNames[i];
                SizeF textSize = myGraphic.MeasureString(playlist, font);
                int TextHeight = Convert.ToInt16(textSize.Height);
                int TextWidth = Convert.ToInt16(textSize.Width);
                int TimePosx = (PlaylistWidth / 2) - (TextWidth / 2);
                int TimePosy = (int)(Row * ScaleRatio) + (TextHeight / 2);
                if (playlist == SelectedPlaylist)
                {
                    myGraphic.DrawString(playlist, font, SelectedTextBrush, TimePosx, TimePosy);
                }
                else
                {
                    myGraphic.DrawString(playlist, font, textBrush, TimePosx, TimePosy);
                }
                Row = Row + 68;
            }

            titleBrush.Dispose();
            textBrush.Dispose();
            SelectedTextBrush.Dispose();
            TitleFont.Dispose();
            font.Dispose();
            ChapterMenu = curImage.GetHbitmap();
            curImage.Dispose();
            myGraphic.Dispose();
        }

        #endregion

        #region Directshow Graph

        private void SetupGraph()
        {
            MeediosWindowHandle = GetForegroundWindow();
            GetMonitorSettings();

            string fileSource = m_sourceUri.OriginalString;
            int hr;

            try
            {
                #region Create Directshow Graph
                /* Creates the GraphBuilder COM object */
                InitializeGraph();

                if (m_graph == null)
                {
                    throw new Exception("Could not create a graph");
                }

                //Add our prefered audio renderer

                //if (Madvr_VideoPlayer.Plugin.AudioRenderer == "Default Audio Renderer")
                //{
                defaultAudioRenderer = new DefaultAudioRenderer();
                var aRenderer = defaultAudioRenderer as DirectShowLib.IBaseFilter;
                if (aRenderer != null)
                {
                    m_graph.AddFilter(aRenderer, "Default Audio Renderer");
                }
                //}
                //else if (Madvr_VideoPlayer.Plugin.AudioRenderer == "Reclock Audio Renderer")
                //{
                //    reclockAudioRenderer = new ReclockAudioRenderer();
                //    var aRenderer = reclockAudioRenderer as DirectShowLib.IBaseFilter;
                //    if (aRenderer != null)
                //    {
                //        Madvr_VideoPlayer.Plugin.WriteLog("Adding Reclock Audio Renderer to graph");
                //        m_graph.AddFilter(aRenderer, "Reclock Audio Renderer");
                //    }
                //}

                filterGraph = m_graph as DirectShowLib.IFilterGraph2;
                if (filterGraph == null)
                {
                    throw new Exception("Could not QueryInterface for the IFilterGraph2");
                }

                //ADD LAV FILTER TO DIRECTSHOW GRAPH

                lavvideo = new LAVVideo();
                var vlavvideo = lavvideo as DirectShowLib.IBaseFilter;
                if (vlavvideo != null)
                {
                    m_graph.AddFilter(vlavvideo, "LAV Video Decoder");
                }

                lavaudio = new LAVAudio();
                var vlavaudio = lavaudio as DirectShowLib.IBaseFilter;
                if (vlavaudio != null)
                {
                    m_graph.AddFilter(vlavaudio, "LAV Audio Decoder");
                }

                //ADD MADVR RENDERER FILTER TO DIRECTSHOW GRAPH

                madvr = new MadVR();
                var Vmadvr = madvr as DirectShowLib.IBaseFilter;
                if (Vmadvr != null)
                {
                    m_graph.AddFilter(Vmadvr, "MadVR Video Renderer");
                }

                //m_graph.SetDefaultSyncSource();
                //filterGraph.SetDefaultSyncSource();

                #endregion

                #region DVD

                if (fileSource.ToLower().EndsWith("video_ts.ifo"))
                {
                    DiscIsDVD = true;
                    try
                    {
                        /* Create a new DVD Navigator. */
                        dvdNav = (DirectShowLib.IBaseFilter)new DVDNavigator();

                        /* The DVDControl2 interface lets us control DVD features */
                        m_dvdControl = dvdNav as IDvdControl2;

                        if (m_dvdControl == null)
                            throw new Exception("Could not QueryInterface the IDvdControl2 interface");

                        /* QueryInterface the DVDInfo2 */
                        m_dvdInfo = dvdNav as IDvdInfo2;

                        /* If a Dvd directory has been set then use it, if not, let DShow find the Dvd */
                        string DvdDirectory = System.IO.Path.GetDirectoryName(fileSource);

                        if (!string.IsNullOrEmpty(DvdDirectory))
                        {
                            hr = m_dvdControl.SetDVDDirectory(DvdDirectory);
                            DsError.ThrowExceptionForHR(hr);
                        }

                        /* This gives us the DVD time in Hours-Minutes-Seconds-Frame time format, and other options */
                        hr = m_dvdControl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true);
                        DsError.ThrowExceptionForHR(hr);

                        /* If the graph stops, resume at the same point */
                        m_dvdControl.SetOption(DvdOptionFlag.ResetOnStop, false);

                        hr = m_graph.AddFilter(dvdNav, "DVD Navigator");
                        DsError.ThrowExceptionForHR(hr);


                        int uTitle = 1;
                        dma = new DvdMenuAttributes();
                        dta = new DvdTitleAttributes();
                        m_dvdInfo.GetTitleAttributes(uTitle, out dma, dta);

                        int iX = dta.VideoAttributes.aspectX;
                        int iY = dta.VideoAttributes.aspectY;
                        bool lb = dta.VideoAttributes.isSourceLetterboxed;
                        int sX = dta.VideoAttributes.sourceResolutionX;
                        int sY = dta.VideoAttributes.sourceResolutionY;


                        #region RENDER FILTER PINS

                        /* We will want to enum all the pins on the source filter */
                        DirectShowLib.IEnumPins pinEnum;

                        hr = dvdNav.EnumPins(out pinEnum);
                        DsError.ThrowExceptionForHR(hr);


                        IntPtr fetched = IntPtr.Zero;
                        DirectShowLib.IPin[] pins = { null };

                        /* Counter for how many pins successfully rendered */
                        int pinsRendered = 0;

                        /* Loop over each pin of the source filter */
                        while (pinEnum.Next(pins.Length, pins, fetched) == 0)
                        {
                            if (filterGraph.RenderEx(pins[0], AMRenderExFlags.RenderToExistingRenderers, IntPtr.Zero) >= 0)
                            {
                                pinsRendered++;
                            }
                            Marshal.ReleaseComObject(pins[0]);
                        }

                        Marshal.ReleaseComObject(pinEnum);
                        //Marshal.ReleaseComObject(dvdNav);

                        if (pinsRendered == 0)
                        {
                            throw new Exception("Could not render any streams from the source Uri");
                        }

                        /* Configure the graph in the base class */
                        //SetupFilterGraph(m_graph);
                        DumpGraph(m_graph);
                        #endregion

                        SetVideoWindow(iX, iY);
                    }
                    catch (Exception)
                    {
                        //FreeResources();
                        TearDownGraph();
                        return;
                    }
                }

                #endregion

                #region Blu-Ray and Media Files
                else
                {
                    DriveLetter = fileSource.Substring(0, 1);
                    if (Directory.Exists(DriveLetter + ":\\BDMV"))
                    {
                        DiscIsBluRay = true;
                    }

                    //DirectShowLib.IBaseFilter sourceFilter = null;

                    /* Have DirectShow find the correct source filter for the Uri */
                    hr = filterGraph.AddSourceFilter(fileSource, fileSource, out sourceFilter);
                    DsError.ThrowExceptionForHR(hr);

                    #region RENDER FILTER PINS

                    /* We will want to enum all the pins on the source filter */
                    DirectShowLib.IEnumPins pinEnum;

                    hr = sourceFilter.EnumPins(out pinEnum);
                    DsError.ThrowExceptionForHR(hr);

                    IntPtr fetched = IntPtr.Zero;
                    DirectShowLib.IPin[] pins = { null };

                    /* Counter for how many pins successfully rendered */
                    int pinsRendered = 0;
                    /* Loop over each pin of the source filter */
                    while (pinEnum.Next(pins.Length, pins, fetched) == 0)
                    {
                        if (filterGraph.RenderEx(pins[0], AMRenderExFlags.RenderToExistingRenderers, IntPtr.Zero) >= 0)
                            pinsRendered++;

                        Marshal.ReleaseComObject(pins[0]);
                    }
                    Marshal.ReleaseComObject(pinEnum);
                    //Marshal.ReleaseComObject(sourceFilter);

                    if (pinsRendered == 0)
                    {
                        throw new Exception("Could not render any streams from the source Uri");
                    }
                    /* Configure the graph in the base class */
                    //SetupFilterGraph(m_graph);
                    DumpGraph(m_graph);
                    #endregion

                    SetVideoWindow(0, 0);
                }
                #endregion

            }
            catch (Exception)
            {
                /* This exection will happen usually if the media does
                 * not exist or could not open due to not having the
                 * proper filters installed */
                TearDownGraph();
                return;
            }

            HasVideo = true;

            // Get the seeking capabilities.
            hr = m_pSeek.GetCapabilities(out m_seekCaps);
            DsError.ThrowExceptionForHR(hr);

            // Update our state.
            m_state = PlaybackState.Stopped;

            Play();

            SetImages();
            if (DiscIsDVD == false)
            {
                GetTotalChapters();
            }
            else if (DiscIsDVD == true)
            {
                DvdTotalChapters();
                StartDvdDomainThread();
                ShowRootMenu();
            }
        }

        private void InitializeGraph()
        {
            disposed = false;
            ReleasePlayerImages();
            EndThreads();

            TearDownGraph();

            // Create the Filter Graph Manager.
            m_graph = new FilterGraphNoThread() as DirectShowLib.IGraphBuilder;

            // Query for graph interfaces. (These interfaces are exposed by the graph
            // manager regardless of which filters are in the graph.)
            m_pControl = (DirectShowLib.IMediaControl)m_graph;
            m_pSeek = (DirectShowLib.IMediaSeeking)m_graph;
            mediaPosition = (DirectShowLib.IMediaPosition)m_graph;
            mediaEventEx = (DirectShowLib.IMediaEventEx)m_graph;
        }

        private void TearDownGraph()
        {
            if (videoWindow != null)
            {
                Marshal.ReleaseComObject(videoWindow);
                videoWindow = null;
            }
            if (mediaEventEx != null)
            {
                mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                mediaEventEx = null;
            }
            if (m_dvdControl != null)
            {
                m_dvdControl = null;
            }
            if (m_dvdInfo != null)
            {
                m_dvdInfo = null;
            }
            if (m_dvdCmd != null)
            {
                m_dvdCmd = null;
            }
            if (dta != null)
            {
                dta = null;
            }
            if (_DVDCurrentPlaybackTime != null)
            {
                _DVDCurrentPlaybackTime = null;
            }
            if (m_pSeek != null)
            {
                m_pSeek = null;
            }
            if (m_pControl != null)
            {
                m_pControl.Stop();
                m_pControl = null;
            }
            if (mediaPosition != null)
            {
                mediaPosition = null;
            }
            if (eSeek != null)
            {
                eSeek = null;
            }
            if (basicVideo != null)
            {
                basicVideo = null;
            }
            if (basicVideo2 != null)
            {
                basicVideo2 = null;
            }
            ReleaseAllFiters();

            if (sourceFilter != null)
            {
                Marshal.ReleaseComObject(sourceFilter);
                sourceFilter = null;
            }
            if (dvdNav != null)
            {
                Marshal.ReleaseComObject(dvdNav);
                dvdNav = null;
            }
            if (madvr != null)
            {
                Marshal.ReleaseComObject(madvr);
                madvr = null;
            }
            if (lavaudio != null)
            {
                Marshal.ReleaseComObject(lavaudio);
                lavaudio = null;
            }
            if (lavvideo != null)
            {
                Marshal.ReleaseComObject(lavvideo);
                lavvideo = null;
            }
            if (defaultAudioRenderer != null)
            {
                Marshal.ReleaseComObject(defaultAudioRenderer);
                defaultAudioRenderer = null;
            }
            if (reclockAudioRenderer != null)
            {
                Marshal.ReleaseComObject(reclockAudioRenderer);
                reclockAudioRenderer = null;
            }
            if (m_graph != null)
            {
                Marshal.ReleaseComObject(m_graph);
                m_graph = null;
            }
            if (filterGraph != null)
            {
                Marshal.ReleaseComObject(filterGraph);
                filterGraph = null;
            }

            m_state = PlaybackState.Closed;
            m_seekCaps = 0;
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    ReleasePlayerImages();
                    EndThreads();
                    TearDownGraph();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        private static void DumpGraph(DirectShowLib.IGraphBuilder graph)
        {
            IEnumFilters enumFilters;
            /* We will want to enum all the pins on the source filter */
            DirectShowLib.IEnumPins pinEnum;

            IntPtr fetched = IntPtr.Zero;
            DirectShowLib.IPin[] pins = { null };

            int hr = graph.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                /* This array is filled with reference to a filter */
                var filters = new DirectShowLib.IBaseFilter[1];

                /* Get reference to all the filters */
                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    FilterInfo fi;
                    filters[0].QueryFilterInfo(out fi);
                    hr = filters[0].EnumPins(out pinEnum);
                    DsError.ThrowExceptionForHR(hr);

                    /* Loop over each pin of the source filter */
                    while (pinEnum.Next(pins.Length, pins, fetched) == 0)
                    {
                        PinInfo pi;

                        pins[0].QueryPinInfo(out pi);
                        DirectShowLib.IPin pConnected = null;
                        String connected;
                        if (pins[0].ConnectedTo(out pConnected) == 0 && pConnected != null)
                            connected = string.Format("Connected to:{0}", pConnected.GetHashCode());
                        else
                            connected = "Not Connected";

                        Marshal.ReleaseComObject(pins[0]);
                    }
                    Marshal.ReleaseComObject(pinEnum);
                }
            }
            finally
            {
                /* Enum filters is a COM, so release that */
                Marshal.ReleaseComObject(enumFilters);
            }
        }

        public void SetVideoWindow(int SourceWidth, int SourceHeight)
        {
            videoWindow = (DirectShowLib.IVideoWindow)m_graph;
            basicVideo = (DirectShowLib.IBasicVideo)m_graph;
            basicVideo2 = (IBasicVideo2)m_graph;

            //GET MEDIA VIDEO SIZE
            int videoW;
            int videoH;
            basicVideo.GetVideoSize(out videoW, out videoH);
            if (SourceWidth > 0 || SourceHeight > 0)
            {
                videoW = SourceWidth;
                videoH = SourceHeight;
            }

            //MeediosWindowHandle = GetForegroundWindow();
            ////REGISTER MADVR WINDOW WITH KEYBOARD PLUGIN
            //MeediOS.IMeedioMessage message = Madvr_VideoPlayer.Plugin.MyCore.NewMessage(MeediOS.MessageSubject.UI.Register, this);
            //message["handle"] = MeediosWindowHandle;
            //message.Send();
            
            // We have to pass this in for MB3 to work properly.
            videoWindow.put_Owner(MB3PlayingWindowHandle);

            // Have the graph signal event via window callbacks for performance
            //MeediosWindowHandle = GetForegroundWindow();
            //mediaEventEx.SetNotifyWindow(MeediosWindowHandle, WM_DVD_EVENT, new IntPtr(this.GetHashCode()));
            //DsError.ThrowExceptionForHR(hr);
            //Madvr_VideoPlayer.Plugin.WriteLog("Set Notify Window: " + DsError.GetErrorText(hr));

            videoWindow.SetWindowPosition(0, 0, ScreenWidth, ScreenHeight);
            videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.Visible | WindowStyle.ClipSiblings);

            //Get Aspect Ratio
            int AspectX = 0;
            int AspectY = 0;

            decimal HeightAsPercentOfWidth = 0;
            basicVideo2.GetPreferredAspectRatio(out AspectX, out AspectY);

            if (SourceWidth > 0 || SourceHeight > 0)
            {
                AspectX = SourceWidth;
                AspectY = SourceHeight;
            }
            if (AspectX > 0 && AspectY > 0)
            {
                HeightAsPercentOfWidth = decimal.Divide(AspectY, AspectX);
            }

            //Adjust Video Size
            double AdjustedHeight = 0;
            int iAdjustedHeight = 0;
            if (AspectX > 0 && AspectY > 0)
            {
                AdjustedHeight = Convert.ToDouble(HeightAsPercentOfWidth * ScreenWidth);
                iAdjustedHeight = Convert.ToInt32(Math.Round(AdjustedHeight));
            }

            //SET MADVR WINDOW TO FULL SCREEN AND SET POSITION
            if (ScreenHeight >= iAdjustedHeight && iAdjustedHeight > 0)
            {
                double TotalMargin = (ScreenHeight - iAdjustedHeight);
                int TopMargin = Convert.ToInt32(Math.Round(TotalMargin / 2));
                basicVideo.SetDestinationPosition(0, TopMargin, ScreenWidth, iAdjustedHeight);
            }
            else if (ScreenHeight < iAdjustedHeight && iAdjustedHeight > 0)
            {
                double AdjustedWidth = Convert.ToDouble(ScreenHeight / HeightAsPercentOfWidth);
                int iAdjustedWidth = Convert.ToInt32(Math.Round(AdjustedWidth));
                if (iAdjustedWidth == 1919)
                    iAdjustedWidth = 1920;
                double TotalMargin = (ScreenWidth - iAdjustedWidth);
                int LeftMargin = Convert.ToInt32(Math.Round(TotalMargin / 2));
                basicVideo.SetDestinationPosition(LeftMargin, 0, iAdjustedWidth, ScreenHeight);
            }
            else if (iAdjustedHeight == 0)
            {
                videoWindow.SetWindowPosition(0, 0, ScreenWidth, ScreenHeight);
            }
            //if (Madvr_VideoPlayer.Plugin.HidePointer == true)
            //{
            //    videoWindow.HideCursor(OABool.True);
            //}
            //else
            //{
            videoWindow.HideCursor(OABool.False);
            //}
        }

        public void ReleaseAllFiters()
        {
            if (m_graph != null)
            {
                IEnumFilters enumFilters;
                int hr = m_graph.EnumFilters(out enumFilters);
                if (hr == 0)
                {
                    List<DirectShowLib.IBaseFilter> filterList = new List<DirectShowLib.IBaseFilter>();

                    DirectShowLib.IBaseFilter[] filters = new DirectShowLib.IBaseFilter[1];
                    
                    while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                    {
                        filterList.Add(filters[0]);
                    }

                    foreach (DirectShowLib.IBaseFilter filter in filterList)
                    {
                        FilterInfo filterInfo;
                        hr = filter.QueryFilterInfo(out filterInfo);
                        if (filter != null)
                        {
                            m_graph.RemoveFilter(filter);
                            Marshal.ReleaseComObject(filter);
                        }
                    }
                }
                Marshal.ReleaseComObject(enumFilters);
            }
        }

        public void EndThreads()
        {
            if (DurationThread != null && DurationThread.IsAlive)
            {
                ShowTime = false;
                DurationThread.Join();
            }
            if (SeekThread != null && SeekThread.IsAlive)
                SeekThread.Abort();

            if (DvdDomainThread != null && DvdDomainThread.IsAlive)
            {
                DvdDomainThreadActive = false;
                DvdDomainThread.Join();
            }
        }

        #endregion

        #region Helper Methods

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private System.Timers.Timer timer = null;
        public void StartTimer(double interval)
        {
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += _timer_Elapsed;
            timer.Enabled = true;
            timer.AutoReset = false;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CommandFromTaskBar == false)
            {
                ShowTime = false;
                DurationThread.Join();
                ClearTransportBar();
                timer.Close();
                timer.Dispose();
                timer = null;
                return;
            }
            else if (iNextFocusChapterTransportBarIsShowing == true)
            {
                if (m_state == PlaybackState.Running)
                {
                    ClearTransportBar();
                    MadvrInterface.ShowMadVrBitmap("iNextFocus Pause TransportBar", iNextFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    iNextFocusPauseTransportBarIsShowing = true;
                    StartDurationThread();
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                    return;
                }
                else if (m_state == PlaybackState.Paused || m_state == PlaybackState.Stopped)
                {
                    ClearTransportBar();
                    MadvrInterface.ShowMadVrBitmap("iNextFocusPlayTransportBar", iNextFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    iNextFocusPlayTransportBarIsShowing = true;
                    StartDurationThread();
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                    return;
                }
            }
            else if (iPreviousFocusChapterTransportBarIsShowing == true)
            {
                if (m_state == PlaybackState.Running)
                {
                    ClearTransportBar();
                    MadvrInterface.ShowMadVrBitmap("iPreviousFocus Pause TransportBar", iPreviousFocusPauseTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    iPreviousFocusPauseTransportBarIsShowing = true;
                    StartDurationThread();
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                    return;
                }
                else if (m_state == PlaybackState.Paused || m_state == PlaybackState.Stopped)
                {
                    ClearTransportBar();
                    MadvrInterface.ShowMadVrBitmap("iPreviousFocusPlayTransportBar", iPreviousFocusPlayTransportBar, 0, posyTB, 0, ImagePriority, madvr);
                    iPreviousFocusPlayTransportBarIsShowing = true;
                    StartDurationThread();
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                    return;
                }
            }

            else if (iNextFocusChapterTransportBarIsShowing == false
            && iPreviousFocusChapterTransportBarIsShowing == false)
            {
                if (iNextFocusPauseTransportBarIsShowing == true
               || iNextFocusPlayTransportBarIsShowing == true)
                {
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                    GoToNextChapter(false);
                    return;
                }
                else if (iPreviousFocusPauseTransportBarIsShowing == true
               || iPreviousFocusPlayTransportBarIsShowing == true)
                {
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                    GoToPreviousChapter(false);
                    return;
                }
            }
        }

        public class MovieInfo
        {
            public string Title = "";
            public string Year = "";
            public string MPAARating = "";
            public string image = "";
            public string Overview = "";
            public List<string> Actors = new List<string>();
            public string Tagline = "";
            public string Director = "";
            public List<string> Genre = new List<string>();
            public string audio_codec = "";
            public string video_codec = "";
            public List<string> Awards = new List<string>();
            public string audio_channels = "";
            public string Media_Type = "";
            public string MeediosId;
            public List<string> Studio = new List<string>();
            public string runtime = "";
        }

        //public string GetGeneralPluginProperty(string PluginId, string PropertyName)
        //{
        //    string CFN = "configuration.xml";
        //    MeediOS.Configuration.MeedioConfiguration config = MeediOS.Configuration.MeedioConfiguration.Deserialize(System.IO.Path.Combine(Madvr_VideoPlayer.Plugin.MyCore.GetDataDirectory(string.Empty), CFN));
        //    foreach (MeediOS.Configuration.MeedioPluginConfiguration MPC in config.plugins)
        //    {
        //        if (MPC.description["plugin_id"].ToString() == PluginId)
        //        {
        //            foreach (string key in MPC.plugin_properties.Keys)
        //            {
        //                if (key == PropertyName)
        //                {
        //                    return MPC.plugin_properties[key].ToString();
        //                }
        //            }
        //        }
        //    }
        //    return "";
        //}

        public void GetMonitorSettings()
        {
            IntPtr ip = GetForegroundWindow();
            Screen monitor = Screen.FromHandle(ip);
            ScreenWidth = monitor.Bounds.Width;
            ScreenHeight = monitor.Bounds.Height;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        #endregion

    }
}
