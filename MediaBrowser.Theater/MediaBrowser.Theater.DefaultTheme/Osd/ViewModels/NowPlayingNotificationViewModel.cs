using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.ViewModels
{
    public class NowPlayingNotificationInfoViewModel : BaseViewModel, IDisposable
    {
        private readonly IDisposable _subscription;

        private BaseItemDto _item;
        private long _progressTicks;
        private long _durationTicks;

        public NowPlayingNotificationInfoViewModel(IObservable<PlaybackStatus> status)
        {
            _subscription = status.Subscribe(s => {
                _item = s.PlayableMedia.Media.Item;

                OnPropertyChanged("DisplayName");
                OnPropertyChanged("ParentName");
                OnPropertyChanged("HasParentName");

                DurationTicks = s.Duration ?? 0;
                ProgressTicks = s.Progress ?? 0;
            });
        }

        public string DisplayName
        {
            get { return _item != null ? _item.GetDisplayName(new DisplayNameFormat(false, false)) : null; }
        }

        public string ParentName
        {
            get
            {
                if (_item == null) {
                    return null;
                }

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

        public long DurationTicks
        {
            get { return _durationTicks; }
            set
            {
                if (value == _durationTicks) {
                    return;
                }
                _durationTicks = value;
                OnPropertyChanged();
                OnPropertyChanged("IsInProgress");
            }
        }

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
            _subscription.Dispose();
        }
    }

    public class NowPlayingNotificationViewModel : ButtonNotificationViewModel
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private IPlaybackSession _session;

        public NowPlayingNotificationViewModel(IConnectionManager connectionManager, IImageManager imageManager, INavigator nav)
            : base(Timeout.InfiniteTimeSpan)
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

        public IPlaybackSession Session
        {
            get { return _session; }
            set
            {
                if (Equals(value, _session)) {
                    return;
                }
                
                _session = value;

                if (_session != null) {
                    Contents = new NowPlayingNotificationInfoViewModel(_session.Events);
                    LoadArtworkImage(_session);
                    CloseOnCompletion(_session);
                }

                OnPropertyChanged();
            }
        }

        private async void LoadArtworkImage(IPlaybackSession session)
        {
            var item = await session.Events.Select(e => e.PlayableMedia.Media.Item).FirstOrDefaultAsync();
            if (item != null) {
                Action setIcon = () => {
                    if (Icon == null) {
                        Icon = new ItemArtworkViewModel(item, _connectionManager, _imageManager) {
                            DesiredImageHeight = 100
                        };
                    }
                };

                setIcon.OnUiThread();
            }
        }

        private async void CloseOnCompletion(IPlaybackSession session)
        {
            await session.Events;
            await Close();
        }
    }
}
