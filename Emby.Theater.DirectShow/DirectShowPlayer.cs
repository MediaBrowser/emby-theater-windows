using System.Drawing;
using DirectShowLib;
using DirectShowLib.Dvd;
using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using DirectShowLib.Utils;
using System.Windows.Input;
using CoreAudioApi;
using System.Windows.Forms;
using Emby.Theater.Common;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Common.Net;
using Emby.Theater.DirectShow.Configuration;
using MediaBrowser.Model.MediaInfo;

namespace Emby.Theater.DirectShow
{
    #region TempStuffSoItWillWork

    public enum PlayState
    {
        /// <summary>
        /// The idle
        /// </summary>
        Idle,

        /// <summary>
        /// The playing
        /// </summary>
        Playing,

        /// <summary>
        /// The paused
        /// </summary>
        Paused
    }

    public class SelectableMediaStream
    {
        private bool _isActive;

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                //OnPropertyChanged("IsActive");
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public MediaStreamType Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        /// <value>The Path - used for external subtitles .</value>
        public string Path { get; set; }
    }

    public class PlayableItem
    {
        public string PlayablePath { get; set; }

        public BaseItemDto OriginalItem { get; set; }
        public MediaSourceInfo MediaSource { get; set; }

        public bool IsVideo
        {
            get { return OriginalItem.IsVideo; }
        }

        public List<MediaStream> MediaStreams
        {
            get { return MediaSource.MediaStreams; }
        }

        public IIsoMount IsoMount { get; set; }
    }
    #endregion

    public class DirectShowPlayer : IDisposable
    {
        private static Guid DvdEncryptedMediaType = new Guid("{ed0b916a-044d-11d1-aa78-00c04fc31d60}");
        private static Guid SubtitleMediaType = new Guid("E487EB08-6B26-4be9-9DD3-993434D313FD");
        private static Guid DvdSubpictureMediaType = new Guid("e06d802d-db46-11cf-b4d1-00805f6cbbea");


        private readonly ILogger _logger;
        //private readonly IHiddenWindow _hiddenWindow;
        private readonly InternalDirectShowPlayer _playerWrapper;
        //private readonly ISessionManager _sessionManager;
        private readonly IHttpClient _httpClient;

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
        private DsROTEntry m_dsRot = null;
        private bool _isDvd = false;
        private DirectShowLib.IPin m_adecOut = null;

        private object _xyVsFilter = null;
        private object _xySubFilter = null;

        private object _lavaudio = null;
        private object _lavvideo = null;

        // EVR filter
        private DirectShowLib.IBaseFilter _mPEvr = null;
        private IMFVideoDisplayControl _mPDisplay = null;

        private DefaultAudioRenderer _defaultAudioRenderer = null;
        private ReclockAudioRenderer _reclockAudioRenderer = null;
        private object _wasapiAR = null;

        // Caps bits for IMediaSeeking
        private AMSeekingSeekingCapabilities _mSeekCaps;

        // Dvd
        //private DirectShowLib.IBaseFilter _dvdNav = null;
        private IDvdControl2 _mDvdControl = null;
        //private IDvdInfo2 _mDvdInfo = null;
        private IDvdCmd _mDvdCmdOption = null;
        private bool _pendingDvdCmd;
        private double _currentPlaybackRate = 1.0;

        private object _madvr = null;

        private PlayableItem _item = null;

        private readonly IntPtr _applicationWindowHandle;
        private bool _isInExclusiveMode;
        private DvdMenuMode _dvdMenuMode = DvdMenuMode.No;
        private bool _removeHandlers = false;
        private DirectShowPlayerConfiguration _config;
        private string _filePath = string.Empty;
        private bool _customEvrPresenterLoaded = false;
        //private IUserInputManager _input = null;
        //private MMDevice _audioDevice = null;
        private Resolution _startResolution = null;
        VideoScalingScheme _iVideoScaling = VideoScalingScheme.FROMINSIDE;

        MainBaseForm _hostForm = null;

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

        public VideoScalingScheme VideoScaling
        {
            get
            {
                return _iVideoScaling;
            }
            set
            {
                if (value != _iVideoScaling)
                {
                    _iVideoScaling = value;
                    SetAspectRatio();
                }
            }
        }

        public bool IsFullScreen
        {
            get
            {
                bool isFS = false;

                //Multimonitor support
                Screen screen = Screen.FromControl(_hostForm);
                if (_hostForm.Height == screen.Bounds.Height && _hostForm.Width == screen.Bounds.Width)
                    isFS = true;

                _logger.Debug("IsFullScreen: W: {0} H: {1} Top: {2} Bottom: {3} Left: {4} Right: {5}", _hostForm.Width, _hostForm.Height, screen.Bounds.Top, screen.Bounds.Bottom, screen.Bounds.Left, screen.Bounds.Right);

                return isFS;
            }
        }

        public DirectShowPlayer(
            InternalDirectShowPlayer playerWrapper
            , MainBaseForm hostForm
            , ILogger logger
            , DirectShowPlayerConfiguration config
            , IHttpClient httpClient)
        {
            _playerWrapper = playerWrapper;
            _hostForm = hostForm;
            _logger = logger;
            _config = config;
            _httpClient = httpClient;

            _hostForm.GraphNotify += _hostForm_GraphNotify;
            _hostForm.DvdEvent += _hostForm_DvdEvent;
        }

        void _hostForm_DvdEvent(object sender, EventArgs e)
        {
            HandleDvdEvent();
        }

        void _hostForm_GraphNotify(object sender, EventArgs e)
        {
            HandleGraphEvent();
        }

        //public DirectShowPlayer(ILogger logger, IHiddenWindow hiddenWindow, InternalDirectShowPlayer playerWrapper,
        //    IntPtr applicationWindowHandle, ISessionManager sessionManager, ITheaterConfigurationManager mbtConfig,
        //    IUserInputManager input, IApiClient apiClient, IZipClient zipClient, IHttpClient httpClient)
        //{
        //    _logger = logger;
        //    _hiddenWindow = hiddenWindow;
        //    _playerWrapper = playerWrapper;
        //    _applicationWindowHandle = applicationWindowHandle;
        //    _sessionManager = sessionManager;
        //    _httpClient = httpClient;
        //    _input = input;
        //    _input.KeyDown += HiddenForm_KeyDown;
        //    //_input.
        //    _mbtConfig = mbtConfig;
        //    _apiClient = apiClient;
        //}

        private IBaseFilter AudioRenderer
        {
            get
            {
                if (_reclockAudioRenderer != null)
                {
                    return _reclockAudioRenderer as IBaseFilter;
                }
                else if (_wasapiAR != null)
                {
                    return _wasapiAR as IBaseFilter;
                }
                else
                {
                    return _defaultAudioRenderer as IBaseFilter;
                }
            }
        }

        //private void _input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    //throw new NotImplementedException();
        //}

        private IntPtr VideoWindowHandle
        {
            get
            {
                return _hostForm.Handle;
                IntPtr chromeWindow = NativeMethods.FindWindowEx(_hostForm.Handle, IntPtr.Zero, null, null);

                //crawl down the window stack until we reach the bottom
                IntPtr lastPtr = IntPtr.Zero;
                do
                {
                    lastPtr = chromeWindow;
                    chromeWindow = NativeMethods.FindWindowEx(chromeWindow, IntPtr.Zero, null, null);

                } while (chromeWindow != IntPtr.Zero);

                if (lastPtr != IntPtr.Zero)
                    return lastPtr;
                else
                    return _hostForm.Handle;
            }
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


        public int? CurrentSubtitleStreamIndex
        {
            get
            {
                if (PlayState != PlayState.Idle)
                {

                    var stream = _streams.FirstOrDefault(i => i.Type == MediaStreamType.Subtitle && i.IsActive == true);
                    return stream != null ? (int?)stream.Index : null;
                }

                return null;
            }
        }


        public int? CurrentAudioStreamIndex
        {
            get
            {
                if (PlayState != PlayState.Idle)
                {

                    var stream = _streams.FirstOrDefault(i => i.Type == MediaStreamType.Audio && i.IsActive == true);
                    return stream != null ? (int?)stream.Index : null;
                }


                return null;
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

        private bool IsEvrAvailable()
        {
            return FilterGraphTools.IsThisComObjectInstalled(typeof(EnhancedVideoRenderer).GUID);
        }

        private bool IsIntelGPU()
        {
            return VideoConfiguration.GpuModel.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) != -1;
        }

        public void Play(PlayableItem item, bool enableFullScreen)
        {
            try
            {
                // Don't need madvr when not playing fullscreen
                string videoRenderer = enableFullScreen ? 
                    _config.VideoConfig.VideoRenderer : 
                    (IsEvrAvailable() ? "evr" : "evrcp");

                if (string.IsNullOrWhiteSpace(videoRenderer))
                {
                    videoRenderer = IsIntelGPU() ? "evrcp" : (IsEvrAvailable() ? "evr" : "madvr");
                }

                var enableMadVr = string.Equals(videoRenderer, "madvr", StringComparison.OrdinalIgnoreCase);

                _logger.Info("Playing {0}. Audio Renderer: {1}, Madvr: {2}, xySubFilter: {3}, ParentID: {4}", item.OriginalItem.Name,
                    _config.AudioConfig.Renderer, enableMadVr,
                    _config.SubtitleConfig.EnableXySubFilter,
                    item.OriginalItem.ParentId);
                _logger.Info("Playing Path {0}", item.PlayablePath);

                var mediaSource = item.MediaSource;

                _item = item;
                _isInExclusiveMode = false;
                TimeSpan itemDuration = TimeSpan.MaxValue;

                if (mediaSource.RunTimeTicks.HasValue && mediaSource.RunTimeTicks.Value > 0)
                    itemDuration = TimeSpan.FromTicks(mediaSource.RunTimeTicks.Value);

                _iVideoScaling = (VideoScalingScheme)_config.VideoConfig.ScalingMode;

                if (item.IsVideo
                    && IsFullScreen
                    && !string.IsNullOrWhiteSpace(item.OriginalItem.ParentId)
                    && _config.VideoConfig.AutoChangeRefreshRate
                    && _config.VideoConfig.MinRefreshRateMin < itemDuration.TotalMinutes
                    )
                {
                    if (item.MediaStreams == null)
                    {
                        _logger.Warn("item.MediaStreams is null, cannot detect framerate");
                    }
                    else
                    {
                        //find the video stream (assume that the first one is the main one)
                        foreach (var ms in item.MediaStreams)
                        {
                            if (ms.Type == MediaStreamType.Video)
                            {
                                _startResolution = Display.GetCurrentResolution(_hostForm);
                                if (ms.RealFrameRate.HasValue && !ms.IsInterlaced)
                                {
                                    int videoRate = (int)ms.RealFrameRate;

                                    if (videoRate == 25 || videoRate == 29 || videoRate == 30)
                                    {
                                        //Every display/GPU should be able to display @2x FPS and it's quite likely that 2x is the rendered FPS anyway
                                        videoRate = (int)(ms.RealFrameRate * 2);
                                    }

                                    _logger.Info("RealFrameRate: {0} videoRate: {1} startRate: {2}", ms.RealFrameRate, videoRate, _startResolution);

                                    if (videoRate != _startResolution.Rate)
                                    {
                                        Resolution desiredRes = new Resolution(_startResolution.ToString());
                                        desiredRes.Rate = videoRate;
                                        if (Display.ChangeResolution(_hostForm, desiredRes, false))
                                            _logger.Info("Changed resolution from {0} to {1}", _startResolution, desiredRes);
                                        else
                                        {
                                            _logger.Info("Couldn't change resolution from {0} to {1}", _startResolution, desiredRes);
                                            _startResolution = null;
                                        }
                                    }
                                    else
                                        _startResolution = null;

                                    break;
                                }
                            }
                            else
                                _startResolution = null;
                        }
                    }
                }

                var isDvd = ((item.MediaSource.VideoType ?? VideoType.VideoFile) == VideoType.Dvd ||
                             (item.MediaSource.IsoType ?? IsoType.BluRay) == IsoType.Dvd);

                Initialize(item.PlayablePath, isDvd, videoRenderer);

                //_hiddenWindow.OnWMGRAPHNOTIFY = HandleGraphEvent;
                //_hiddenWindow.OnDVDEVENT = HandleDvdEvent;

                //pre-roll the graph
                _logger.Debug("pre-roll the graph");
                var hr = _mediaControl.Pause();
                DsError.ThrowExceptionForHR(hr);

                _logger.Debug("run the graph");
                hr = _mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);

                PlayState = PlayState.Playing;
                _currentPlaybackRate = 1.0;

                _streams = GetStreams();

                _logger.Debug("DSPlayer Done in play");
            }
            catch (Exception ex)
            {
                CloseInterfaces();
                throw ex;
            }
        }

        public void SetVolume(int level)
        {
            int hr = 0;
            if (_basicAudio != null)
                hr = _basicAudio.put_Volume(level);
        }

        private void InitializeGraph(bool isDvd)
        {
            int hr = 0;
            _isDvd = isDvd;
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
                hr = _mediaEventEx.SetNotifyWindow(VideoWindowHandle, DirectShowPlayerConfiguration.WM_DVD_EVENT, IntPtr.Zero);
            else
                hr = _mediaEventEx.SetNotifyWindow(VideoWindowHandle, DirectShowPlayerConfiguration.WM_GRAPH_NOTIFY, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);

            if (_config.PublishGraph)
                m_dsRot = new DsROTEntry(m_graph as IFilterGraph);
        }

        private void Initialize(string path,
            bool isDvd, string videoRenderer)
        {
            _filePath = path;

            InitializeGraph(isDvd);

            int hr = 0;

            if (isDvd)
            {
                _logger.Debug("Initializing dvd player to play {0}", path);

                /* Create a new DVD Navigator. */
                _sourceFilter = (DirectShowLib.IBaseFilter)new DVDNavigator();

                InitializeDvd(path);

                // Try to render the streams.
                RenderStreams(_sourceFilter, videoRenderer, false);
                //we don't need XySubFilter for DVD 
            }
            else
            {
                //prefer LAV Spliter Source
                bool loadSource = true;
                object objLavSource = URCOMLoader.Instance.GetObject(typeof(LAVSplitterSource).GUID, true);
                _sourceFilter = objLavSource as IBaseFilter;

                if (_sourceFilter != null)
                {
                    hr = m_graph.AddFilter(_sourceFilter, "LAV Splitter Source");
                    DsError.ThrowExceptionForHR(hr);

                    if (_sourceFilter != null)
                    {
                        ILAVSplitterSettings lss = _sourceFilter as ILAVSplitterSettings;
                        if (lss != null)
                        {
                            _logger.Info("Configure LAV Splitter");

                            hr = lss.SetRuntimeConfig(true);
                            DsError.ThrowExceptionForHR(hr);

                            if (!string.IsNullOrWhiteSpace(_config.SplitterConfig.PreferredAudioLanguages))
                            {
                                _logger.Info("Set preferred audio lang: {0}", _config.SplitterConfig.PreferredAudioLanguages);
                                hr = lss.SetPreferredLanguages(_config.SplitterConfig.PreferredAudioLanguages);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            if (!string.IsNullOrWhiteSpace(_config.SplitterConfig.PreferredSubtitleLanguages))
                            {
                                _logger.Info("Set preferred subs lang: {0}", _config.SplitterConfig.PreferredSubtitleLanguages);
                                hr = lss.SetPreferredSubtitleLanguages(_config.SplitterConfig.PreferredSubtitleLanguages);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            if (!string.IsNullOrWhiteSpace(_config.SplitterConfig.AdvancedSubtitleConfig))
                            {
                                _logger.Info("Set preferred subs lang: {0}", _config.SplitterConfig.AdvancedSubtitleConfig);
                                hr = lss.SetAdvancedSubtitleConfig(_config.SplitterConfig.AdvancedSubtitleConfig);
                                DsError.ThrowExceptionForHR(hr);
                            }

                            _logger.Info("SetSubtitleMode: {0}", _config.SplitterConfig.PreferredSubtitleLanguages);
                            hr = lss.SetSubtitleMode((LAVSubtitleMode)Enum.Parse(typeof(LAVSubtitleMode), _config.SplitterConfig.SubtitleMode));
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Info("SetPGSForcedStream: {0}", _config.SplitterConfig.PGSForcedStream);
                            //hr = lss.SetPGSForcedStream(_config.SplitterConfig.PGSForcedStream);
                            hr = lss.SetPGSForcedStream(false);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Info("SetPGSOnlyForced: {0}", _config.SplitterConfig.PGSOnlyForced);
                            hr = lss.SetPGSOnlyForced(_config.SplitterConfig.PGSOnlyForced);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetVC1TimestampMode: {0}", _config.SplitterConfig.VC1TimestampMode);
                            hr = lss.SetVC1TimestampMode((short)Enum.Parse(typeof(VC1TimestampMode), _config.SplitterConfig.VC1TimestampMode));
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetSubstreamsEnabled: {0}", _config.SplitterConfig.SubstreamsEnabled);
                            hr = lss.SetSubstreamsEnabled(_config.SplitterConfig.SubstreamsEnabled);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetStreamSwitchRemoveAudio: {0}", _config.SplitterConfig.StreamSwitchRemoveAudio);
                            hr = lss.SetStreamSwitchRemoveAudio(_config.SplitterConfig.StreamSwitchRemoveAudio);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetUseAudioForHearingVisuallyImpaired: {0}", _config.SplitterConfig.UseAudioForHearingVisuallyImpaired);
                            hr = lss.SetUseAudioForHearingVisuallyImpaired(_config.SplitterConfig.UseAudioForHearingVisuallyImpaired);
                            DsError.ThrowExceptionForHR(hr);

                            int MaxQueueMemSize = lss.GetMaxQueueMemSize();

                            _logger.Debug("SetMaxQueueMemSize: from {0} to {1}", MaxQueueMemSize, _config.SplitterConfig.MaxQueueMemSize);
                            hr = lss.SetMaxQueueMemSize(_config.SplitterConfig.MaxQueueMemSize);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetTrayIcon: {0}", _config.SplitterConfig.ShowTrayIcon);
                            hr = lss.SetTrayIcon(_config.SplitterConfig.ShowTrayIcon);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetPreferHighQualityAudioStreams: from {0}", _config.SplitterConfig.PreferHighQualityAudioStreams);
                            hr = lss.SetPreferHighQualityAudioStreams(_config.SplitterConfig.PreferHighQualityAudioStreams);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Debug("SetLoadMatroskaExternalSegments: {0}", _config.SplitterConfig.LoadMatroskaExternalSegments);
                            hr = lss.SetLoadMatroskaExternalSegments(_config.SplitterConfig.LoadMatroskaExternalSegments);
                            DsError.ThrowExceptionForHR(hr);

                            int NetworkStreamAnalysisDuration = lss.GetNetworkStreamAnalysisDuration();

                            _logger.Debug("SetNetworkStreamAnalysisDuration: from {0} to {1}", NetworkStreamAnalysisDuration, _config.SplitterConfig.NetworkStreamAnalysisDuration);
                            hr = lss.SetNetworkStreamAnalysisDuration(_config.SplitterConfig.NetworkStreamAnalysisDuration);
                            DsError.ThrowExceptionForHR(hr);

                            int MaxQueueSize = lss.GetMaxQueueSize();

                            _logger.Debug("SetMaxQueueSize: from {0} to {1}", MaxQueueSize, _config.SplitterConfig.MaxQueueSize);
                            hr = lss.SetMaxQueueSize(_config.SplitterConfig.MaxQueueSize);
                            DsError.ThrowExceptionForHR(hr);
                        }

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
                RenderStreams(_sourceFilter, videoRenderer, true);
            }

            // Get the seeking capabilities.
            hr = _mediaSeeking.GetCapabilities(out _mSeekCaps);
            DsError.ThrowExceptionForHR(hr);

            _logger.Debug("Retrieved seeking capabilities: {0}", _mSeekCaps);
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

        private void RenderStreams(DirectShowLib.IBaseFilter pSource,
            string videoRenderer,
            bool enableXySubFilter)
        {
            int hr;

            _filterGraph = m_graph as DirectShowLib.IFilterGraph2;
            if (_filterGraph == null)
            {
                throw new Exception("Could not QueryInterface for the IFilterGraph2");
            }

            var useDefaultRenderer = true;

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

                    var enableMadvr = string.Equals(videoRenderer, "madvr", StringComparison.OrdinalIgnoreCase);

                    try
                    {
                        if (mediaTypes[m] == DirectShowLib.MediaType.Video)
                        {
                            #region Video

                            //add the video renderer first so we know whether to enable DXVA2 in "Auto" mode.
                            if (enableMadvr)
                            {
                                try
                                {
                                    _madvr = URCOMLoader.Instance.GetObject(typeof(MadVR).GUID, true); // new MadVR();
                                    var vmadvr = _madvr as DirectShowLib.IBaseFilter;
                                    if (vmadvr != null)
                                    {
                                        hr = m_graph.AddFilter(vmadvr, "MadVR Video Renderer");
                                        DsError.ThrowExceptionForHR(hr);

                                        try
                                        {
                                            MadVRSettings msett = new MadVRSettings(_madvr);

                                            bool smoothMotion = msett.GetBool("smoothMotionEnabled");

                                            if (smoothMotion !=
                                                _config.VideoConfig
                                                    .UseMadVrSmoothMotion)
                                                msett.SetBool("smoothMotionEnabled",
                                                    _config.VideoConfig
                                                        .UseMadVrSmoothMotion);

                                            if (
                                                string.Compare(msett.GetString("smoothMotionMode"),
                                                    _config.VideoConfig
                                                        .MadVrSmoothMotionMode, true) != 0)
                                            {
                                                bool success = msett.SetString("smoothMotionMode",
                                                    _config.VideoConfig
                                                        .MadVrSmoothMotionMode);
                                            }
                                            MFNominalRange levels = (MFNominalRange)_config.VideoConfig.NominalRange;
                                            string madVrLevelInitial = msett.GetString("levels");
                                            switch (levels)
                                            {
                                                case MFNominalRange.MFNominalRange_0_255:
                                                    msett.SetString("levels", "PC Levels");
                                                    break;
                                                case MFNominalRange.MFNominalRange_16_235:
                                                    msett.SetString("levels", "TV Levels");
                                                    break;
                                            }
                                            string madVrLevel = msett.GetString("levels");

                                            if (string.Compare(madVrLevel, madVrLevelInitial, false) != 0)
                                                _logger.Debug("Changed madVR levels from {0} to {1}", madVrLevelInitial, madVrLevel);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.ErrorException("Error configuring madVR", ex);
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.ErrorException("Error adding MadVR filter", ex);
                                }
                            }
                            else // Add default video renderer
                            {
                                _mPEvr = (DirectShowLib.IBaseFilter)new EnhancedVideoRenderer();
                                hr = m_graph.AddFilter(_mPEvr, "EVR");
                                DsError.ThrowExceptionForHR(hr);

                                //we only need 2 input pins on the EVR if LAV Video isn't used for DVDs, but it doesn't hurt to have them
                                InitializeEvr(_mPEvr, _isDvd ? 2 : 1, videoRenderer);
                            }

                            try
                            {
                                _lavvideo = URCOMLoader.Instance.GetObject(typeof(LAVVideo).GUID, true); //new LAVVideo();
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

                                        _logger.Info("GPU Model: {0}", VideoConfiguration.GpuModel);

                                        LAVHWAccel configuredMode =
                                            VideoConfigurationUtils.GetHwaMode(
                                                _config.VideoConfig,
                                                _customEvrPresenterLoaded);

                                        LAVHWAccel testme = vsett.GetHWAccel();
                                        _logger.Info("Current HWA Mode: {0} Desired Mode: {1}", testme, configuredMode);
                                        if (testme != configuredMode)
                                        {
                                            hr = vsett.SetHWAccel(configuredMode);
                                            DsError.ThrowExceptionForHR(hr);
                                        }

                                        //TODO: add this back when CODECs are surfaced through the config UI
                                        //foreach (string c in DirectShowPlayer.GetLAVVideoCodecs())
                                        //{
                                        //    LAVVideoCodec codec = (LAVVideoCodec)Enum.Parse(typeof(LAVVideoCodec), c);

                                        //    bool isEnabled = vsett.GetFormatConfiguration(codec);
                                        //    if (
                                        //        _config.VideoConfig.EnabledCodecs
                                        //            .Contains(c))
                                        //    {
                                        //        if (!isEnabled)
                                        //        {
                                        //            _logger.Debug("Enable support for: {0}", c);
                                        //            hr = vsett.SetFormatConfiguration(codec, true);
                                        //            DsError.ThrowExceptionForHR(hr);
                                        //        }
                                        //    }
                                        //    else if (isEnabled)
                                        //    {
                                        //        _logger.Debug("Disable support for: {0}", c);
                                        //        hr = vsett.SetFormatConfiguration(codec, false);
                                        //        DsError.ThrowExceptionForHR(hr);
                                        //    }
                                        //}

                                        foreach (string hwaCodec in DirectShowPlayer.GetLAVVideoHwaCodecs())
                                        {
                                            LAVVideoHWCodec codec = (LAVVideoHWCodec)Enum.Parse(typeof(LAVVideoHWCodec), hwaCodec);
                                            bool hwaIsEnabled = vsett.GetHWAccelCodec(codec);

                                            if (
                                                _config.VideoConfig.HwaEnabledCodecs
                                                    .Contains(hwaCodec))
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
                                        if (hwaRes != _config.VideoConfig.HwaResolution
                                            && _config.VideoConfig.HwaResolution > 0)
                                        {
                                            _logger.Debug("Change HWA resolution support from {0} to {1}.", hwaRes,
                                                _config.VideoConfig.HwaResolution);
                                            hr =
                                                vsett.SetHWAccelResolutionFlags(
                                                    VideoConfigurationUtils.GetHwaResolutions(
                                                        _config.VideoConfig));
                                            DsError.ThrowExceptionForHR(hr);
                                        }

                                        hr =
                                            vsett.SetTrayIcon(
                                                _config.VideoConfig.ShowTrayIcon);
                                        DsError.ThrowExceptionForHR(hr);
                                    }
                                }

                                decIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavvideo, PinDirection.Input, 0);
                                if (decIn != null)
                                {
                                    hr = _filterGraph.ConnectDirect(pins[0], decIn, null);
                                    DsError.ThrowExceptionForHR(hr);
                                    decOut = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavvideo,
                                        PinDirection.Output, 0);

                                    if (enableXySubFilter) //this flag indicates whether we should handle subtitle rendering
                                    {
                                        _logger.Debug("Enable XySubFilter.");
                                        var xySubFilterSucceeded = false;

                                        // Load xySubFilter if configured and if madvr succeeded
                                        if (_madvr != null || _customEvrPresenterLoaded)
                                        {
                                            try
                                            {
                                                _xySubFilter = URCOMLoader.Instance.GetObject(typeof(XySubFilter).GUID, true); //new XySubFilter();
                                                var vxySubFilter = _xySubFilter as DirectShowLib.IBaseFilter;
                                                if (vxySubFilter != null)
                                                {
                                                    hr = m_graph.AddFilter(vxySubFilter, "xy-SubFilter");
                                                    DsError.ThrowExceptionForHR(hr);
                                                }

                                                xySubFilterSucceeded = true;

                                                _logger.Debug("Enable XySubFilter : {0}.", xySubFilterSucceeded);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.ErrorException("Error adding xy-SubFilter filter", ex);
                                            }
                                        }

                                        // Fallback to xyVsFilter
                                        if (!xySubFilterSucceeded)
                                        {
                                            _logger.Debug("Fallback xyVsFilter.");
                                            try
                                            {
                                                _xyVsFilter = URCOMLoader.Instance.GetObject(typeof(XYVSFilter).GUID, true); //new XYVSFilter();
                                                var vxyVsFilter = _xyVsFilter as DirectShowLib.IBaseFilter;
                                                if (vxyVsFilter != null)
                                                {
                                                    hr = m_graph.AddFilter(vxyVsFilter, "xy-VSFilter");
                                                    DsError.ThrowExceptionForHR(hr);

                                                    _logger.Debug("Added xy-VSFilter");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.ErrorException("Error adding xy-VSFilter filter", ex);
                                            }
                                        }

                                        if (_xyVsFilter != null) //If using VSFilter
                                        {
                                            _logger.Debug("insert xyVsFilter b/w LAV Video and the renderer");
                                            //insert xyVsFilter b/w LAV Video and the renderer
                                            rendIn = DsFindPin.ByName((DirectShowLib.IBaseFilter)_xyVsFilter, "Video");

                                            //connect it to VSFilter
                                            if (decOut != null && rendIn != null)
                                            {
                                                hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                                DsError.ThrowExceptionForHR(hr);

                                                CleanUpInterface(rendIn);
                                                CleanUpInterface(decOut);
                                                rendIn = null;
                                                decOut = null;
                                            }

                                            //grab xyVsFilter's output pin so it can be connected to the renderer
                                            decOut = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_xyVsFilter,
                                                    PinDirection.Output, 0);
                                        }
                                    }

                                    if (_madvr != null)
                                    {
                                        rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_madvr,
                                            PinDirection.Input, 0);
                                    }
                                    else
                                    {
                                        rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_mPEvr,
                                            PinDirection.Input, 0);
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
                            catch (Exception ex)
                            {
                                _logger.ErrorException("Error adding LAV Video filter", ex);
                            }

                            #endregion
                        }
                        else if (mediaTypes[m] == DirectShowLib.MediaType.Audio)
                        {
                            #region Audio
                            //we have an audio pin so add a renderer and decoder
                            switch (_config.AudioConfig.Renderer)
                            {
                                case AudioRendererChoice.Reclock:
                                    try
                                    {
                                        _reclockAudioRenderer = new ReclockAudioRenderer();
                                        var aRenderer = _reclockAudioRenderer as DirectShowLib.IBaseFilter;
                                        if (aRenderer != null)
                                        {
                                            hr = m_graph.AddFilter(aRenderer, "Reclock Audio Renderer");
                                            DsError.ThrowExceptionForHR(hr);
                                            useDefaultRenderer = false;

                                            _logger.Debug("Added reclock audio renderer");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.ErrorException("Error adding reclock filter", ex);
                                    }
                                    break;
                                case AudioRendererChoice.WASAPI:
                                    try
                                    {
                                        _wasapiAR = URCOMLoader.Instance.GetObject(typeof(MPAudioFilter).GUID, true);
                                        var aRenderer = _wasapiAR as DirectShowLib.IBaseFilter;
                                        if (aRenderer != null)
                                        {
                                            hr = m_graph.AddFilter(aRenderer, "WASAPI Audio Renderer");
                                            DsError.ThrowExceptionForHR(hr);
                                            useDefaultRenderer = false;
                                            _logger.Debug("Added WASAPI audio renderer");

                                            IMPAudioRendererConfig arSett = aRenderer as IMPAudioRendererConfig;
                                            if (arSett != null)
                                            {
                                                arSett.SetInt(MPARSetting.WASAPI_MODE, (int)AUDCLNT_SHAREMODE.EXCLUSIVE);
                                                arSett.SetBool(MPARSetting.WASAPI_EVENT_DRIVEN, _config.AudioConfig.UseWasapiEventMode);
                                                _logger.Debug("Set WASAPI use event mode: {0}", _config.AudioConfig.UseWasapiEventMode);
                                                arSett.SetString(MPARSetting.SETTING_AUDIO_DEVICE, _config.AudioConfig.AudioDevice);
                                                _logger.Debug("Set WASAPI audio device: {0}", _config.AudioConfig.AudioDevice);
                                                SpeakerConfig sc = SpeakerConfig.Stereo; //use stereo for maxium compat
                                                Enum.TryParse<SpeakerConfig>(_config.AudioConfig.SpeakerLayout, out sc);
                                                arSett.SetInt(MPARSetting.SPEAKER_CONFIG, (int)sc);
                                                _logger.Debug("Set WASAPI speaker config: {0}", sc);
                                                //audSett.SetSpeakerMatchOutput(true);
                                                arSett.SetBool(MPARSetting.ALLOW_BITSTREAMING, true);
                                                arSett.SetInt(MPARSetting.USE_FILTERS, _config.AudioConfig.WasapiARFilters);
                                                _logger.Debug("Set WASAPI filter config: {0}", _config.AudioConfig.WasapiARFilters);
                                                AC3Encoding a3 = (AC3Encoding)_config.AudioConfig.Ac3EncodingMode;
                                                arSett.SetInt(MPARSetting.AC3_ENCODING, (int)a3);
                                                _logger.Debug("Set WASAPI AC3 encoding: {0}", a3);
                                                arSett.SetBool(MPARSetting.ENABLE_TIME_STRETCHING, _config.AudioConfig.EnableTimeStretching);
                                                _logger.Debug("Set WASAPI use time stretching: {0}", _config.AudioConfig.EnableTimeStretching);
                                                arSett.SetInt(MPARSetting.OUTPUT_BUFFER_LENGTH, _config.AudioConfig.OutputBufferSize);
                                                _logger.Debug("Set WASAPI buffer: {0}", _config.AudioConfig.OutputBufferSize);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.ErrorException("Error adding WASAPI audio filter", ex);
                                    }
                                    break;
                            }

                            if (useDefaultRenderer)
                            {
                                AddDefaultAudioRenderer();
                            }

                            try
                            {
                                _lavaudio = URCOMLoader.Instance.GetObject(typeof(LAVAudio).GUID, true); // new LAVAudio();
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

                                        //TODO: add this back when CODECs are surfaced through the config UI
                                        //foreach (string c in DirectShowPlayer.GetLAVAudioCodecs())
                                        //{
                                        //    LAVAudioCodec codec = (LAVAudioCodec)Enum.Parse(typeof(LAVAudioCodec), c);

                                        //    bool isEnabled = asett.GetFormatConfiguration(codec);
                                        //    if (
                                        //        _config.AudioConfig.EnabledCodecs.Contains(
                                        //            c))
                                        //    {
                                        //        if (!isEnabled)
                                        //        {
                                        //            _logger.Debug("Enable support for: {0}", c);
                                        //            hr = asett.SetFormatConfiguration(codec, true);
                                        //            DsError.ThrowExceptionForHR(hr);
                                        //        }
                                        //    }
                                        //    else if (isEnabled)
                                        //    {
                                        //        _logger.Debug("Disable support for: {0}", c);
                                        //        hr = asett.SetFormatConfiguration(codec, false);
                                        //        DsError.ThrowExceptionForHR(hr);
                                        //    }
                                        //}

                                        //enable/disable bitstreaming
                                        if ((_config.AudioConfig.AudioBitstreaming &
                                             BitstreamChoice.SPDIF) == BitstreamChoice.SPDIF)
                                        {
                                            _logger.Debug("Enable LAVAudio S/PDIF bitstreaming");

                                            hr = asett.SetBitstreamConfig(LAVBitstreamCodec.AC3, true);
                                            DsError.ThrowExceptionForHR(hr);

                                            hr = asett.SetBitstreamConfig(LAVBitstreamCodec.DTS, true);
                                            DsError.ThrowExceptionForHR(hr);
                                        }

                                        if ((_config.AudioConfig.AudioBitstreaming &
                                             BitstreamChoice.HDMI) == BitstreamChoice.HDMI)
                                        {
                                            _logger.Debug("Enable LAVAudio HDMI bitstreaming");

                                            hr = asett.SetBitstreamConfig(LAVBitstreamCodec.EAC3, true);
                                            DsError.ThrowExceptionForHR(hr);

                                            hr = asett.SetBitstreamConfig(LAVBitstreamCodec.TRUEHD, true);
                                            DsError.ThrowExceptionForHR(hr);

                                            hr = asett.SetBitstreamConfig(LAVBitstreamCodec.DTSHD, true);
                                            DsError.ThrowExceptionForHR(hr);

                                        }

                                        if (_config.AudioConfig.Delay > 0)
                                        {
                                            _logger.Debug("Set LAVAudio audio delay: {0}",
                                                _config.AudioConfig.Delay);

                                            hr = asett.SetAudioDelay(true,
                                                _config.AudioConfig.Delay);
                                            DsError.ThrowExceptionForHR(hr);
                                        }

                                        _logger.Debug("Set LAVAudio auto AV Sync: {0}",
                                            _config.AudioConfig.EnableAutoSync);
                                        hr =
                                            asett.SetAutoAVSync(
                                                _config.AudioConfig.EnableAutoSync);
                                        DsError.ThrowExceptionForHR(hr);

                                        _logger.Debug("Set LAVAudio Expand61: {0}",
                                            _config.AudioConfig.Expand61);
                                        hr = asett.SetExpand61(_config.AudioConfig.Expand61);
                                        DsError.ThrowExceptionForHR(hr);

                                        _logger.Debug("Set LAVAudio ExpandMono: {0}",
                                            _config.AudioConfig.ExpandMono);
                                        hr =
                                            asett.SetExpandMono(
                                                _config.AudioConfig.ExpandMono);
                                        DsError.ThrowExceptionForHR(hr);

                                        _logger.Debug("Set LAVAudio ConvertToStandardLayout: {0}",
                                            _config.AudioConfig.ConvertToStandardLayout);
                                        hr =
                                            asett.SetOutputStandardLayout(
                                                _config.AudioConfig.ConvertToStandardLayout);
                                        DsError.ThrowExceptionForHR(hr);

                                        _logger.Debug("Set LAVAudio audio EnableDRC: {0}",
                                            _config.AudioConfig.EnableDRC);
                                        hr = asett.SetDRC(_config.AudioConfig.EnableDRC,
                                            _config.AudioConfig.DRCLevel);
                                        DsError.ThrowExceptionForHR(hr);

                                        _logger.Debug("Set LAVAudio audio ShowTrayIcon: {0}",
                                            _config.AudioConfig.ShowTrayIcon);
                                        hr =
                                            asett.SetTrayIcon(
                                                _config.AudioConfig.ShowTrayIcon);
                                        DsError.ThrowExceptionForHR(hr);

                                        bool mixingEnabled = asett.GetMixingEnabled();
                                        if (mixingEnabled !=
                                            _config.AudioConfig.EnablePCMMixing)
                                        {
                                            _logger.Debug("Set LAVAudio EnablePCMMixing: {0}",
                                                _config.AudioConfig.EnablePCMMixing);
                                            hr = asett.SetMixingEnabled(!mixingEnabled);
                                            DsError.ThrowExceptionForHR(hr);
                                        }

                                        if (_config.AudioConfig.EnablePCMMixing)
                                        {
                                            _logger.Debug("Set LAVAudio MixingSetting: {0}",
                                                _config.AudioConfig.MixingSetting);
                                            LAVAudioMixingFlag amf =
                                                (LAVAudioMixingFlag)
                                                    _config.AudioConfig.MixingSetting;
                                            hr = asett.SetMixingFlags(amf);
                                            DsError.ThrowExceptionForHR(hr);

                                            _logger.Debug("Set LAVAudio MixingEncoding: {0}",
                                                _config.AudioConfig.MixingEncoding);
                                            LAVAudioMixingMode amm =
                                                (LAVAudioMixingMode)
                                                    Enum.Parse(typeof(LAVAudioMixingMode),
                                                        _config.AudioConfig.MixingEncoding);
                                            hr = asett.SetMixingMode(amm);
                                            DsError.ThrowExceptionForHR(hr);

                                            _logger.Debug("Set LAVAudio MixingLayout: {0}",
                                                _config.AudioConfig.MixingLayout);
                                            LAVAudioMixingLayout aml =
                                                (LAVAudioMixingLayout)
                                                    Enum.Parse(typeof(LAVAudioMixingLayout),
                                                        _config.AudioConfig.MixingLayout);
                                            hr = asett.SetMixingLayout(aml);
                                            DsError.ThrowExceptionForHR(hr);

                                            _logger.Debug(
                                                "Set LAVAudio LfeMixingLevel: {0} CenterMixingLevel: {1} SurroundMixingLevel: {2}",
                                                _config.AudioConfig.LfeMixingLevel,
                                                _config.AudioConfig.CenterMixingLevel,
                                                _config.AudioConfig.SurroundMixingLevel);
                                            int lfe, center, surround;
                                            //convert to the # that LAV Audio expects
                                            lfe =
                                                (int)
                                                    (_config.AudioConfig.LfeMixingLevel *
                                                     10000.01);
                                            center =
                                                (int)
                                                    (_config.AudioConfig.CenterMixingLevel *
                                                     10000.01);
                                            surround =
                                                (int)
                                                    (_config.AudioConfig
                                                        .SurroundMixingLevel * 10000.01);

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

                            _logger.Log(LogSeverity.Debug, "Connect Audio decoder to renderer");
                            decIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavaudio, PinDirection.Input, 0);
                            if (decIn != null)
                            {
                                _logger.Log(LogSeverity.Debug, "Got Audio decoder input pin");
                                hr = _filterGraph.ConnectDirect(pins[0], decIn, null);
                                if (hr < 0) //LAV cannot handle this audio type
                                {
                                    _logger.Warn("LAV Audio could not decode audio media type.");
                                }
                                else
                                {
                                    //DsError.ThrowExceptionForHR(hr);
                                    decOut = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_lavaudio,
                                        PinDirection.Output, 0);
                                }

                                rendIn = DsFindPin.ByDirection(AudioRenderer, PinDirection.Input, 0);

                                if (decOut != null && rendIn != null)
                                {
                                    hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                    if (hr == -2004287474 && _wasapiAR != null) //AUDCLNT_E_EXCLUSIVE_MODE_NOT_ALLOWED
                                    {
                                        IMPAudioRendererConfig arSett = _wasapiAR as IMPAudioRendererConfig;
                                        if (arSett != null)
                                        {
                                            arSett.SetInt(MPARSetting.WASAPI_MODE, (int)AUDCLNT_SHAREMODE.SHARED);
                                            _logger.Warn("WASAPI AR failed to connected in exclusive mode, check device properties");
                                            hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                        }
                                    }
                                    else if (hr == -2147220900) //audio format not supported
                                    {
                                        _logger.Warn("Couldn't connect to Audio Renderer, disable bit streaming and try again");
                                        ILAVAudioSettings asett = _lavaudio as ILAVAudioSettings;
                                        if (asett != null)
                                        {
                                            _logger.Log(LogSeverity.Debug, "Disconnect Audio decoder from splitter");
                                            hr = pins[0].Disconnect();
                                            DsError.ThrowExceptionForHR(hr);
                                            hr = decIn.Disconnect();
                                            DsError.ThrowExceptionForHR(hr);

                                            for (int i = 0; i < (int)LAVBitstreamCodec.NB; i++)
                                            {
                                                LAVBitstreamCodec codec = (LAVBitstreamCodec)i;
                                                asett.SetBitstreamConfig(codec, false);
                                                _logger.Log(LogSeverity.Warn, "Disable {0} bitstreaming.", codec);
                                                bool isEnabled = asett.GetBitstreamConfig(codec);
                                                _logger.Log(LogSeverity.Warn, "{0} bitstreaming: {1}", codec, isEnabled);
                                            }

                                            _logger.Log(LogSeverity.Debug, "Reconnect Audio decoder to splitter");
                                            hr = _filterGraph.ConnectDirect(pins[0], decIn, null);
                                            DsError.ThrowExceptionForHR(hr);

                                            hr = _filterGraph.ConnectDirect(decOut, rendIn, null);
                                            _logger.Log(LogSeverity.Warn, "Renderer reconnect result:{0}", hr);
                                        }
                                    }
                                    DsError.ThrowExceptionForHR(hr);

                                    needsRender = false;
                                    break;
                                }
                            }
                            #endregion
                        }
                        else if (mediaTypes[m] == SubtitleMediaType
                            /*DirectShowLib.MediaType.Subtitle*/)
                        {
                            #region subtitles
                            _logger.Log(LogSeverity.Debug, "Connect subtitle filter");

                            if (_xySubFilter != null)
                            {
                                _logger.Log(LogSeverity.Debug, "Using xySubFilter");
                                rendIn = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)_xySubFilter,
                                    PinDirection.Input, 0);
                            }
                            else if (_xyVsFilter != null)
                            {
                                _logger.Log(LogSeverity.Debug, "Using xyVsFilter");
                                rendIn = DsFindPin.ByName((DirectShowLib.IBaseFilter)_xyVsFilter, "Input");
                            }

                            if (rendIn != null)
                            {
                                _logger.Log(LogSeverity.Debug, "Connect subtitle pin to subtitle renderer");

                                try
                                {
                                    hr = _filterGraph.ConnectDirect(pins[0], rendIn, null);
                                    DsError.ThrowExceptionForHR(hr);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warn("Error rendering subtites {0} : {1}", hr, ex.Message);
                                }
                                needsRender = false;
                                break;
                            }
                            #endregion
                        }
                        else if (mediaTypes[m] == DvdSubpictureMediaType)
                        {
                            #region DVD Subpicture
                            _logger.Log(LogSeverity.Debug, "Connect DVD Subpicture");
                            if (_lavvideo != null)
                            {
                                rendIn = DsFindPin.ByName((DirectShowLib.IBaseFilter)_lavvideo, "Subtitle Input");
                                if (rendIn != null)
                                {
                                    _logger.Log(LogSeverity.Debug, "Connect dvd subtitle pin to subtitle renderer");

                                    try
                                    {
                                        hr = _filterGraph.ConnectDirect(pins[0], rendIn, null);
                                        DsError.ThrowExceptionForHR(hr);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warn("Error rendering subtites {0} : {1}", hr, ex.Message);
                                    }
                                    needsRender = false;
                                    break;
                                }
                            }
                            #endregion
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

            _logger.Debug("Completed RenderStreams with {0} pins.", pinsRendered);

            if (_item.IsVideo)
            {
                SetVideoWindow();
                if (_mPEvr != null)
                    SetEvrVppMode(_mPEvr);
            }
        }

        private void AddDefaultAudioRenderer()
        {
            _defaultAudioRenderer = new DefaultAudioRenderer();
            var aRenderer = _defaultAudioRenderer as DirectShowLib.IBaseFilter;
            if (aRenderer != null)
            {
                m_graph.AddFilter(aRenderer, "Default Audio Renderer");
                _logger.Debug("Added default audio renderer");
            }
        }


        public bool ShowDvdMenu(DvdMenuId menu)
        {
            IDvdCmd icmd;

            if ((this.PlayState != PlayState.Playing) || (_mDvdControl == null))
            {
                return false;
            }
            else
            {
                int hr = _mDvdControl.ShowMenu(menu, DvdCmdFlags.Block | DvdCmdFlags.Flush, out icmd);
                DsError.ThrowExceptionForHR(hr);
                return true;
            }
        }

        //public bool ToogleDvdSubtitles()
        //{
        //    int hr = 0;

        //    if (_mDvdControl != null)
        //    {
        //        AMLine21CCState cState = AMLine21CCState.On;
        //        IAMLine21Decoder dvdSubtitle = FilterGraphTools.FindLine21Filter(m_graph);
        //        try
        //        {
        //            if (dvdSubtitle != null)
        //            {
        //                hr = dvdSubtitle.GetServiceState(out cState);
        //                DsError.ThrowExceptionForHR(hr);

        //                _logger.Debug("ToogleDvdSubtitles: - CurrentState: {0}", cState);

        //                if (cState == AMLine21CCState.Off)
        //                    hr = dvdSubtitle.SetServiceState(AMLine21CCState.On);
        //                else
        //                    hr = dvdSubtitle.SetServiceState(AMLine21CCState.Off);
        //                DsError.ThrowExceptionForHR(hr);

        //            }
        //            return cState != AMLine21CCState.On;
        //        }
        //        finally
        //        {
        //            if (dvdSubtitle != null)
        //                Marshal.ReleaseComObject(dvdSubtitle);
        //        }
        //    }
        //    else
        //        return false;
        //}


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
                    if (amtDvr[0].majorType == DvdEncryptedMediaType)
                    {
                        if (amtDvr[0].subType == DirectShowLib.MediaSubType.Mpeg2Video)
                        {
                            mt.Add(DirectShowLib.MediaType.Video);
                        }
                        else if (amtDvr[0].subType == DirectShowLib.MediaSubType.DolbyAC3)
                        {
                            mt.Add(DirectShowLib.MediaType.Audio);
                        }
                        else
                            mt.Add(amtDvr[0].subType);
                    }
                    else
                        mt.Add(amtDvr[0].majorType);

                    DsUtils.FreeAMMediaType(amtDvr[0]);
                    amtDvr[0] = null;
                }
            }
            return mt;
        }

        private void InitializeEvr(DirectShowLib.IBaseFilter pEvr, int dwStreams, string videoRenderer)
        {
            int hr = 0;
            var pGetService = pEvr as IMFGetService;
            IMFVideoDisplayControl pDisplay;

            // Continue with the rest of the set-up.

            //try to load the custom presenter
            IMFVideoPresenter pPresenter = null;
            _logger.Debug("InitializeEvr videoRenderer: {0}", videoRenderer);

            if (string.Equals(videoRenderer, "evrcp", StringComparison.OrdinalIgnoreCase))
            {
                IMFVideoRenderer pRenderer = pEvr as IMFVideoRenderer;
                pPresenter = URCOMLoader.Instance.GetObject("EVR Presenter (babgvant)", false) as IMFVideoPresenter;

                try
                {
                    if (pPresenter != null)
                    {
                        _logger.Debug("Got EVR Presenter (babgvant)");
                        hr = pRenderer.InitializeRenderer(null, pPresenter);
                        if (hr > -1)
                        {
                            _customEvrPresenterLoaded = true;
                            IEVRCPConfig cp = pPresenter as IEVRCPConfig;
                            if (cp != null)
                            {
                                int nRange;
                                bool bCr;

                                hr = cp.SetInt(EVRCPSetting.NOMINAL_RANGE, _config.VideoConfig.NominalRange);
                                DsError.ThrowExceptionForHR(hr);
                                hr = cp.SetBool(EVRCPSetting.CORRECT_AR, true);
                                DsError.ThrowExceptionForHR(hr);

                                hr = cp.GetInt(EVRCPSetting.NOMINAL_RANGE, out nRange);
                                DsError.ThrowExceptionForHR(hr);
                                hr = cp.GetBool(EVRCPSetting.CORRECT_AR, out bCr);
                                DsError.ThrowExceptionForHR(hr);

                                _logger.Debug("IEVRCPConfig Nominal Range: {0} Correct AR: {1}", nRange, bCr);
                            }
                            else
                            {
                                _logger.Debug("Couldn't get IEVRCPConfig");
                            }
                        }
                    }
                    else
                    {
                        _logger.Error("Couldn't get EVR Presenter (babgvant)");
                    }
                }
                finally
                {
                    if (pPresenter != null)
                        Marshal.ReleaseComObject(pPresenter);
                }
            }

            // Set the video window.
            object o;
            hr = pGetService.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);
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

            IEVRFilterConfig evrConfig = pEvr as IEVRFilterConfig;
            int pdwMaxStreams;

            if (evrConfig != null)
            {
                hr = evrConfig.GetNumberOfStreams(out pdwMaxStreams);
                DsError.ThrowExceptionForHR(hr);
                _logger.Debug("NumberOfStreams: {0}", pdwMaxStreams);

                if (pdwMaxStreams < dwStreams)
                {
                    hr = evrConfig.SetNumberOfStreams(dwStreams);
                    DsError.ThrowExceptionForHR(hr);
                    _logger.Debug("Set NumberOfStreams: {0}", dwStreams);
                }
            }
            else
                _logger.Error("Couldn't get IEVRFilterConfig from EVR");

            // Return the IMFVideoDisplayControl pointer to the caller.
            _mPDisplay = pDisplay;
        }

        private void SetEvrVppMode(DirectShowLib.IBaseFilter pEvr)
        {
            int hr = 0;
            object objVideoProc = null;
            IMFGetService mfgs = pEvr as IMFGetService;
            if (mfgs != null)
            {
                try
                {
                    mfgs.GetService(MFServices.MR_VIDEO_MIXER_SERVICE,
                        typeof(IMFVideoProcessor).GUID,
                        out objVideoProc
                        );
                    IMFVideoProcessor evrProc =
                        objVideoProc as IMFVideoProcessor;
                    int dModes;
                    IntPtr ppModes = IntPtr.Zero;
                    Guid lpMode = Guid.Empty;
                    Guid bestMode = Guid.Empty;
                    hr = evrProc.GetVideoProcessorMode(out lpMode);
                    DsError.ThrowExceptionForHR(hr);
                    List<Guid> vpModes = new List<Guid>();

                    try
                    {
                        hr = evrProc.GetAvailableVideoProcessorModes(out dModes, out ppModes);
                        DsError.ThrowExceptionForHR(hr);
                        if (dModes > 0)
                        {
                            for (int i = 0; i < dModes; i++)
                            {
                                int offSet = Marshal.SizeOf(Guid.Empty) * i;
                                Guid vpMode =
                                    (Guid)Marshal.PtrToStructure(((IntPtr)((int)ppModes + offSet)), typeof(Guid));
                                vpModes.Add(vpMode);
                                _logger.Debug("VideoMode Found: {0}", vpMode);
                            }
                        }
                    }
                    finally
                    {
                        if (ppModes != IntPtr.Zero)
                            Marshal.FreeCoTaskMem(ppModes);
                    }

                    bestMode = vpModes[0];
                    _logger.Debug("Set ProcessorMode: {0} BestMode: {1}", lpMode, bestMode);
                    if (lpMode.CompareTo(bestMode) != 0)
                    {
                        hr = evrProc.SetVideoProcessorMode(ref bestMode);
                        DsError.ThrowExceptionForHR(hr);

                        hr = evrProc.GetVideoProcessorMode(out lpMode);
                        DsError.ThrowExceptionForHR(hr);
                        _logger.Debug("Current ProcessorMode: {0} BestMode: {1}", lpMode, bestMode);
                    }
                }
                finally
                {
                    if (objVideoProc != null)
                        Marshal.ReleaseComObject(objVideoProc);
                }
            }
        }

        private void SetVideoWindow()
        {
            int hr = 0;
            bool enableExclusiveMode = _config.VideoConfig.TryEnableFSE;

            _isInExclusiveMode = _madvr != null && enableExclusiveMode;

            if (!enableExclusiveMode)
            {
                _hostForm.SizeChanged += _hiddenWindow_SizeChanged;
                _hostForm.MouseClick += HiddenForm_MouseClick;
                //_hiddenWindow.KeyDown += HiddenForm_KeyDown;
                _removeHandlers = true;
            }

            if (_cursorHidden)
            {
                _videoWindow.HideCursor(OABool.True);
            }

            if (_madvr != null)
            {
                var ownerHandle = enableExclusiveMode ? _applicationWindowHandle : VideoWindowHandle;

                _videoWindow.put_Owner(ownerHandle);
                _videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings);
                _videoWindow.put_WindowStyleEx(WindowStyleEx.ToolWindow);

                _videoWindow.put_Visible(OABool.True);
                _videoWindow.put_AutoShow(OABool.True);
                _videoWindow.put_WindowState(WindowState.Show);

                hr = _videoWindow.SetWindowForeground(OABool.True);
                DsError.ThrowExceptionForHR(hr);

                if (enableExclusiveMode)
                {
                    //_videoWindow.put_FullScreenMode(OABool.True);
                }
                else
                {
                    hr = _videoWindow.put_MessageDrain(VideoWindowHandle); //XXX
                    //hr = _videoWindow.put_MessageDrain(_applicationWindowHandle);
                    DsError.ThrowExceptionForHR(hr);
                }

            }

            SetAspectRatio();

            if (_madvr != null)
            {
                SetExclusiveMode(enableExclusiveMode);
            }
        }

        private void _hiddenWindow_SizeChanged(object sender, EventArgs e)
        {
            SetAspectRatio(null);
        }

        private void HiddenForm_KeyDown(object sender, KeyEventArgs e)
        {
            _logger.Debug("HiddenForm_KeyDown: {0} {1}", e.KeyCode, (int)e.Modifiers);
            switch (e.KeyCode)
            {
                case Keys.Return:
                    if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
                    {
                        _mDvdControl.ActivateButton();
                        e.Handled = true;
                    }
                    else if ((_dvdMenuMode == DvdMenuMode.Still) && (_mDvdControl != null))
                    {
                        _mDvdControl.StillOff();
                        e.Handled = true;
                    }
                    break;
                case Keys.Left:
                    if (_mDvdControl != null)
                    {
                        if (_dvdMenuMode == DvdMenuMode.Buttons)
                            _mDvdControl.SelectRelativeButton(DvdRelativeButton.Left);
                        e.Handled = true;
                    }
                    break;
                case Keys.Right:
                    if (_mDvdControl != null)
                    {
                        if (_dvdMenuMode == DvdMenuMode.Buttons)
                            _mDvdControl.SelectRelativeButton(DvdRelativeButton.Right);
                        e.Handled = true;
                    }
                    break;
                case Keys.Up:
                    if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
                    {
                        _mDvdControl.SelectRelativeButton(DvdRelativeButton.Upper);
                        e.Handled = true;
                    }
                    break;
                case Keys.Down:
                    if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
                    {
                        _mDvdControl.SelectRelativeButton(DvdRelativeButton.Lower);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void HiddenForm_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _logger.Debug("HiddenForm_MouseClick: {0}", e);
            if ((_dvdMenuMode == DvdMenuMode.Buttons) && (_mDvdControl != null))
            {
                Point pt = new Point();
                pt.X = e.X;
                pt.Y = e.Y;
                _mDvdControl.SelectAtPosition(pt);
                _mDvdControl.ActivateButton();
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
                var size = System.Windows.Forms.Screen.FromControl(_hostForm).Bounds;

                screenWidth = size.Width;
                screenHeight = size.Height;
            }
            else
            {
                var hiddenWindowContentSize = _hostForm.Bounds;

                screenWidth = hiddenWindowContentSize.Width;
                screenHeight = hiddenWindowContentSize.Height;
            }

            // Set the display position to the entire window.
            if (_mPDisplay != null)
            {
                MFRect dRect = new MFRect(0, 0, screenWidth, screenHeight);
                MFSize vSize = new MFSize(), vAR = new MFSize();
                double m_ZoomX = 1, m_ZoomY = 1, m_PosX = 0.5, m_PosY = 0.5;
                MFVideoNormalizedRect sRect = new MFVideoNormalizedRect();
                sRect.top = 0;
                sRect.left = 0;
                sRect.right = 1;
                sRect.bottom = 1;

                int hr = _mPDisplay.GetNativeVideoSize(vSize, vAR);
                if (hr > -1)
                {
                    double dVideoAR = (double)vSize.Width / vSize.Height;

                    double dWRWidth = screenWidth;
                    double dWRHeight = screenHeight;

                    double dVRWidth = dWRHeight * dVideoAR;
                    double dVRHeight;

                    _logger.Debug("Scale: {0} Video Width: {1} Video Height: {2} X-AR: {3} Y-AR: {4}", _iVideoScaling, vSize.Width, vSize.Height, vAR.cx, vAR.cy);

                    switch (_iVideoScaling)
                    {
                        case VideoScalingScheme.HALF:
                            dVRWidth = vSize.Width * 0.5;
                            dVRHeight = vSize.Height * 0.5;
                            break;
                        case VideoScalingScheme.NORMAL:
                            dVRWidth = vSize.Width;
                            dVRHeight = vSize.Height;
                            break;
                        case VideoScalingScheme.DOUBLE:
                            dVRWidth = vSize.Width * 2.0;
                            dVRHeight = vSize.Height * 2.0;
                            break;
                        case VideoScalingScheme.STRETCH:
                            dVRWidth = dWRWidth;
                            dVRHeight = dWRHeight;
                            break;
                        default:
                        //ASSERT(FALSE);
                        // Fallback to "Touch Window From Inside" if settings were corrupted.
                        case VideoScalingScheme.FROMINSIDE:
                        case VideoScalingScheme.FROMOUTSIDE:
                            if ((screenWidth < dVRWidth) != (_iVideoScaling == VideoScalingScheme.FROMOUTSIDE))
                            {
                                dVRWidth = dWRWidth;
                                dVRHeight = dVRWidth / dVideoAR;
                            }
                            else
                            {
                                dVRHeight = dWRHeight;
                            }
                            break;
                        case VideoScalingScheme.ZOOM1:
                        case VideoScalingScheme.ZOOM2:
                            {
                                double minw = dWRWidth < dVRWidth ? dWRWidth : dVRWidth;
                                double maxw = dWRWidth > dVRWidth ? dWRWidth : dVRWidth;

                                double scale = _iVideoScaling == VideoScalingScheme.ZOOM1 ? 1.0 / 3.0 : 2.0 / 3.0;
                                dVRWidth = minw + (maxw - minw) * scale;
                                dVRHeight = dVRWidth / dVideoAR;
                                break;
                            }
                    }

                    // Scale video frame
                    double dScaledVRWidth = m_ZoomX * dVRWidth;
                    double dScaledVRHeight = m_ZoomY * dVRHeight;

                    // Position video frame
                    // left and top parts are allowed to be negative
                    dRect.left = (int)Math.Round(m_PosX * (dWRWidth * 3.0 - dScaledVRWidth) - dWRWidth);
                    dRect.top = (int)Math.Round(m_PosY * (dWRHeight * 3.0 - dScaledVRHeight) - dWRHeight);
                    // right and bottom parts are always at picture center or beyond, so never negative
                    dRect.right = (int)Math.Round(dRect.left + dScaledVRWidth);
                    dRect.bottom = (int)Math.Round(dRect.top + dScaledVRHeight);

                    //apply overscan
                    //dRect.top = dRect.top - (ps.OverscanHeight / 2);
                    //dRect.left = dRect.left - (ps.OverscanWidth / 2);
                    //dRect.right = dRect.right + (ps.OverscanWidth / 2);//this.Width;
                    //dRect.bottom = dRect.bottom + (ps.OverscanHeight / 2);//this.Height;
                }
                _logger.Debug("Source Rect T: {0} L: {1} B: {2} R: {3} Dest Rect T: {4} L: {5} B: {6} R: {7}", sRect.top, sRect.left, sRect.bottom, sRect.right, dRect.top, dRect.left, dRect.bottom, dRect.right);

                _mPDisplay.SetVideoPosition(sRect, dRect);
            }
            else if (_madvr != null)
            {
                //configure madVR to figure out AR & window size
                IMadVRCommand _madCmd = _madvr as IMadVRCommand;
                if (_madCmd != null)
                {
                    int hr = 0;
                    string zoomMode = "autoDetect";
                    switch (_iVideoScaling)
                    {
                        case VideoScalingScheme.FROMINSIDE:
                            zoomMode = "touchInside";
                            break;
                        case VideoScalingScheme.FROMOUTSIDE:
                            zoomMode = "touchOutside";
                            break;
                        case VideoScalingScheme.STRETCH:
                            zoomMode = "stretch";
                            break;
                    }

                    hr = _madCmd.SendCommandString("setZoomMode", zoomMode);
                }
            }
            //this shouldn't be necessary anymore. EVR & EVR+ are configured via _mPDisplay, and madVR via IMadVRCommand
            //else
            //{
            //    // Get Aspect Ratio
            //    int aspectX;
            //    int aspectY;

            //    if (ratio.HasValue)
            //    {
            //        aspectX = ratio.Value.Width;
            //        aspectY = ratio.Value.Height;
            //    }
            //    else
            //    {
            //        var basicVideo2 = (IBasicVideo2)m_graph;
            //        basicVideo2.GetPreferredAspectRatio(out aspectX, out aspectY);

            //        var sourceHeight = 0;
            //        var sourceWidth = 0;

            //        _basicVideo.GetVideoSize(out sourceWidth, out sourceHeight);

            //        if (aspectX == 0 || aspectY == 0 || sourceWidth > 0 || sourceHeight > 0)
            //        {
            //            aspectX = sourceWidth;
            //            aspectY = sourceHeight;
            //        }
            //    }

            //    // Adjust Video Size
            //    var iAdjustedHeight = 0;

            //    if (aspectX > 0 && aspectY > 0)
            //    {
            //        double adjustedHeight = aspectY * screenWidth;
            //        adjustedHeight /= aspectX;

            //        iAdjustedHeight = Convert.ToInt32(Math.Round(adjustedHeight));
            //    }

            //    if (screenHeight > iAdjustedHeight && iAdjustedHeight > 0)
            //    {
            //        double totalMargin = (screenHeight - iAdjustedHeight);
            //        var topMargin = Convert.ToInt32(Math.Round(totalMargin / 2));

            //        _basicVideo.SetDestinationPosition(0, topMargin, screenWidth, iAdjustedHeight);
            //    }
            //    else if (iAdjustedHeight > 0)
            //    {
            //        double adjustedWidth = aspectX * screenHeight;
            //        adjustedWidth /= aspectY;

            //        var iAdjustedWidth = Convert.ToInt32(Math.Round(adjustedWidth));

            //        double totalMargin = (screenWidth - iAdjustedWidth);
            //        var leftMargin = Convert.ToInt32(Math.Round(totalMargin / 2));

            //        _basicVideo.SetDestinationPosition(leftMargin, 0, iAdjustedWidth, screenHeight);
            //    }
            //}
            _logger.Debug("Screen Width: {0} Screen Height: {1}", screenWidth, screenHeight);

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

            if (_startResolution != null)
            {
                _logger.Info("Change resolution back to {0}", _startResolution);
                Display.ChangeResolution(_hostForm, _startResolution, false);
            }

            DsError.ThrowExceptionForHR(hr);

            OnStopped(reason, pos, newTrackIndex);
        }

        private void ModifyRate(double dRateAdjust)
        {
            int hr = 0;
            double dRate;
            double dNewRate = 0;

            if (_mDvdControl != null)
            {
                dNewRate += dRateAdjust;

                if (dNewRate == 0) dNewRate += dRateAdjust;
            }
            else if ((this._mediaSeeking != null) && (dRateAdjust != 0.0))
            {
                hr = this._mediaSeeking.GetRate(out dRate);
                if (hr == 0)
                {
                    // Add current rate to adjustment value
                    dNewRate = dRate + dRateAdjust;
                }
            }

            SetRate(dNewRate);
        }

        private void SetupGraphForRateChange(double rate, IBaseFilter audioRenderer)
        {
            int hr = 0;

            _logger.Debug("SetupGraphForRateChange: {0}", rate);

            if (_wasapiAR != null)
            {
                IBasicAudio ba = _filterGraph as IBasicAudio;
                if (ba != null)
                {
                    int orgVol = 0;
                    hr = ba.get_Volume(out orgVol);
                    DsError.ThrowExceptionForHR(hr);

                    _logger.Debug("SetupGraphForRateChange: Current Volume: {0}", orgVol);

                    if (Math.Abs(rate) > 1.5)
                    {
                        hr = ba.put_Volume(-10000); //turn off the volume so we can ffwd
                        DsError.ThrowExceptionForHR(hr);
                        _logger.Debug("SetupGraphForRateChange: mute");
                    }
                    else if (Math.Abs(rate) <= 1.5)
                    {
                        hr = ba.put_Volume(0); //set the volume back to full
                        DsError.ThrowExceptionForHR(hr);
                        _logger.Debug("SetupGraphForRateChange: enable volume");
                    }
                }
            }
            else
            {
                IPin arIn = null;

                try
                {
                    if (audioRenderer != null)
                    {
                        if (Math.Abs(rate) > 4 && m_adecOut == null)
                        {
                            _logger.Debug("SetupGraphForRateChange: remove audio renderer");

                            //grab the audio decoder's output pin
                            arIn = DsFindPin.ByDirection(audioRenderer, PinDirection.Input, 0);
                            hr = arIn.ConnectedTo(out m_adecOut);
                            DsError.ThrowExceptionForHR(hr);

                            //stop the graph
                            hr = _mediaControl.Stop();
                            DsError.ThrowExceptionForHR(hr);

                            //remove it
                            hr = _filterGraph.RemoveFilter(audioRenderer);
                            DsError.ThrowExceptionForHR(hr);

                            //start the graph again
                            hr = _mediaControl.Run();
                            DsError.ThrowExceptionForHR(hr);
                        }
                        else if (Math.Abs(rate) <= 4 && m_adecOut != null)
                        {
                            _logger.Debug("SetupGraphForRateChange: add the audio renderer back");

                            //stop the graph
                            hr = _mediaControl.Stop();
                            DsError.ThrowExceptionForHR(hr);

                            //add the audio renderer back into the graph
                            hr = _filterGraph.AddFilter(audioRenderer, "Audio Renderer");
                            DsError.ThrowExceptionForHR(hr);

                            //connect it to the decoder pin
                            arIn = DsFindPin.ByDirection(audioRenderer, PinDirection.Input, 0);
                            hr = _filterGraph.ConnectDirect(m_adecOut, arIn, null);
                            DsError.ThrowExceptionForHR(hr);

                            Marshal.ReleaseComObject(m_adecOut);
                            m_adecOut = null;

                            //start the graph again
                            hr = _mediaControl.Run();
                            DsError.ThrowExceptionForHR(hr);
                        }
                    }

                    if (Math.Abs(rate) <= 4)
                    {
                        IBasicAudio ba = _filterGraph as IBasicAudio;
                        if (ba != null)
                        {
                            int orgVol = 0;
                            hr = ba.get_Volume(out orgVol);
                            DsError.ThrowExceptionForHR(hr);
                            _logger.Debug("SetupGraphForRateChange: Current Volume: {0}", orgVol);

                            if (Math.Abs(rate) > 1.5)
                            {
                                hr = ba.put_Volume(-10000); //turn off the volume so we can ffwd
                                DsError.ThrowExceptionForHR(hr);
                                _logger.Debug("SetupGraphForRateChange: mute");
                            }
                            else if (Math.Abs(rate) <= 1.5)
                            {
                                hr = ba.put_Volume(0); //set the volume back to full
                                DsError.ThrowExceptionForHR(hr);
                                _logger.Debug("SetupGraphForRateChange: enable volume");
                            }
                        }
                    }
                }
                finally
                {
                    if (arIn != null)
                        Marshal.ReleaseComObject(arIn);
                }
            }
        }

        public void SetRate(double rate)
        {
            int hr = 0;
            //_currentPlaybackRate = rate;
            SetupGraphForRateChange(rate, AudioRenderer);

            if (_mDvdControl != null)
            {
                if (rate < 0)
                    hr = _mDvdControl.PlayBackwards(Math.Abs(rate), DvdCmdFlags.SendEvents, out _mDvdCmdOption);
                else
                    hr = _mDvdControl.PlayForwards(rate, DvdCmdFlags.SendEvents, out _mDvdCmdOption);
                //DsError.ThrowExceptionForHR(hr);
                if (hr >= 0)
                {
                    _currentPlaybackRate = rate;
                    if (_mDvdCmdOption != null)
                    {
                        _pendingDvdCmd = true;
                    }
                }

                //hr = _mDvdControl.PlayForwards(rate, DvdCmdFlags.SendEvents, out _mDvdCmdOption);
                //DsError.ThrowExceptionForHR(hr);

                //if (_mDvdCmdOption != null)
                //{
                //    _pendingDvdCmd = true;
                //}
            }
            else if (_mediaSeeking != null)
            {
                hr = _mediaSeeking.SetRate(rate);
                //DsError.ThrowExceptionForHR(hr);
                if (hr >= 0)
                {
                    _currentPlaybackRate = rate;
                }
                else
                {
                    _logger.Debug("SetRate: changing rate failed");
                    hr = _mediaSeeking.GetRate(out _currentPlaybackRate);
                    if (hr >= 0)
                    {
                        SetupGraphForRateChange(_currentPlaybackRate, AudioRenderer);
                    }
                }
            }
        }

        public void Seek(long ticks)
        {
            if (_mediaSeeking != null)
            {
                long duration;

                var hr = _mediaSeeking.GetDuration(out duration);

                if (ticks < 0)
                    ticks = 0;
                else if (ticks > duration)
                    ticks = duration;

                // Seek to the position
                hr = _mediaSeeking.SetPositions(new DsLong(ticks), AMSeekingSeekingFlags.AbsolutePositioning,
                    new DsLong(duration), AMSeekingSeekingFlags.AbsolutePositioning);
            }
        }

        public void SeekRelative(long ticks)
        {
            if (_mediaSeeking != null)
            {
                long position;

                var hr = _mediaSeeking.GetCurrentPosition(out position);
                position += ticks;
                Seek(position);
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
                            OnDvdCmdComplete(evParam1, evParam2);
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

        private void OnDvdCmdComplete(IntPtr evParam1, IntPtr evParam2)
        {
            IDvdInfo2 dvdInfo = _sourceFilter as IDvdInfo2;

            if ((_pendingDvdCmd == false) || (dvdInfo == null))
            {
                return;
            }

            IDvdCmd cmd = null;
            int hr = dvdInfo.GetCmdFromEvent(evParam1, out cmd);
            DsError.ThrowExceptionForHR(hr);

            if (cmd == null)
            {
                // DVD OnCmdComplete GetCmdFromEvent failed
                return;
            }

            if (cmd != _mDvdCmdOption)
            {
                // DVD OnCmdComplete UNKNOWN CMD
                Marshal.ReleaseComObject(cmd);
                cmd = null;
                return;
            }

            Marshal.ReleaseComObject(cmd);
            cmd = null;
            Marshal.ReleaseComObject(_mDvdCmdOption);
            _mDvdCmdOption = null;
            _pendingDvdCmd = false;
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
                    else if (evCode == EventCode.ErrorAbort ||
                        evCode == EventCode.ErrorStPlaying ||
                        evCode == EventCode.StErrStopped ||
                        evCode == EventCode.ErrorStPlaying)
                    {
                        Stop(TrackCompletionReason.Failure, null);
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
            _hostForm.GraphNotify -= _hostForm_GraphNotify;
            _hostForm.DvdEvent -= _hostForm_DvdEvent;

            if (_removeHandlers)
            {
                _hostForm.SizeChanged -= _hiddenWindow_SizeChanged;
                _hostForm.MouseClick -= HiddenForm_MouseClick;
                //_hiddenWindow.KeyDown -= HiddenForm_KeyDown;
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

            if (m_adecOut != null)
            {
                CleanUpInterface(m_adecOut);
                m_adecOut = null;
            }

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

            if (_wasapiAR != null)
            {
                m_graph.RemoveFilter(_wasapiAR as DirectShowLib.IBaseFilter);

                CleanUpInterface(_wasapiAR);
                _wasapiAR = null;
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

        private List<SelectableMediaStream> _streams = new List<SelectableMediaStream>();

        public IReadOnlyList<SelectableMediaStream> GetSelectableStreams()
        {
            return _streams ?? (_streams = GetStreams());
        }

        private Guid _audioSelector = Guid.Empty;
        private Guid _vobsubSelector = Guid.Empty;
        private Guid _grp2Selector = Guid.Empty;

        private List<SelectableMediaStream> GetInternalStreams()
        {
            _logger.Debug("GetInternalStreams");

            var streams = new List<SelectableMediaStream>();

            IEnumFilters enumFilters = null;
            try
            {
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
            }
            finally
            {
                if (enumFilters != null)
                    Marshal.ReleaseComObject(enumFilters);
            }

            _logger.Debug("Return InternalStreamCount: {0}", streams.Count);
            return streams;
        }

        private List<SelectableMediaStream> GetExternalSubtitleStreams(List<SelectableMediaStream> internalStreams)
        {
            var externalSubtitleStreams = new List<SelectableMediaStream>();
            try
            {
                var hasActiveInternalSubtitleStream = (internalStreams != null
                    ? internalStreams.FirstOrDefault(i => i.Type == MediaStreamType.Subtitle && i.IsActive)
                    : null) != null;

                if (_item != null && _item.MediaStreams != null)
                {
                    foreach (var s in _item.MediaStreams.Where(i => i.Type == MediaStreamType.Subtitle && i.IsExternal))
                    {
                        externalSubtitleStreams.Add(new SelectableMediaStream
                        {
                            Index = s.Index,
                            Name = s.Language ?? "Unknown",
                            Path = string.IsNullOrWhiteSpace(s.DeliveryUrl) ? s.Path : s.DeliveryUrl,
                            Type = MediaStreamType.Subtitle,
                            Identifier = "external",
                            IsActive = !String.IsNullOrEmpty(s.Language)
                        });
                    }
                }

                if (externalSubtitleStreams.Any() && (internalStreams == null || internalStreams.All(i => i.Type != MediaStreamType.Subtitle)))
                {
                    // have to add a nosubtitle stream, so the user can turn subS  off
                    externalSubtitleStreams.Add(new SelectableMediaStream
                    {
                        Name = "No Subtitles",
                        Type = MediaStreamType.Subtitle,
                        Identifier = "external",
                        IsActive = !(hasActiveInternalSubtitleStream || externalSubtitleStreams.Any(i => i.IsActive))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("GetExternalSubtitleStreams", ex);
            }

            return externalSubtitleStreams;
        }

        private List<SelectableMediaStream> OrderStreams(List<SelectableMediaStream> streamsIn)
        {
            var streamsOut = streamsIn.Where(i => i.Type != MediaStreamType.Subtitle).ToList();
            var noSubtitleStream = streamsIn.FirstOrDefault(i => i.Type == MediaStreamType.Subtitle && i.Name.ToLower().Contains("no subtitles"));
            if (noSubtitleStream != null)
            {
                streamsOut.Add(noSubtitleStream);

            }
            var subtitleStreams = streamsIn.Where(i => i.Type == MediaStreamType.Subtitle && !i.Name.ToLower().Contains("no subtitles"));
            foreach (var subStream in subtitleStreams)
            {
                streamsOut.Add(subStream);
            }


            return streamsOut;
        }

        private List<SelectableMediaStream> GetStreams()
        {
            _logger.Debug("GetStreams()");

            var streams = GetInternalStreams();
            var externalSubtitleStreams = GetExternalSubtitleStreams(streams);
            if (externalSubtitleStreams != null && externalSubtitleStreams.Any())
            {
                streams = streams.Concat(externalSubtitleStreams).ToList();
            }

            // modify the subtitle stream so the "No Subtitles" subtitle selection  is at the front of subtitles list
            return OrderStreams(streams);
        }

        public void LoadExternalSubtitle(string subtitleFile)
        {
            _logger.Info("LoadExternalSubtitle {0}", subtitleFile);
            IBaseFilter subtitleFilter = _xySubFilter != null ? _xySubFilter as IBaseFilter : _xyVsFilter as IBaseFilter;

            if (subtitleFilter != null)
            {
                var extSubSource = subtitleFilter as IDirectVobSub;
                if (extSubSource != null && !string.IsNullOrWhiteSpace(subtitleFile))
                {
                    string subName = Path.GetFileNameWithoutExtension(subtitleFile);

                    var hr = extSubSource.put_FileName(subtitleFile);
                    DsError.ThrowExceptionForHR(hr);

                    int iCount;

                    hr = extSubSource.get_LanguageCount(out iCount);
                    DsError.ThrowExceptionForHR(hr);
                    _logger.Info("LoadExternalSubtitle Count: {0}", iCount);

                    for (int i = 0; i < iCount; i++)
                    {
                        string ppName;

                        hr = extSubSource.get_LanguageName(i, out ppName);
                        DsError.ThrowExceptionForHR(hr);

                        _logger.Info("LoadExternalSubtitle SubName {0}", ppName);

                        if (subName == ppName)
                        {
                            _logger.Info("LoadExternalSubtitle Select Stream {0}", i);

                            hr = extSubSource.put_SelectedLanguage(i);
                            DsError.ThrowExceptionForHR(hr);

                            int iSelected = 0;
                            hr = extSubSource.get_SelectedLanguage(ref iSelected);
                            DsError.ThrowExceptionForHR(hr);

                            _logger.Info("LoadExternalSubtitle Select Result: {0}", iSelected);

                            break;
                        }
                    }
                }
            }
        }

        private void ClearExternalSubtitles()
        {
            _logger.Info("ClearExternalSubtitles");
            LoadExternalSubtitle("");
        }

        public void ToggleHideSubtitles(bool hide)
        {
            _logger.Info("ToggleHideSubtitles {0}", hide);
            IBaseFilter subtitleFilter = _xySubFilter != null ? _xySubFilter as IBaseFilter : _xyVsFilter as IBaseFilter;

            if (subtitleFilter != null)
            {
                var extSubSource = subtitleFilter as IDirectVobSub;
                if (extSubSource != null)
                {
                    var hr = extSubSource.put_HideSubtitles(hide);
                    DsError.ThrowExceptionForHR(hr);
                }
            }
        }

        public void ToggleVideoScaling()
        {
            VideoScalingScheme cScheme = VideoScaling;
            int iScheme = (int)cScheme;
            iScheme++;

            var vsVals = Enum.GetValues(typeof(VideoScalingScheme));
            if (iScheme >= vsVals.Length)
                iScheme = 0;

            VideoScaling = (VideoScalingScheme)iScheme;
        }

        public void SetAudioTrack(SelectableMediaStream stream)
        {
            SetInternalStream(stream);
        }

        public void SetSubtitleStreamIndex(int subtitleStreamIndex)
        {
            // subtitleStreamIndex is based on server metadata
            _logger.Info("SetSubtitleStreamIndex: {0}", subtitleStreamIndex);
            var allSubtitleStreams = _item.MediaStreams.Where(i => i.Type == MediaStreamType.Subtitle).ToList();
            var trackOffset = allSubtitleStreams.FindIndex(i => i.Index == subtitleStreamIndex);

            _logger.Info("SetSubtitleStreamIndex {0}. Track offset: {1}", subtitleStreamIndex, trackOffset);

            if (subtitleStreamIndex == -1)
            {
                var stream = _streams
                    .FirstOrDefault(i => i.Type == MediaStreamType.Subtitle && i.Name.ToLower().Contains("no subtitles"));

                if (stream != null)
                {
                    SetSubtitleStream(stream);
                }
            }
            else
            {
                var rawSubtitleStreams = _streams.Where(i => i.Type == MediaStreamType.Subtitle && !i.Name.ToLower().Contains("no subtitles")).ToList();
                var stream = trackOffset == -1 ? null : rawSubtitleStreams.ElementAtOrDefault(trackOffset);

                if (stream != null)
                {
                    SetSubtitleStream(stream);
                }
            }
        }

        public void NextSubtitleStream()
        {
            _logger.Info("NextSubtitleStream");

            var subtitleStreams = _streams.Where(i => i.Type == MediaStreamType.Subtitle).ToList();
            var nextSubtitleStream = subtitleStreams.SkipWhile(i => !i.IsActive).Skip(1).FirstOrDefault() ??
                                     subtitleStreams.FirstOrDefault();

            if (nextSubtitleStream != null)
            {
                SetSubtitleStream(nextSubtitleStream);
            }

        }

        public async void SetSubtitleStream(SelectableMediaStream stream)
        {
            _logger.Info("SetSubtitleStream {0} {1} {2} {3}", stream.Index, stream.Type, stream.Name, stream.Identifier);
            if (stream.Identifier == "external" || stream.Name.ToLower().Contains("no subtitles"))
            {
                SetExternalSubtitleStream(stream);
            }

            if (stream.Identifier != "external")
            {
                SetInternalStream(stream);
            }

            ToggleHideSubtitles(stream.Name.ToLower().Contains("no subtitles"));
        }

        private async void LoadExternalSubtitleFromStream(SelectableMediaStream stream)
        {
            _logger.Info("LoadExternalSubtitleFromStream: {0}", stream.Path);
            var path = stream.Path;

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // as xvfilter throw's an error for a stream, we copy the stream to
                    // a local temp file and change it ext to 'srt'
                    var tempFile = await _httpClient.GetTempFile(new HttpRequestOptions
                    {
                        Url = path,
                        Progress = new Progress<double>()
                    });

                    _logger.Info("Downloaded subtitle to {0}", tempFile);

                    path = path.Split('?')[0];
                    path = path.Substring(path.LastIndexOf('/') + 1);

                    var pathWithExtension = Path.ChangeExtension(tempFile, Path.GetExtension(path));
                    _logger.Info("Saving subtitle to {0}", pathWithExtension);
                    File.Move(tempFile, pathWithExtension);
                    path = pathWithExtension;
                    _logger.Info("Saved subtitle to {0}", pathWithExtension);
                }
                catch
                {
                    return;
                }
            }

            _logger.Info("loadExternalSubtitleFromStream {0} {1} {2}", stream.Index, stream.Type, path);
            LoadExternalSubtitle(path);
        }

        private async void SetExternalSubtitleStream(SelectableMediaStream stream)
        {
            _logger.Info("SetExternalSubtitleStream: {0}", stream);

            if (stream.Name.ToLower().Contains("no subtitles"))
            {
                ToggleHideSubtitles(true);
                UpdateStreamsAsInactive(stream.Type); // display all streams as inactive
                stream.IsActive = true; // set no subtitles stream active
            }
            else
            {
                //make sure the splitter isn't providing a subtitle
                int subIndex = (int)CurrentSubtitleStreamIndex;
                if (subIndex > 0)
                {
                    _logger.Debug("Clear internal subtitle");
                    var iss = _sourceFilter as IAMStreamSelect;
                    if (iss != null)
                    {
                        int hr = iss.Enable(subIndex, AMStreamSelectEnableFlags.DisableAll);
                    }
                }
                // if not, we need to copy the stream to the local system and play form there (xyfilter issue)
                LoadExternalSubtitleFromStream(stream);

                UpdateStreamActiveSetting(stream.Name, stream.Type); // display this  streams as active
            }
        }

        private void UpdateStreamActiveSetting(string streamName, MediaStreamType streamType)
        {
            _logger.Info(String.Format("UpdateStreamActiveSetting name = '{0}'", streamName));

            foreach (var i in GetSelectableStreams().Where(s => s.Type == streamType))
            {
                i.IsActive = i.Name == streamName;
            }
        }

        private void UpdateStreamsAsInactive(MediaStreamType streamType)
        {
            foreach (var i in GetSelectableStreams().Where(s => s.Type == streamType))
            {
                i.IsActive = false;
            }
        }


        private SelectableMediaStream GetActiveExternalSubtitleStream()
        {
            return _streams.FirstOrDefault(i => i.Type == MediaStreamType.Subtitle && i.IsActive == true && i.Identifier == "external");
        }

        private void SetInternalStream(SelectableMediaStream stream)
        {
            _logger.Info("SetInternalStream {0} {1} {2}", stream.Index, stream.Type, stream.Name);

            // if we are already playing an external subtitle, we have to clear it first or
            // we wont be able to swicth subtitles
            var activeExternalSub = GetActiveExternalSubtitleStream();
            if (activeExternalSub != null)
            {
                ClearExternalSubtitles();
            }

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

                if (iss != null)
                    iss.Enable(stream.Index, AMStreamSelectEnableFlags.Enable);

                Marshal.ReleaseComObject(filters[0]);
            }

            Marshal.ReleaseComObject(enumFilters);

            UpdateStreamActiveSetting(stream.Name, stream.Type);
        }

        public void SetAudioStreamIndex(int audioStreamIndex)
        {
            // audioStreamIndex is based on server metadata

            _logger.Info("SetAudioStreamIndex {0}", audioStreamIndex);
            var allAudioStreams = _item.MediaStreams.Where(i => i.Type == MediaStreamType.Audio).ToList();
            var audioOffset = allAudioStreams.FindIndex(i => i.Index == audioStreamIndex);

            _logger.Info("SetAudioStreamIndex {0}. Track offset: {1}", audioStreamIndex, audioOffset);

            var internalAudioStreams = _streams.Where(i => i.Type == MediaStreamType.Audio).ToList();
            var stream = audioOffset == -1 ? null : internalAudioStreams.ElementAtOrDefault(audioOffset);

            if (stream != null)
            {
                SetInternalStream(stream);
            }
            else
            {
                _logger.Info("Invalid audioStreamIndex {0}", audioStreamIndex);
                //throw new ApplicationException(String.Format("Invalid audioStreamIndex {0}", audioStreamIndex));
            }
        }

        public void Dispose()
        {
        }
    }
}