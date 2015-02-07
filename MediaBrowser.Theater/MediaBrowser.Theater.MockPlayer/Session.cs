using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.MockPlayer
{
    /// <summary>
    ///     The Session class is an example implementation of the <see cref="IPlaybackSession" /> interface.
    ///     Each session represents the playback of one media item.
    /// </summary>
    internal class Session : IPlaybackSession
    {
        private readonly object _lock;
        private readonly PlayableMedia _media;
        private readonly ISubject<PlaybackStatus> _statusEvents;
        private readonly long _duration;

        private SessionCompletionAction _compeletionAction;
        private PlaybackStatus _latestStatus;
        private long _progress;
        private double _speed;
        private PlaybackStatusType _state;

        public Session(PlayableMedia media)
        {
            _lock = new object();
            _media = media;
            _duration = media.Source.RunTimeTicks ?? TimeSpan.FromSeconds(30).Ticks;
            _statusEvents = new Subject<PlaybackStatus>();
            _compeletionAction = new SessionCompletionAction { Direction = NavigationDirection.Forward };
            _speed = 1;
        }

        public void Play()
        {
            lock (_lock) {
                if (_state != PlaybackStatusType.Paused) {
                    return;
                }

                _state = PlaybackStatusType.Playing;
                PublishState();
            }
        }

        public void Pause()
        {
            lock (_lock) {
                if (_state != PlaybackStatusType.Playing) {
                    return;
                }

                _state = PlaybackStatusType.Paused;
                PublishState();
            }
        }

        public async Task Stop()
        {
            lock (_lock) {
                if (!_state.IsActiveState()) {
                    return;
                }

                _state = PlaybackStatusType.Stopped;
                PublishState();
            }

            // await for the status events to complete, which will mark the end of the session
            await _statusEvents;
        }

        public void Seek(long ticks)
        {
            lock (_lock) {
                _progress = Math.Max(0, Math.Min(ticks, _duration));
                PublishState();
            }
        }

        public void SkipNext()
        {
            lock (_lock) {
                if (!_state.IsActiveState()) {
                    return;
                }

                _state = PlaybackStatusType.Skipped;
                _compeletionAction = new SessionCompletionAction {
                    Direction = NavigationDirection.Forward
                };

                PublishState();
            }
        }

        public void SkipPrevious()
        {
            lock (_lock) {
                if (!_state.IsActiveState()) {
                    return;
                }

                var threshold = TimeSpan.FromSeconds(30).Ticks;
                if (_progress < threshold && _duration > threshold) {
                    _progress = 0;
                } else {
                    _state = PlaybackStatusType.Skipped;
                    _compeletionAction = new SessionCompletionAction {
                        Direction = NavigationDirection.Backward
                    };

                    PublishState();
                }
            }
        }

        public void SkipTo(int itemIndex)
        {
            lock (_lock) {
                if (!_state.IsActiveState()) {
                    return;
                }

                _state = PlaybackStatusType.Skipped;
                _compeletionAction = new SessionCompletionAction {
                    Direction = NavigationDirection.Skip,
                    Index = itemIndex
                };

                PublishState();
            }
        }

        public void SelectStream(MediaStreamType channel, int index) { }

        public void SetPlaybackSpeed(double speedMultiplier)
        {
            _speed = speedMultiplier;
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
                    CanChangeStreams = false,
                    CanChangeSpeed = true
                };
            }
        }

        public PlaybackStatus Status
        {
            get { return _latestStatus; }
        }

        /// <summary>
        ///     Begins playback of the current item.
        /// </summary>
        /// <returns>The action to take to move to the next item to play.</returns>
        public async Task<SessionCompletionAction> Run()
        {
            ChangeAndPublishState(PlaybackStatusType.Started);

            // player initialization may happen here
            // file opening, graph setup, etc

            ChangeAndPublishState(PlaybackStatusType.Playing);

            // report progress while we are playing
            while (_progress < _media.Source.RunTimeTicks) {
                // tick once per second
                // some players may perform actual playback in this thread, and report status as they go
                // others may play in another thread or process, and poll status periodically
                await Task.Delay(TimeSpan.FromSeconds(1));

                lock (_lock) {
                    // don't do anything if playback is paused
                    if (_state == PlaybackStatusType.Paused) {
                        continue;
                    }

                    // exit out of our loop if playback has stopped
                    if (!_state.IsActiveState()) {
                        break;
                    }

                    // report our progress
                    _progress = Math.Min(_progress + (long)(TimeSpan.FromSeconds(1).Ticks * _speed),
                                         _media.Source.RunTimeTicks ?? TimeSpan.FromSeconds(30).Ticks);

                    PublishState();
                }
            }

            if (_state.IsActiveState()) {
                ChangeAndPublishState(PlaybackStatusType.Complete);
            }

            // close the status observable once we are done
            _statusEvents.OnCompleted();

            return _compeletionAction;
        }

        private void ChangeAndPublishState(PlaybackStatusType type)
        {
            lock (_lock) {
                _state = type;
                PublishState();
            }
        }

        private void PublishState()
        {
            _latestStatus = new PlaybackStatus {
                Media = _media,
                StatusType = _state,
                Progress = _progress,
                Duration = _media.Source.RunTimeTicks ?? TimeSpan.FromSeconds(40).Ticks,
                Speed = _speed
            };

            _statusEvents.OnNext(_latestStatus);
        }
    }
}