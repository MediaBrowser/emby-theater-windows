using System.Drawing;
using DirectShowLib;
using DirectShowLib.Dvd;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Playback;
using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using DirectShowLib.Utils;
using System.Diagnostics;
using MediaBrowser.Theater.Interfaces.Configuration;
using System.Text;

namespace MediaBrowser.Theater.DirectShow
{
    public class DirectShowPlayer : IDisposable
    {
        private const int WM_APP = 0x8000;
        private const int WM_GRAPHNOTIFY = WM_APP + 1;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_DVD_EVENT = 0x00008002;

        private readonly ILogger _logger;
        private readonly IHiddenWindow _hiddenWindow;
        private readonly InternalDirectShowPlayer _playerWrapper;
    
        private DirectShowLib.IGraphBuilder m_graph = null;
        private DirectShowLib.FilterGraphNoThread m_filterGraph = null;

        private DirectShowLib.IMediaControl _mediaControl = null;
        private DirectShowLib.IMediaEventEx _mediaEventEx = null;
        private DirectShowLib.IVideoWindow _videoWindow = null;
        private DirectShowLib.IBasicAudio _basicAudio = null;
        private DirectShowLib.IBasicVideo _basicVideo = null;
        private DirectShowLib.IMediaSeeking _mediaSeeking = null;
        private DirectShowLib.IMediaPosition _mediaPosition = null;
        private DirectShowLib.IBaseFilter _sourceFilter = null;
        private DirectShowLib.IFilterGraph2 _filterGraph = null;
        DsROTEntry m_dsRot = null;

        private XYVSFilter _xyVsFilter = null;
        private XySubFilter _xySubFilter = null;

        private LAVAudio _lavaudio = null;
        private LAVVideo _lavvideo = null;

        // EVR filter
        private DirectShowLib.IBaseFilter _mPEvr = null;
        private IMFVideoDisplayControl _mPDisplay = null;

        private DefaultAudioRenderer _defaultAudioRenderer = null;
        private ReclockAudioRenderer _reclockAudioRenderer = null;

        // Caps bits for IMediaSeeking
        private AMSeekingSeekingCapabilities _mSeekCaps;

        // Dvd
        //private DirectShowLib.IBaseFilter _dvdNav = null;
        private IDvdControl2 _mDvdControl = null;
        //private IDvdInfo2 _mDvdInfo = null;

        private MadVR _madvr = null;

        private PlayableItem _item = null;

        private readonly IntPtr _applicationWindowHandle;
        private bool _isInExclusiveMode;
        private DvdMenuMode _dvdMenuMode = DvdMenuMode.No;
        private bool _removeHandlers = false;
        private VideoConfiguration _videoConfig;
        private AudioConfiguration _audioConfig;

        #region LAVConfigurationValues

        public static List<string> GetLAVVideoHwaCodecs()
        {
            return BuildListFromEnumType(typeof(LAVVideoHWCodec));
        }

        public static List<string> GetLAVVideoCodecs()
        {
            return BuildListFromEnumType(typeof(LAVVideoCodec));
        }

        public static List<string> GetLAVVideoHWAResolutions()
        {
            return BuildListFromEnumType(typeof(LAVHWResFlag));
        }

        public static List<string> GetLAVAudioCodecs()
        {
            return BuildListFromEnumType(typeof(LAVAudioCodec));
        }

        public static List<string> GetLAVAudioMixingModes()
        {
            return BuildListFromEnumType(typeof(LAVAudioMixingMode));
        }

        public static List<string> GetLAVAudioMixingControl()
        {
            return BuildListFromEnumType(typeof(LAVAudioMixingFlag));
        }

        public static List<string> GetLAVAudioMixingLayout()
        {
            return BuildListFromEnumType(typeof(LAVAudioMixingLayout));
        }

        private static List<string> BuildListFromEnumType(Type enumType)
        {
            List<string> values = new List<string>();
            foreach (string etype in Enum.GetNames(enumType))
            {
                if (etype != "NB")
                    values.Add(etype);
            }

            return values;
        }

        #endregion

        public DirectShowPlayer(ILogger logger, IHiddenWindow hiddenWindow, InternalDirectShowPlayer playerWrapper, IntPtr applicationWindowHandle)
        {
            _logger = logger;
            _hiddenWindow = hiddenWindow;
            _playerWrapper = playerWrapper;
            _applicationWindowHandle = applicationWindowHandle;
        }

        private IntPtr VideoWindowHandle
        {
            get { return _hiddenWindow.Form.Handle; }
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
                //for some reason we're loosing our reference to the original IDvdInfo2
                IDvdInfo2 dvdInfo = _sourceFilter as IDvdInfo2;
                if (dvdInfo != null && PlayState != PlayState.Idle)
                {
                    var totaltime = new DvdHMSFTimeCode();
                    DvdTimeCodeFlags ulTimeCodeFlags;
                    dvdInfo.GetTotalTitleTime(totaltime, out ulTimeCodeFlags);

                    return new TimeSpan(totaltime.bHours, totaltime.bMinutes, totaltime.bSeconds).Ticks;
                }

                if (_mediaSeeking != null && PlayState != PlayState.Idle)
                {
                    long pos;

                    int hr = _mediaSeeking.GetDuration(out pos);

                    return pos;
                }

                return null;
            }
        }

        public void Play(PlayableItem item, bool enableReclock, bool enableMadvr, bool enableMadvrExclusiveMode, bool enableXySubFilter, VideoConfiguration videoConfig, AudioConfiguration audioConfig)
        {
            _logger.Info("Playing {0}. Reclock: {1}, Madvr: {2}, xySubFilter: {3}", item.OriginalItem.Name, enableReclock, enableMadvr, enableXySubFilter);
            _logger.Info("Playing Path {0}", item.PlayablePath);

            _item = item;
            _isInExclusiveMode = false;

            _videoConfig = videoConfig;
            _audioConfig = audioConfig;

            _videoConfig.SetDefaults();
            _audioConfig.SetDefaults();

            var isDvd = ((item.OriginalItem.VideoType ?? VideoType.VideoFile) == VideoType.Dvd || (item.OriginalItem.IsoType ?? IsoType.BluRay) == IsoType.Dvd) &&
                item.PlayablePath.IndexOf("http://", StringComparison.OrdinalIgnoreCase) == -1;

            Initialize(item.PlayablePath, enableReclock, enableMadvr, enableMadvrExclusiveMode, enableXySubFilter, isDvd);

            _hiddenWindow.OnWMGRAPHNOTIFY = HandleGraphEvent;
            _hiddenWindow.OnDVDEVENT = HandleDvdEvent;

            var hr = _mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);

            PlayState = PlayState.Playing;

            _streams = GetStreams();
        }

        private void InitializeGraph(bool isDvd)
        {
            int hr = 0;
            m_filterGraph = new FilterGraphNoThread();
            m_graph = (m_filterGraph as DirectShowLib.IGraphBuilder);

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
            if (isDvd)
                hr = _mediaEventEx.SetNotifyWindow(VideoWindowHandle, WM_DVD_EVENT, IntPtr.Zero);
            else
                hr = _mediaEventEx.SetNotifyWindow(VideoWindowHandle, WM_GRAPHNOTIFY, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);

            m_dsRot = new DsROTEntry(m_graph as IFilterGraph);
        }

        private void Initialize(string path, bool enableReclock, bool enableMadvr, bool enableMadvrExclusiveMode, bool enableXySubFilter, bool isDvd)
        {
            InitializeGraph(isDvd);

            int hr = 0;

            if (isDvd)
            {
                _logger.Debug("Initializing dvd player to play {0}", path);

                /* Create a new DVD Navigator. */
                _sourceFilter = (DirectShowLib.IBaseFilter)new DVDNavigator();

                InitializeDvd(path);

                // Try to render the streams.
                RenderStreams(_sourceFilter, enableReclock, enableMadvr, enableMadvrExclusiveMode, false); //we don't need XySubFilter for DVD 
            }
            else
            {
                //prefer LAV Spliter Source
                bool loadSource = true;
                _sourceFilter = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("{B98D13E7-55DB-4385-A33D-09FD1BA26338}"))) as DirectShowLib.IBaseFilter;
                if (_sourceFilter != null)
                {
                    hr = m_graph.AddFilter(_sourceFilter, "LAV Splitter Source");
                    DsError.ThrowExceptionForHR(hr);

                    if (_sourceFilter != null)
                    {
                        hr = ((IFileSourceFilter)_sourceFilter).Load(path, null);
                        if (hr < 0)
                        {
                            //LAV can't load this file type
                            hr = m_graph.RemoveFilter(_sourceFilter);
                            Marshal.ReleaseComObject(_sourceFilter);
                            _sourceFilter = null;
                            DsError.ThrowExceptionForHR(hr);
                        }
                        else
                            loadSource = false;
                    }
                }

                if (loadSource)
                {
                    hr = m_graph.AddSourceFilter(path, path, out _sourceFilter);
                    DsError.ThrowExceptionForHR(hr);
                }
                // Try to render the streams.
                RenderStreams(_sourceFilter, enableReclock, enableMadvr, enableMadvrExclusiveMode, enableXySubFilter);
            }

            // Get the seeking capabilities.
            hr = _mediaSeeking.GetCapabilities(out _mSeekCaps);
            DsError.ThrowExceptionForHR(hr);
        }

        private void InitializeDvd(string path)
        {
            int hr = m_graph.AddFilter(_sourceFilter, "DVD Navigator");
            DsError.ThrowExceptionForHR(hr);

            /* The DVDControl2 interface lets us control DVD features */
            _mDvdControl = _sourceFilter as IDvdControl2;

            if (_mDvdControl == null)
                throw new Exception("Could not QueryInterface the IDvdControl2 interface");

            var videoTsPath = Path.Combine(path, "video_ts");
            if (Directory.Exists(videoTsPath))
            {
                path = videoTsPath;
            }

            /* If a Dvd directory has been set then use it, if not, let DShow find the Dvd */
            hr = _mDvdControl.SetDVDDirectory(path);
            DsError.ThrowExceptionForHR(hr);

            /* This gives us the DVD time in Hours-Minutes-Seconds-Frame time format, and other options */
            hr = _mDvdControl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true);
            DsError.ThrowExceptionForHR(hr);

            /* If the graph stops, resume at the same point */
            _mDvdControl.SetOption(DvdOptionFlag.ResetOnStop, false);

            /* QueryInterface the DVDInfo2 */
            //dvdInfo = _sourceFilter as IDvdInfo2;

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

        private void RenderStreams(DirectShowLib.IBaseFilter pSource, bool enableReclock, bool enableMadvr, bool enableMadvrExclusiveMode, bool enableXySubFilter)
        {
            int hr;

            _filterGraph = m_graph as DirectShowLib.IFilterGraph2;
            if (_filterGraph == null)
            {
                throw new Exception("Could not QueryInterface for the IFilterGraph2");
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

                        ILAVVideoSettings vsett = vlavvideo as ILAVVideoSettings;
                        if (vsett != null)
                        {
                            //we only want to set it for MB
                            hr = vsett.SetRuntimeConfig(true);
                            DsError.ThrowExceptionForHR(hr);

                            var configuredMode = VideoConfigurationUtils.GetHwaMode(_videoConfig);

                            LAVHWAccel testme = vsett.GetHWAccel();
                            if (testme != (LAVHWAccel)configuredMode)
                            {
                                hr = vsett.SetHWAccel((LAVHWAccel)configuredMode);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            foreach (string c in DirectShowPlayer.GetLAVVideoCodecs())
                            {
                                LAVVideoCodec codec = (LAVVideoCodec)Enum.Parse(typeof(LAVVideoCodec), c);

                                bool isEnabled = vsett.GetFormatConfiguration(codec);
                                if (_videoConfig.EnabledCodecs.Contains(c))
                                {
                                    if (!isEnabled)
                                    {
                                        _logger.Debug("Enable support for: {0}", c);
                                        hr = vsett.SetFormatConfiguration(codec, true);
                                        DsError.ThrowExceptionForHR(hr);
                                    }
                                }
                                else if (isEnabled)
                                {
                                    _logger.Debug("Disable support for: {0}", c);
                                    hr = vsett.SetFormatConfiguration(codec, false);
                                    DsError.ThrowExceptionForHR(hr);
                                }
                            }

                            foreach (string hwaCodec in DirectShowPlayer.GetLAVVideoHwaCodecs())
                            {
                                LAVVideoHWCodec codec = (LAVVideoHWCodec)Enum.Parse(typeof(LAVVideoHWCodec), hwaCodec);
                                bool hwaIsEnabled = vsett.GetHWAccelCodec(codec);

                                if (_videoConfig.HwaEnabledCodecs.Contains(hwaCodec))
                                {
                                    if (!hwaIsEnabled)
                                    {
                                        _logger.Debug("Enable HWA support for: {0}", hwaCodec);
                                        hr = vsett.SetHWAccelCodec(codec, true);
                                        DsError.ThrowExceptionForHR(hr);
                                    }
                                }
                                else if (hwaIsEnabled)
                                {
                                    _logger.Debug("Disable HWA support for: {0}", hwaCodec);
                                    hr = vsett.SetHWAccelCodec(codec, false);
                                    DsError.ThrowExceptionForHR(hr);
                                }
                            }

                            if (!vsett.GetDVDVideoSupport())
                            {
                                _logger.Debug("Enable DVD support.");
                                hr = vsett.SetDVDVideoSupport(true);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            int hwaRes = vsett.GetHWAccelResolutionFlags();
                            if (hwaRes != _videoConfig.HwaResolution)
                            {
                                _logger.Debug("Change HWA resolution support from {0} to {1}.", hwaRes, _videoConfig.HwaResolution);
                                hr = vsett.SetHWAccelResolutionFlags(_videoConfig.HwaResolution);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            hr = vsett.SetTrayIcon(_videoConfig.ShowTrayIcon);
                            DsError.ThrowExceptionForHR(hr);
                        }
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
                        _logger.Debug("Add LAVAudio to the graph.");

                        hr = m_graph.AddFilter(vlavaudio, "LAV Audio Decoder");
                        DsError.ThrowExceptionForHR(hr);

                        ILAVAudioSettings asett = vlavaudio as ILAVAudioSettings;
                        if (asett != null)
                        {
                            _logger.Debug("Enable LAVAudio Runtime Config");

                            //we only want to set it for MB
                            hr = asett.SetRuntimeConfig(true);
                            DsError.ThrowExceptionForHR(hr);

                            foreach (string c in DirectShowPlayer.GetLAVAudioCodecs())
                            {
                                LAVAudioCodec codec = (LAVAudioCodec)Enum.Parse(typeof(LAVAudioCodec), c);

                                bool isEnabled = asett.GetFormatConfiguration(codec);
                                if (_audioConfig.EnabledCodecs.Contains(c))
                                {
                                    if (!isEnabled)
                                    {
                                        _logger.Debug("Enable support for: {0}", c);
                                        hr = asett.SetFormatConfiguration(codec, true);
                                        DsError.ThrowExceptionForHR(hr);
                                    }
                                }
                                else if (isEnabled)
                                {
                                    _logger.Debug("Disable support for: {0}", c);
                                    hr = asett.SetFormatConfiguration(codec, false);
                                    DsError.ThrowExceptionForHR(hr);
                                }
                            }

                            //enable/disable bitstreaming
                            if ((_audioConfig.AudioBitstreaming & BitstreamChoice.SPDIF) == BitstreamChoice.SPDIF)
                            {
                                _logger.Debug("Enable LAVAudio S/PDIF bitstreaming");

                                hr = asett.SetBitstreamConfig(LAVBitstreamCodec.AC3, true);
                                DsError.ThrowExceptionForHR(hr);

                                hr = asett.SetBitstreamConfig(LAVBitstreamCodec.DTS, true);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            if ((_audioConfig.AudioBitstreaming & BitstreamChoice.HDMI) == BitstreamChoice.HDMI)
                            {
                                _logger.Debug("Enable LAVAudio HDMI bitstreaming");

                                hr = asett.SetBitstreamConfig(LAVBitstreamCodec.EAC3, true);
                                DsError.ThrowExceptionForHR(hr);

                                hr = asett.SetBitstreamConfig(LAVBitstreamCodec.TRUEHD, true);
                                DsError.ThrowExceptionForHR(hr);

                                hr = asett.SetBitstreamConfig(LAVBitstreamCodec.DTSHD, true);
                                DsError.ThrowExceptionForHR(hr);

                            }

                            if (_audioConfig.Delay > 0)
                            {
                                _logger.Debug("Set LAVAudio audio delay: {0}", _audioConfig.Delay);

                                hr = asett.SetAudioDelay(true, _audioConfig.Delay);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            _logger.Debug("Set LAVAudio auto AV Sync: {0}", _audioConfig.EnableAutoSync);
                            hr = asett.SetAutoAVSync(_audioConfig.EnableAutoSync);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("Set LAVAudio Expand61: {0}", _audioConfig.Expand61);
                            hr = asett.SetExpand61(_audioConfig.Expand61);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("Set LAVAudio ExpandMono: {0}", _audioConfig.ExpandMono);
                            hr = asett.SetExpandMono(_audioConfig.ExpandMono);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("Set LAVAudio ConvertToStandardLayout: {0}", _audioConfig.ConvertToStandardLayout);
                            hr = asett.SetOutputStandardLayout(_audioConfig.ConvertToStandardLayout);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("Set LAVAudio audio EnableDRC: {0}", _audioConfig.EnableDRC);
                            hr = asett.SetDRC(_audioConfig.EnableDRC, _audioConfig.DRCLevel);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("Set LAVAudio audio ShowTrayIcon: {0}", _audioConfig.ShowTrayIcon);
                            hr = asett.SetTrayIcon(_audioConfig.ShowTrayIcon);
                            DsError.ThrowExceptionForHR(hr);

                            bool mixingEnabled = asett.GetMixingEnabled();
                            if (mixingEnabled != _audioConfig.EnablePCMMixing)
                            {
                                _logger.Debug("Set LAVAudio EnablePCMMixing: {0}", _audioConfig.EnablePCMMixing);
                                hr = asett.SetMixingEnabled(!mixingEnabled);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            if (_audioConfig.EnablePCMMixing)
                            {
                                _logger.Debug("Set LAVAudio MixingSetting: {0}", _audioConfig.MixingSetting);
                                LAVAudioMixingFlag amf = (LAVAudioMixingFlag)_audioConfig.MixingSetting;
                                hr = asett.SetMixingFlags(amf);
                                DsError.ThrowExceptionForHR(hr);

                                _logger.Debug("Set LAVAudio MixingEncoding: {0}", _audioConfig.MixingEncoding);
                                LAVAudioMixingMode amm = (LAVAudioMixingMode)Enum.Parse(typeof(LAVAudioMixingMode), _audioConfig.MixingEncoding);
                                hr = asett.SetMixingMode(amm);
                                DsError.ThrowExceptionForHR(hr);

                                _logger.Debug("Set LAVAudio MixingLayout: {0}", _audioConfig.MixingLayout);
                                LAVAudioMixingLayout aml = (LAVAudioMixingLayout)Enum.Parse(typeof(LAVAudioMixingLayout), _audioConfig.MixingLayout);
                                hr = asett.SetMixingLayout(aml);
                                DsError.ThrowExceptionForHR(hr);

                                _logger.Debug("Set LAVAudio LfeMixingLevel: {0} CenterMixingLevel: {1} SurroundMixingLevel: {2}", _audioConfig.LfeMixingLevel, _audioConfig.CenterMixingLevel, _audioConfig.SurroundMixingLevel);
                                int lfe, center, surround;
                                //convert to the # that LAV Audio expects
                                lfe = (int)(_audioConfig.LfeMixingLevel * 10000.01);
                                center = (int)(_audioConfig.CenterMixingLevel * 10000.01);
                                surround = (int)(_audioConfig.SurroundMixingLevel * 10000.01);

                                hr = asett.SetMixingLevels(center, surround, lfe);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            for (int i = 0; i < (int)LAVBitstreamCodec.NB; i++)
                            {
                                LAVBitstreamCodec codec = (LAVBitstreamCodec)i;
                                bool isEnabled = asett.GetBitstreamConfig(codec);
                                _logger.Log(LogSeverity.Debug, "{0} bitstreaming: {1}", codec, isEnabled);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error adding LAV Audio filter", ex);
                }

                if (_item.IsVideo)
                {
                    var xySubFilterSucceeded = false;
                    var madVrSucceded = false;

                    if (enableMadvr)
                    {

                        try
                        {
                            _madvr = new MadVR();
                            var vmadvr = _madvr as DirectShowLib.IBaseFilter;
                            if (vmadvr != null)
                            {
                                hr = m_graph.AddFilter(vmadvr, "MadVR Video Renderer");
                                DsError.ThrowExceptionForHR(hr);

                                try
                                {
                                    MadVRSettings msett = new MadVRSettings(_madvr);

                                    bool smoothMotion = msett.GetBool("smoothMotionEnabled");

                                    if (smoothMotion != _videoConfig.UseMadVrSmoothMotion)
                                        msett.SetBool("smoothMotionEnabled", _videoConfig.UseMadVrSmoothMotion);

                                    if (string.Compare(msett.GetString("smoothMotionMode"), _videoConfig.MadVrSmoothMotionMode, true) != 0)
                                    {
                                        bool success = msett.SetString("smoothMotionMode", _videoConfig.MadVrSmoothMotionMode);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.ErrorException("Error configuring madVR", ex);
                                }

                                madVrSucceded = true;
                            }
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

                    // Add video renderer
                    if (!madVrSucceded)
                    {
                        _mPEvr = (DirectShowLib.IBaseFilter)new EnhancedVideoRenderer();
                        hr = m_graph.AddFilter(_mPEvr, "EVR");
                        DsError.ThrowExceptionForHR(hr);

                        InitializeEvr(_mPEvr, 1);
                    }

                    // Fallback to xyVsFilter
                    if (!xySubFilterSucceeded && enableXySubFilter)
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
                //explicitly build graph to avoid unwanted filters worming their way in
                List<Guid> mediaTypes = GetPinMediaTypes(pins[0]);
                bool needsRender = true;

                for (int m = 0; m < mediaTypes.Count; m++)
                {
                    DirectShowLib.IPin decIn = null;
                    DirectShowLib.IPin decOut = null;
                    DirectShowLib.IPin rendIn = null;

                    try
                    {
                        if (mediaTypes[m] == DirectShowLib.MediaType.Video && _lavvideo != null)
                        {
                            decIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavvideo, PinDirection.Input, 0);
                            if (decIn != null)
                            {
                                hr = _filterGraph.ConnectDirect(pins[0], decIn, null);
                                DsError.ThrowExceptionForHR(hr);
                                decOut = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavvideo, PinDirection.Output, 0);

                                if (_xyVsFilter != null)
                                {
                                    //insert xyVsFilter b/w LAV Video and the renderer
                                    rendIn = DsFindPin.ByName((DirectShowLib.IBaseFilter)_xyVsFilter, "Video");
                                    if (decOut != null && rendIn != null)
                                    {
                                        hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                        DsError.ThrowExceptionForHR(hr);
                                        CleanUpInterface(decOut);
                                        CleanUpInterface(rendIn);
                                        //grab xyVsFilter's output pin so it can be connected to the renderer
                                        decOut = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_xyVsFilter, PinDirection.Output, 0);
                                    }
                                }

                                if (_madvr != null)
                                {
                                    rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_madvr, PinDirection.Input, 0);
                                }
                                else
                                {
                                    rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_mPEvr, PinDirection.Input, 0);
                                }

                                if (decOut != null && rendIn != null)
                                {
                                    hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                    DsError.ThrowExceptionForHR(hr);

                                    needsRender = false;
                                    break;
                                }
                            }
                        }
                        else if (mediaTypes[m] == DirectShowLib.MediaType.Audio && _lavaudio != null)
                        {
                            decIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavaudio, PinDirection.Input, 0);
                            if (decIn != null)
                            {
                                hr = _filterGraph.ConnectDirect(pins[0], decIn, null);
                                DsError.ThrowExceptionForHR(hr);
                                decOut = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavaudio, PinDirection.Output, 0);

                                if (_reclockAudioRenderer != null)
                                {
                                    rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_reclockAudioRenderer, PinDirection.Input, 0);
                                }
                                else
                                {
                                    rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_defaultAudioRenderer, PinDirection.Input, 0);
                                }

                                if (decOut != null && rendIn != null)
                                {
                                    hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                    DsError.ThrowExceptionForHR(hr);

                                    needsRender = false;
                                    break;
                                }
                            }
                        }
                        else if (mediaTypes[m] == new Guid("E487EB08-6B26-4be9-9DD3-993434D313FD") /*DirectShowLib.MediaType.Subtitle*/
                            && (_xySubFilter != null || _xyVsFilter != null))
                        {

                            if (_xySubFilter != null)
                            {
                                rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_xySubFilter, PinDirection.Input, 0);
                            }
                            else
                            {
                                rendIn = DsFindPin.ByName((DirectShowLib.IBaseFilter)_xyVsFilter, "Input");
                            }

                            if (rendIn != null)
                            {
                                hr = _filterGraph.ConnectDirect(pins[0], rendIn, null);
                                DsError.ThrowExceptionForHR(hr);

                                needsRender = false;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        CleanUpInterface(decIn);
                        CleanUpInterface(decOut);
                        CleanUpInterface(rendIn);
                    }
                }

                if (needsRender)
                {
                    if (_filterGraph.RenderEx(pins[0], AMRenderExFlags.RenderToExistingRenderers, IntPtr.Zero) >= 0)
                        pinsRendered++;
                }
                else
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
                SetVideoWindow(enableMadvrExclusiveMode);
            }
        }

        private List<Guid> GetPinMediaTypes(DirectShowLib.IPin pin)
        {
            int hr = 0;
            int j = -1;
            var mt = new List<Guid>();

            IEnumMediaTypes emtDvr;
            pin.EnumMediaTypes(out emtDvr);

            while (j != 0)
            {
                var amtDvr = new DirectShowLib.AMMediaType[1];
                IntPtr d = Marshal.AllocCoTaskMem(4);
                try
                {
                    hr = emtDvr.Next(1, amtDvr, d);
                    DsError.ThrowExceptionForHR(hr);
                    j = Marshal.ReadInt32(d);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(d);
                }

                if (j != 0)
                {
                    mt.Add(amtDvr[0].majorType);

                    DsUtils.FreeAMMediaType(amtDvr[0]);
                    amtDvr[0] = null;
                }
            }
            return mt;
        }

        private void InitializeEvr(DirectShowLib.IBaseFilter pEvr, int dwStreams)
        {
            IMFVideoDisplayControl pDisplay;

            // Continue with the rest of the set-up.

            // Set the video window.
            object o;
            var pGetService = (IMFGetService)pEvr;
            var hr = pGetService.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);

            DsError.ThrowExceptionForHR(hr);

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
            hr = pDisplay.SetVideoWindow(VideoWindowHandle);
            DsError.ThrowExceptionForHR(hr);

            // Return the IMFVideoDisplayControl pointer to the caller.
            _mPDisplay = pDisplay;
        }

        private void SetVideoWindow(bool enableMadVrExclusiveMode)
        {
            _isInExclusiveMode = _madvr != null && enableMadVrExclusiveMode;

            if (!enableMadVrExclusiveMode)
            {
                _hiddenWindow.SizeChanged += _hiddenWindow_SizeChanged;
                _hiddenWindow.MouseClick += HiddenForm_MouseClick;
                _hiddenWindow.KeyDown += HiddenForm_KeyDown;
                _removeHandlers = true;
            }

            if (_cursorHidden)
            {
                _videoWindow.HideCursor(OABool.True);
            }

            if (_madvr != null)
            {
                var ownerHandle = enableMadVrExclusiveMode ? _applicationWindowHandle : VideoWindowHandle;

                _videoWindow.put_Owner(ownerHandle);
                _videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings);
                _videoWindow.put_WindowStyleEx(WindowStyleEx.ToolWindow);

                _videoWindow.put_Visible(OABool.True);
                _videoWindow.put_AutoShow(OABool.True);
                _videoWindow.put_WindowState(WindowState.Show);

                var hr = _videoWindow.SetWindowForeground(OABool.True);
                DsError.ThrowExceptionForHR(hr);

                if (enableMadVrExclusiveMode)
                {
                    //_videoWindow.put_FullScreenMode(OABool.True);
                }

                else
                {
                   // hr = _videoWindow.put_MessageDrain(VideoWindowHandle); XXX
                    hr = _videoWindow.put_MessageDrain(_applicationWindowHandle);
                    DsError.ThrowExceptionForHR(hr);
                }
            }

            SetAspectRatio();

            if (_madvr != null)
            {
                SetExclusiveMode(enableMadVrExclusiveMode);
            }
        }

        void _hiddenWindow_SizeChanged(object sender, EventArgs e)
        {
            SetAspectRatio(null);
        }

        void HiddenForm_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("HiddenForm_KeyDown: {0} {1}", e.KeyCode, (int) e.KeyCode);
            switch (e.KeyCode)
            {
                case Keys.Return:
                    if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
                    {
                        _mDvdControl.ActivateButton();
                        e.SuppressKeyPress = true;
                    }
                    else if ((_dvdMenuMode == DvdMenuMode.Still) && (_mDvdControl != null))
                    {
                        _mDvdControl.StillOff();
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.Left:
                    if (_mDvdControl != null)
                    {
                        if (_dvdMenuMode == DvdMenuMode.Buttons)
                            _mDvdControl.SelectRelativeButton(DvdRelativeButton.Left);
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.Right:
                    if (_mDvdControl != null)
                    {
                        if (_dvdMenuMode == DvdMenuMode.Buttons)
                            _mDvdControl.SelectRelativeButton(DvdRelativeButton.Right);
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.Up:
                    if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
                    {
                        _mDvdControl.SelectRelativeButton(DvdRelativeButton.Upper);
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.Down:
                    if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
                    {
                        _mDvdControl.SelectRelativeButton(DvdRelativeButton.Lower);
                        e.SuppressKeyPress = true;
                    }
                    break;
            }
        }

        void HiddenForm_MouseClick(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("HiddenForm_MouseClick: {0}", e);
            if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
            {
                Point pt = new Point();
                pt.X = e.X;
                pt.Y = e.Y;
                _mDvdControl.SelectAtPosition(pt);
                //_mDvdControl.ActivateButton();
            }
        }

        private void SetAspectRatio(Size? ratio = null, bool setVideoWindow = true)
        {
            int screenWidth;
            int screenHeight;

            if (m_graph == null)
                return;

            if (_isInExclusiveMode)
            {
                var size = Screen.FromControl(_hiddenWindow.Form).Bounds;

                screenWidth = size.Width;
                screenHeight = size.Height;
            }
            else
            {
                var hiddenWindowContentSize = _hiddenWindow.ContentPixelSize;

                screenWidth = hiddenWindowContentSize.Width;
                screenHeight = hiddenWindowContentSize.Height;
            }

            // Set the display position to the entire window.
            if (_mPDisplay != null)
            {
                var rc = new MFRect(0, 0, screenWidth, screenHeight);
                _mPDisplay.SetVideoPosition(null, rc);
            }

            // Get Aspect Ratio
            int aspectX;
            int aspectY;

            if (ratio.HasValue)
            {
                aspectX = ratio.Value.Width;
                aspectY = ratio.Value.Height;
            }
            else
            {
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
            }

            // Adjust Video Size
            var iAdjustedHeight = 0;

            if (aspectX > 0 && aspectY > 0)
            {
                double adjustedHeight = aspectY * screenWidth;
                adjustedHeight /= aspectX;

                iAdjustedHeight = Convert.ToInt32(Math.Round(adjustedHeight));
            }

            if (screenHeight > iAdjustedHeight && iAdjustedHeight > 0)
            {
                double totalMargin = (screenHeight - iAdjustedHeight);
                var topMargin = Convert.ToInt32(Math.Round(totalMargin / 2));

                _basicVideo.SetDestinationPosition(0, topMargin, screenWidth, iAdjustedHeight);
            }
            else if (iAdjustedHeight > 0)
            {
                double adjustedWidth = aspectX * screenHeight;
                adjustedWidth /= aspectY;

                var iAdjustedWidth = Convert.ToInt32(Math.Round(adjustedWidth));

                double totalMargin = (screenWidth - iAdjustedWidth);
                var leftMargin = Convert.ToInt32(Math.Round(totalMargin / 2));

                _basicVideo.SetDestinationPosition(leftMargin, 0, iAdjustedWidth, screenHeight);
            }

            if (setVideoWindow)
            {
                _videoWindow.SetWindowPosition(0, 0, screenWidth, screenHeight);
            }
        }

        //private readonly Bitmap _cursorBitmap = new Bitmap(1, 1);
        //private Cursor _blankCursor;
        private bool _cursorHidden;

        //public void ShowCursor()
        //{
        //    Cursor = Cursors.Default;

        //    if (_videoWindow != null)
        //    {
        //        _videoWindow.HideCursor(OABool.False);
        //    }
        //    _cursorHidden = false;
        //}

        //public void HideCursor()
        //{
        //    _blankCursor = _blankCursor ?? (_blankCursor = CustomCursor.CreateCursor(_cursorBitmap, 1, 1));
        //    Cursor = _blankCursor;

        //    if (_videoWindow != null)
        //    {
        //        _videoWindow.HideCursor(OABool.True);
        //    }
        //    _cursorHidden = true;
        //}

        public void SetExclusiveMode(bool enable)
        {
            try
            {
                MadvrInterface.EnableExclusiveMode(true, _madvr);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error changing exclusive mode", ex);
            }
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

        private void HandleDvdEvent()
        {
            int hr = 0;
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
                    _logger.Debug("Received media event code {0}", evCode);

                    switch (evCode)
                    {
                        case EventCode.DvdCurrentHmsfTime:
                            byte[] ati = BitConverter.GetBytes(evParam1.ToInt32());
                            var currnTime = new DvdHMSFTimeCode();
                            currnTime.bHours = ati[0];
                            currnTime.bMinutes = ati[1];
                            currnTime.bSeconds = ati[2];
                            currnTime.bFrames = ati[3];
                            //UpdateMainTitle();
                            break;
                        case EventCode.DvdChapterStart:
                            //currnChapter = evParam1.ToInt32();
                            //UpdateMainTitle();
                            break;
                        case EventCode.DvdTitleChange:
                            //currnTitle = evParam1.ToInt32();
                            //UpdateMainTitle();
                            break;
                        case EventCode.DvdDomainChange:
                            //currnDomain = (DvdDomain)evParam1;
                            //UpdateMainTitle();
                            break;
                        case EventCode.DvdCmdStart:
                            break;
                        case EventCode.DvdCmdEnd:
                            //OnCmdComplete(evParam1, evParam2);
                            break;
                        case EventCode.DvdStillOn:
                            if (evParam1 == IntPtr.Zero)
                                _dvdMenuMode = DvdMenuMode.Buttons;
                            else
                                _dvdMenuMode = DvdMenuMode.Still;
                            break;
                        case EventCode.DvdStillOff:
                            if (_dvdMenuMode == DvdMenuMode.Still)
                                _dvdMenuMode = DvdMenuMode.No;
                            break;
                        case EventCode.DvdButtonChange:
                            if (evParam1.ToInt32() <= 0)
                                _dvdMenuMode = DvdMenuMode.No;
                            else
                                _dvdMenuMode = DvdMenuMode.Buttons;
                            break;
                        case EventCode.DvdNoFpPgc:
                            IDvdCmd icmd;

                            if (_mDvdControl != null)
                                hr = _mDvdControl.PlayTitle(1, DvdCmdFlags.None, out icmd);
                            break;
                    }

                    // Free memory associated with callback, since we're not using it
                    hr = _mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

                }
            }
            catch
            {

            }
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

                    _logger.Debug("Received media event code {0}", evCode);

                    // If this is the end of the clip, close
                    if (evCode == EventCode.Complete)
                    {
                        Stop(TrackCompletionReason.Ended, null);
                    }
                    else if (evCode == EventCode.VideoSizeChanged)
                    {
                        var param1Val = evParam1.ToInt32();
                        var x = param1Val & 0xffff;
                        var y = param1Val >> 16;
                        var ratio = new Size(x, y);

                        SetAspectRatio(ratio, false);
                    }
                }
            }
            catch
            {

            }
        }

        private void DisposePlayer()
        {
            _hiddenWindow.OnWMGRAPHNOTIFY = null;
            _hiddenWindow.OnDVDEVENT = null;

            if (_removeHandlers)
            {
                _hiddenWindow.SizeChanged -= _hiddenWindow_SizeChanged;
                _hiddenWindow.MouseClick -= HiddenForm_MouseClick;
                _hiddenWindow.KeyDown -= HiddenForm_KeyDown;
                _removeHandlers = false;
            }

            _logger.Debug("Disposing player");

            CloseInterfaces();
        }

        private void CleanUpInterface(object o)
        {
            if (o != null)
                while (Marshal.ReleaseComObject(o) > 0) ;
            o = null;
        }

        private void CloseInterfaces()
        {
            int hr;

            if (_defaultAudioRenderer != null)
            {
                m_graph.RemoveFilter(_defaultAudioRenderer as DirectShowLib.IBaseFilter);

                CleanUpInterface(_defaultAudioRenderer);
                _defaultAudioRenderer = null;
            }

            if (_reclockAudioRenderer != null)
            {
                m_graph.RemoveFilter(_reclockAudioRenderer as DirectShowLib.IBaseFilter);

                CleanUpInterface(_reclockAudioRenderer);
                _reclockAudioRenderer = null;
            }

            if (_lavaudio != null)
            {
                m_graph.RemoveFilter(_lavaudio as DirectShowLib.IBaseFilter);

                CleanUpInterface(_lavaudio);
                _lavaudio = null;
            }

            if (_xyVsFilter != null)
            {
                m_graph.RemoveFilter(_xyVsFilter as DirectShowLib.IBaseFilter);

                CleanUpInterface(_xyVsFilter);
                _xyVsFilter = null;
            }

            if (_xySubFilter != null)
            {
                m_graph.RemoveFilter(_xySubFilter as DirectShowLib.IBaseFilter);

                CleanUpInterface(_xySubFilter);
                _xySubFilter = null;
            }

            if (_lavvideo != null)
            {
                m_graph.RemoveFilter(_lavvideo as DirectShowLib.IBaseFilter);

                CleanUpInterface(_lavvideo);
                _lavvideo = null;
            }

            if (_madvr != null)
            {
                m_graph.RemoveFilter(_madvr as DirectShowLib.IBaseFilter);

                CleanUpInterface(_madvr);
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
                //Marshal.ReleaseComObject(_mediaEventEx);
                //_mediaEventEx = null;
            }

            //if (_dvdNav != null)
            //{
            //    Marshal.ReleaseComObject(_dvdNav);
            //    _dvdNav = null;
            //}
            /* //this will double release the source filter
            if (dvdInfo != null)
            {
                Marshal.ReleaseComObject(dvdInfo);
                dvdInfo = null;
            }

            if (_mDvdControl != null)
            {
                Marshal.ReleaseComObject(_mDvdControl);                
            }
            */
            _mDvdControl = null;

            CleanUpInterface(_mPDisplay);
            _mPDisplay = null;
            CleanUpInterface(_sourceFilter);
            _sourceFilter = null;
            CleanUpInterface(_mPEvr);
            _mPEvr = null;
            CleanUpInterface(m_filterGraph);
            m_filterGraph = null;

            m_filterGraph = null;
            _mediaEventEx = null;
            _mediaSeeking = null;
            _mediaPosition = null;
            _mediaControl = null;
            _basicAudio = null;
            _basicVideo = null;
            m_graph = null;
            _videoWindow = null;
            _filterGraph = null;

            if (m_dsRot != null)
                m_dsRot.Dispose();
            m_dsRot = null;

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

        public void Dispose()
        {
        }
    }
}