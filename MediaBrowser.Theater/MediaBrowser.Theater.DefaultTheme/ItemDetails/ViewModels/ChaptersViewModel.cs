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
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ChaptersViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly BaseItemDto _item;

        private bool _isVisible;

        public ChaptersViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager)
        {
            _item = item;
            _apiClient = apiClient;
            _imageManager = imageManager;

            Title = "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_ChaptersHeader".Localize();
            Items = new RangeObservableCollection<IViewModel>();
            LoadItems();
        }

        public string Title { get; set; }

        public RangeObservableCollection<IViewModel> Items { get; private set; }

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

                return new Size(600, 700);
            }
        }

        private void LoadItems()
        {
            Items.Clear();

            if (_item.Chapters != null) {
                var filter = Theme.Instance.Configuration.ShowOnlyWatchedChapters && _item.UserData != null;
                var visibleChapters = filter ? _item.Chapters.Where(c => c.StartPositionTicks < _item.UserData.PlaybackPositionTicks) : _item.Chapters;
                IEnumerable<IViewModel> items = visibleChapters.Select(c => new ChapterViewModel(_item, c, _apiClient, _imageManager));
                Items.AddRange(items);
            }

            IsVisible = Items.Count > 0;
            OnPropertyChanged("Size");
        }
    }

    public class ChapterViewModel
        : BaseViewModel, IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly ChapterInfoDto _chapter;
        private readonly IImageManager _imageManager;
        private readonly BaseItemDto _item;
        private Image _image;
        private CancellationTokenSource _imageCancellationTokenSource;

        public ChapterViewModel(BaseItemDto item, ChapterInfoDto chapter, IApiClient apiClient, IImageManager imageManager)
        {
            _item = item;
            _chapter = chapter;
            _apiClient = apiClient;
            _imageManager = imageManager;

            //todo play chapter command
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

            if (_chapter.ImageTag.HasValue) {
                var options = new ImageOptions {
                    Height = 100,
                    ImageIndex = _item.Chapters.IndexOf(_chapter),
                    ImageType = ImageType.Chapter,
                    Tag = _chapter.ImageTag
                };

                Image = await _imageManager.GetRemoteImageAsync(_apiClient.GetImageUrl(_item, options), _imageCancellationTokenSource.Token);
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
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;

        public ChapterSectionGenerator(IApiClient apiClient, IImageManager imageManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item.Chapters != null && item.Chapters.Count > 0 && (!Theme.Instance.Configuration.ShowOnlyWatchedChapters || HasWatchedAny(item));
        }

        private bool HasWatchedAny(BaseItemDto item)
        {
            return item.UserData == null || item.UserData.Played || (item.Chapters != null && item.UserData.PlaybackPositionTicks > item.Chapters.First().StartPositionTicks);
        }

        public Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            var section = new ChaptersViewModel(item, _apiClient, _imageManager);
            return Task.FromResult<IEnumerable<IItemDetailSection>>(new[] { section });
        }
    }
}