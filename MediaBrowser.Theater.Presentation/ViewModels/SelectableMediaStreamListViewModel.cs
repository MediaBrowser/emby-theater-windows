using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class SelectableMediaStreamListViewModel : BaseViewModel
    {
        private readonly RangeObservableCollection<SelectableMediaStream> _listItems = new RangeObservableCollection<SelectableMediaStream>();

        private readonly IPlaybackManager _playbackManager;

        private ListCollectionView _listCollectionView;

        public ICommand ActivateCommand { get; private set; }
        
        public SelectableMediaStreamListViewModel(IPlaybackManager playbackManager)
        {
            _playbackManager = playbackManager;

            ActivateCommand = new RelayCommand(ActivateStream);
        }

        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    ReloadList();
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

        public MediaStreamType Type { get; set; }

        private void ReloadList()
        {
            _listItems.Clear();

            var player = _playbackManager.MediaPlayers
                .OfType<IVideoPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player == null)
            {
                return;
            }

            _listItems.AddRange(player.SelectableStreams.Where(i => i.Type == Type));
        }

        private void ActivateStream(object param)
        {
            var stream = (SelectableMediaStream)param;

            var player = _playbackManager.MediaPlayers
                .OfType<IVideoPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player != null)
            {
                if (stream.Type == MediaStreamType.Audio)
                {
                    player.ChangeAudioStream(stream);
                }
                if (stream.Type == MediaStreamType.Subtitle)
                {
                    player.ChangeSubtitleStream(stream);
                }
            }
        }
    }
}
