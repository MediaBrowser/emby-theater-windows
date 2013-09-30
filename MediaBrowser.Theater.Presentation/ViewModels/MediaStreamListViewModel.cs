using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class MediaStreamListViewModel : BaseViewModel
    {
        private readonly IPlaybackManager _playback;
        private readonly IPresentationManager _presentationManager;

        public ICommand ChangeTrackCommand { get; private set; }
        
        private readonly RangeObservableCollection<MediaStreamViewModel> _listItems = new RangeObservableCollection<MediaStreamViewModel>();

        public RangeObservableCollection<MediaStreamViewModel> Streams
        {
            get { return _listItems; }
        }

        private ListCollectionView _listCollectionView;
        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                }
                return _listCollectionView;
            }

            private set
            {
                var changed = _listCollectionView != value;
                _listCollectionView = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        public MediaStreamListViewModel(IPlaybackManager playback, IPresentationManager presentationManager)
        {
            _playback = playback;
            _presentationManager = presentationManager;
            ChangeTrackCommand = new RelayCommand(ChangeTrack);
        }

        private void ChangeTrack(object commandParameter)
        {
            var track = (MediaStreamViewModel)commandParameter;

            var player = _playback.MediaPlayers
                .OfType<IVideoPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle && i.CanSeek);

            if (player != null)
            {
                try
                {
                    if (track.Type == MediaStreamType.Audio)
                    {
                        player.ChangeAudioStream(track.MediaStream);
                    }
                    else if (track.Type == MediaStreamType.Subtitle)
                    {
                        player.ChangeSubtitleStream(track.MediaStream);
                    }
                }
                catch
                {
                    _presentationManager.ShowDefaultErrorMessage();
                }
            }
        }
    }
}
