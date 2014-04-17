using System.Collections.Generic;
using System.Media;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Interfaces.Commands;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBoxIcon = MediaBrowser.Theater.Interfaces.Theming.MessageBoxIcon;

namespace MediaBrowser.UI.EntryPoints
{
    public class WebSocketEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;
        private readonly ApplicationHost _appHost;
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playbackManager;
        private readonly IPresentationManager _presentation;
        private readonly ICommandManager _commandManager;

        private bool _isDisposed;

        private ApiWebSocket ApiWebSocket
        {
            get { return _appHost.ApiWebSocket; }
        }

        public WebSocketEntryPoint(ISessionManager session, IApiClient apiClient, IJsonSerializer json, ILogManager logManager, IApplicationHost appHost, INavigationService nav, IPlaybackManager playbackManager, IPresentationManager presentation, ICommandManager commandManager)
        {
            _session = session;
            _apiClient = apiClient;
            _json = json;
            _logger = logManager.GetLogger(GetType().Name);
            _appHost = (ApplicationHost)appHost;
            _nav = nav;
            _playbackManager = playbackManager;
            _presentation = presentation;
            _commandManager = commandManager;
        }

        public void Run()
        {
            _session.UserLoggedIn += _session_UserLoggedIn;
            _apiClient.ServerLocationChanged += _apiClient_ServerLocationChanged;
            _nav.Navigated += _nav_Navigated;

            var socket = ApiWebSocket;

            socket.BrowseCommand += _apiWebSocket_BrowseCommand;
            socket.UserDeleted += _apiWebSocket_UserDeleted;
            socket.UserUpdated += _apiWebSocket_UserUpdated;
            socket.PlaystateCommand += _apiWebSocket_PlaystateCommand;
            socket.GeneralCommand += socket_GeneralCommand;
            socket.MessageCommand += socket_MessageCommand;
            socket.PlayCommand += _apiWebSocket_PlayCommand;
            socket.Closed += socket_Closed;
            socket.Connected += socket_Connected;

            UpdateServerLocation();
        }

        void socket_Connected(object sender, EventArgs e)
        {
            ReportCapabilities(CancellationToken.None);
        }

        private async void ReportCapabilities(CancellationToken cancellationToken)
        {
            try
            {
                await _apiClient.ReportCapabilities(new ClientCapabilities
                {

                    PlayableMediaTypes = new List<string>
                    {
                        MediaType.Audio,
                        MediaType.Video,
                        MediaType.Game,
                        MediaType.Photo,
                        MediaType.Book
                    },

                    // MBT should be able to implement them all
                    SupportedCommands = Enum.GetNames(typeof(GeneralCommandType)).ToList()

                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error reporting capabilities", ex);
            }
        }

        void _apiClient_ServerLocationChanged(object sender, EventArgs e)
        {
            UpdateServerLocation();
        }

        private async void UpdateServerLocation()
        {
            try
            {
                var systemInfo = await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);

                var socket = ApiWebSocket;

                await socket.ChangeServerLocation(_apiClient.ServerHostName, systemInfo.WebSocketPortNumber, CancellationToken.None).ConfigureAwait(false);

                socket.StartEnsureConnectionTimer(5000);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error establishing web socket connection", ex);
            }
        }

        void _session_UserLoggedIn(object sender, EventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            EnsureWebSocket();
        }

        private void EnsureWebSocket()
        {
            ApiWebSocket.EnsureConnectionAsync(CancellationToken.None);
        }

        void socket_Closed(object sender, EventArgs e)
        {
            EnsureWebSocket();
        }

        void socket_MessageCommand(object sender, MessageCommandEventArgs e)
        {
            _presentation.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = e.Request.Header,
                Text = e.Request.Text,
                TimeoutMs = Convert.ToInt32(e.Request.TimeoutMs ?? 0)
            });
        }

        void ExecuteSetVolumeCommand(object sender, GeneralCommandEventArgs e)
        {
            _logger.Debug("ExecuteSetVolumeCommand {0}", e.Command.Arguments != null && e.Command.Arguments.Count > 0 ? e.Command.Arguments.First().Value : null);

            float volume;

            if (e.Command.Arguments == null || e.Command.Arguments.Count() != 1)
                throw new ArgumentException("ExecuteSetVolumeCommand: expecting a single float 0..100 argurment for ExecuteSetVolumeCommand");

            try
            {
                volume = (float) Convert.ToDouble(e.Command.Arguments.First().Value);
            }
            catch (FormatException)
            {
                throw new ArgumentException("ExecuteSetVolumeCommand: Invalid format, expecting a single float 0..100 argurment for ExecuteSetVolumeCommand");
            }

            if (volume < 0.0 || volume  > 100.0)
            {
                throw new ArgumentException(string.Format("ExecuteSetVolumeCommand: Invalid Volume {0}. Volume range is 0..100", volume));
            }
            
            _playbackManager.SetVolume(volume);
        }

        void ExecuteSetAudioStreamIndex(object sender, GeneralCommandEventArgs e)
        {
            _logger.Debug("ExecuteSetAudioStreamIndex {0}", e.Command.Arguments != null && e.Command.Arguments.Count > 0 ? e.Command.Arguments.First().Value : null);

            int index;

            if (e.Command.Arguments == null || e.Command.Arguments.Count() != 1)
                throw new ArgumentException("ExecuteSetAudioStreamIndex: expecting a single integer argurment for AudiostreamIndex");

            try
            {
                index = Convert.ToInt32(e.Command.Arguments.First().Value);
            }
            catch (FormatException)
            {
                throw new ArgumentException("ExecuteSetAudioStreamIndex: Invalid format, expecting a single integer argurment for AudiostreamIndex");
            }
            

            _playbackManager.SetAudioStreamIndex(index);
        }

        void ExecuteSetSubtitleStreamIndex(object sender, GeneralCommandEventArgs e)
        {
            _logger.Debug("ExecuteSetSubtitleStreamIndex {0}", e.Command.Arguments != null && e.Command.Arguments.Count > 0 ? e.Command.Arguments.First().Value : null);

            int index;

            if (e.Command.Arguments == null || e.Command.Arguments.Count() != 1)
                throw new ArgumentException("ExecuteSetSubtitleStreamIndex: expecting a single integer argurment for SubtitleStreamIndex");

            try
            {
                index = Convert.ToInt32(e.Command.Arguments.First().Value);
            }
            catch (FormatException)
            {
                throw new ArgumentException("ExecuteSetSubtitleStreamIndex: Invalid format, expecting a single integer argurment for SubtitleStreamIndex");
            }
            
          _playbackManager.SetSubtitleStreamIndex(index);
          
        }

        //
        // see http://stackoverflow.com/questions/1645815/how-can-i-programmatically-generate-keypress-events-in-c
        // Send a Key to currently active element, 
        //      1. the element will in receive a KeyDown event which will 
        //      2. The elemen will implement the actions - i.e move 
        //      3. The Keydown will be picked up by the input manager and the generate a Command matching the key
        //
        void SendKeyDownEventToFocusedElement(System.Windows.Input.Key key)
        {
            _logger.Debug("SendKeyDownEventToFocusedElement {0}", key);
              _presentation.Window.Dispatcher.Invoke(() =>
              {
                // _presentation.EnsureApplicationWindowHasFocus(); // todo - need this when testing and running on the same display and input, proably not in prod env
                var source = (HwndSource)PresentationSource.FromVisual(_presentation.Window);
                var keyEventArgs = new KeyEventArgs
                                    (
                                        Keyboard.PrimaryDevice,
                                        source,
                                        0,
                                        key
                                    ) { RoutedEvent = Keyboard.KeyDownEvent };

                InputManager.Current.ProcessInput(keyEventArgs);
             });
        }

        //
        //
        // Send a text string to currently active element, 
        //
        void SendTextInputToFocusedElement(string inputText)
        {
            _logger.Debug("SendTextInputToFocusedElement {0}", inputText);
            SendKeys.SendWait(inputText);
        }

        void ExecuteSendStringCommand(object sender, GeneralCommandEventArgs e)
        {
            _logger.Debug("ExecuteSendStringCommand {0}", e.Command.Arguments != null && e.Command.Arguments.Count > 0 ? e.Command.Arguments.First().Value : null);
            if (e.Command.Arguments == null || e.Command.Arguments.Count() != 1)
                throw new ArgumentException("ExecuteSendStringCommand: expecting a single string argurment for send string");

          
             string inputText = e.Command.Arguments.First().Value;

             SendTextInputToFocusedElement(inputText);
        }

       private bool IsWindowsKeyEnum(int input, out System.Windows.Input.Key key)
       {
           key = (System.Windows.Input.Key) input;
           return System.Enum.IsDefined(typeof(System.Windows.Input.Key), key);
       }

       void ExecuteSendSendKeyCommand(object sender, GeneralCommandEventArgs e)
       {
           _logger.Debug("ExecuteSendStringCommand {0}", e.Command.Arguments != null && e.Command.Arguments.Count > 0 ? e.Command.Arguments.First().Value : null);
           if (e.Command.Arguments == null || e.Command.Arguments.Count() != 1)
               throw new ArgumentException("ExecuteSendStringCommand: expecting a single string argurment for send Key");

           var input = e.Command.Arguments.First().Value;

           // now the key can be
           // 1. An integer value of the  System.Windows.Input.Key enum, 0 to 172
           // 2. The text representation of System.Windows.Input.Key ie. Key.None..Key.DeadCharProcessed
           // 3. A single Char value

           // try int first
           int intVal;
           if (int.TryParse(input, out intVal))
           {
               System.Windows.Input.Key key;

               if (!IsWindowsKeyEnum(intVal, out key))
                   throw new ArgumentException(String.Format("ExecuteSendStringCommand: integer argument {0} does not map to  System.Windows.Input.Key", intVal));

               SendKeyDownEventToFocusedElement(key);
           }
           else if (input.StartsWith("Key."))  // check if the string maps to a enum element name i.e Key.A
           {
               System.Windows.Input.Key key;
               try
               {
                   key  = (System.Windows.Input.Key) System.Enum.Parse(typeof(System.Windows.Input.Key), input);
               }
               catch (Exception)
               {
                   throw new ArgumentException(String.Format("ExecuteSendStringCommand: Argument '{0}' must be a Single Char or a Windows Key literial (i.e Key.A)  or the Integer value for Key literial", input));
               }
               
               SendKeyDownEventToFocusedElement(key);
           }
           else if (input.Length == 1)
           {
               SendTextInputToFocusedElement(input);
           }
           else
           {
               throw new ArgumentException(String.Format("ExecuteSendStringCommand: Argument '{0}' must be a Single Char or a Windows Key literial (i.e Key.A)  or the Integer value for Key literial", input));
           }
       }



        void socket_GeneralCommand(object sender, GeneralCommandEventArgs e)
        {
            if (e.KnownCommandType.HasValue)
            {
                switch (e.KnownCommandType.Value)
                {
                    case GeneralCommandType.MoveUp:
                        SendKeyDownEventToFocusedElement(Key.Up);
                        break;

                    case GeneralCommandType.MoveDown:
                        SendKeyDownEventToFocusedElement(Key.Down);
                        break;

                    case GeneralCommandType.MoveLeft:
                         SendKeyDownEventToFocusedElement(Key.Left);
                        break;

                    case GeneralCommandType.MoveRight:
                         SendKeyDownEventToFocusedElement(Key.Right);
                        break;

                    case GeneralCommandType.PageUp:
                        SendKeyDownEventToFocusedElement(Key.PageUp);
                        break;

                    case GeneralCommandType.PreviousLetter:
                        _commandManager.SendCommand(Command.Null, null); //todo
                        break;

                    case GeneralCommandType.NextLetter:
                        _commandManager.SendCommand(Command.Null, null); //todo
                        break;

                    case GeneralCommandType.ToggleOsd:
                        _commandManager.SendCommand(Command.ToggleOsd, null); 
                        break;

                    case GeneralCommandType.ToggleContextMenu:
                        _commandManager.SendCommand(Command.Null, null);        // todo
                        break;

                    case GeneralCommandType.Select:
                        SendKeyDownEventToFocusedElement(Key.Enter);
                        break;

                    case GeneralCommandType.Back:
                        _nav.NavigateBack();
                        break;

                    case GeneralCommandType.TakeScreenshot:
                        _commandManager.SendCommand(Command.Null, null);        // todo
                        break;

                    case GeneralCommandType.SendKey:
                        ExecuteSendSendKeyCommand(sender, e);  
                        break;


                    case GeneralCommandType.SendString:
                        ExecuteSendStringCommand(sender, e);
                        break;

                    case GeneralCommandType.GoHome:
                        _nav.NavigateToHomePage();
                        break;
                    case GeneralCommandType.GoToSettings:
                        _nav.NavigateToSettingsPage();
                        break;
                    case GeneralCommandType.VolumeDown:
                        _playbackManager.VolumeStepDown();
                        break;
                    case GeneralCommandType.VolumeUp:
                        _playbackManager.VolumeStepUp();
                        break;
                    case GeneralCommandType.Mute:
                        _playbackManager.Mute();
                        break;
                    case GeneralCommandType.Unmute:
                        _playbackManager.UnMute();
                        break;
                    case GeneralCommandType.ToggleMute:
                        if (_playbackManager.IsMuted)
                        {
                            _playbackManager.UnMute();
                        }
                        else
                        {
                            _playbackManager.Mute();
                        }
                        break;
                    case GeneralCommandType.SetVolume:
                        ExecuteSetVolumeCommand(sender, e);
                        break;
                    case GeneralCommandType.SetAudioStreamIndex:
                        ExecuteSetAudioStreamIndex(sender, e);
                        break;
                    case GeneralCommandType.SetSubtitleStreamIndex:
                        ExecuteSetSubtitleStreamIndex(sender, e);
                        break;
                    default:
                        _logger.Warn("Unrecognized command: " + e.KnownCommandType.Value);
                        break;
                }
            }
        }

        async void _apiWebSocket_PlayCommand(object sender, PlayRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try
            {
                var result = await _apiClient.GetItemsAsync(new ItemQuery
                {
                    Ids = e.Request.ItemIds,
                    UserId = _session.CurrentUser.Id,

                    Fields = new[]
                    {
                        ItemFields.Chapters, 
                        ItemFields.MediaStreams, 
                        ItemFields.Overview,
                        ItemFields.Path,
                        ItemFields.People,
                    }
                });

                await _playbackManager.Play(new PlayOptions
                {
                    StartPositionTicks = e.Request.StartPositionTicks ?? 0,
                    GoFullScreen = true,
                    Items = result.Items.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error processing play command", ex);
            }
        }

        void _apiWebSocket_PlaystateCommand(object sender, PlaystateRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            var player = _playbackManager.MediaPlayers
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player == null)
            {
                return;
            }

            var request = e.Request;

            switch (request.Command)
            {
                case PlaystateCommand.Pause:
                    player.Pause();
                    break;
                case PlaystateCommand.Stop:
                    player.Stop();
                    break;
                case PlaystateCommand.Unpause:
                    player.UnPause();
                    break;
                case PlaystateCommand.Seek:
                    player.Seek(e.Request.SeekPositionTicks ?? 0);
                    break;
                case PlaystateCommand.PreviousTrack:
                    {
                        player.GoToPreviousTrack();
                        break;
                    }
                case PlaystateCommand.NextTrack:
                    {
                        player.GoToNextTrack();
                        break;
                    }
            }
        }

        void _apiWebSocket_UserUpdated(object sender, UserUpdatedEventArgs e)
        {
        }

        async void _apiWebSocket_UserDeleted(object sender, UserDeletedEventArgs e)
        {
            if (_session.CurrentUser != null && string.Equals(e.Id, _session.CurrentUser.Id))
            {
                await _session.Logout();
            }
        }

        async void _apiWebSocket_BrowseCommand(object sender, BrowseRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try
            {
                var dto = new BaseItemDto
                {
                    Name = e.Request.ItemName,
                    Type = e.Request.ItemType,
                    Id = e.Request.ItemId
                };

                var viewType = ViewType.Folders;

                if (!string.IsNullOrEmpty(e.Request.Context))
                {
                    Enum.TryParse(e.Request.Context, true, out viewType);
                }
                await _nav.NavigateToItem(dto, viewType);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error processing browse command", ex);
            }
        }

        private void OnAnonymousRemoteControlCommand()
        {
            _logger.Error("Cannot process remote control command without a logged in user.");
            _presentation.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = "Error",
                Icon = MessageBoxIcon.Error,
                Text = "Please sign in before attempting to use remote control"
            });
        }

        async void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            var itemPage = e.NewPage as IItemPage;

            if (itemPage != null)
            {
                var item = itemPage.PageItem;

                try
                {
                    await ApiWebSocket.SendContextMessageAsync(item.Type, item.Id, item.Name, itemPage.ViewType.ToString(), CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending context message", ex);
                }
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _session.UserLoggedIn -= _session_UserLoggedIn;
            DisposeSocket();
        }

        private void DisposeSocket()
        {
            var socket = ApiWebSocket;

            if (socket != null)
            {
                socket.BrowseCommand -= _apiWebSocket_BrowseCommand;
                socket.UserDeleted -= _apiWebSocket_UserDeleted;
                socket.UserUpdated -= _apiWebSocket_UserUpdated;
                socket.PlaystateCommand -= _apiWebSocket_PlaystateCommand;
                socket.GeneralCommand -= socket_GeneralCommand;
                socket.MessageCommand -= socket_MessageCommand;
                socket.PlayCommand -= _apiWebSocket_PlayCommand;
                socket.Closed -= socket_Closed;
                socket.Connected -= socket_Connected;

                socket.Dispose();
            }
        }
    }
}
