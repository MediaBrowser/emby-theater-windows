using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class ChapterInfoListViewModel : BaseViewModel, IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IPlaybackManager _playback;
        private readonly IPresentationManager _presentationManager;

        public ICommand PlayCommand { get; private set; }
        public ICommand SeekCommand { get; private set; }

        private readonly RangeObservableCollection<ChapterInfoViewModel> _listItems =
         new RangeObservableCollection<ChapterInfoViewModel>();

        public IReadOnlyList<ChapterInfoViewModel> Chapters
        {
            get { return _listItems; }
        }

        private BaseItemDto _item;
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                var changed = _item != value;
                _item = value;

                if (changed)
                {
                    OnPropertyChanged("Item");

                    if (_chapters != null)
                    {
                        ReloadChapters(_item);
                    }
                }
            }
        }

        private int? _imageWidth;
        public int? ImageWidth
        {
            get { return _imageWidth; }

            set
            {
                var changed = _imageWidth != value;
                _imageWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageWidth");
                }
            }
        }

        private ListCollectionView _chapters;
        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_chapters == null)
                {
                    _chapters = new ListCollectionView(_listItems);

                    if (Item != null)
                    {
                        ReloadChapters(Item);
                    }
                }
                return _chapters;
            }

            private set
            {
                var changed = _chapters != value;
                _chapters = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        public ChapterInfoListViewModel(IApiClient apiClient, IImageManager imageManager, IPlaybackManager playback, IPresentationManager presentationManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _playback = playback;
            _presentationManager = presentationManager;

            PlayCommand = new RelayCommand(Play);
            SeekCommand = new RelayCommand(Seek);
        }

        public void ReloadChapters(BaseItemDto item)
        {
            if (item == null)
            {
                _listItems.Clear();
                return;
            }

            var chapters = item.Chapters ?? new List<ChapterInfoDto>();

            _listItems.Clear();

            _listItems.AddRange(chapters.Select(i => new ChapterInfoViewModel(_apiClient, _imageManager, _playback)
            {
                Chapter = i,
                Item = item,
                ImageWidth = ImageWidth
            }));

            if (StartPositionTicks.HasValue)
            {
                //SetPositionTicks(StartPositionTicks.Value);
            }
        }

        private async void Play(object commandParameter)
        {
            var chapter = (ChapterInfoViewModel)commandParameter;

            try
            {
                await _playback.Play(new PlayOptions
                {
                    Items = new List<BaseItemDto>() { Item },
                    StartPositionTicks = chapter.Chapter.StartPositionTicks
                });
            }
            catch
            {
                _presentationManager.ShowDefaultErrorMessage();
            }
        }

        public long? StartPositionTicks { get; set; }

        public void SetPositionTicks(long ticks)
        {
            var newIndex = 0;

            var chapters = Chapters.ToList();

            for (var i = 0; i < chapters.Count; i++)
            {
                if (ticks >= chapters[i].Chapter.StartPositionTicks)
                {
                    newIndex = i;
                }
                else
                {
                    break;
                }
            }

            ListCollectionView.MoveCurrentToPosition(newIndex);
        }

        private void Seek(object commandParameter)
        {
            var chapter = (ChapterInfoViewModel)commandParameter;

            var player = _playback.MediaPlayers
                .OfType<IVideoPlayer>()
                .FirstOrDefault(i => i.PlayState != PlayState.Idle && i.CanSeek);

            if (player != null)
            {
                try
                {
                    player.Seek(chapter.Chapter.StartPositionTicks);
                }
                catch
                {
                    _presentationManager.ShowDefaultErrorMessage();
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in _listItems.ToList())
            {
                item.Dispose();
            }
        }
    }
}
