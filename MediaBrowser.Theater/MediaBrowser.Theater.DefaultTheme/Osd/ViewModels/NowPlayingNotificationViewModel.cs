using System;
using System.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.ViewModels
{
    public class NowPlayingNotificationInfoViewModel : BaseViewModel, IDisposable
    {
        private readonly BaseItemDto _item;
        private readonly Timer _positionTimer;
        private long _progressTicks;

        public NowPlayingNotificationInfoViewModel(IMediaPlayer player)
        {
            _item = player.CurrentMedia;

            var duration = player.CurrentDurationTicks;
            DurationTicks = duration.HasValue ? duration.Value : 0;

            _positionTimer = new Timer(arg => {
                var position = player.CurrentPositionTicks;
                ProgressTicks = position.HasValue ? position.Value : 0;
            }, null, 0, 250);
        }

        public string DisplayName
        {
            get { return ItemTileViewModel.GetDisplayNameWithAiredSpecial(_item); }
        }

        public string ParentName
        {
            get
            {
                switch (_item.Type)
                {
                    case "Season":
                    case "Episode":
                        return _item.SeriesName;
                    case "Album":
                        return _item.AlbumArtist;
                    case "Track":
                        return _item.Artists.ToLocalizedList();
                }

                return null;
            }
        }

        public long ProgressTicks
        {
            get { return _progressTicks; }
            set
            {
                if (value == _progressTicks) {
                    return;
                }
                _progressTicks = value;
                OnPropertyChanged();
                OnPropertyChanged("IsInProgress");
            }
        }

        public long DurationTicks { get; set; }

        public bool HasParentName
        {
            get { return !string.IsNullOrEmpty(ParentName); }
        }

        public bool IsInProgress
        {
            get
            {
                var progress = ProgressTicks;
                return progress > 0 && progress < DurationTicks;
            }
        }

        public void Dispose()
        {
            _positionTimer.Dispose();
        }
    }

    public class NowPlayingNotificationViewModel : ButtonNotificationViewModel
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private IMediaPlayer _activePlayer;

        public NowPlayingNotificationViewModel(IConnectionManager connectionManager, IImageManager imageManager, INavigator nav)
            : base(TimeSpan.MaxValue)
        {
            _connectionManager = connectionManager;
            _imageManager = imageManager;

            Closed += (s, e) => {
                var contents = Contents as NowPlayingNotificationInfoViewModel;
                if (contents != null) {
                    contents.Dispose();
                }
            };

            PressedCommand = new RelayCommand(arg => nav.Navigate(Go.To.FullScreenPlayback()));
        }

        public IMediaPlayer ActivePlayer
        {
            get { return _activePlayer; }
            set
            {
                if (Equals(value, _activePlayer)) {
                    return;
                }
                
                _activePlayer = value;

                if (_activePlayer != null) {
                    var item = _activePlayer.CurrentMedia;

                    Icon = new ItemArtworkViewModel(item, _connectionManager, _imageManager) {
                        DesiredImageHeight=100
                    };

                    Contents = new NowPlayingNotificationInfoViewModel(_activePlayer);

                    EventHandler<PlaybackStopEventArgs> playbackStopped = null;
                    playbackStopped = (s, e) => {
                        Close();
                        value.PlaybackCompleted -= playbackStopped;
                    };

                    _activePlayer.PlaybackCompleted += playbackStopped;
                }

                OnPropertyChanged();
            }
        }
    }
}
