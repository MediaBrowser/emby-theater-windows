using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
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
        private readonly CancellationToken _cancellationToken;
        private readonly ILogger _log;
        private readonly Subject<PlaybackStatus> _statusEvents;

        private PlaybackStatus _latestStatus;

        private long _progress;
        private long _duration;
        private List<MediaStream> _activeStreams;

        public Session(PlayableMedia item, RemoteClient api, CancellationToken cancellationToken, ILogManager logManager)
        {
            _item = item;
            _api = api;
            _cancellationToken = cancellationToken;
            _log = logManager.GetLogger("Mpdn");
            _statusEvents = new Subject<PlaybackStatus>();
            _progress = item.Media.Options.StartPositionTicks ?? 0;
            _duration = item.Source.RunTimeTicks ?? 0;
            _activeStreams = new[] {
                item.Source.VideoStream,
                item.Source.MediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Audio),
                item.Source.MediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Subtitle)
            }.Where(s => s != null).ToList();
        }

        public void Play()
        {
            // api -> play
            throw new NotImplementedException();
        }

        public void Pause()
        {
            // api -> pause
            throw new NotImplementedException();
        }

        public void Seek(long ticks)
        {
            // api -> seek
            throw new NotImplementedException();
        }

        public void SkipToNext()
        {
            // flag status as skipped
            // set completion action to forward
            // complete completion task

            throw new NotImplementedException();
        }

        public void SkipToPrevious()
        {
            // either seek or skip

            // flag status as skipped
            // set completion action to backward
            // complete completion task

            throw new NotImplementedException();
        }

        public void SkipTo(int itemIndex)
        {
            // flag status as skipped
            // set completion action to index
            // complete completion task

            throw new NotImplementedException();
        }

        public void SelectStream(MediaStreamType channel, int index)
        {
            // lookup matching stream description
            // api -> change[audio/subtitle]track

            throw new NotImplementedException();
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

        public Task<SessionCompletionAction> Run()
        {
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
            //   
            // api->open
            // post playing status
            //
            // await until exit, stopped or finished from api client, or until task is cancelled
            
            throw new NotImplementedException();

            // post final state (completed, stopped, skipped or error)
            // close status events observable
            // unhook api client events
            // return next action
        }
    }
}
