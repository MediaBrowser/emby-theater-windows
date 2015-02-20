using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Mpdn
{
    public class SessionSequence : IPreparedSessions
    {
        private readonly IPlaySequence _sequence;
        private readonly CancellationToken _cancellationToken;
        private readonly ILogManager _logManager;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly ILogger _log;
        private readonly Subject<IPlaybackSession> _sessions;
        private readonly Subject<PlaybackStatus> _status;
        
        public SessionSequence(IPlaySequence sequence, CancellationToken cancellationToken, ILogManager logManager, IWindowManager windowManager, IEventAggregator events, ILogger log)
        {
            _sequence = sequence;
            _cancellationToken = cancellationToken;
            _logManager = logManager;
            _windowManager = windowManager;
            _events = events;
            _log = log;
            _sessions = new Subject<IPlaybackSession>();
            _status = new Subject<PlaybackStatus>();
        }

        public async Task<SessionCompletion> Start()
        {
            _log.Debug("Starting sessions sequence");

            // start MPDN process
            // parent window

            var process = await StartMpdn();

            using (Disposable.Create(() => process.CloseMainWindow()))
            using (_windowManager.UseBackgroundWindow(process.MainWindowHandle)) {

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

            // esablish remote api connection
            // start session loop

            return _cancellationToken.IsCancellationRequested ? SessionCompletion.Stopped : SessionCompletion.Complete;
        }

        private Task<Process> StartMpdn()
        {
            return Task.Run(() => {
                var process = Process.Start(new ProcessStartInfo {
                    FileName = @"D:\Projects\MPDN\MediaPlayerDotNet.exe",
                    //WindowStyle = ProcessWindowStyle.Minimized,
                });

                process.WaitForInputIdle();
                return process;
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
    }
}
