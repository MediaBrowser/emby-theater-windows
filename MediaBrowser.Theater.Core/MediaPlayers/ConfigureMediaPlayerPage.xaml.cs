using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Core.MediaPlayers
{
    /// <summary>
    /// Interaction logic for ConfigureMediaPlayerPage.xaml
    /// </summary>
    public partial class ConfigureMediaPlayerPage : BasePage
    {
        private readonly PlayerConfiguration _playerConfig;
        private readonly bool _isNew;

        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IApiClient _apiClient;

        private List<GameSystemSummary> _gameSystems;

        public ConfigureMediaPlayerPage(IPlaybackManager playbackManager, ITheaterConfigurationManager config, IPresentationManager presentation, INavigationService nav, IApiClient apiClient)
            : this(new PlayerConfiguration(), playbackManager, config, presentation, nav, apiClient)
        {
            _isNew = true;
        }

        public ConfigureMediaPlayerPage(PlayerConfiguration playerConfig, IPlaybackManager playbackManager, ITheaterConfigurationManager config, IPresentationManager presentation, INavigationService nav, IApiClient apiClient)
        {
            _playerConfig = playerConfig;
            _playbackManager = playbackManager;
            _config = config;
            _presentation = presentation;
            _nav = nav;
            _apiClient = apiClient;

            InitializeComponent();
        }

        private IEnumerable<IConfigurableMediaPlayer> ConfigurablePlayers
        {
            get { return _playbackManager.MediaPlayers.OfType<IConfigurableMediaPlayer>(); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var mediaTypes = new[] { MediaType.Video, MediaType.Audio, MediaType.Game, MediaType.Book };

            SelectMediaType.Options = mediaTypes.Select(i => new SelectListItem
            {
                Text = i,
                Value = i

            }).ToList();

            SelectMediaType.SelectedValue = SelectMediaType.Options[0].Value;

            SelectIsoSupport.Options = new List<SelectListItem>
                {
                    new SelectListItem{ Text = "None", Value = IsoConfiguration.None.ToString()},
                    new SelectListItem{ Text = "Mount", Value = IsoConfiguration.Mount.ToString()},
                    new SelectListItem{ Text = "Pass directly to player", Value = IsoConfiguration.PassThrough.ToString()}
                };

            Loaded += ConfigureMediaPlayerPage_Loaded;
            SelectPlayer.SelectedItemChanged += SelectPlayer_SelectedItemChanged;
            SelectMediaType.SelectedItemChanged += SelectMediaType_SelectedItemChanged;
            SelectGameSystem.SelectedItemChanged += SelectGameSystem_SelectedItemChanged;
            BtnSubmit.Click += BtnSubmit_Click;
        }

        void SelectGameSystem_SelectedItemChanged(object sender, EventArgs e)
        {
            if (_gameSystems == null)
            {
                return;
            }

            var system = _gameSystems.FirstOrDefault(i => string.Equals(i.Name, SelectGameSystem.SelectedValue));

            if (system == null)
            {
                return;
            }

            PanelIsoSupport.Visibility = system.GameFileExtensions.Contains(".iso", StringComparer.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;

            UpdateFileExtensions();
        }

        private async Task LoadGameSystems()
        {
            _presentation.ShowLoadingAnimation();

            try
            {
                var systems = await _apiClient.GetGameSystemSummariesAsync(CancellationToken.None);

                _gameSystems = systems.Where(i => i.GameFileExtensions.Count > 0).OrderBy(i => i.DisplayName).ToList();

                SelectGameSystem.Options = _gameSystems.Select(i => new SelectListItem
                {
                    Text = i.DisplayName,
                    Value = i.Name

                }).ToList();

                SelectGameSystem.SelectedValue = _gameSystems[0].Name;

                BtnSubmit.Visibility = Visibility.Visible;
                _presentation.HideLoadingAnimation();
            }
            catch
            {
                _presentation.HideLoadingAnimation();
                _presentation.ShowDefaultErrorMessage();

                BtnSubmit.Visibility = Visibility.Collapsed;
            }
        }

        void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                Save();
            }
        }

        private bool ValidateInput()
        {
            if (PanelPlayerPath.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(TxtPath.Text) || !File.Exists(TxtPath.Text))
                {
                    _presentation.ShowMessage(new MessageBoxInfo
                    {
                        Button = MessageBoxButton.OK,
                        Caption = "Error",
                        Icon = MessageBoxIcon.Error,
                        Text = "Please enter a valid player path."
                    });

                    TxtPath.Focus();

                    return false;
                }
            }

            if (PanelPlayerArgs.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(TxtArgs.Text))
                {
                    _presentation.ShowMessage(new MessageBoxInfo
                    {
                        Button = MessageBoxButton.OK,
                        Caption = "Error",
                        Icon = MessageBoxIcon.Error,
                        Text = "Please enter command line arguments."
                    });

                    TxtArgs.Focus();

                    return false;
                }
            }

            return true;
        }

        private async void Save()
        {
            _playerConfig.PlayerName = SelectedPlayer.Name;

            _playerConfig.Play3DVideo = ChkPlay3DVideo.IsChecked ?? false;

            _playerConfig.PlayBluray = ChkBluray.IsChecked ?? false;
            _playerConfig.PlayDvd = ChkDvd.IsChecked ?? false;

            _playerConfig.MediaType = SelectMediaType.SelectedValue;

            _playerConfig.FileExtensions = SelectedFileExtensions.ToArray();

            _playerConfig.CloseOnStopButton = ChkClosePlayerOnStop.IsChecked ?? false;

            _playerConfig.Command = TxtPath.Text;
            _playerConfig.Args = TxtArgs.Text;

            _playerConfig.GameSystem = SelectGameSystem.SelectedValue;

            IsoConfiguration iso;

            if (Enum.TryParse(SelectIsoSupport.SelectedValue, true, out iso))
            {
                _playerConfig.IsoMethod = iso;
            }

            if (_isNew)
            {
                var players = _config.Configuration.MediaPlayers.ToList();
                players.Add(_playerConfig);
                _config.Configuration.MediaPlayers = players.ToArray();
            }

            _config.SaveConfiguration();

            await _nav.Navigate(new MediaPlayersPage(_nav, _playbackManager, _config, _presentation, _apiClient));

            _nav.RemovePagesFromHistory(2);
        }

        void SelectMediaType_SelectedItemChanged(object sender, EventArgs e)
        {
            var value = SelectMediaType.SelectedValue;

            PanelVideoTypes.Visibility = string.Equals(value, MediaType.Video) ? Visibility.Visible : Visibility.Collapsed;

            PanelIsoSupport.Visibility = string.Equals(value, MediaType.Video) || string.Equals(value, MediaType.Game) ? Visibility.Visible : Visibility.Collapsed;

            PnlGameSystems.Visibility = string.Equals(value, MediaType.Game) ? Visibility.Visible : Visibility.Collapsed;

            UpdateFileExtensions();

            if (string.Equals(value, MediaType.Game))
            {
                SelectGameSystem_SelectedItemChanged(null, EventArgs.Empty);
            }

            SelectPlayer.Options = ConfigurablePlayers.Where(i => i.CanPlayMediaType(value)).Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name

            }).ToList();
        }

        void SelectPlayer_SelectedItemChanged(object sender, EventArgs e)
        {
            var player = SelectedPlayer;

            var externalPlayer = player as IConfigurableMediaPlayer;

            ChkClosePlayerOnStop.Visibility = externalPlayer != null && !externalPlayer.CanCloseAutomaticallyOnStopButton
                                                  ? Visibility.Visible
                                                  : Visibility.Collapsed;

            PanelPlayerPath.Visibility = externalPlayer != null && externalPlayer.RequiresConfiguredPath
                                          ? Visibility.Visible
                                          : Visibility.Collapsed;

            PanelPlayerArgs.Visibility = externalPlayer != null && externalPlayer.RequiresConfiguredArguments
                                          ? Visibility.Visible
                                          : Visibility.Collapsed;
        }

        async void ConfigureMediaPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTitle.Text = _isNew ? "Add Media Player" : "Edit Media Player";

            SelectMediaType_SelectedItemChanged(null, EventArgs.Empty);

            var option = SelectPlayer.Options.FirstOrDefault(i => string.Equals(i.Text, _playerConfig.PlayerName, StringComparison.OrdinalIgnoreCase)) ?? SelectPlayer.Options.First();

            SelectPlayer.SelectedValue = option.Value;

            ChkPlay3DVideo.IsChecked = _playerConfig.Play3DVideo;

            ChkDvd.IsChecked = _playerConfig.PlayDvd;
            ChkBluray.IsChecked = _playerConfig.PlayBluray;

            TxtArgs.Text = _playerConfig.Args;
            TxtPath.Text = _playerConfig.Command;

            SelectIsoSupport.SelectedValue = _playerConfig.IsoMethod.ToString();

            ChkClosePlayerOnStop.IsChecked = _playerConfig.CloseOnStopButton;

            await LoadGameSystems();

            var mediaTypeToSelect = SelectMediaType.Options.FirstOrDefault(i => string.Equals(_playerConfig.MediaType, i.Value, StringComparison.OrdinalIgnoreCase));

            if (mediaTypeToSelect != null)
            {
                SelectMediaType.SelectedValue = mediaTypeToSelect.Value;
                SelectMediaType_SelectedItemChanged(null, EventArgs.Empty);
            }

            var systemToSelect = SelectGameSystem.Options.FirstOrDefault(i => string.Equals(_playerConfig.GameSystem, i.Value, StringComparison.OrdinalIgnoreCase));

            if (systemToSelect != null)
            {
                SelectGameSystem.SelectedValue = systemToSelect.Value;
                SelectGameSystem_SelectedItemChanged(null, EventArgs.Empty);
            }
        }

        private IMediaPlayer SelectedPlayer
        {
            get { return _playbackManager.MediaPlayers.FirstOrDefault(i => string.Equals(i.Name, SelectPlayer.SelectedValue)); }
        }

        private void UpdateFileExtensions()
        {
            var mediaType = SelectMediaType.SelectedValue;

            if (string.Equals(mediaType, MediaType.Audio, StringComparison.OrdinalIgnoreCase))
            {
                UpdateFileExtensions(AudioFileExtensions);
            }
            else if (string.Equals(mediaType, MediaType.Video, StringComparison.OrdinalIgnoreCase))
            {
                UpdateFileExtensions(VideoFileExtensions);
            }
            else if (string.Equals(mediaType, MediaType.Book, StringComparison.OrdinalIgnoreCase))
            {
                UpdateFileExtensions(BookFileExtensions);
            }
            else if (string.Equals(mediaType, MediaType.Game, StringComparison.OrdinalIgnoreCase))
            {
                var system = _gameSystems.FirstOrDefault(i => string.Equals(i.Name, SelectGameSystem.SelectedValue));

                if (system == null)
                {
                    UpdateFileExtensions(new string[] { });
                }
                else
                {
                    UpdateFileExtensions(system.GameFileExtensions);
                }
            }
            else
            {
                UpdateFileExtensions(new string[] { });
            }
        }

        private void UpdateFileExtensions(ICollection<string> extensions)
        {
            if (extensions.Count == 0)
            {
                PanelFileExtensions.Visibility = Visibility.Collapsed;
                return;
            }

            PanelFileExtensions.Visibility = Visibility.Visible;

            WrapPanelFileExtensions.Children.Clear();

            foreach (var ext in extensions.OrderBy(i => i).Select(i => i.TrimStart('.')))
            {
                var checkbox = new CheckBox
                {
                    Tag = ext,

                    IsChecked = _playerConfig.FileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase),

                    Margin = new Thickness(0, 0, 30, 10)
                };

                var textBlock = new TextBlock();
                textBlock.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
                textBlock.Text = ext;

                checkbox.Content = textBlock;

                WrapPanelFileExtensions.Children.Add(checkbox);
            }
        }

        private IEnumerable<string> SelectedFileExtensions
        {
            get
            {
                var checkboxes = WrapPanelFileExtensions.Children
                    .OfType<CheckBox>()
                    .ToList();

                var checkedExtensions = checkboxes.Where(i => i.IsChecked ?? false)
                    .Select(i => i.Tag.ToString())
                    .ToList();

                return checkedExtensions;
            }
        }

        private static readonly string[] AudioFileExtensions = new[] { 
            ".mp3",
            ".flac",
            ".wma",
            ".aac",
            ".acc",
            ".m4a",
            ".m4b",
            ".wav",
            ".ape",
            ".ogg",
            ".oga"
        };

        private static readonly string[] VideoFileExtensions = new[]
            {
                ".mkv",
                ".mk3d",
                ".m2t",
                ".m2ts",
                ".ts",
                ".rmvb",
                ".mov",
                ".avi",
                ".mpg",
                ".mpeg",
                ".wmv",
                ".mp4",
                ".divx",
                ".dvr-ms",
                ".wtv",
                ".ogm",
                ".ogv",
                ".asf",
                ".m4v",
                ".flv",
                ".f4v",
                ".3gp",
                ".webm",
                ".mts"
        };

        private static readonly string[] BookFileExtensions = new[]
            {
                ".pdf",
                ".mobi",
                ".epub",
                ".cbr",
                ".cbz"
        };

    }
}
