using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Mpdn
{
    public class RemoteClient : IDisposable
    {
        public const string Guid = "0A235600-68B5-4C75-B73B-C2D5C463945D";

        private readonly Socket _socket;

        private StreamWriter _writer;
        private bool _running;

        public RemoteClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _running = true;
        }

        public static async  Task<RemoteClient> Connect(IPEndPoint endPoint)
        {
            var client = new RemoteClient();
            var tcs = new TaskCompletionSource<object>();

            Task.Run(() => client.Connect(endPoint, tcs));

            await tcs.Task;
            return client;
        }

        private async Task Connect(IPEndPoint endPoint, TaskCompletionSource<object> connectedTask)
        {
            try {
                _socket.Connect(endPoint);

                var stream = new NetworkStream(_socket);

                using (var reader = new StreamReader(stream))
                using (_writer = new StreamWriter(stream) { AutoFlush = true }) {
                    await _writer.WriteLineAsync(Guid);

                    while (_running) {
                        var data = await reader.ReadLineAsync();
                        HandleMessage(data, connectedTask);
                    }
                }
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }
            finally {
                _socket.Close();
            }
        }

        #region Sending

        public Task Open(string path)
        {
            return Write("Open|" + path);
        }

        public Task Play()
        {
            return Write("Play|false");
        }

        public Task Pause()
        {
            return Write("Pause|false");
        }

        public Task Stop()
        {
            return Write("Stop|unused");
        }

        public Task Seek(long position)
        {
            return Write("Seek|" + (position / 10));
        }

        public Task ChangeAudioTrack(string description)
        {
            return Write("ActiveAudioTrack|" + description);
        }

        public Task ChangeSubtitleTrack(string description)
        {
            return Write("ActiveSubtitleTrack|" + description);
        }

        public Task Mute(bool muted)
        {
            return Write("Mute|" + muted);
        }

        public Task ChangeVolume(decimal volume)
        {
            return Write("Volume|" + (int)volume);
        }

        public Task MoveWindow(int left, int top, int width, int height, WindowState state)
        {
            string stateString = null;
            switch (state) {
                case WindowState.Windowed:
                    stateString = "Normal";
                    break;
                case WindowState.Maximized:
                    stateString = "Maximized";
                    break;
                case WindowState.Minimized:
                    stateString = "Minimized";
                    break;
            }

            return Write(string.Format("MoveWindow|{0}>>{1}>>{2}>>{3}>>{4}", left, top, width, height, stateString));
        }

        private Task Write(string line)
        {
            Debug.WriteLine(line);
            return _writer.WriteLineAsync(line);
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
                case "Postion": // typo in the remote client
                    long progress;
                    if (long.TryParse(cmd[1], out progress)) {
                        OnProgressChanged(progress * 10);
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
                    int volume;
                    if (int.TryParse(cmd[1], out volume)) {
                        OnVolumeChanged(volume);
                    }
                    break;
                case "Chapters":
                    var chapters = cmd[1].Split(new[] { "]]" }, StringSplitOptions.None)
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .Select(s => {
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
                    var subtitles = cmd[1].Split(new[] { "]]" }, StringSplitOptions.None)
                                          .Where(s => !string.IsNullOrEmpty(s))
                                          .Select(s => {
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
                    var audioTracks = cmd[1].Split(new[] { "]]" }, StringSplitOptions.None)
                                            .Where(s => !string.IsNullOrEmpty(s))
                                            .Select(s => {
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

        public event Action<decimal> VolumeChanged;

        protected virtual void OnVolumeChanged(decimal obj)
        {
            Action<decimal> handler = VolumeChanged;
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

            public override string ToString()
            {
                return string.Format("Index: {0}, Description: \"{1}\", Type: {2}, IsActive: {3}", Index, Description, Type, IsActive);
            }

            public bool Equals(SubtileTrack other)
            {
                return Index == other.Index && string.Equals(Description, other.Description) && string.Equals(Type, other.Type);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                return obj is SubtileTrack && Equals((SubtileTrack) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    int hashCode = Index;
                    hashCode = (hashCode*397) ^ (Description != null ? Description.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Type != null ? Type.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public struct AudioTrack
        {
            public int Index { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public bool IsActive { get; set; }

            public override string ToString()
            {
                return string.Format("Index: {0}, Description: \"{1}\", Type: {2}, IsActive: {3}", Index, Description, Type, IsActive);
            }

            public bool Equals(AudioTrack other)
            {
                return Index == other.Index && string.Equals(Description, other.Description) && string.Equals(Type, other.Type);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                return obj is AudioTrack && Equals((AudioTrack) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    int hashCode = Index;
                    hashCode = (hashCode*397) ^ (Description != null ? Description.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Type != null ? Type.GetHashCode() : 0);
                    return hashCode;
                }
            }
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
