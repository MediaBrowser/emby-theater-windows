using System;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemTileViewModel
        : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private BaseItemDto _item;
        private readonly INavigator _navigator;
        //private readonly IPlaybackManager _playbackManager;

        private bool _imageInvalid;
        private readonly ImageViewerViewModel _image;
        private double? _imageWidth;
        private double? _imageHeight;
        private bool _downloadImagesAtExactSize;
        private bool _downloadPrimaryImageAtExactSize;
        private bool _enableServerImageEnhancers;
        private ImageType[] _preferredImageTypes;

        public ItemTileViewModel(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents,
                                 INavigator navigator, /*IPlaybackManager playbackManager,*/ BaseItemDto item)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _navigator = navigator;
            //_playbackManager = playbackManager;
            _item = item;

            _image = new ImageViewerViewModel(imageManager, Enumerable.Empty<ImageViewerImage>());
            
            DisplayNameGenerator = i => i.Name;
            PreferredImageTypes = new[] { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };
            GoToDetailsCommand = new RelayCommand(o => navigator.Navigate(Go.To.Item(item)));

            serverEvents.UserDataChanged += serverEvents_UserDataChanged;

            _imageInvalid = true;
        }

        public BaseItemDto Item
        {
            get { return _item; }
            set
            {
                if (Equals(_item, value)) {
                    return;
                }

                _item = value;
                OnPropertyChanged();
                RefreshItemFields();
                InvalidateImage();
            }
        }

        public string DisplayName
        {
            get { return _item == null ? string.Empty : DisplayNameGenerator(_item); }
        }

        public bool IsFolder 
        {
            get { return _item != null && _item.IsFolder; }
        }

        public Func<BaseItemDto, string> DisplayNameGenerator { get; set; }

        public string Creator
        {
            get { return _item == null ? string.Empty : _item.AlbumArtist; }
        }

        public bool HasCreator
        {
            get { return _item != null && !string.IsNullOrEmpty(_item.AlbumArtist); }
        }

        public ImageViewerViewModel Image
        {
            get
            {
                if (_imageInvalid) {
                    DownloadImage();
                }

                return _image;
            }
        }

        public ImageType[] PreferredImageTypes
        {
            get { return _preferredImageTypes; }
            set 
            {
                _preferredImageTypes = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool IsPlayed
        {
            get { return _item != null && _item.UserData.Played; }
        }

        public bool IsInProgress
        {
            get
            {
                double percent = PlayedPercent;
                return percent > 0 && percent < 100;
            }
        }

        public double PlayedPercent
        {
            get
            {
                if (_item == null) {
                    return 0;
                }

                if (_item.IsFolder) {
                    return _item.PlayedPercentage ?? 0;
                }

                if (_item.RunTimeTicks.HasValue) {
                    if (_item.UserData != null && _item.UserData.PlaybackPositionTicks > 0) {
                        double percent = _item.UserData.PlaybackPositionTicks;
                        percent /= _item.RunTimeTicks.Value;

                        return percent*100;
                    }
                }

                return 0;
            }
        }

        public ICommand PlayCommand { get; private set; }
        public ICommand GoToDetailsCommand { get; private set; }
        public ICommand PlayTrailerCommand { get; private set; }

        public double? ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (Equals(_imageWidth, value)) {
                    return;
                }

                _imageWidth = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public double? ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                if (Equals(_imageHeight, value)) {
                    return;
                }

                _imageHeight = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool DownloadImagesAtExactSize
        {
            get { return _downloadImagesAtExactSize; }
            set
            {
                if (Equals(_downloadImagesAtExactSize, value)) {
                    return;
                }

                _downloadImagesAtExactSize = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool DownloadPrimaryImageAtExactSize
        {
            get { return _downloadPrimaryImageAtExactSize; }
            set
            {
                if (Equals(_downloadPrimaryImageAtExactSize, value)) {
                    return;
                }

                _downloadPrimaryImageAtExactSize = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool EnableServerImageEnhancers
        {
            get { return _enableServerImageEnhancers; }
            set
            {
                if (Equals(_enableServerImageEnhancers, value)) {
                    return;
                }

                _enableServerImageEnhancers = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        private void serverEvents_UserDataChanged(object sender, UserDataChangedEventArgs e)
        {
            RefreshUserDataFields();
        }

        private void RefreshUserDataFields()
        {
            OnPropertyChanged("PlayedPercent");
            OnPropertyChanged("IsInProgress");
            OnPropertyChanged("IsPlayed");
        }

        private void RefreshItemFields()
        {
            OnPropertyChanged("DisplayName");
            OnPropertyChanged("IsFolder");
            OnPropertyChanged("Creator");
            OnPropertyChanged("HasCreator");
            OnPropertyChanged("IsPlayed");
            OnPropertyChanged("IsInProgress");
            OnPropertyChanged("PlayedPercent");
        }

        private void InvalidateImage()
        {
            _imageInvalid = true;
            OnPropertyChanged("Image");
        }
        
        /// <summary>
        ///     Gets an image url that can be used to download an image from the api
        /// </summary>
        /// <param name="imageType">The type of image requested</param>
        /// <param name="imageIndex">
        ///     The image index, if there are multiple. Currently only applies to backdrops. Supply null or 0
        ///     for first backdrop.
        /// </param>
        /// <returns>System.String.</returns>
        private string GetImageUrl(ImageType imageType, int? imageIndex = null)
        {
            var imageOptions = new ImageOptions {
                ImageType = imageType,
                ImageIndex = imageIndex,
                Height = ImageHeight != null ? (int?) Convert.ToInt32(ImageHeight) : null,
                EnableImageEnhancers = EnableServerImageEnhancers
            };

            if ((imageType == ImageType.Primary && DownloadPrimaryImageAtExactSize)
                || (imageType != ImageType.Primary && DownloadImagesAtExactSize)) {
                imageOptions.Width = ImageWidth != null ? (int?) Convert.ToInt32(ImageWidth) : null;
            }

            if (imageType == ImageType.Thumb) {
                return _apiClient.GetThumbImageUrl(_item, imageOptions);
            }

            return _apiClient.GetImageUrl(_item, imageOptions);
        }

        public void DownloadImage()
        {
            ImageType[] preferredImageTypes = PreferredImageTypes;
            BaseItemDto item = _item;

            _imageInvalid = false;

            if (item != null) {
                foreach (ImageType imageType in preferredImageTypes) {
                    if (imageType == ImageType.Backdrop) {
                        if (item.BackdropCount == 0) {
                            continue;
                        }
                    } else if (imageType == ImageType.Thumb) {
                        if (!item.ImageTags.ContainsKey(imageType) && !item.ParentThumbImageTag.HasValue && !item.SeriesThumbImageTag.HasValue) {
                            continue;
                        }
                    } else {
                        if (!item.ImageTags.ContainsKey(imageType)) {
                            continue;
                        }
                    }

                    string url = GetImageUrl(imageType);
                    Image.Images.Clear();
                    Image.Images.Add(new ImageViewerImage { Url = url });
                    Image.StartRotating();

                    return;
                }
            }

            Image.Images.Clear();
            Image.StartRotating();
        }
    }
}