using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using MediaBrowser.ApiInteraction;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Session;
using MediaBrowser.Plugins.DefaultTheme.Home;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
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
using MediaBrowser.Theater.Presentation.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBoxIcon = MediaBrowser.Theater.Interfaces.Theming.MessageBoxIcon;
using System.Configuration;
using NavigationEventArgs = MediaBrowser.Theater.Interfaces.Navigation.NavigationEventArgs;

namespace MediaBrowser.UI.EntryPoints
{
    public class WebSocketEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly ApplicationHost _appHost;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navigationService;
        private readonly IPlaybackManager _playbackManager;
        private readonly IPresentationManager _presentationManager;
        private readonly ICommandManager _commandManager;
        private readonly IServerEvents _serverEvents;

        private bool _isDisposed;

        private ApiWebSocket ApiWebSocket
        {
            get { return _appHost.ApiWebSocket; }
        }

        public WebSocketEntryPoint(ISessionManager sessionManagerManager, IApiClient apiClient, ILogManager logManager, IApplicationHost appHost, IImageManager imageManager, INavigationService navigationService, IPlaybackManager playbackManager, IPresentationManager presentationManager, ICommandManager commandManager, IServerEvents serverEvents)
        {
            _sessionManager = sessionManagerManager;
            _apiClient = apiClient;
            _logger = logManager.GetLogger(GetType().Name);
            _appHost = (ApplicationHost)appHost;
            _imageManager = imageManager;
            _navigationService = navigationService;
            _playbackManager = playbackManager;
            _presentationManager = presentationManager;
            _commandManager = commandManager;
            _serverEvents = serverEvents;
        }

        public void Run()
        {
            _sessionManager.UserLoggedIn += SessionManagerUserLoggedIn;
            _apiClient.ServerLocationChanged += _apiClient_ServerLocationChanged;
            _navigationService.Navigated += NavigationServiceNavigated;

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

        void SessionManagerUserLoggedIn(object sender, EventArgs e)
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
            _presentationManager.ShowMessage(new MessageBoxInfo
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
              _presentationManager.Window.Dispatcher.Invoke(() =>
              {
                // _presentation.EnsureApplicationWindowHasFocus(); // todo - need this when testing and running on the same display and input, proably not in prod env
                var source = (HwndSource)PresentationSource.FromVisual(_presentationManager.Window);
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
     
        private string DictTpString(Dictionary<String, String> dict)
       {
           return string.Join(";", dict.Select(i => String.Format("{0}={1}", i.Key , i.Value)));
       }

        private ViewType MapContextToNavigateViewType(string context)
        {
            switch (context)
            {
                case "movies": 
                    return ViewType.Movies;
                    break;

                case "tv":
                    return ViewType.Tv;
                    break;

                case "music":
                    return ViewType.Music;

                case "folders":
                    return ViewType.Folders;
                    break;

                case "games":
                    return ViewType.Games;
                    break;


                default:
                    return ViewType.Home;
                    break;
            }
        }

        private Task<ItemsResult> GetMoviesByGenre(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Movie" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            var indexOption = viewModel.CurrentIndexOption;

            if (indexOption != null)
            {
                query.Genres = new[] { indexOption.Name };
            }

            return _apiClient.GetItemsAsync(query);
        }

        
        protected async Task<BaseItemDto> GetRootFolder()
        {
            return await _apiClient.GetRootFolderAsync(_apiClient.CurrentUserId);
        }

        private async Task NavigateToGenres(string itemType,  string pageTotle, Func<ItemListViewModel, DisplayPreferences, Task<ItemsResult>> query, string selectedGenre)
        {
            var item = await GetRootFolder();

            var displayPreferences = await _presentationManager.GetDisplayPreferences("MovieGenres", CancellationToken.None);

            var genres = await _apiClient.GetGenresAsync(new ItemsByNameQuery
            {
                IncludeItemTypes = new[] { itemType },
                SortBy = new[] { ItemSortBy.SortName },
                Recursive = true,
                UserId = _sessionManager.CurrentUser.Id
            });

            var indexOptions = genres.Items.Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name
            });

            var options = new ListPageConfig
            {
                IndexOptions = indexOptions.ToList(),
                IndexValue = selectedGenre,
                PageTitle = itemType,
                CustomItemQuery = query
            };

            options.DefaultViewType = ListViewTypes.PosterStrip;

            var page = new FolderPage(item, displayPreferences, _apiClient, _imageManager, _presentationManager, _navigationService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Movies
            };

            await _navigationService.Navigate(page);
        }
     

        private async void ExecuteDisplayContent(Dictionary<string, string> args)
        {
             _logger.Debug("ExecuteDisplayContent {0}", DictTpString(args));
            var itemId = args["ItemId"];
            if (! String.IsNullOrEmpty(itemId))
            {
                ViewType viewType = ViewType.Home;
               
                if (args.ContainsKey("Context"))
                {
                    viewType = MapContextToNavigateViewType(args["Context"]);
                }
               
                if (args.ContainsKey("ItemType") && args["ItemType"] == "Genre")
                {
                    var selectedGenre = args.ContainsKey("ItemName") ? args["ItemName"] : null;
                    await _presentationManager.Window.Dispatcher.Invoke(() => NavigateToGenres("Movie", "Movies", GetMoviesByGenre, selectedGenre));
                }
                else
                    await _navigationService.NavigateToItem(new BaseItemDto { Id = itemId }, viewType);
            }
        }

        void socket_GeneralCommand(object sender, GeneralCommandEventArgs e)
        {
            _logger.Debug("socket_GeneralCommand {0} {1}", e.KnownCommandType, e.Command.Arguments);

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
                        _commandManager.ExecuteCommand(Command.Null, null); //todo
                        break;

                    case GeneralCommandType.NextLetter:
                        _commandManager.ExecuteCommand(Command.Null, null); //todo
                        break;

                    case GeneralCommandType.ToggleOsd:
                        _commandManager.ExecuteCommand(Command.ToggleOsd, null); 
                        break;

                    case GeneralCommandType.ToggleContextMenu:
                        _commandManager.ExecuteCommand(Command.Null, null);        // todo
                        break;

                    case GeneralCommandType.Select:
                        SendKeyDownEventToFocusedElement(Key.Enter);
                        break;

                    case GeneralCommandType.Back:
                        _navigationService.NavigateBack();
                        break;

                    case GeneralCommandType.TakeScreenshot:
                        _commandManager.ExecuteCommand(Command.ScreenDump, null); 
                        break;

                    case GeneralCommandType.SendKey:
                        ExecuteSendSendKeyCommand(sender, e);  
                        break;


                    case GeneralCommandType.SendString:
                        ExecuteSendStringCommand(sender, e);
                        break;

                    case GeneralCommandType.GoHome:
                        _commandManager.ExecuteCommand(Command.GotoHome, null); 
                        break;

                    case GeneralCommandType.GoToSettings:
                        _commandManager.ExecuteCommand(Command.GotoSettings, null); 
                        break;

                    case GeneralCommandType.VolumeDown:
                        _commandManager.ExecuteCommand(Command.VolumeDown, null);
                        break;

                    case GeneralCommandType.VolumeUp:
                        _commandManager.ExecuteCommand(Command.VolumeUp, null);
                        break;

                    case GeneralCommandType.Mute:
                        _commandManager.ExecuteCommand(Command.Mute, null); 
                        break;

                    case GeneralCommandType.Unmute:
                        _commandManager.ExecuteCommand(Command.UnMute, null); 
                        break;

                    case GeneralCommandType.ToggleMute:
                        _commandManager.ExecuteCommand(Command.ToggleMute, null);
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

                    case GeneralCommandType.ToggleFullscreen:
                          _commandManager.ExecuteCommand(Command.ToggleFullScreen, null);
                        break;

                    case GeneralCommandType.DisplayContent:
                          ExecuteDisplayContent(e.Command.Arguments);
                        break;
                    default:
                        _logger.Warn("Unrecognized command: " + e.KnownCommandType.Value);
                        break;
                }
            }
        }

        async void _apiWebSocket_PlayCommand(object sender, PlayRequestEventArgs e)
        {
            _logger.Debug("_apiWebSocket_PlayCommand {0} {1}", e.Request.ItemIds, e.Request.StartPositionTicks);
            if (_sessionManager.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try
            {
                var result = await _apiClient.GetItemsAsync(new ItemQuery
                {
                    Ids = e.Request.ItemIds,
                    UserId = _sessionManager.CurrentUser.Id,

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
            _logger.Debug("_apiWebSocket_PlaystateCommand {0} {1}", e.Request.Command, e.Request.SeekPositionTicks);

            if (_sessionManager.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            var player = _playbackManager.MediaPlayers.FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player == null)
            {
                return;
            }

            var request = e.Request;

            switch (request.Command)
            {
                case PlaystateCommand.Pause:
                    _commandManager.ExecuteCommand(Command.Pause, null); 
                    break;
                case PlaystateCommand.Stop:
                    _commandManager.ExecuteCommand(Command.Stop, null); 
                    break;
                case PlaystateCommand.Unpause:
                    _commandManager.ExecuteCommand(Command.UnPause, null); 
                    break;
                case PlaystateCommand.Seek:
                    _commandManager.ExecuteCommand(Command.Seek, e.Request.SeekPositionTicks ?? 0);
                   
                    break;

                case PlaystateCommand.PreviousTrack:
                    _commandManager.ExecuteCommand(Command.PrevisousTrack, null);
                    break;

                case PlaystateCommand.NextTrack:
                    _commandManager.ExecuteCommand(Command.NextTrack, null);
                    break;

            }
        }

        void _apiWebSocket_UserUpdated(object sender, UserUpdatedEventArgs e)
        {
        }

        async void _apiWebSocket_UserDeleted(object sender, UserDeletedEventArgs e)
        {
            if (_sessionManager.CurrentUser != null && string.Equals(e.Id, _sessionManager.CurrentUser.Id))
            {
                await _sessionManager.Logout();
            }
        }

        async void _apiWebSocket_BrowseCommand(object sender, BrowseRequestEventArgs e)
        {
            if (_sessionManager.CurrentUser == null)
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
                await _navigationService.NavigateToItem(dto, viewType);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error processing browse command", ex);
            }
        }

        private void OnAnonymousRemoteControlCommand()
        {
            _logger.Error("Cannot process remote control command without a logged in user.");
            _presentationManager.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = "Error",
                Icon = MessageBoxIcon.Error,
                Text = "Please sign in before attempting to use remote control"
            });
        }

        async void NavigationServiceNavigated(object sender, NavigationEventArgs e)
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
            _sessionManager.UserLoggedIn -= SessionManagerUserLoggedIn;
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
