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

        private readonly RangeObservableCollection<ChapterInfoViewModel> _listItems =
         new RangeObservableCollection<ChapterInfoViewModel>();

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

                    ReloadChapters(_item);
                }
            }
        }

        private int? _imageDownloadWidth;
        public int? ImageDownloadWidth
        {
            get { return _imageDownloadWidth; }

            set
            {
                var changed = _imageDownloadWidth != value;
                _imageDownloadWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageDownloadWidth");
                }
            }
        }

        private ListCollectionView _chapters;
        public ListCollectionView Chapters
        {
            get
            {
                return _chapters;
            }

            private set
            {
                var changed = _chapters != value;
                _chapters = value;

                if (changed)
                {
                    OnPropertyChanged("Chapters");
                }
            }
        }

        public ChapterInfoListViewModel(IApiClient apiClient, IImageManager imageManager, IPlaybackManager playback, IPresentationManager presentationManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _playback = playback;
            _presentationManager = presentationManager;
            _chapters = new ListCollectionView(_listItems);

            PlayCommand = new RelayCommand(Play);
        }

        private void ReloadChapters(BaseItemDto item)
        {
            if (item == null)
            {
                _listItems.Clear();
                return;
            }

            // Record the current item
            var currentItem = _chapters.CurrentItem as ChapterInfoViewModel;

            var chapters = item.Chapters ?? new List<ChapterInfoDto>();

            int? selectedIndex = null;

            if (currentItem != null)
            {
                var index = Array.FindIndex(chapters.ToArray(), i => i.StartPositionTicks == currentItem.Chapter.StartPositionTicks);

                if (index != -1)
                {
                    selectedIndex = index;
                }
            }

            _listItems.Clear();

            _listItems.AddRange(chapters.Select(i => new ChapterInfoViewModel(_apiClient, _imageManager, _playback)
            {
                Chapter = i,
                Item = item,
                ImageDownloadWidth = ImageDownloadWidth
            }));

            if (selectedIndex.HasValue)
            {
                Chapters.MoveCurrentToPosition(selectedIndex.Value);
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

        public void Dispose()
        {
            foreach (var item in _listItems.ToList())
            {
                item.Dispose();
            }
        }
    }
}
