using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.MockPlayer
{
    public class MockMediaPlayer : IMediaPlayer
    {
        public int Priority
        {
            get { return int.MaxValue; }
        }

        public string Name
        {
            get { return "Mock Player"; }
        }

        public bool CanPlay(Media media)
        {
            return true;
        }

        public Task<IPreparedSessions> Prepare(IPlaySequence sequence)
        {
            return Task.FromResult<IPreparedSessions>(new SessionSequence(sequence));
        }
    }

    class SessionSequence : IPreparedSessions
    {
        private readonly IPlaySequence _sequence;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;

        public SessionSequence(IPlaySequence sequence)
        {
            _sequence = sequence;
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public Task Start()
        {
            Task.Run(async () => {
                while (_sequence.Next()) {
                    var item = GetPlayableMedia(_sequence.Current);
                    var session = new Session(item);

                    using (session.Events.Subscribe(status => _status.OnNext(status))) {
                        _sessions.OnNext(session);
                        await session.Run();
                    }
                }

                _sessions.OnCompleted();
            });

            return Task.FromResult(0);
        }

        private PlayableMedia GetPlayableMedia(Media media)
        {
            return new PlayableMedia {
                Media = media,
            };
        }

        public IObservable<IPlaybackSession> Sessions
        {
            get { return _sessions; }
        }

        public IObservable<PlaybackStatus> Status
        {
            get { return _status; }
        }
    }

    class Session : IPlaybackSession
    {
        private readonly PlayableMedia _media;
        private readonly ISubject<PlaybackStatus> _statusEvents;
        private readonly object _lock;

        private PlaybackStatusType _state;
        private PlaybackStatus _latestStatus;
        private long _progress;

        public Session(PlayableMedia media)
        {
            _lock = new object();
            _media = media;
            _statusEvents = new Subject<PlaybackStatus>();
        }

        public async Task Run()
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
                    if (_state == PlaybackStatusType.Stopped ||
                        _state == PlaybackStatusType.Skipped ||
                        _state == PlaybackStatusType.Error) {
                        break;
                    }

                    // report our progress
                    _progress = Math.Min(_progress + TimeSpan.FromSeconds(1).Ticks,
                                         _media.Source.RunTimeTicks ?? int.MaxValue);

                    PublishState();
                }
            }

            ChangeAndPublishState(PlaybackStatusType.Complete);

            // close the status observable once we are done
            _statusEvents.OnCompleted();
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
            };

            _statusEvents.OnNext(_latestStatus);
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
                if (_state != PlaybackStatusType.Started &&
                    _state != PlaybackStatusType.Playing &&
                    _state != PlaybackStatusType.Paused) {
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
                _progress = Math.Max(0, Math.Min(ticks, _media.Source.RunTimeTicks ?? int.MaxValue));
                PublishState();
            }
        }

        public void SkipNext()
        {
            throw new NotImplementedException();
        }

        public void SkipPrevious()
        {
            throw new NotImplementedException();
        }

        public void SkipTo(int itemIndex)
        {
            throw new NotImplementedException();
        }

        public void SelectStream(MediaStreamType channel, int index)
        {
            throw new NotImplementedException();
        }

        public void SetVolume(decimal volume)
        {
        }

        public void SetMuted(bool muted)
        {
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
                    CanSkip = false,
                    CanChangeStreams = false,
                    CanChangeVolume = false
                };
            }
        }

        public PlaybackStatus Status
        {
            get { return _latestStatus; }
        }
    }
}
