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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emby.Theater.Common;
using Emby.Theater.DirectShow.Configuration;
using MediaBrowser.Common.Configuration;

namespace Emby.Theater.DirectShow
{
    public class InternalDirectShowPlayer : IDisposable
    {
        private DirectShowPlayer _mediaPlayer;

        private readonly ILogger _logger;
        private readonly MainBaseForm _hostForm;
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

        public InternalDirectShowPlayer(
            ILogManager logManager
            , MainBaseForm hostForm
            //, IPresentationManager presentation
            //, ISessionManager sessionManager
            , IApplicationPaths appPaths
            , IIsoManager isoManager
            //, IUserInputManager inputManager
            , IZipClient zipClient
            , IHttpClient httpClient, IConfigurationManager configurationManager)
        {
            _logger = logManager.GetLogger("InternalDirectShowPlayer");
            _hostForm = hostForm;
            //_presentation = presentation;
            //_sessionManager = sessionManager;
            _httpClient = httpClient;
            _config = configurationManager;
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
                URCOMLoader.Instance.EnsureObjects(_appPaths.ProgramDataPath, _zipClient, false, true);
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

        public void Play(string path, long startPositionTicks, bool isVideo, BaseItemDto item, MediaSourceInfo mediaSource, bool enableFullScreen)
        {
            try
            {
                PlayTrack(path, startPositionTicks, isVideo, item, mediaSource, enableFullScreen);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error beginning playback", ex);

                DisposePlayer();

                throw;
            }
        }

        public void SetVolume(int level)
        {
            _mediaPlayer.SetVolume(level);
        }

        private void PlayTrack(string path, long startPositionTicks, bool isVideo, BaseItemDto item, MediaSourceInfo mediaSource, bool enableFullScreen)
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
                    DisposePlayer();

                    _mediaPlayer = new DirectShowPlayer(this, _hostForm, _logger, GetConfiguration(), _httpClient);
                    _mediaPlayer.Play(playableItem, enableFullScreen);

                    try
                    {
                        Standby.PreventSleepAndMonitorOff();
                    }
                    catch
                    {

                    }

                }, true);
            }
            catch
            {
                OnPlaybackStopped(playableItem, null, TrackCompletionReason.Failure, null);

                throw;
            }

            if (startPositionTicks > 0)
            {
                InvokeOnPlayerThread(() => _mediaPlayer.Seek(startPositionTicks));
            }

            if (playableItem.OriginalItem.IsVideo)
            {
                var audioIndex = playableItem.MediaSource.DefaultAudioStreamIndex;
                var subtitleIndex = playableItem.MediaSource.DefaultSubtitleStreamIndex;

                if (audioIndex.HasValue && audioIndex.Value != -1)
                {
                    SetAudioStreamIndex(audioIndex.Value);
                }
                SetSubtitleStreamIndex(subtitleIndex ?? -1);
            }
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
            if (_mediaPlayer != null)
            {
                InvokeOnPlayerThread(() =>
                {
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null; //force the object to get cleaned up
                });
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
                    InvokeOnPlayerThread(() => _mediaPlayer.Stop(TrackCompletionReason.Stop, null));
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

        private void DisposeMount(PlayableItem media)
        {
            if (media.IsoMount != null)
            {
                try
                {
                    media.IsoMount.Dispose();
                    media.IsoMount = null;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error unmounting iso {0}", ex, media.IsoMount.MountedPath);
                }
            }
        }

        internal async void OnPlaybackStopped(PlayableItem media, long? positionTicks, TrackCompletionReason reason, int? newTrackIndex)
        {
            DisposeMount(media);

            try
            {
                InvokeOnPlayerThread(() =>
                {
                    Standby.AllowSleep();
                });
            }
            catch
            {
                
            }

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

        public void ChangeAudioStream(SelectableMediaStream track)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetAudioTrack(track));
        }


        public void SetSubtitleStreamIndex(int subtitleStreamIndex)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetSubtitleStreamIndex(subtitleStreamIndex));
        }

        public void NextSubtitleStream()
        {
            InvokeOnPlayerThread(() => _mediaPlayer.NextSubtitleStream());
        }

        public void SetAudioStreamIndex(int audioStreamIndex)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetAudioStreamIndex(audioStreamIndex));
        }

        public void ChangeSubtitleStream(SelectableMediaStream track)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetSubtitleStream(track));
        }

        public void RemoveSubtitles()
        {

        }

        private void InvokeOnPlayerThread(Action action, bool throwOnError = false)
        {
            try
            {
                //if (_hostForm.Form.InvokeRequired)
                //{
                //    _hostForm.Form.Invoke(action);
                //}
                //else
                //{
                //    action();
                //}

                if (_hostForm.InvokeRequired)
                {
                    _hostForm.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("InvokeOnPlayerThread", ex);

                if (throwOnError) throw ex;
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
