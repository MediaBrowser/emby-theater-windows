using System;
using System.ComponentModel;
using System.Windows;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class SelectableMediaStreamListViewModel : BaseViewModel, IDisposable
    {
        private readonly RangeObservableCollection<SelectableMediaStream> _listItems = new RangeObservableCollection<SelectableMediaStream>();

        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playbackManager;

        private ListCollectionView _listCollectionView;

        public ICommand ActivateCommand { get; private set; }
        
        public SelectableMediaStreamListViewModel(IPresentationManager presentationManager, IPlaybackManager playbackManager)
        {
            _playbackManager = playbackManager;
            _presentationManager = presentationManager;

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

                    EnsureActiveStreamIsVisible();
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

        private void EnsureActiveStreamIsVisible()
        {
             var activeItemIndex = _listItems.IndexOf(_listItems.FirstOrDefault(i => i.IsActive));

             _presentationManager.Window.Dispatcher.InvokeAsync(() => ListCollectionView.MoveCurrentToPosition(activeItemIndex));
        }

        private void SelectableMediaStream_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "IsActive"))
            {
                var stream = sender as SelectableMediaStream;
                if (stream != null && stream.IsActive)
                {
                    EnsureActiveStreamIsVisible();
                }
            }
        }

        public MediaStreamType Type { get; set; }

        private void ReloadList()
        {
            foreach (var s in _listItems)
            {
                s.PropertyChanged -= SelectableMediaStream_PropertyChanged;
            }
            _listItems.Clear();

            var player = _playbackManager.MediaPlayers
                .OfType<IVideoPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player == null)
            {
                return;
            }

            _listItems.AddRange(player.SelectableStreams.Where(i => i.Type == Type));

            
            foreach (var s in _listItems)
            {
                s.PropertyChanged += SelectableMediaStream_PropertyChanged;
            }
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


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var s in _listItems)
            {
                s.PropertyChanged -= SelectableMediaStream_PropertyChanged;
            }
        }
    }

}
