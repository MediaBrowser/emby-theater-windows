using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Mpdn
{
    public class Session : IPlaybackSession
    {
        private readonly PlayableMedia _item;
        private readonly RemoteClient _api;
        private readonly ILogger _log;
        private readonly IPlaybackManager _playbackManager;
        private readonly Subject<PlaybackStatus> _statusEvents;
        private readonly TaskCompletionSource<SessionCompletionAction> _completed;
        private readonly object _lock;

        private bool _isPlaying;

        private PlaybackStatus _latestStatus;

        private long _progress;
        private long _duration;
        private List<MediaStream> _activeStreams;
        private PlaybackStatusType _statusType;
        private SessionCompletionAction? _completionAction;

        public Session(PlayableMedia item, RemoteClient api, CancellationToken cancellationToken, ILogger log, IPlaybackManager playbackManager)
        {
            _item = item;
            _api = api;
            _log = log;
            _playbackManager = playbackManager;
            _statusEvents = new Subject<PlaybackStatus>();
            _progress = item.Media.Options.StartPositionTicks ?? 0;
            _duration = item.Source.RunTimeTicks ?? 0;
            _activeStreams = new[] {
                item.Source.VideoStream,
                item.Source.MediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Audio),
                item.Source.MediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Subtitle)
            }.Where(s => s != null).ToList();
            _completed = new TaskCompletionSource<SessionCompletionAction>();
            _lock = new object();

            cancellationToken.Register(() => {
                lock (_lock) {
                    if (!_isPlaying) {
                        return;
                    }

                    _api.Stop().Wait();

                    _statusType = PlaybackStatusType.Stopped;
                    PublishStatus();

                    var action = new SessionCompletionAction { Direction = NavigationDirection.Stop };
                    Finish(action);
                }
            });
        }

        public void Play()
        {
            _api.Play();
        }

        public void Pause()
        {
            _api.Pause();
        }

        public void Seek(long ticks)
        {
            _api.Seek(Math.Max(0, ticks));
        }

        public void SkipToNext()
        {
            // flag status as skipped
            // set completion action to forward
            // complete completion task

            lock (_lock) {
                if (!_isPlaying) {
                    return;
                }

                _api.Stop().Wait();

                _statusType = PlaybackStatusType.Skipped;
                PublishStatus();

                var action = new SessionCompletionAction { Direction = NavigationDirection.Forward };
                Finish(action);
            }
        }

        public void SkipToPrevious()
        {
            // either seek or skip

            // flag status as skipped
            // set completion action to backward
            // complete completion task

            lock (_lock) {
                if (!_isPlaying) {
                    return;
                }

                if (_progress > TimeSpan.FromSeconds(30).Ticks) {
                    Seek(0);
                    return;
                }

                _api.Stop().Wait();

                _statusType = PlaybackStatusType.Skipped;
                PublishStatus();

                var action = new SessionCompletionAction { Direction = NavigationDirection.Backward };
                Finish(action);
            }
        }

        public void SkipTo(int itemIndex)
        {
            // flag status as skipped
            // set completion action to index
            // complete completion task

            lock (_lock) {
                if (!_isPlaying) {
                    return;
                }

                _api.Stop().Wait();

                _statusType = PlaybackStatusType.Skipped;
                PublishStatus();

                var action = new SessionCompletionAction { Direction = NavigationDirection.Skip, Index = itemIndex };
                Finish(action);
            }
        }

        public void SelectStream(MediaStreamType channel, int index)
        {
            // lookup matching stream description
            // api -> change[audio/subtitle]track

            if (channel == MediaStreamType.Subtitle) {
                var track = FindSubtitleStream(_item.Source.MediaStreams[index]);
                if (track != null) {
                    _api.ChangeSubtitleTrack(track.Value.Description);
                }
            }

            if (channel == MediaStreamType.Audio) {
                var track = FindAudioStream(_item.Source.MediaStreams[index]);
                if (track != null) {
                    _api.ChangeAudioTrack(track.Value.Description);
                }
            }
        }

        public void SetPlaybackSpeed(double speedMultiplier)
        {
            throw new NotImplementedException();
        }

        public IObservable<PlaybackStatus> Events
        {
            get { return _statusEvents; }
        }

        public PlaybackCapabilities Capabilities
        {
            get
            {
                return new PlaybackCapabilities {
                    CanPlay = true,
                    CanPause = true,
                    CanSeek = true,
                    CanStop = true,
                    CanSkip = true,
                    CanChangeStreams = true,
                    CanChangeSpeed = false
                };
            }
        }

        public PlaybackStatus Status
        {
            get { return _latestStatus; }
        }

        public async Task<SessionCompletionAction> Run()
        {
            _isPlaying = true;

            // hook api client events
            //   exit, finished -> 
            //   position -> post playing status update
            //   paused -> post pause status update
            //   mute -> update player manager global audio setting
            //   volume -> update player manager global audio setting
            //   subchanged -> update active streams status with new subtitle stream
            //   audiochanged -> update active streams status with new audio stream
            //   fulllength -> update status duration
            //   subtitles, audiotracks -> map descriptions to media streams. if not yet done, change streams to default values
            using (SubscribeToApiEvents()) {

                _log.Info("Starting {0}", _item.Media.Item.Name);

                // api->open
                await _api.Open(_item.Path);
                await _api.ChangeVolume(_playbackManager.GlobalSettings.Audio.Volume);
                await _api.Mute(_playbackManager.GlobalSettings.Audio.IsMuted);
                await _api.Seek(_item.Media.Options.StartPositionTicks ?? 0);

                // post playing status
                _statusType = PlaybackStatusType.Started;
                PublishStatus();

                // await until exit, stopped or finished from api client, or until task is cancelled
                var completionAction = await _completed.Task;

                // close status events observable
                // unhook api client events
                // return next action

                _log.Info("Completing playback of {0}", _item.Media.Item.Name);

                try {
                    _statusEvents.OnCompleted();
                }
                catch (Exception e) {
                    _log.ErrorException("Error closing status events observable.", e);
                }

                return completionAction;
            }
        }

        private IDisposable SubscribeToApiEvents()
        {
            Action<string> finished = path => {
                lock (_lock) {
                    if (path != _item.Path) {
                        return;
                    }

                    if (_completionAction == null) {
                        _statusType = PlaybackStatusType.Complete;
                        PublishStatus();
                    }

                    _isPlaying = false;
                    _completed.SetResult(_completionAction ?? new SessionCompletionAction { Direction = NavigationDirection.Forward });
                }
            };

            Action<long> progress = t => {
                lock (_lock) {
                    if (!_isPlaying) {
                        return;
                    }

                    _statusType = PlaybackStatusType.Playing;
                    _progress = t;
                    PublishStatus();
                }
            };

            Action<string> playing = path => {
                lock (_lock) {
                    if (!_isPlaying) {
                        return;
                    }

                    _statusType = PlaybackStatusType.Playing;
                    PublishStatus();
                }
            };

            Action<string> paused = path => {
                lock (_lock) {
                    if (!_isPlaying) {
                        return;
                    }

                    _statusType = PlaybackStatusType.Paused;
                    PublishStatus();
                }
            };

            Action<long> duration = d => _duration = d;

            Action<IEnumerable<RemoteClient.SubtileTrack>> subtitlesChanged = subs =>
            {
                // record subtitles
                _remoteSubtitles = subs.ToList();

                // if not yet initialised, then locate default subtitle and set
                if (!_initializedSubtitles)
                {
                    if (_item.Source.DefaultSubtitleStreamIndex != null)
                    {
                        var defaultSubtitles = _item.Source.MediaStreams[_item.Source.DefaultSubtitleStreamIndex.Value];
                        var remoteSubtitle = FindSubtitleStream(defaultSubtitles);
                        if (remoteSubtitle != null)
                        {
                            _api.ChangeSubtitleTrack(remoteSubtitle.Value.Description);
                        }
                    }

                    _initializedSubtitles = true;
                }
            };

            Action<string> activeSubtitleChanged = sub =>
            {
                // locate subtitle record
                // find subtitle mediastream
                // update active streams
                var track = _remoteSubtitles.FirstOrDefault(t => t.Description == sub);
                var stream = FindSubtitleStream(track);
                if (stream != null)
                {
                    _activeStreams.RemoveAll(s => s.Type == MediaStreamType.Subtitle);
                    _activeStreams.Add(stream);
                }
            };

            Action<IEnumerable<RemoteClient.AudioTrack>> audioTracksChanged = tracks =>
            {
                // record audio tracks
                _remoteAudioTracks = tracks.ToList();

                // if not yet initialised, then locate default audio track and set
                if (!_initializedAudioTrack)
                {
                    if (_item.Source.DefaultAudioStream != null)
                    {
                        var remoteTrack = FindAudioStream(_item.Source.DefaultAudioStream);
                        if (remoteTrack != null)
                        {
                            _api.ChangeAudioTrack(remoteTrack.Value.Description);
                        }
                    }

                    _initializedAudioTrack = true;
                }
            };

            Action<string> activeAudioTrackChanged = desc =>
            {
                // locate subtitle record
                // find subtitle mediastream
                // update active streams
                var track = _remoteAudioTracks.FirstOrDefault(t => t.Description == desc);
                var stream = FindAudioStream(track);
                if (stream != null)
                {
                    _activeStreams.RemoveAll(s => s.Type == MediaStreamType.Audio);
                    _activeStreams.Add(stream);
                }
            };

            _api.Finished += finished;
            _api.ProgressChanged += progress;
            _api.Playing += playing;
            _api.Paused += paused;
            _api.DurationChanged += duration;
            _api.SubtitlesChanged += subtitlesChanged;
            _api.ActiveSubtitleChanged += activeSubtitleChanged;
            _api.AudioTracksChanged += audioTracksChanged;
            _api.ActiveAudioTrackChanged += activeAudioTrackChanged;

            return Disposable.Create(() => {
                _api.Finished -= finished;
                _api.ProgressChanged -= progress;
                _api.Playing -= playing;
                _api.Paused -= paused;
                _api.DurationChanged -= duration;
                _api.SubtitlesChanged -= subtitlesChanged;
                _api.ActiveSubtitleChanged -= activeSubtitleChanged;
                _api.AudioTracksChanged -= audioTracksChanged;
                _api.ActiveAudioTrackChanged -= activeAudioTrackChanged;
            });
        }

        private List<RemoteClient.SubtileTrack> _remoteSubtitles = new List<RemoteClient.SubtileTrack>();
        private bool _initializedSubtitles = false;

        private MediaStream FindSubtitleStream(RemoteClient.SubtileTrack sub)
        {
            return _item.Source.MediaStreams.Where(s => s.Type == MediaStreamType.Subtitle)
                        .FirstOrDefault(s => Equals(FindSubtitleStream(s) ?? new RemoteClient.SubtileTrack(), sub));
        }

        private RemoteClient.SubtileTrack? FindSubtitleStream(MediaStream stream)
        {
            var matchingDescription = FindMatchingStreamDescription(_remoteSubtitles.Select(t => t.Description),
                                                                    new[] { stream.Language }.Where(t => !string.IsNullOrEmpty(t)),
                                                                    new[] { stream.Codec, stream.IsDefault ? "default" : null, stream.IsForced ? "forced" : null }.Where(t => !string.IsNullOrEmpty(t)));

            return _remoteSubtitles.FirstOrDefault(t => t.Description == matchingDescription);
        }

        private List<RemoteClient.AudioTrack> _remoteAudioTracks = new List<RemoteClient.AudioTrack>();
        private bool _initializedAudioTrack = false;

        private MediaStream FindAudioStream(RemoteClient.AudioTrack track)
        {
            return _item.Source.MediaStreams.Where(s => s.Type == MediaStreamType.Audio)
                        .FirstOrDefault(s => Equals(FindAudioStream(s) ?? new RemoteClient.AudioTrack(), track));
        }

        private RemoteClient.AudioTrack? FindAudioStream(MediaStream stream)
        {
            var matchingDescription = FindMatchingStreamDescription(_remoteAudioTracks.Select(t => t.Description),
                                                                    new[] { stream.Language, stream.ChannelLayout }.Where(t => !string.IsNullOrEmpty(t)),
                                                                    new[] { stream.Codec, stream.SampleRate + " Hz", stream.IsDefault ? "default" : null }.Where(t => !string.IsNullOrEmpty(t)));

            return _remoteAudioTracks.FirstOrDefault(t => t.Description == matchingDescription);
        }

        private string FindMatchingStreamDescription(IEnumerable<string> descriptions, IEnumerable<string> requiredTags, IEnumerable<string> desiredTags)
        {
//            var regex = new Regex(@"[\(\[](?<tags>[^\[\(\]\)]+)[\)\]]");
            var regex = new Regex(@"(?<tags>[^\[\(\]\)]+)");

            var tags = descriptions.Select(d => new {
                Description = d,
                Tags = regex.Matches(d).Cast<Match>()
                            .Select(m => m.Groups["tags"])
                            .Select(g => g.Value)
                            .SelectMany(v => v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            .Select(t => t.Trim())
                            .ToList()
            });

            return tags.Where(t => requiredTags.All(t.Tags.Contains))
                       .OrderByDescending(t => desiredTags.Count(t.Tags.Contains))
                       .ThenBy(t => t.Tags.Count)
                       .Select(t => t.Description)
                       .FirstOrDefault();
        }
        
        private void Finish(SessionCompletionAction completionAction)
        {
            _completionAction = completionAction;
            _isPlaying = false;
        }
        
        private void PublishStatus()
        {
            _latestStatus = new PlaybackStatus {
                PlayableMedia = _item,
                ActiveStreams = _activeStreams.ToArray(),
                Duration = _duration,
                Progress = _progress,
                Speed = 1,
                StatusType = _statusType
            };

            Task.Run(() => _statusEvents.OnNext(_latestStatus));
        }
    }
}
