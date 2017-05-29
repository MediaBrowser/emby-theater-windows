using System.Windows;
using MediaBrowser.Common.Events;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Emby.Theater.DirectShow.Configuration;
using MediaBrowser.Common.Configuration;

namespace Emby.Theater.DirectShow
{
    public class InternalDirectShowPlayer : IDisposable
    {
        private DirectShowPlayer _mediaPlayer;

        private readonly ILogger _logger;
        //private readonly IPresentationManager _presentation;
        //private readonly ISessionManager _sessionManager;
        private readonly IIsoManager _isoManager;
        //private readonly IUserInputManager _inputManager;
        private readonly IHttpClient _httpClient;
        private readonly IZipClient _zipClient;
        private URCOMLoader _privateCom = null;
        private readonly IConfigurationManager _config;
        IApplicationPaths _appPaths = null;
        //public URCOMLoader PrivateCom
        //{
        //    get
        //    {
        //        return _privateCom;
        //    }
        //}

        private readonly Dispatcher _dispatcher;

        public InternalDirectShowPlayer(
            ILogManager logManager
            //, IPresentationManager presentation
            //, ISessionManager sessionManager
            , IApplicationPaths appPaths
            , IIsoManager isoManager
            //, IUserInputManager inputManager
            , IZipClient zipClient
            , IHttpClient httpClient, IConfigurationManager configurationManager, Dispatcher dispatcher)
        {
            _logger = logManager.GetLogger("InternalDirectShowPlayer");
            //_presentation = presentation;
            //_sessionManager = sessionManager;
            _httpClient = httpClient;
            _config = configurationManager;

            _dispatcher = dispatcher;
            _isoManager = isoManager;
            //_inputManager = inputManager;
            _zipClient = zipClient;
            _appPaths = appPaths;

            var config = GetConfiguration();

            config.VideoConfig.SetDefaults();
            config.AudioConfig.SetDefaults();
            config.SubtitleConfig.SetDefaults();
            config.COMConfig.SetDefaults();

            //use a static object so we keep the libraries in the same place. Doesn't usually matter, but the EVR Presenter does some COM hooking that has problems if we change the lib address.
            //if (_privateCom == null)
            //    _privateCom = new URCOMLoader(_config, _zipClient);
            URCOMLoader.Instance.Initialize(appPaths.ProgramDataPath, _zipClient, logManager, configurationManager);

            EnsureMediaFilters(appPaths.ProgramDataPath);
        }

        public DirectShowPlayerConfiguration GetConfiguration()
        {
            return _config.GetConfiguration<DirectShowPlayerConfiguration>("directshowplayer");
        }

        public void UpdateConfiguration(DirectShowPlayerConfiguration config)
        {
            var curConfig = GetConfiguration();
            _config.SaveConfiguration("directshowplayer", config);
            if (string.Compare(curConfig.FilterSet, config.FilterSet, true) != 0)
            {
                //update filters
                URCOMLoader.Instance.EnsureObjects(_appPaths.ProgramDataPath, _zipClient, false);
            }
        }

        public void ResetConfiguration(string section)
        {
            var curConfig = GetConfiguration();
            switch (section)
            {
                case "video":
                    curConfig.VideoConfig.ResetDefaults();
                    break;
                case "audio":
                    curConfig.AudioConfig.ResetDefaults();
                    break;
            }
        }

        private void EnsureMediaFilters(string appProgramDataPath)
        {
            Task.Run(() =>
            {
                try
                {
                    URCOMLoader.Instance.EnsureObjects(appProgramDataPath, _zipClient, false);
                }
                catch
                {
                }
            });
        }

        public bool RequiresGlobalMouseHook
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can select audio track.
        /// </summary>
        /// <value><c>true</c> if this instance can select audio track; otherwise, <c>false</c>.</value>
        public virtual bool CanSelectAudioTrack
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can select subtitle track.
        /// </summary>
        /// <value><c>true</c> if this instance can select subtitle track; otherwise, <c>false</c>.</value>
        public virtual bool CanSelectSubtitleTrack
        {
            get { return true; }
        }

        public PlayState PlayState
        {
            get
            {
                if (_mediaPlayer == null)
                {
                    return PlayState.Idle;
                }

                return _mediaPlayer.PlayState;
            }
        }

        public long? CurrentPositionTicks
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentPositionTicks;
                }

                return null;
            }
        }

        public long? CurrentDurationTicks
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentDurationTicks;
                }

                return null;
            }
        }

        /// <summary>
        /// Get the current subtitle index.
        /// </summary>
        /// <value>The current subtitle index.</value>
        public int? CurrentSubtitleStreamIndex
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentSubtitleStreamIndex;
                }

                return null;
            }
        }

        /// <summary>
        /// Get the current audio index.
        /// </summary>
        /// <value>The current audio index.</value>
        public int? CurrentAudioStreamIndex
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentAudioStreamIndex;
                }

                return null;
            }
        }

        public void Play(string path, long startPositionTicks, bool isVideo, BaseItemDto item, MediaSourceInfo mediaSource, bool enableFullScreen, IntPtr videoWindowHandle)
        {
            var playableItem = new PlayableItem
            {
                MediaSource = mediaSource,
                PlayablePath = path,
                OriginalItem = item
            };

            try
            {
                InvokeOnPlayerThread(() =>
                {
                    //create a fresh DS Player everytime we want one
                    DisposePlayerInternal();

                    _mediaPlayer = new DirectShowPlayer(this, _logger, GetConfiguration(), _httpClient, videoWindowHandle);
                    _mediaPlayer.Play(playableItem, enableFullScreen);

                    try
                    {
                        Standby.PreventSleepAndMonitorOff();
                    }
                    catch
                    {

                    }

                    try
                    {
                        if (startPositionTicks > 0)
                        {
                            _mediaPlayer.Seek(startPositionTicks);
                        }
                    }
                    catch
                    {

                    }

                    if (playableItem.OriginalItem.IsVideo)
                    {
                        var audioIndex = playableItem.MediaSource.DefaultAudioStreamIndex;
                        var subtitleIndex = playableItem.MediaSource.DefaultSubtitleStreamIndex;

                        if (audioIndex.HasValue && audioIndex.Value != -1)
                        {
                            try
                            {
                                SetAudioStreamIndexInternal(audioIndex.Value);
                            }
                            catch
                            {

                            }
                        }
                        try
                        {
                            SetSubtitleStreamIndexInternal(subtitleIndex ?? -1);
                        }
                        catch
                        {

                        }
                    }

                }, true);
            }
            catch (Exception ex)
            {
                OnPlaybackStopped(playableItem, null, TrackCompletionReason.Failure, null);

                throw;
            }
        }

        public void SetVolume(int level)
        {
            _mediaPlayer.SetVolume(level);
        }

        private static string GetFolderRipPath(VideoType videoType, string root)
        {
            if (videoType == VideoType.BluRay)
            {
                return GetBlurayPath(root);
            }

            return root;
        }

        private static string GetBlurayPath(string root)
        {
            var file = new DirectoryInfo(root)
                .EnumerateFiles("index.bdmv", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (file != null)
            {
                Uri uri;

                if (Uri.TryCreate(file.FullName, UriKind.RelativeOrAbsolute, out uri))
                {
                    return uri.OriginalString;
                }

                return file.FullName;
            }

            return root;
        }

        private void DisposePlayer()
        {
            InvokeOnPlayerThread(DisposePlayerInternal);
        }

        private void DisposePlayerInternal()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Dispose();
                _mediaPlayer = null; //force the object to get cleaned up
            }
        }

        public void Pause()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(_mediaPlayer.Pause);
                }
            }
        }

        public void UnPause()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(_mediaPlayer.Unpause);
                }
            }
        }

        public void ToggleVideoScaling()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(_mediaPlayer.ToggleVideoScaling);
                }
            }
        }

        private readonly object _commandLock = new object();

        public void Stop()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    _logger.Info("Invoke stop");
                    InvokeOnPlayerThread(() => _mediaPlayer.Stop(TrackCompletionReason.Stop, null));
                    _logger.Info("Invoke stop complete");
                }
            }
        }

        public void Seek(long positionTicks)
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(() => _mediaPlayer.Seek(positionTicks));
                }
            }
        }

        public void SetRate(double rate)
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(() => _mediaPlayer.SetRate(rate));
                }
            }
        }

        /// <summary>
        /// Occurs when [play state changed].
        /// </summary>
        public event EventHandler PlayStateChanged;
        internal void OnPlayStateChanged()
        {
            EventHelper.FireEventIfNotNull(PlayStateChanged, this, EventArgs.Empty, _logger);
        }

        internal void OnPlaybackStopped(PlayableItem media, long? positionTicks, TrackCompletionReason reason, int? newTrackIndex)
        {
            // TODO
            // Notify
        }

        public void ClosePlayer()
        {
            // Close any visible UI

            DisposePlayer();
        }

        public IReadOnlyList<SelectableMediaStream> SelectableStreams
        {
            get { return _mediaPlayer.GetSelectableStreams(); }
        }

        public void SetSubtitleStreamIndex(int subtitleStreamIndex)
        {
            InvokeOnPlayerThread(() => SetSubtitleStreamIndexInternal(subtitleStreamIndex));
        }

        private void SetSubtitleStreamIndexInternal(int subtitleStreamIndex)
        {
            _mediaPlayer.SetSubtitleStreamIndex(subtitleStreamIndex);
        }

        public void NextSubtitleStream()
        {
            InvokeOnPlayerThread(() => _mediaPlayer.NextSubtitleStream());
        }

        public void SetAudioStreamIndex(int audioStreamIndex)
        {
            InvokeOnPlayerThread(() => SetAudioStreamIndexInternal(audioStreamIndex));
        }

        public void SetAudioStreamIndexInternal(int audioStreamIndex)
        {
            _mediaPlayer.SetAudioStreamIndex(audioStreamIndex);
        }

        public void RemoveSubtitles()
        {

        }

        public void HandleWindowSizeChanged()
        {
            if (_mediaPlayer != null)
            {
                InvokeOnPlayerThread(() => _mediaPlayer.HandleWindowSizeChanged(), false, false);
            }
        }

        private void InvokeOnPlayerThread(Action action, bool throwOnError = false, bool wait = true)
        {
            try
            {
                //var task = Task.Factory.StartNew(() =>
                //{
                //    _logger.Info("thread id: {0}", Thread.CurrentThread.ManagedThreadId);
                //    action();

                //}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _dispatcher);

                TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();

                //_dispatcher.BeginInvoke(action, DispatcherPriority.Normal);

                Action act = () =>
                {
                    _logger.Info("thread id: {0}", Thread.CurrentThread.ManagedThreadId);

                    try
                    {
                        action();
                        source.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        source.TrySetException(ex);
                    }
                };

                //_queue.Enqueue(act);

                _dispatcher.BeginInvoke(act, DispatcherPriority.Send);

                var task = source.Task;

                if (wait)
                {
                    Task.WaitAll(task);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("InvokeOnPlayerThread", ex);

                if (throwOnError) throw;
            }
        }

        #region IDisposable

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    if (_privateCom != null)
                        _privateCom.Dispose();
                    _privateCom = null;

                    _disposed = true;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public enum TrackCompletionReason
    {
        Stop,
        Ended,
        ChangeTrack,
        Failure
    }
}
