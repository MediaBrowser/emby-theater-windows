// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Mpdn.PlayerExtensions
{
    public class AcmPlug : PlayerExtension<RemoteControlSettings, RemoteControlConfig>
    {
        #region Variables
        private Socket _serverSocket;
        private readonly Dictionary<Guid, StreamWriter> _writers = new Dictionary<Guid, StreamWriter>();
        private readonly Dictionary<Guid, Socket> _clients = new Dictionary<Guid, Socket>();
        private readonly RemoteControl_AuthHandler _authHandler = new RemoteControl_AuthHandler();
        private RemoteClients _clientManager;
        private Timer _locationTimer;
        private Guid _playlistGuid = Guid.Parse("A1997E34-D67B-43BB-8FE6-55A71AE7184B");
        private Playlist.Playlist _playlistInstance;
        #endregion

        #region Properties
        public Dictionary<Guid, Socket> GetClients
        {
            get { return _clients; }
        }
        #endregion

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("C7FC1078-6471-409D-A2F1-34FF8903D6DA"),
                    Name = "Remote Control",
                    Description = "Remote Control extension to allow control of MPDN over the network.",
                    Copyright = "Copyright DeadlyEmbrace © 2015. All rights reserved."
                };
            }
        }


        protected override string ConfigFileName
        {
            get { return "Example.RemoteSettings"; }
        }

        public override void Destroy()
        {
            base.Destroy();
            if(Settings.IsActive)
                ShutdownServer();
        }

        public override void Initialize()
        {
            base.Initialize();
            Settings.PropertyChanged += Settings_PropertyChanged;
            if(Settings.IsActive)
                SetupServer();
                 
        }

        private void ShutdownServer()
        {
            Unsubscribe();
            _locationTimer.Stop();
            _locationTimer = null;
            foreach (var writer in _writers)
            {
                try
                {
                    writer.Value.WriteLine("Closing|Close");
                    writer.Value.Flush();
                }
                catch
                { }
            }
            if (_serverSocket != null)
                _serverSocket.Close();
        }

        private void SetupServer()
        {
            Subscribe();
            _locationTimer = new Timer(100);
            _locationTimer.Elapsed += _locationTimer_Elapsed;
            _clientManager = new RemoteClients(this);
            var playlist = PlayerControl.PlayerExtensions.FirstOrDefault(t => t.Descriptor.Guid == _playlistGuid);
            if (playlist != null)
            {
                _playlistInstance = playlist as Playlist.Playlist;
                if (_playlistInstance != null)
                {
                    _playlistInstance.GetPlaylistForm.VisibleChanged += GetPlaylistForm_VisibleChanged;
                    _playlistInstance.GetPlaylistForm.PlaylistChanged += GetPlaylistForm_PlaylistChanged;
                }
            }
            Task.Factory.StartNew(Server);   
        }

        void GetPlaylistForm_PlaylistChanged(object sender, EventArgs e)
        {
            GetPlaylist(Guid.Empty, true);
        }

        void GetPlaylistForm_VisibleChanged(object sender, EventArgs e)
        {
            PushToAllListeners("PlaylistShow|" + _playlistInstance.GetPlaylistForm.Visible);
        }


        private void Subscribe()
        {
            PlayerControl.PlaybackCompleted += m_PlayerControl_PlaybackCompleted;
            PlayerControl.PlayerStateChanged += m_PlayerControl_PlayerStateChanged;
            PlayerControl.EnteringFullScreenMode += m_PlayerControl_EnteringFullScreenMode;
            PlayerControl.ExitingFullScreenMode += m_PlayerControl_ExitingFullScreenMode;
            PlayerControl.VolumeChanged += PlayerControl_VolumeChanged;
            PlayerControl.SubtitleTrackChanged += PlayerControl_SubtitleTrackChanged;
            PlayerControl.AudioTrackChanged += PlayerControl_AudioTrackChanged;            
        }

        void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive" && Settings.IsActive)
            {
                SetupServer();
            }
            if (e.PropertyName == "IsActive" && !Settings.IsActive)
            {
                ShutdownServer();
            }
        }

        private void Unsubscribe()
        {
            PlayerControl.PlaybackCompleted -= m_PlayerControl_PlaybackCompleted;
            PlayerControl.PlayerStateChanged -= m_PlayerControl_PlayerStateChanged;
            PlayerControl.EnteringFullScreenMode -= m_PlayerControl_EnteringFullScreenMode;
            PlayerControl.ExitingFullScreenMode -= m_PlayerControl_ExitingFullScreenMode;
            PlayerControl.VolumeChanged -= PlayerControl_VolumeChanged;
            PlayerControl.SubtitleTrackChanged -= PlayerControl_SubtitleTrackChanged;
            PlayerControl.AudioTrackChanged -= PlayerControl_AudioTrackChanged;
        }

        void PlayerControl_AudioTrackChanged(object sender, EventArgs e)
        {
            PushToAllListeners("AudioChanged|" + PlayerControl.ActiveAudioTrack.Description);
        }

        void PlayerControl_SubtitleTrackChanged(object sender, EventArgs e)
        {
            PushToAllListeners("SubChanged|" + PlayerControl.ActiveSubtitleTrack.Description);
        }

        void PlayerControl_VolumeChanged(object sender, EventArgs e)
        {
            PushToAllListeners("Volume|" + PlayerControl.Volume.ToString());
            PushToAllListeners("Mute|" + PlayerControl.Mute.ToString());
        }

        void m_PlayerControl_ExitingFullScreenMode(object sender, EventArgs e)
        {
            PushToAllListeners("Fullscreen|False");
        }

        void m_PlayerControl_EnteringFullScreenMode(object sender, EventArgs e)
        {
            PushToAllListeners("Fullscreen|True");
        }


        void m_PlayerControl_PlayerStateChanged(object sender, PlayerStateEventArgs e)
        {
            switch (e.NewState)
            {
                case PlayerState.Playing:
                    _locationTimer.Start();
                    PushToAllListeners(GetAllChapters());
                    PushToAllListeners(GetAllSubtitleTracks());
                    PushToAllListeners(GetAllAudioTracks());
                    break;
                case PlayerState.Stopped:
                    _locationTimer.Stop();
                    break;
                case PlayerState.Paused:
                    _locationTimer.Stop();
                    break;
            }

            PushToAllListeners(e.NewState + "|" + PlayerControl.MediaFilePath);
        }

        private string GetAllAudioTracks()
        {
            if (PlayerControl.PlayerState == PlayerState.Playing || PlayerControl.PlayerState == PlayerState.Paused)
            {
                MediaTrack activeTrack = null;
                if (PlayerControl.ActiveAudioTrack != null)
                    activeTrack = PlayerControl.ActiveAudioTrack;
                var audioTracks = PlayerControl.AudioTracks;
                int counter = 1;
                StringBuilder audioStringBuilder = new StringBuilder();
                foreach (var track in audioTracks)
                {
                    if (counter > 1)
                        audioStringBuilder.Append("]]");
                    audioStringBuilder.Append(counter + ">>" + track.Description + ">>" + track.Type);
                    if (activeTrack != null && track.Description == activeTrack.Description)
                        audioStringBuilder.Append(">>True");
                    else
                        audioStringBuilder.Append(">>False");
                    counter++;
                }
                return "AudioTracks|" + audioStringBuilder;
            }
            else
            {
                return String.Empty;
            }
        }

        private string GetAllSubtitleTracks()
        {
            if (PlayerControl.PlayerState == PlayerState.Playing || PlayerControl.PlayerState == PlayerState.Paused)
            {
                MediaTrack activeSub = null;
                if (PlayerControl.ActiveSubtitleTrack != null)
                    activeSub = PlayerControl.ActiveSubtitleTrack;
                var subtitles = PlayerControl.SubtitleTracks;
                int counter = 1;
                StringBuilder subSb = new StringBuilder();
                foreach (var sub in subtitles)
                {
                    if (counter > 1)
                        subSb.Append("]]");
                    subSb.Append(counter + ">>" + sub.Description + ">>" + sub.Type);
                    if (activeSub != null && sub.Description == activeSub.Description)
                        subSb.Append(">>True");
                    else
                        subSb.Append(">>False");
                    counter++;
                }
                return "Subtitles|" + subSb;
            }
            else
            {
                return String.Empty;
            }
        }

        private string GetAllChapters()
        {
            if (PlayerControl.PlayerState == PlayerState.Playing || PlayerControl.PlayerState == PlayerState.Paused)
            {
                var chapters = PlayerControl.Chapters;
                int counter = 1;
                StringBuilder chapterSb = new StringBuilder();
                foreach (var chapter in chapters)
                {
                    if (counter > 1)
                        chapterSb.Append("]]");
                    chapterSb.Append(counter + ">>" + chapter.Name + ">>" + chapter.Position);
                    counter++;
                }
                return "Chapters|" + chapterSb;
            }
            else
            {
                return String.Empty;
            }
        }

        void m_PlayerControl_PlaybackCompleted(object sender, EventArgs e)
        {
            PushToAllListeners("Finished|" + PlayerControl.MediaFilePath);
        }

        public override IList<Verb> Verbs
        {
            get
            {
                return new[]
                {
                    new Verb(Category.Help, string.Empty, "Connected Clients", "Ctrl+Shift+R", "Show Remote Client connections", Test1Click),
                };
            }
        }

        private void Test1Click()
        {
            _clientManager.ShowDialog(PlayerControl.VideoPanel);
        }

        private void Server()
        {
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, Settings.ConnectionPort);
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(localEndpoint);
            _serverSocket.Listen(10);
            while (true)
            {
                try
                {
                    var clientSocket = _serverSocket.Accept();
                    Task.Factory.StartNew(() => ClientHandler(clientSocket));
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void ClientHandler(Socket client)
        {
            Guid clientGuid = Guid.NewGuid();
            _clients.Add(clientGuid, client);

            NetworkStream nStream = new NetworkStream(client);
            StreamReader reader = new StreamReader(nStream);
            StreamWriter writer = new StreamWriter(nStream);
            _writers.Add(clientGuid, writer);
            var clientGUID = reader.ReadLine();
            if (!_authHandler.IsGUIDAuthed(clientGUID) && Settings.ValidateClients)
            {
                ClientAuth(clientGUID.ToString(), clientGuid);
            }
            else
            {
                DisplayTextMessage("Remote Connected");
                WriteToSpesificClient("Connected|Authorized", clientGuid.ToString());
                WriteToSpesificClient("ClientGUID|" + clientGuid.ToString(), clientGuid.ToString());
                if (!_authHandler.IsGUIDAuthed(clientGUID))
                    _authHandler.AddAuthedClient(clientGUID);
                if (_clientManager.Visible)
                    _clientManager.ForceUpdate();
            }
            while (true)
            {
                try
                {
                    var data = reader.ReadLine();
                    if (data == "Exit")
                    {
                        HandleData(data);
                        client.Close();
                        break;
                    }
                    else
                    {
                        HandleData(data);
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private void ClientAuth(string msgValue, Guid clientGuid)
        {
            WriteToSpesificClient("AuthCode|" + msgValue, clientGuid.ToString());
            if (MessageBox.Show("Allow Remote Connection for " + msgValue, "Remote Authentication", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DisplayTextMessage("Remote Connected");
                WriteToSpesificClient("Connected|Authorized", clientGuid.ToString());
                _authHandler.AddAuthedClient(msgValue);
                if (_clientManager.Visible)
                    _clientManager.ForceUpdate();
            }
            else
            {
                DisconnectClient("Unauthorized", clientGuid);
            }
        }

        private void WriteToSpesificClient(string msg, string clientGuid)
        {
            Guid pushGuid;
            Guid.TryParse(clientGuid, out pushGuid);

            if (pushGuid != null)
            {
                if (_writers.ContainsKey(pushGuid))
                {
                    _writers[pushGuid].WriteLine(msg);
                    _writers[pushGuid].Flush();
                }
            }
        }

        private void DisconnectClient(string exitMessage, Guid clientGuid)
        {
            WriteToSpesificClient("Exit|" + exitMessage, clientGuid.ToString());

            _clients[clientGuid].Disconnect(true);
            RemoveWriter(clientGuid.ToString());
        }

        private void HandleData(string data)
        {
            var command = data.Split('|');
            switch (command[0])
            {
                case "Exit":
                    DisplayTextMessage("Remote Disconnected");
                    RemoveWriter(command[1]);
                    break;
                case "Open":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => OpenMedia(command[1])));
                    break;
                case "Pause":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => PauseMedia(command[1])));
                    break;
                case "Play":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => PlayMedia(command[1])));
                    break;
                case "Stop":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => StopMedia(command[1])));
                    break;
                case "Seek":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => SeekMedia(command[1])));
                    break;
                case "GetDuration":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => GetFullDuration(command[1])));
                    break;
                case "GetCurrentState":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => GetCurrentState(command[1])));
                    break;
                case "FullScreen":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => FullScreen(command[1])));
                    break;
                case "MoveWindow":
                    PlayerControl.Form.BeginInvoke((MethodInvoker)(() => MoveWindow(command[1])));
                    break;
                case "WriteToScreen":
                    DisplayTextMessage(command[1]);
                    break;
                case "Mute":
                    bool mute = false;
                    Boolean.TryParse(command[1], out mute);
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => Mute(mute)));
                    break;
                case "Volume":
                    int vol = 0;
                    int.TryParse(command[1], out vol);
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => SetVolume(vol)));
                    break;
                case "ActiveSubTrack":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => SetSubtitle(command[1])));
                    break;
                case "ActiveAudioTrack":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => SetAudioTrack(command[1])));
                    break;
                case "AddFilesToPlaylist":
                    AddFilesToPlaylist(command[1]);
                    break;
                case "InsertFileInPlaylist":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => InsertIntoPlaylist(command[1], command[2])));
                    break;
                case "ClearPlaylist":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(ClearPlaylist));
                    break;
                case "FocusPlayer":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(FocusMpdn));
                    break;
                case "PlayNext":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)PlaylistPlayNext);
                    break;
                case "PlayPrevious":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)PlaylistPlayPrevious);
                    break;
                case "ShowPlaylist":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)ShowPlaylist);
                    break;
                case "HidePlaylist":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)HidePlaylist);
                    break;
                case "GetPlaylist":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => GetPlaylist(command[1])));
                    break;
                case "PlaySelectedFile":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => PlaySelectedFile(command[1])));
                    break;
                case "RemoveFile":
                    PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => RemoveFromPlaylist(command[1])));
                    break;
            }
        }

        private void InsertIntoPlaylist(string fileIndex, string filePath)
        {
            int index;
            int.TryParse(fileIndex, out index);
            _playlistInstance.GetPlaylistForm.InsertFile(index, filePath);
        }

        private void RemoveFromPlaylist(string fileIndex)
        {
            int index;
            int.TryParse(fileIndex, out index);
            _playlistInstance.GetPlaylistForm.RemoveFile(index);
        }

        private void PlaySelectedFile(string fileIndex)
        {
            int myIndex;
            int.TryParse(fileIndex, out myIndex);
            _playlistInstance.GetPlaylistForm.SetPlaylistIndex(myIndex);
        }

        private void GetPlaylist(object guid, bool notify = false)
        {
            int counter = 0;
            StringBuilder sb = new StringBuilder();
            var fullPlaylist = _playlistInstance.GetPlaylistForm.Playlist;
            foreach (var item in fullPlaylist)
            {
                counter++;
                if (counter > 1)
                    sb.Append(">>");
                sb.Append(item.FilePath + "]]" + item.Active);

            }
            if (!notify)
            {
                WriteToSpesificClient("PlaylistContent|" + sb, guid.ToString());
            }
            else
            {
                PushToAllListeners("PlaylistContent|" + sb);
            }
        }

        private void HidePlaylist()
        {
            _playlistInstance.GetPlaylistForm.Hide();
        }

        private void ShowPlaylist()
        {
            _playlistInstance.GetPlaylistForm.Show();
        }

        private void PlaylistPlayNext()
        {
            _playlistInstance.GetPlaylistForm.PlayNext();
        }

        private void PlaylistPlayPrevious()
        {
            _playlistInstance.GetPlaylistForm.PlayPrevious();
        }

        private void FocusMpdn()
        {
            PlayerControl.Form.Focus();
        }

        private void ClearPlaylist()
        {
            _playlistInstance.GetPlaylistForm.ClearPlaylist();
        }

        private void AddFilesToPlaylist(string files)
        {
            List<string> filesToAdd = new List<string>();
            var filePaths = Regex.Split(files, ">>");
            if (filePaths.Any())
            {
                foreach (var file in filePaths)
                {
                    if (File.Exists(file))
                    {
                        filesToAdd.Add(file);
                    }
                }
            }
            if (filesToAdd.Any())
            {
                PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => _playlistInstance.GetPlaylistForm.AddFiles(filesToAdd.ToArray())));
            }
        }

        private void RemoveWriter(string guid)
        {
            Guid callerGuid = Guid.Parse(guid);
            _writers.Remove(callerGuid);
            _clients.Remove(callerGuid);
            _clientManager.ForceUpdate();
        }

        private void OpenMedia(object file)
        {
            PlayerControl.OpenMedia(file.ToString());
        }

        private void PauseMedia(object showOsd)
        {
            bool dispOsd = false;
            Boolean.TryParse(showOsd.ToString(), out dispOsd);
            PlayerControl.PauseMedia(dispOsd);
        }

        private void PlayMedia(object showOsd)
        {
            bool dispOsd = false;
            Boolean.TryParse(showOsd.ToString(), out dispOsd);
            if (!String.IsNullOrEmpty(PlayerControl.MediaFilePath))
            {
                PlayerControl.PlayMedia(dispOsd);
            }
            else
            {
                _playlistInstance.GetPlaylistForm.PlayActive();
            }

        }

        private void StopMedia(object blank)
        {
            PlayerControl.StopMedia();
        }

        private void SeekMedia(object seekLocation)
        {
            double location = -1;
            double.TryParse(seekLocation.ToString(), out location);
            if (location != -1)
            {
                PlayerControl.SeekMedia((long)location);
            }
        }

        private void SetVolume(int level)
        {
            PlayerControl.Volume = level;
        }

        private void SetSubtitle(string subDescription)
        {
            var selTrack = PlayerControl.SubtitleTracks.FirstOrDefault(t => t.Description == subDescription);
            if (selTrack != null)
                PlayerControl.SelectSubtitleTrack(selTrack);
        }

        private void SetAudioTrack(string audioDescription)
        {
            var selTrack = PlayerControl.AudioTracks.FirstOrDefault(t => t.Description == audioDescription);
            if (selTrack != null)
                PlayerControl.SelectAudioTrack(selTrack);
        }

        private void Mute(bool silence)
        {
            PlayerControl.Mute = silence;
            PushToAllListeners("Mute|" + silence);
        }

        private void GetFullDuration(object guid)
        {
            WriteToSpesificClient("FullLength|" + PlayerControl.MediaDuration, guid.ToString());
        }

        private void GetCurrentState(object guid)
        {
            WriteToSpesificClient(GetAllChapters(), guid.ToString());
            WriteToSpesificClient(PlayerControl.PlayerState + "|" + PlayerControl.MediaFilePath, guid.ToString());
            WriteToSpesificClient("Fullscreen|" + PlayerControl.InFullScreenMode, guid.ToString());
            WriteToSpesificClient("Mute|" + PlayerControl.Mute, guid.ToString());
            WriteToSpesificClient("Volume|" + PlayerControl.Volume, guid.ToString());
            GetPlaylist(guid);
            if (PlayerControl.PlayerState == PlayerState.Playing || PlayerControl.PlayerState == PlayerState.Paused)
            {
                WriteToSpesificClient("FullLength|" + PlayerControl.MediaDuration, guid.ToString());
                WriteToSpesificClient("Position|" + PlayerControl.MediaPosition, guid.ToString());
            }
            if (_playlistInstance != null)
            {
                PushToAllListeners("PlaylistShow|" + _playlistInstance.GetPlaylistForm.Visible);
            }
            WriteToSpesificClient(GetAllSubtitleTracks(), guid.ToString());
            WriteToSpesificClient(GetAllAudioTracks(), guid.ToString());
        }

        private void FullScreen(object fullScreen)
        {
            bool goFullscreen = false;
            Boolean.TryParse(fullScreen.ToString(), out goFullscreen);
            if (goFullscreen)
            {
                PlayerControl.GoFullScreen();
            }
            else
            {
                PlayerControl.GoWindowed();
            }
        }

        private void MoveWindow(string msg)
        {
            var args = msg.Split(new[] {">>"}, StringSplitOptions.None);

            int left, top, width, height;
            if (int.TryParse(args[0], out left) &&
                int.TryParse(args[1], out top) &&
                int.TryParse(args[2], out width) &&
                int.TryParse(args[3], out height))
            {
                PlayerControl.Form.Left = left;
                PlayerControl.Form.Top = top;
                PlayerControl.Form.Width = width;
                PlayerControl.Form.Height = height;

                switch (args[4])
                {
                    case "Normal":
                        PlayerControl.Form.WindowState = FormWindowState.Normal;
                        break;
                    case "Maximized":
                        PlayerControl.Form.WindowState = FormWindowState.Maximized;
                        break;
                    case "Minimized":
                        PlayerControl.Form.WindowState = FormWindowState.Minimized;
                        break;
                }
            }
        }

        private void PushToAllListeners(string msg)
        {
            foreach (var writer in _writers)
            {
                try
                {
                    writer.Value.WriteLine(msg);
                    writer.Value.Flush();
                }
                catch
                { }
            }
        }

        private void DisplayTextMessage(object msg)
        {
            PlayerControl.VideoPanel.BeginInvoke((MethodInvoker)(() => PlayerControl.ShowOsdText(msg.ToString())));
        }

        public void DisconnectClient(string guid)
        {
            Guid clientGuid;
            Guid.TryParse(guid, out clientGuid);
            DisconnectClient("Disconnected by User", clientGuid);
        }

        void _locationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                PushToAllListeners("Postion|" + PlayerControl.MediaPosition);
            }
            catch (Exception)
            { }
        }
    }

    public class RemoteControlSettings : INotifyPropertyChanged
    {
        #region Variables

        private int _connectionPort;
        private bool _validateClients;
        private bool _isActive;

        #endregion

        #region Properties

        public int ConnectionPort
        {
            get
            {
                return _connectionPort;
                
            }
            set
            {
                _connectionPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ConnectionPort"));
            }
        }

        public bool ValidateClients
        {
            get
            {
              return _validateClients;  
            }
            set
            {
                _validateClients = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ValidateClients"));
            }
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsActive"));
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Methods

        public RemoteControlSettings()
        {
            ConnectionPort = 6545;
            ValidateClients = true;
        }

        #endregion

        #region Private Methods

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }
}
