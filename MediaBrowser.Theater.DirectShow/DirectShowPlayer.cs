using System.IO;
using DirectShowLib;
using DirectShowLib.Dvd;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace MediaBrowser.Theater.DirectShow
{
    public class DirectShowPlayer : Form
    {
        private const int WM_APP = 0x8000;
        private const int WM_GRAPHNOTIFY = WM_APP + 1;
        private const int EC_COMPLETE = 0x01;

        private readonly ILogger _logger;
        private readonly IHiddenWindow _hiddenWindow;
        private readonly InternalDirectShowPlayer _playerWrapper;

        private DirectShowLib.IGraphBuilder m_graph;

        private DirectShowLib.IMediaControl _mediaControl;
        private DirectShowLib.IMediaEventEx _mediaEventEx;
        private DirectShowLib.IVideoWindow _videoWindow;
        private DirectShowLib.IBasicAudio _basicAudio;
        private DirectShowLib.IBasicVideo _basicVideo;
        private DirectShowLib.IMediaSeeking _mediaSeeking;
        private DirectShowLib.IMediaPosition _mediaPosition;
        private DirectShowLib.IBaseFilter _sourceFilter;
        private DirectShowLib.IFilterGraph2 _filterGraph;
        private DirectShowLib.IBaseFilter _pSource;

        private XYVSFilter _xyVsFilter;
        private XySubFilter _xySubFilter;

        private LAVAudio _lavaudio;
        private LAVVideo _lavvideo;

        // EVR filter
        private DirectShowLib.IBaseFilter _mPEvr;
        private IMFVideoDisplayControl _mPDisplay;

        private DefaultAudioRenderer _defaultAudioRenderer;
        private ReclockAudioRenderer _reclockAudioRenderer;

        // Caps bits for IMediaSeeking
        private AMSeekingSeekingCapabilities _mSeekCaps;

        // Dvd
        private DirectShowLib.IBaseFilter _dvdNav;
        private IDvdControl2 _mDvdControl;
        private IDvdInfo2 _mDvdInfo;

        private MadVR _madvr;

        private PlayableItem _item;

        public DirectShowPlayer(ILogger logger, IHiddenWindow hiddenWindow, InternalDirectShowPlayer playerWrapper)
        {
            _logger = logger;
            _hiddenWindow = hiddenWindow;
            _playerWrapper = playerWrapper;
        }

        private PlayState _playstate;
        public PlayState PlayState
        {
            get { return _playstate; }
            private set
            {
                _playstate = value;

                _playerWrapper.OnPlayStateChanged();
            }
        }

        public long? CurrentPositionTicks
        {
            get
            {
                if (_mediaSeeking != null && PlayState != PlayState.Idle)
                {
                    long pos;

                    var hr = _mediaSeeking.GetCurrentPosition(out pos);

                    return pos;
                }

                return null;
            }
        }

        public long? CurrentDurationTicks
        {
            get
            {
                if (_mDvdInfo != null && PlayState != PlayState.Idle)
                {
                    var totaltime = new DvdHMSFTimeCode();
                    DvdTimeCodeFlags ulTimeCodeFlags;
                    _mDvdInfo.GetTotalTitleTime(totaltime, out ulTimeCodeFlags);

                    return new TimeSpan(totaltime.bHours, totaltime.bMinutes, totaltime.bSeconds).Ticks;
                }
                if (_mediaSeeking != null && PlayState != PlayState.Idle)
                {
                    long pos;

                    var hr = _mediaSeeking.GetDuration(out pos);

                    return pos;
                }

                return null;
            }
        }

        public void Play(PlayableItem item, bool enableReclock, bool enableMadvr, bool enableXySubFilter)
        {
            _logger.Info("Playing {0}. Reclock: {1}, Madvr: {2}, xySubFilter: {3}", item.OriginalItem.Name, enableReclock, enableMadvr, enableXySubFilter);

            _item = item;

            var isDvd = (item.OriginalItem.VideoType ?? VideoType.VideoFile) == VideoType.Dvd || (item.OriginalItem.IsoType ?? IsoType.BluRay) == IsoType.Dvd && item.PlayablePath.IndexOf("http://", StringComparison.OrdinalIgnoreCase) == -1;

            Initialize(item.PlayablePath, enableReclock, enableMadvr, enableXySubFilter, isDvd);

            var hr = _mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);

            PlayState = PlayState.Playing;
        }

        private void InitializeGraph()
        {
            m_graph = (DirectShowLib.IGraphBuilder)new FilterGraphNoThread();

            // QueryInterface for DirectShow interfaces
            _mediaControl = (DirectShowLib.IMediaControl)m_graph;
            _mediaEventEx = (DirectShowLib.IMediaEventEx)m_graph;
            _mediaSeeking = (DirectShowLib.IMediaSeeking)m_graph;
            _mediaPosition = (DirectShowLib.IMediaPosition)m_graph;

            // Query for video interfaces, which may not be relevant for audio files
            _videoWindow = m_graph as DirectShowLib.IVideoWindow;
            _basicVideo = m_graph as DirectShowLib.IBasicVideo;

            // Query for audio interfaces, which may not be relevant for video-only files
            _basicAudio = m_graph as DirectShowLib.IBasicAudio;

            // Set up event notification.
            var hr = _mediaEventEx.SetNotifyWindow(Handle, WM_GRAPHNOTIFY, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
        }

        private void Initialize(string path, bool enableReclock, bool enableMadvr, bool enableXySubFilter, bool isDvd)
        {
            InitializeGraph();

            int hr = 0;

            if (!isDvd)
            {
                hr = m_graph.AddSourceFilter(path, path, out _pSource);
                DsError.ThrowExceptionForHR(hr);

                // Try to render the streams.
                RenderStreams(_pSource, enableReclock, enableMadvr, enableXySubFilter);
            }
            else
            {
                _logger.Debug("Initializing dvd player to play {0}", path);

                /* Create a new DVD Navigator. */
                _dvdNav = (DirectShowLib.IBaseFilter)new DVDNavigator();

                InitializeDvd(path);

                // Try to render the streams.
                RenderStreams(_dvdNav, enableReclock, enableMadvr, enableXySubFilter);
            }

            // Get the seeking capabilities.
            hr = _mediaSeeking.GetCapabilities(out _mSeekCaps);
            DsError.ThrowExceptionForHR(hr);
        }

        private void InitializeDvd(string path)
        {
            /* The DVDControl2 interface lets us control DVD features */
            _mDvdControl = _dvdNav as IDvdControl2;

            if (_mDvdControl == null)
                throw new Exception("Could not QueryInterface the IDvdControl2 interface");

            /* QueryInterface the DVDInfo2 */
            _mDvdInfo = _dvdNav as IDvdInfo2;

            var videoTsPath = Path.Combine(path, "video_ts");
            if (Directory.Exists(videoTsPath))
            {
                path = videoTsPath;
            }

            /* If a Dvd directory has been set then use it, if not, let DShow find the Dvd */
            var hr = _mDvdControl.SetDVDDirectory(path);
            DsError.ThrowExceptionForHR(hr);

            /* This gives us the DVD time in Hours-Minutes-Seconds-Frame time format, and other options */
            hr = _mDvdControl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true);
            DsError.ThrowExceptionForHR(hr);

            /* If the graph stops, resume at the same point */
            _mDvdControl.SetOption(DvdOptionFlag.ResetOnStop, false);

            hr = m_graph.AddFilter(_dvdNav, "DVD Navigator");
            DsError.ThrowExceptionForHR(hr);

            //int uTitle = 1;
            //dma = new DvdMenuAttributes();
            //dta = new DvdTitleAttributes();
            //m_dvdInfo.GetTitleAttributes(uTitle, out dma, dta);

            //int iX = dta.VideoAttributes.aspectX;
            //int iY = dta.VideoAttributes.aspectY;
            //DvdIsLetterBoxed = dta.VideoAttributes.isSourceLetterboxed;
            //int sX = dta.VideoAttributes.sourceResolutionX;
            //int sY = dta.VideoAttributes.sourceResolutionY;
        }

        private void RenderStreams(DirectShowLib.IBaseFilter pSource, bool enableReclock, bool enableMadvr, bool enableXySubFilter)
        {
            int hr;

            _filterGraph = m_graph as DirectShowLib.IFilterGraph2;
            if (_filterGraph == null)
            {
                throw new Exception("Could not QueryInterface for the IFilterGraph2");
            }

            // Add video renderer
            if (_item.IsVideo)
            {
                _mPEvr = (DirectShowLib.IBaseFilter)new EnhancedVideoRenderer();
                hr = m_graph.AddFilter(_mPEvr, "EVR");
                DsError.ThrowExceptionForHR(hr);

                InitializeEvr(_mPEvr, 1);
            }

            // Add audio renderer
            var useDefaultRenderer = true;

            if (enableReclock)
            {
                try
                {
                    _reclockAudioRenderer = new ReclockAudioRenderer();
                    var aRenderer = _reclockAudioRenderer as DirectShowLib.IBaseFilter;
                    if (aRenderer != null)
                    {
                        hr = m_graph.AddFilter(aRenderer, "Reclock Audio Renderer");
                        DsError.ThrowExceptionForHR(hr);
                        useDefaultRenderer = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding reclock filter", ex);
                }
            }

            if (useDefaultRenderer)
            {
                _defaultAudioRenderer = new DefaultAudioRenderer();
                var aRenderer = _defaultAudioRenderer as DirectShowLib.IBaseFilter;
                if (aRenderer != null)
                {
                    m_graph.AddFilter(aRenderer, "Default Audio Renderer");
                }
            }

            if (_item.IsVideo)
            {
                try
                {
                    _lavvideo = new LAVVideo();
                    var vlavvideo = _lavvideo as DirectShowLib.IBaseFilter;
                    if (vlavvideo != null)
                    {
                        hr = m_graph.AddFilter(vlavvideo, "LAV Video Decoder");
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding LAV Video filter", ex);
                }

                try
                {
                    _lavaudio = new LAVAudio();
                    var vlavaudio = _lavaudio as DirectShowLib.IBaseFilter;
                    if (vlavaudio != null)
                    {
                        hr = m_graph.AddFilter(vlavaudio, "LAV Audio Decoder");
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding LAV Audio filter", ex);
                }

                if (_item.IsVideo)
                {
                    var xySubFilterSucceeded = false;

                    if (enableMadvr)
                    {
                        var madVrSucceded = false;

                        try
                        {
                            _madvr = new MadVR();
                            var vmadvr = _madvr as DirectShowLib.IBaseFilter;
                            if (vmadvr != null)
                            {
                                hr = m_graph.AddFilter(vmadvr, "MadVR Video Renderer");
                                DsError.ThrowExceptionForHR(hr);
                            }

                            madVrSucceded = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Error adding MadVR filter", ex);
                        }

                        // Load xySubFilter if configured and if madvr succeeded
                        if (enableXySubFilter && madVrSucceded)
                        {
                            try
                            {
                                _xySubFilter = new XySubFilter();
                                var vxySubFilter = _xySubFilter as DirectShowLib.IBaseFilter;
                                if (vxySubFilter != null)
                                {
                                    hr = m_graph.AddFilter(vxySubFilter, "xy-SubFilter");
                                    DsError.ThrowExceptionForHR(hr);
                                }

                                xySubFilterSucceeded = true;
                            }
                            catch (Exception ex)
                            {
                                _logger.ErrorException("Error adding xy-SubFilter filter", ex);
                            }
                        }
                    }

                    // Fallback to xyVsFilter
                    if (!xySubFilterSucceeded)
                    {
                        try
                        {
                            _xyVsFilter = new XYVSFilter();
                            var vxyVsFilter = _xyVsFilter as DirectShowLib.IBaseFilter;
                            if (vxyVsFilter != null)
                            {
                                hr = m_graph.AddFilter(vxyVsFilter, "xy-VSFilter");
                                DsError.ThrowExceptionForHR(hr);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Error adding xy-VSFilter filter", ex);
                        }
                    }
                }
            }

            DirectShowLib.IEnumPins pEnum;
            hr = pSource.EnumPins(out pEnum);
            DsError.ThrowExceptionForHR(hr);

            DirectShowLib.IPin[] pins = { null };

            /* Counter for how many pins successfully rendered */
            var pinsRendered = 0;
            /* Loop over each pin of the source filter */
            while (pEnum.Next(1, pins, IntPtr.Zero) == 0)
            {
                if (_filterGraph.RenderEx(pins[0], AMRenderExFlags.RenderToExistingRenderers, IntPtr.Zero) >= 0)
                    pinsRendered++;

                Marshal.ReleaseComObject(pins[0]);
            }

            Marshal.ReleaseComObject(pEnum);

            if (pinsRendered == 0)
            {
                throw new Exception("Could not render any streams from the source Uri");
            }

            _logger.Info("Completed RenderStreams with {0} pins.", pinsRendered);

            if (_item.IsVideo)
            {
                SetVideoWindow();
            }
        }

        private void InitializeEvr(DirectShowLib.IBaseFilter pEvr, int dwStreams)
        {
            IMFVideoDisplayControl pDisplay;

            // Continue with the rest of the set-up.

            // Set the video window.
            object o;
            var pGetService = (IMFGetService)pEvr;
            pGetService.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);

            try
            {
                pDisplay = (IMFVideoDisplayControl)o;
            }
            catch
            {
                Marshal.ReleaseComObject(o);
                throw;
            }

            // Set the number of streams.
            pDisplay.SetVideoWindow(Handle);

            if (dwStreams > 1)
            {
                var pConfig = (IEVRFilterConfig)pEvr;
                pConfig.SetNumberOfStreams(dwStreams);
            }

            // Return the IMFVideoDisplayControl pointer to the caller.
            _mPDisplay = pDisplay;
        }

        private void SetVideoWindow()
        {
            SetVideoPositions();
            _hiddenWindow.SizeChanged += _hiddenWindow_SizeChanged;

            //_videoWindow.HideCursor(OABool.True);
            _videoWindow.put_Owner(Handle);
            _videoWindow.put_WindowStyle(DirectShowLib.WindowStyle.Child | DirectShowLib.WindowStyle.Visible | DirectShowLib.WindowStyle.ClipSiblings);
            //_videoWindow.put_FullScreenMode(OABool.True);

            if (_madvr != null)
            {
                SetExclusiveMode(false);
            }
        }

        private void SetVideoPositions()
        {
            var screenWidth = Convert.ToInt32(_hiddenWindow.ContentWidth);
            var screenHeight = Convert.ToInt32(_hiddenWindow.ContentHeight);

            // Set the display position to the entire window.
            var rc = new MFRect(0, 0, screenWidth, screenHeight);

            _mPDisplay.SetVideoPosition(null, rc);
            //_mPDisplay.SetFullscreen(true);

            // Get Aspect Ratio
            int aspectX;
            int aspectY;

            decimal heightAsPercentOfWidth = 0;

            var basicVideo2 = (IBasicVideo2)m_graph;
            basicVideo2.GetPreferredAspectRatio(out aspectX, out aspectY);

            var sourceHeight = 0;
            var sourceWidth = 0;

            _basicVideo.GetVideoSize(out sourceWidth, out sourceHeight);

            if (aspectX == 0 || aspectY == 0 || sourceWidth > 0 || sourceHeight > 0)
            {
                aspectX = sourceWidth;
                aspectY = sourceHeight;
            }

            if (aspectX > 0 && aspectY > 0)
            {
                heightAsPercentOfWidth = decimal.Divide(aspectY, aspectX);
            }

            // Adjust Video Size
            var iAdjustedHeight = 0;

            if (aspectX > 0 && aspectY > 0)
            {
                var adjustedHeight = Convert.ToDouble(heightAsPercentOfWidth * screenWidth);
                iAdjustedHeight = Convert.ToInt32(Math.Round(adjustedHeight));
            }

            //SET MADVR WINDOW TO FULL SCREEN AND SET POSITION
            if (screenHeight >= iAdjustedHeight && iAdjustedHeight > 0)
            {
                double totalMargin = (screenHeight - iAdjustedHeight);
                var topMargin = Convert.ToInt32(Math.Round(totalMargin / 2));
                _basicVideo.SetDestinationPosition(0, topMargin, screenWidth, iAdjustedHeight);
            }
            else if (screenHeight < iAdjustedHeight && iAdjustedHeight > 0)
            {
                var adjustedWidth = Convert.ToDouble(screenHeight / heightAsPercentOfWidth);

                var iAdjustedWidth = Convert.ToInt32(Math.Round(adjustedWidth));

                if (iAdjustedWidth == 1919)
                    iAdjustedWidth = 1920;

                double totalMargin = (screenWidth - iAdjustedWidth);
                var leftMargin = Convert.ToInt32(Math.Round(totalMargin / 2));
                _basicVideo.SetDestinationPosition(leftMargin, 0, iAdjustedWidth, screenHeight);
            }
            else if (iAdjustedHeight == 0)
            {
                _videoWindow.SetWindowPosition(0, 0, screenWidth, screenHeight);
            }
            _videoWindow.SetWindowPosition(0, 0, screenWidth, screenHeight);
        }

        public void SetExclusiveMode(bool enable)
        {
            try
            {
                var inExclusiveMode = MadvrInterface.InExclusiveMode(_madvr);

                if (inExclusiveMode && !enable)
                {
                    MadvrInterface.EnableExclusiveMode(false, _madvr);
                }
                else if (!inExclusiveMode && enable)
                {
                    MadvrInterface.EnableExclusiveMode(true, _madvr);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error changing exclusive mode", ex);
            }
        }

        void _hiddenWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetVideoPositions();
        }

        public void Pause()
        {
            if (_mediaControl == null)
                return;

            if (_mediaControl.Pause() >= 0)
                PlayState = PlayState.Paused;
        }

        public void Unpause()
        {
            if (_mediaControl == null)
                return;

            if (_mediaControl.Run() >= 0)
                PlayState = PlayState.Playing;
        }

        public void Stop(TrackCompletionReason reason, int? newTrackIndex)
        {
            var hr = 0;

            var pos = CurrentPositionTicks;

            // Stop media playback
            if (_mediaControl != null)
                hr = _mediaControl.Stop();

            DsError.ThrowExceptionForHR(hr);

            OnStopped(reason, pos, newTrackIndex);
        }

        public void Seek(long ticks)
        {
            if (_mediaSeeking != null)
            {
                long duration;

                var hr = _mediaSeeking.GetDuration(out duration);

                // Seek to the position
                hr = _mediaSeeking.SetPositions(new DsLong(ticks), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(duration), AMSeekingSeekingFlags.AbsolutePositioning);
            }
        }

        private void OnStopped(TrackCompletionReason reason, long? endingPosition, int? newTrackIndex)
        {
            // Clear global flags
            PlayState = PlayState.Idle;

            DisposePlayer();

            _playerWrapper.OnPlaybackStopped(_item, endingPosition, reason, newTrackIndex);
        }

        private void HandleGraphEvent()
        {
            // Make sure that we don't access the media event interface
            // after it has already been released.
            if (_mediaEventEx == null)
                return;

            try
            {
                EventCode evCode;
                IntPtr evParam1, evParam2;

                // Process all queued events
                while (_mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
                {
                    // Free memory associated with callback, since we're not using it
                    var hr = _mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

                    // If this is the end of the clip, close
                    if (evCode == EventCode.Complete)
                    {
                        Stop(TrackCompletionReason.Ended, null);
                    }
                }
            }
            catch
            {

            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GRAPHNOTIFY)
            {
                HandleGraphEvent();
            }

            base.WndProc(ref m);
        }

        private void DisposePlayer()
        {
            _logger.Debug("Disposing player");

            CloseInterfaces();
        }

        private void CloseInterfaces()
        {
            _hiddenWindow.SizeChanged -= _hiddenWindow_SizeChanged;

            int hr;

            if (_defaultAudioRenderer != null)
            {
                m_graph.RemoveFilter(_defaultAudioRenderer as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_defaultAudioRenderer);
                _defaultAudioRenderer = null;
            }

            if (_reclockAudioRenderer != null)
            {
                m_graph.RemoveFilter(_reclockAudioRenderer as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_reclockAudioRenderer);
                _reclockAudioRenderer = null;
            }

            if (_lavaudio != null)
            {
                m_graph.RemoveFilter(_lavaudio as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_lavaudio);
                _lavaudio = null;
            }

            if (_xyVsFilter != null)
            {
                m_graph.RemoveFilter(_xyVsFilter as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_xyVsFilter);
                _xyVsFilter = null;
            }

            if (_xySubFilter != null)
            {
                m_graph.RemoveFilter(_xySubFilter as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_xySubFilter);
                _xySubFilter = null;
            }

            if (_lavvideo != null)
            {
                m_graph.RemoveFilter(_lavvideo as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_lavvideo);
                _lavvideo = null;
            }

            if (_madvr != null)
            {
                m_graph.RemoveFilter(_madvr as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_madvr);
                _madvr = null;
            }

            if (_videoWindow != null)
            {
                // Relinquish ownership (IMPORTANT!) after hiding video window
                hr = _videoWindow.put_Visible(OABool.False);

                hr = _videoWindow.put_Owner(IntPtr.Zero);
            }

            if (_mediaEventEx != null)
            {
                hr = _mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                Marshal.ReleaseComObject(_mediaEventEx);
                _mediaEventEx = null;
            }

            if (_dvdNav != null)
            {
                Marshal.ReleaseComObject(_dvdNav);
                _dvdNav = null;
            }

            if (_mDvdInfo != null)
            {
                Marshal.ReleaseComObject(_mDvdInfo);
                _mDvdInfo = null;
            }

            if (_mDvdControl != null)
            {
                Marshal.ReleaseComObject(_mDvdControl);
                _mDvdControl = null;
            }

            if (_mPDisplay != null)
            {
                Marshal.ReleaseComObject(_mPDisplay);
                _mPDisplay = null;
            }

            if (_filterGraph != null)
            {
                Marshal.ReleaseComObject(_filterGraph);
                _filterGraph = null;
            }

            if (_mPEvr != null)
            {
                Marshal.ReleaseComObject(_mPEvr);
                _mPEvr = null;
            }

            if (_mediaEventEx != null)
            {
                Marshal.ReleaseComObject(_mediaEventEx);
                _mediaEventEx = null;
            }

            if (_mediaSeeking != null)
            {
                Marshal.ReleaseComObject(_mediaSeeking);
                _mediaSeeking = null;
            }

            if (_mediaPosition != null)
            {
                Marshal.ReleaseComObject(_mediaPosition);
                _mediaPosition = null;
            }

            if (_mediaControl != null)
            {
                Marshal.ReleaseComObject(_mediaControl);
                _mediaControl = null;
            }

            if (_basicAudio != null)
            {
                Marshal.ReleaseComObject(_basicAudio);
                _basicAudio = null;
            }

            if (_basicVideo != null)
            {
                Marshal.ReleaseComObject(_basicVideo);
                _basicVideo = null;
            }

            if (_sourceFilter != null)
            {
                Marshal.ReleaseComObject(_sourceFilter);
                _sourceFilter = null;
            }

            if (m_graph != null)
            {
                Marshal.ReleaseComObject(m_graph);
                m_graph = null;
            }

            if (_videoWindow != null)
            {
                Marshal.ReleaseComObject(_videoWindow);
                _videoWindow = null;
            }

            _mSeekCaps = 0;

            _streams = null;

            GC.Collect();
        }

        private List<SelectableMediaStream> _streams;
        public IReadOnlyList<SelectableMediaStream> GetSelectableStreams()
        {
            return _streams ?? (_streams = GetStreams());
        }

        private Guid _audioSelector = Guid.Empty;
        private Guid _vobsubSelector = Guid.Empty;
        private Guid _grp2Selector = Guid.Empty;

        private List<SelectableMediaStream> GetStreams()
        {
            var streams = new List<SelectableMediaStream>();

            IEnumFilters enumFilters;
            var hr = m_graph.EnumFilters(out enumFilters);

            DsError.ThrowExceptionForHR(hr);

            var filters = new DirectShowLib.IBaseFilter[1];

            while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
            {
                FilterInfo filterInfo;

                hr = filters[0].QueryFilterInfo(out filterInfo);
                DsError.ThrowExceptionForHR(hr);

                Guid cl;
                filters[0].GetClassID(out cl);

                if (filterInfo.pGraph != null)
                {
                    Marshal.ReleaseComObject(filterInfo.pGraph);
                }

                var iss = filters[0] as IAMStreamSelect;

                if (iss != null)
                {
                    int count;

                    hr = iss.Count(out count);
                    DsError.ThrowExceptionForHR(hr);

                    for (int i = 0; i < count; i++)
                    {
                        DirectShowLib.AMMediaType type;
                        AMStreamSelectInfoFlags flags;
                        int plcid, pwdGrp; // language
                        String pzname;

                        object ppobject, ppunk;

                        hr = iss.Info(i, out type, out flags, out plcid, out pwdGrp, out pzname, out ppobject, out ppunk);
                        DsError.ThrowExceptionForHR(hr);

                        if (ppobject != null)
                        {
                            Marshal.ReleaseComObject(ppobject);
                        }

                        if (type != null)
                        {
                            DsUtils.FreeAMMediaType(type);
                        }

                        if (ppunk != null)
                        {
                            Marshal.ReleaseComObject(ppunk);
                        }

                        if (pwdGrp == 2)
                        {
                            if (_grp2Selector == Guid.Empty)
                            {
                                filters[0].GetClassID(out _grp2Selector);
                            }

                            var stream = new SelectableMediaStream
                            {
                                Index = i,
                                Name = pzname,
                                Type = MediaStreamType.Subtitle
                            };

                            if ((AMStreamSelectInfoFlags.Enabled & flags) == AMStreamSelectInfoFlags.Enabled)
                            {
                                stream.IsActive = true;
                            }
                            streams.Add(stream);
                        }

                        if (pwdGrp == 1)
                        {
                            if (_audioSelector == Guid.Empty)
                            {
                                filters[0].GetClassID(out _audioSelector);
                            }
                            var stream = new SelectableMediaStream
                            {
                                Index = i,
                                Name = pzname,
                                Type = MediaStreamType.Audio
                            };
                            if ((AMStreamSelectInfoFlags.Enabled & flags) == AMStreamSelectInfoFlags.Enabled)
                            {
                                stream.IsActive = true;
                            }
                            streams.Add(stream);
                        }

                        if (pwdGrp == 6590033)
                        {
                            if (_vobsubSelector == Guid.Empty)
                            {
                                filters[0].GetClassID(out _vobsubSelector);
                            }

                            var stream = new SelectableMediaStream
                            {
                                Index = i,
                                Name = pzname,
                                Type = MediaStreamType.Subtitle,
                                Identifier = "Vobsub"
                            };

                            if ((AMStreamSelectInfoFlags.Enabled & flags) == AMStreamSelectInfoFlags.Enabled)
                            {
                                stream.IsActive = true;
                            }
                            streams.Add(stream);
                        }
                    }
                }

                Marshal.ReleaseComObject(filters[0]);
            }

            Marshal.ReleaseComObject(enumFilters);

            return streams;
        }

        public void SetAudioTrack(SelectableMediaStream stream)
        {
            SetStream(stream);
        }

        public void SetSubtitleTrack(SelectableMediaStream stream)
        {
            SetStream(stream);
        }

        private void SetStream(SelectableMediaStream stream)
        {
            IEnumFilters enumFilters;
            var hr = m_graph.EnumFilters(out enumFilters);

            DsError.ThrowExceptionForHR(hr);

            var filters = new DirectShowLib.IBaseFilter[1];

            while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
            {
                FilterInfo filterInfo;

                hr = filters[0].QueryFilterInfo(out filterInfo);
                DsError.ThrowExceptionForHR(hr);

                Guid cl;
                filters[0].GetClassID(out cl);

                if (stream.Type == MediaStreamType.Audio)
                {
                    if (cl != _audioSelector)
                    {
                        continue;
                    }
                }
                else if (stream.Type == MediaStreamType.Subtitle)
                {
                    if (cl != _grp2Selector && cl != _vobsubSelector)
                    {
                        continue;
                    }
                }

                if (filterInfo.pGraph != null)
                {
                    Marshal.ReleaseComObject(filterInfo.pGraph);
                }

                var iss = filters[0] as IAMStreamSelect;

                iss.Enable(stream.Index, AMStreamSelectEnableFlags.Enable);

                Marshal.ReleaseComObject(filters[0]);
            }

            Marshal.ReleaseComObject(enumFilters);

            foreach (var i in GetSelectableStreams().Where(s => s.Type == stream.Type))
            {
                i.IsActive = i.Index == stream.Index;
            }
        }
    }
}
