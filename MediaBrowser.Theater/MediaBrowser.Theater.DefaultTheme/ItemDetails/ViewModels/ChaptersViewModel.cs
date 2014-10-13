using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ChaptersViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly IImageManager _imageManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly BaseItemDto _item;
        private readonly IConnectionManager _connectionManager;

        private bool _isVisible;

        public ChaptersViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager)
        {
            _item = item;
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _playbackManager = playbackManager;

            Title = "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_ChaptersHeader".Localize();
            Items = new RangeObservableCollection<ChapterViewModel>();
            LoadItems();
        }

        public string Title { get; set; }

        public RangeObservableCollection<ChapterViewModel> Items { get; private set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (Equals(_isVisible, value)) {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public int SortOrder
        {
            get { return 2; }
        }

        public Size Size
        {
            get
            {
                if (Items.Count == 0) {
                    return new Size();
                }

                return new Size(200 * (16.0/9), 700);
            }
        }

        private void LoadItems()
        {
            Items.Clear();

            if (_item.Chapters != null) {
                IEnumerable<ChapterViewModel> items = _item.Chapters.Select(c => new ChapterViewModel(_item, c, _connectionManager, _imageManager, _playbackManager));
                Items.AddRange(items);
            }

            IsVisible = Items.Count > 0;
            OnPropertyChanged("Size");
        }
    }

    public class ChapterViewModel
        : BaseViewModel, IDisposable
    {
        private readonly ChapterInfoDto _chapter;
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly BaseItemDto _item;
        private Image _image;
        private CancellationTokenSource _imageCancellationTokenSource;

        public ChapterViewModel(BaseItemDto item, ChapterInfoDto chapter, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager)
        {
            _item = item;
            _chapter = chapter;
            _connectionManager = connectionManager;
            _imageManager = imageManager;

            PlayCommand = new RelayCommand(o => playbackManager.Play(new PlayOptions(item) {
                GoFullScreen = true,
                EnableCustomPlayers = true,
                Resume = false,
                StartPositionTicks = chapter.StartPositionTicks
            }));
        }

        public string Name
        {
            get { return _chapter.Name; }
        }

        public string Time
        {
            get
            {
                TimeSpan time = TimeSpan.FromTicks(_chapter.StartPositionTicks);
                return time.ToString(time.TotalHours < 1 ? "m':'ss" : "h':'mm':'ss");
            }
        }

        public bool IsUnwatched
        {
            get
            {
                var filter = Theme.Instance.Configuration.ShowOnlyWatchedChapters && _item.UserData != null;
                return filter && (_item.UserData.Played || _chapter.StartPositionTicks >= _item.UserData.PlaybackPositionTicks);
            }
        }

        public bool ShowImage
        {
            get { return !IsUnwatched; }
        }

        public ICommand PlayCommand { get; private set; }

        public Image Image
        {
            get
            {
                Image img = _image;

                CancellationTokenSource tokenSource = _imageCancellationTokenSource;

                if (img == null && (tokenSource == null || tokenSource.IsCancellationRequested)) {
                    DownloadImage();
                }

                return _image;
            }

            private set
            {
                bool changed = !Equals(_image, value);

                _image = value;

                if (value == null) {
                    CancellationTokenSource tokenSource = _imageCancellationTokenSource;

                    if (tokenSource != null) {
                        tokenSource.Cancel();
                    }
                }

                if (changed) {
                    OnPropertyChanged();
                }
            }
        }

        public void Dispose()
        {
            DisposeCancellationTokenSource();
        }

        private async void DownloadImage()
        {
            _imageCancellationTokenSource = new CancellationTokenSource();

            if (!string.IsNullOrEmpty(_chapter.ImageTag)) {
                var options = new ImageOptions {
                    Height = 200,
                    ImageIndex = _item.Chapters.IndexOf(_chapter),
                    ImageType = ImageType.Chapter,
                    Tag = _chapter.ImageTag
                };

                var apiClient = _connectionManager.GetApiClient(_item);
                Image = await _imageManager.GetRemoteImageAsync(apiClient.GetImageUrl(_item, options), _imageCancellationTokenSource.Token);
            }
        }

        private void DisposeCancellationTokenSource()
        {
            if (_imageCancellationTokenSource != null) {
                _imageCancellationTokenSource.Cancel();
                _imageCancellationTokenSource.Dispose();
                _imageCancellationTokenSource = null;
            }
        }
    }

    public class ChapterSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly IPlaybackManager _playbackManager;

        public ChapterSectionGenerator(IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager)
        {
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _playbackManager = playbackManager;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item.Chapters != null && item.Chapters.Count > 1;
        }

        public Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            var section = new ChaptersViewModel(item, _connectionManager, _imageManager, _playbackManager);
            return Task.FromResult<IEnumerable<IItemDetailSection>>(new[] { section });
        }
    }
}