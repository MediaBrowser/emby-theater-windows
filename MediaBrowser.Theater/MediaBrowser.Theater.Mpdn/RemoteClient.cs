using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Mpdn
{
    public class RemoteClient : IDisposable
    {
        private readonly Socket _socket;
        private readonly string _guid;

        private StreamWriter _writer;
        private bool _running;

        public RemoteClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _guid = Guid.NewGuid().ToString();
            _running = true;
        }

        public Task Conect(IPEndPoint endPoint)
        {
            var tcs = new TaskCompletionSource<object>();
            Task.Run(() => Connect(endPoint, tcs));

            return tcs.Task;
        }

        private async Task Connect(IPEndPoint endPoint, TaskCompletionSource<object> connectedTask)
        {
            try {
                _socket.Connect(endPoint);

                var stream = new NetworkStream(_socket);

                using (var reader = new StreamReader(stream))
                using (_writer = new StreamWriter(stream)) {
                    await _writer.WriteLineAsync(_guid);
                    
                    while (_running) {
                        var data = await reader.ReadLineAsync();
                        HandleMessage(data, connectedTask);
                    }
                }
            }
            finally {
                _socket.Close();
            }
        }

        #region Sending

        public Task Play(string path)
        {
            return _writer.WriteLineAsync("Play|" + path);
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Seek(long position)
        {
            throw new NotImplementedException();
        }

        public void ChangeAudioTrack(string description)
        {
            throw new NotImplementedException();
        }

        public void ChangeSubtitleTrack(string description)
        {
            throw new NotImplementedException();
        }

        public void Mute(bool muted)
        {
            throw new NotImplementedException();
        }

        public void ChangeVolume(double volume)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Receiving
        private void HandleMessage(string data, TaskCompletionSource<object> connectedTask)
        {
            var cmd = data.Split('|');
            if (cmd.Length <= 0) {
                return;
            }

            switch (cmd[0]) {
                case "Exit":
                    _running = false;
                    OnExited();
                    break;
                case "ClientGUID":
                    connectedTask.SetResult(null);
                    break;
                case "Playing":
                    OnPlaying(cmd[1]);
                    break;
                case "Paused":
                    OnPaused(cmd[1]);
                    break;
                case "Stopped":
                    OnStopped(cmd[1]);
                    break;
                case "Finished":
                    OnFinished(cmd[1]);
                    break;
                case "Closed":
                case "Closing":
                    break;
                case "Position":
                    long progress;
                    if (long.TryParse(cmd[1], out progress)) {
                        OnProgressChanged(progress);
                    }
                    break;
                case "FullLength":
                    long duration;
                    if (long.TryParse(cmd[1], out duration)) {
                        OnDurationChanged(duration);
                    }
                    break;
                case "Fullscreen":
                case "Mute":
                    bool muted;
                    if (bool.TryParse(cmd[1], out muted)) {
                        OnMuted(muted);
                    }
                    break;
                case "Volume":
                    double volume;
                    if (double.TryParse(cmd[1], out volume)) {
                        OnVolumeChanged(volume);
                    }
                    break;
                case "Chapters":
                    var chapters = cmd[1].Split(new[] { "]]" }, StringSplitOptions.None).Select(s => {
                        var parts = s.Split(new[] { ">>" }, StringSplitOptions.None);
                        return new Chapter {
                            Index = int.Parse(parts[0]),
                            Name = parts[1],
                            StartPosition = long.Parse(cmd[2])
                        };
                    });
                    OnChaptersChanged(chapters);
                    break;
                case "Subtitles":
                    var subtitles = cmd[1].Split(new[] { "]]" }, StringSplitOptions.None).Select(s => {
                        var parts = s.Split(new[] { ">>" }, StringSplitOptions.None);
                        return new SubtileTrack {
                            Index = int.Parse(parts[0]),
                            Description = parts[1],
                            Type = parts[2],
                            IsActive = bool.Parse(parts[3])
                        };
                    });
                    OnSubtitlesChanged(subtitles);
                    break;
                case "SubChanged":
                    OnActiveSubtitleChanged(cmd[1]);
                    break;
                case "AudioTracks":
                    var audioTracks = cmd[1].Split(new[] { "]]" }, StringSplitOptions.None).Select(s => {
                        var parts = s.Split(new[] { ">>" }, StringSplitOptions.None);
                        return new AudioTrack {
                            Index = int.Parse(parts[0]),
                            Description = parts[1],
                            Type = parts[2],
                            IsActive = bool.Parse(parts[3])
                        };
                    });
                    OnAudioTracksChanged(audioTracks);
                    break;
                case "AudioChanged":
                    OnActiveAudioTrackChanged(cmd[1]);
                    break;
            }
        }

        public event Action Exited;

        protected virtual void OnExited()
        {
            Action handler = Exited;
            if (handler != null) {
                handler();
            }
        }

        public event Action<string> Playing;

        protected virtual void OnPlaying(string obj)
        {
            Action<string> handler = Playing;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<string> Paused;

        protected virtual void OnPaused(string obj)
        {
            Action<string> handler = Paused;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<string> Stopped;

        protected virtual void OnStopped(string obj)
        {
            Action<string> handler = Stopped;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<string> Finished;

        protected virtual void OnFinished(string obj)
        {
            Action<string> handler = Finished;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<bool> Muted;

        protected virtual void OnMuted(bool obj)
        {
            Action<bool> handler = Muted;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<double> VolumeChanged;

        protected virtual void OnVolumeChanged(double obj)
        {
            Action<double> handler = VolumeChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<long> DurationChanged;

        protected virtual void OnDurationChanged(long obj)
        {
            Action<long> handler = DurationChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<long> ProgressChanged;

        protected virtual void OnProgressChanged(long obj)
        {
            Action<long> handler = ProgressChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<IEnumerable<SubtileTrack>> SubtitlesChanged;

        protected virtual void OnSubtitlesChanged(IEnumerable<SubtileTrack> obj)
        {
            Action<IEnumerable<SubtileTrack>> handler = SubtitlesChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<IEnumerable<AudioTrack>> AudioTracksChanged;

        protected virtual void OnAudioTracksChanged(IEnumerable<AudioTrack> obj)
        {
            Action<IEnumerable<AudioTrack>> handler = AudioTracksChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<IEnumerable<Chapter>> ChaptersChanged;

        protected virtual void OnChaptersChanged(IEnumerable<Chapter> obj)
        {
            Action<IEnumerable<Chapter>> handler = ChaptersChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<string> ActiveSubtitleChanged;

        protected virtual void OnActiveSubtitleChanged(string obj)
        {
            Action<string> handler = ActiveSubtitleChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public event Action<string> ActiveAudioTrackChanged;

        protected virtual void OnActiveAudioTrackChanged(string obj)
        {
            Action<string> handler = ActiveAudioTrackChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public struct SubtileTrack
        {
            public int Index { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public bool IsActive { get; set; }
        }

        public struct AudioTrack
        {
            public int Index { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public bool IsActive { get; set; }
        }

        public struct Chapter
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public long StartPosition { get; set; }
        }
        #endregion

        public void Dispose()
        {
            _running = false;
        }
    }
}
