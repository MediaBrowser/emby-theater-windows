using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.MockPlayer
{
    /// <summary>
    ///     The SessionSequence class is an example implementation of <see cref="IPreparedSessions" />.
    ///     The class is responsible for managing the sessions in a play sequence.
    /// </summary>
    internal class SessionSequence : IPreparedSessions
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

        public void Start()
        {
            // run all playback in another task
            Task.Run(async () => {
                var nextAction = new SessionCompletionAction {
                    Direction = NavigationDirection.Forward
                };

                // keep moving to the next media until the sequence is complete
                while (MoveNext(nextAction)) {
                    // create a session for the media
                    PlayableMedia item = GetPlayableMedia(_sequence.Current);
                    var session = new Session(item);

                    // forward playback events to our own event observable
                    using (session.Events.Subscribe(status => _status.OnNext(status))) {
                        // notify the playback manager of a new session
                        _sessions.OnNext(session);

                        // run the session. the session reports which direction to move in to get to the next item
                        nextAction = await session.Run();
                    }
                }

                // notify the playback manager that we have finished, and that there are no more sessions
                _sessions.OnCompleted();
            });
        }

        public IObservable<IPlaybackSession> Sessions
        {
            get { return _sessions; }
        }

        public IObservable<PlaybackStatus> Status
        {
            get { return _status; }
        }

        private bool MoveNext(SessionCompletionAction action)
        {
            switch (action.Direction) {
                case NavigationDirection.Forward:
                    return _sequence.Next();
                case NavigationDirection.Backward:
                    return _sequence.Previous();
                case NavigationDirection.Skip:
                    return _sequence.SkipTo(action.Index);
            }

            return false;
        }

        private PlayableMedia GetPlayableMedia(Media media)
        {
            return new PlayableMedia {
                Media = media,
            };
        }
    }
}