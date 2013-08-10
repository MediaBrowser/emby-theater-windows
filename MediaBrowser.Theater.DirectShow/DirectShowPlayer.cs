using System.Collections.Generic;
using DirectShowLib;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using System;
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
        private readonly IUserInputManager _userInputManager;

        private DirectShowLib.IGraphBuilder _graphBuilder;

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

        private LAVAudio _lavaudio;
        private LAVVideo _lavvideo;

        // EVR filter
        private DirectShowLib.IBaseFilter _mPEvr;
        private IMFVideoDisplayControl _mPDisplay;
        private IMFVideoMixerControl _mPMixer;
        private IMFVideoPositionMapper _mPMapper;

        private DefaultAudioRenderer _defaultAudioRenderer;
        private ReclockAudioRenderer _reclockAudioRenderer;

        // Caps bits for IMediaSeeking
        private AMSeekingSeekingCapabilities _mSeekCaps;

        private MadVR _madvr;

        private BaseItemDto _item;

        private System.Threading.Timer _activityTimer;

        public DirectShowPlayer(ILogger logger, IHiddenWindow hiddenWindow, InternalDirectShowPlayer playerWrapper, IUserInputManager userInputManager)
        {
            _logger = logger;
            _hiddenWindow = hiddenWindow;
            _playerWrapper = playerWrapper;
            _userInputManager = userInputManager;
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

        private DateTime _lastMouseInput;

        /// <summary>
        /// The _is mouse idle
        /// </summary>
        private bool _isMouseIdle = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse idle.
        /// </summary>
        /// <value><c>true</c> if this instance is mouse idle; otherwise, <c>false</c>.</value>
        public bool IsMouseIdle
        {
            get { return _isMouseIdle; }
            set
            {
                var changed = _isMouseIdle != value;

                _isMouseIdle = value;

                if (changed && _videoWindow != null)
                {
                    _videoWindow.HideCursor((value ? OABool.True : OABool.False));
                }
            }
        }

        void _userInputManager_MouseMove(object sender, MouseEventArgs e)
        {
            _lastMouseInput = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 5000;
        }

        public void Play(BaseItemDto item, bool enableReclock, bool enableMadvr)
        {
            _item = item;

            Initialize(item.Path, enableReclock, enableMadvr);

            var hr = _mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);

            PlayState = PlayState.Playing;

            _userInputManager.MouseMove += _userInputManager_MouseMove;
            _activityTimer = new System.Threading.Timer(TimerCallback, null, 100, 100);
        }

        private void InitializeGraph()
        {
            _graphBuilder = (DirectShowLib.IGraphBuilder)new FilterGraphNoThread();

            // QueryInterface for DirectShow interfaces
            _mediaControl = (DirectShowLib.IMediaControl)_graphBuilder;
            _mediaEventEx = (DirectShowLib.IMediaEventEx)_graphBuilder;
            _mediaSeeking = (DirectShowLib.IMediaSeeking)_graphBuilder;
            _mediaPosition = (DirectShowLib.IMediaPosition)_graphBuilder;

            // Query for video interfaces, which may not be relevant for audio files
            _videoWindow = _graphBuilder as DirectShowLib.IVideoWindow;
            _basicVideo = _graphBuilder as DirectShowLib.IBasicVideo;

            // Query for audio interfaces, which may not be relevant for video-only files
            _basicAudio = _graphBuilder as DirectShowLib.IBasicAudio;

            // Set up event notification.
            var hr = _mediaEventEx.SetNotifyWindow(Handle, WM_GRAPHNOTIFY, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
        }

        private void Initialize(string path, bool enableReclock, bool enableMadvr)
        {
            InitializeGraph();

            var hr = _graphBuilder.AddSourceFilter(path, path, out _pSource);
            DsError.ThrowExceptionForHR(hr);

            // Try to render the streams.
            RenderStreams(_pSource, enableReclock, enableMadvr);

            // Get the seeking capabilities.
            hr = _mediaSeeking.GetCapabilities(out _mSeekCaps);
            DsError.ThrowExceptionForHR(hr);
        }

        private void RenderStreams(DirectShowLib.IBaseFilter pSource, bool enableReclock, bool enableMadvr)
        {
            int hr;

            _filterGraph = _graphBuilder as DirectShowLib.IFilterGraph2;
            if (_filterGraph == null)
            {
                throw new Exception("Could not QueryInterface for the IFilterGraph2");
            }

            // Add video renderer
            if (_item.IsVideo)
            {
                _mPEvr = (DirectShowLib.IBaseFilter)new EnhancedVideoRenderer();
                hr = _graphBuilder.AddFilter(_mPEvr, "EVR");
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
                        hr = _graphBuilder.AddFilter(aRenderer, "Reclock Audio Renderer");
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
                    _graphBuilder.AddFilter(aRenderer, "Default Audio Renderer");
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
                        hr = _graphBuilder.AddFilter(vlavvideo, "LAV Video Decoder");
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding LAV Video filter", ex);
                }

                try
                {
                    _xyVsFilter = new XYVSFilter();
                    var vxyVsFilter = _xyVsFilter as DirectShowLib.IBaseFilter;
                    if (vxyVsFilter != null)
                    {
                        hr = _graphBuilder.AddFilter(vxyVsFilter, "xy-VSFilter");
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding xy-VSFilter filter", ex);
                }
            }

            try
            {
                _lavaudio = new LAVAudio();
                var vlavaudio = _lavaudio as DirectShowLib.IBaseFilter;
                if (vlavaudio != null)
                {
                    hr = _graphBuilder.AddFilter(vlavaudio, "LAV Audio Decoder");
                    DsError.ThrowExceptionForHR(hr);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error adding LAV Audio filter", ex);
            }

            if (enableMadvr && _item.IsVideo)
            {
                try
                {
                    _madvr = new MadVR();
                    var vmadvr = _madvr as DirectShowLib.IBaseFilter;
                    if (vmadvr != null)
                    {
                        hr = _graphBuilder.AddFilter(vmadvr, "MadVR Video Renderer");
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding MadVR filter", ex);
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

            _mPMixer = null;
        }

        private void SetVideoWindow()
        {
            SetVideoPositions();
            _hiddenWindow.SizeChanged += _hiddenWindow_SizeChanged;

            _videoWindow.put_Owner(Handle);
            _videoWindow.put_WindowStyle(DirectShowLib.WindowStyle.Child | DirectShowLib.WindowStyle.Visible | DirectShowLib.WindowStyle.ClipSiblings);
            //_videoWindow.put_FullScreenMode(OABool.True);

            _videoWindow.HideCursor(OABool.True);

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

            var basicVideo2 = (IBasicVideo2)_graphBuilder;
            basicVideo2.GetPreferredAspectRatio(out aspectX, out aspectY);

            var sourceHeight = 0;
            var sourceWidth = 0;

            var videoStream = _item.MediaStreams.FirstOrDefault(i => i.Type == MediaStreamType.Video);

            if (videoStream != null)
            {
                sourceWidth = videoStream.Width ?? 0;
                sourceHeight = videoStream.Height ?? 0;
            }

            if (aspectX == 0 || aspectY == 0)
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

        public void Stop()
        {
            var hr = 0;

            // Stop media playback
            if (_mediaControl != null)
                hr = _mediaControl.Stop();

            DsError.ThrowExceptionForHR(hr);

            OnStopped();
        }

        public void Seek(long ticks)
        {
            // In Directx time is measured in 100 nanoseconds. 
            var pos = new DsLong(ticks);

            long duration;

            var hr = _mediaSeeking.GetDuration(out duration);

            // Seek to the position
            hr = _mediaSeeking.SetPositions(new DsLong(ticks), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(duration), AMSeekingSeekingFlags.AbsolutePositioning);
        }

        private void OnStopped()
        {
            // Clear global flags
            PlayState = PlayState.Idle;

            var pos = CurrentPositionTicks;

            DisposePlayer();

            _playerWrapper.OnPlaybackStopped(_item, CurrentPositionTicks);
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
                        Stop();
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

            if (_activityTimer != null)
            {
                _activityTimer.Dispose();
                _activityTimer = null;
            }

            CloseInterfaces();
        }

        private void CloseInterfaces()
        {
            _hiddenWindow.SizeChanged -= _hiddenWindow_SizeChanged;

            int hr;

            if (_defaultAudioRenderer != null)
            {
                _graphBuilder.RemoveFilter(_defaultAudioRenderer as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_defaultAudioRenderer);
                _defaultAudioRenderer = null;
            }

            if (_reclockAudioRenderer != null)
            {
                _graphBuilder.RemoveFilter(_reclockAudioRenderer as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_reclockAudioRenderer);
                _reclockAudioRenderer = null;
            }

            if (_lavaudio != null)
            {
                _graphBuilder.RemoveFilter(_lavaudio as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_lavaudio);
                _lavaudio = null;
            }

            if (_xyVsFilter != null)
            {
                _graphBuilder.RemoveFilter(_xyVsFilter as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_xyVsFilter);
                _xyVsFilter = null;
            }

            if (_lavvideo != null)
            {
                _graphBuilder.RemoveFilter(_lavvideo as DirectShowLib.IBaseFilter);

                Marshal.ReleaseComObject(_lavvideo);
                _lavvideo = null;
            }

            if (_madvr != null)
            {
                _graphBuilder.RemoveFilter(_madvr as DirectShowLib.IBaseFilter);

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

            if (_graphBuilder != null)
            {
                Marshal.ReleaseComObject(_graphBuilder);
                _graphBuilder = null;
            }

            if (_videoWindow != null)
            {
                Marshal.ReleaseComObject(_videoWindow);
                _videoWindow = null;
            }

            _mSeekCaps = 0;

            GC.Collect();
        }
    }
}
