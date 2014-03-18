using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemTileViewModel
        : BaseViewModel, IKnownSize
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private BaseItemDto _item;
        private readonly INavigator _navigator;
        //private readonly IPlaybackManager _playbackManager;

        private bool _imageInvalid;
        private readonly ImageViewerViewModel _image;
        private double? _desiredImageWidth;
        private double? _desiredImageHeight;
        private bool _downloadImagesAtExactSize;
        private bool _downloadPrimaryImageAtExactSize;
        private bool _enableServerImageEnhancers;
        private ImageType[] _preferredImageTypes;
        private bool? _showDisplayName;

        public ItemTileViewModel(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents,
                                 INavigator navigator, /*IPlaybackManager playbackManager,*/ BaseItemDto item)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _navigator = navigator;
            //_playbackManager = playbackManager;
            _item = item;

            _image = new ImageViewerViewModel(imageManager, Enumerable.Empty<ImageViewerImage>());
            _image.PropertyChanged += (senger, args) => {
                if (args.PropertyName == "CurrentImage") {
                    OnPropertyChanged("ActualWidth");
                    OnPropertyChanged("ShowDisplayName");
                }

                if (args.PropertyName == "ImageHeight") {
                    OnPropertyChanged("ActualHeight");
                    OnPropertyChanged("ShowDisplayName");
                }

                if (args.PropertyName == "CurrentImage") {
                    OnPropertyChanged("Size");
                }
            };

            DisplayNameGenerator = GetDisplayNameWithAiredSpecial;
            PreferredImageTypes = new[] { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };
            GoToDetailsCommand = new RelayCommand(o => navigator.Navigate(Go.To.Item(item)));

            serverEvents.UserDataChanged += serverEvents_UserDataChanged;

            _imageInvalid = true;
        }

        public static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IndexNumber.HasValue && !item.IsType("season")) {
                name = item.IndexNumber + " - " + name;
            }

            if (item.ParentIndexNumber.HasValue && item.IsAudio) {
                name = item.ParentIndexNumber + "." + name;
            }

            return name;
        }

        public static string GetDisplayNameWithAiredSpecial(BaseItemDto item)
        {
            if (item.IsType("episode") && item.ParentIndexNumber.HasValue && item.ParentIndexNumber.Value == 0) {
                return "Special - " + item.Name;
            }

            return GetDisplayName(item);
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

        public bool ShowCaptionBar
        {
            get { return ShowDisplayName || HasCreator || IsInProgress; }
        }

        public bool ShowDisplayName
        {
            get { return _showDisplayName ?? ShouldShowDisplayNameByImageType(); }
            set
            {
                if (Equals(_showDisplayName, value)) {
                    return;
                }

                _showDisplayName = value;
                OnPropertyChanged();
                OnPropertyChanged("ShowCaptionBar");
            }
        }

        private bool ShouldShowDisplayNameByImageType()
        {
            var aspectRatio = ActualWidth/ActualHeight;
            return aspectRatio >= 1;
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

        public double? DesiredImageWidth
        {
            get { return _desiredImageWidth; }
            set
            {
                if (Equals(_desiredImageWidth, value)) {
                    return;
                }

                _desiredImageWidth = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public double? DesiredImageHeight
        {
            get { return _desiredImageHeight; }
            set
            {
                if (Equals(_desiredImageHeight, value)) {
                    return;
                }

                _desiredImageHeight = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public double ActualWidth
        {
            get
            {
                if (DesiredImageWidth != null) {
                    return (double)DesiredImageWidth;
                }

                if (DesiredImageHeight != null && (int)Image.ImageWidth != 0 && (int)Image.ImageHeight != 0)
                {
                    var aspectRatio = Image.ImageWidth/Image.ImageHeight;
                    return (double)DesiredImageHeight*aspectRatio;
                }

                return Image.ImageWidth;
            }
        }

        public double ActualHeight
        {
            get
            {
                if (DesiredImageHeight != null)
                {
                    return (double)DesiredImageHeight;
                }

                if (DesiredImageWidth != null && (int)Image.ImageWidth != 0 && (int)Image.ImageHeight != 0)
                {
                    var aspectRatio = Image.ImageWidth / Image.ImageHeight;
                    return (double)DesiredImageWidth / aspectRatio;
                }

                return Image.ImageHeight;
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
            OnPropertyChanged("ShowCaptionBar");
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
            OnPropertyChanged("ShowCaptionBar");
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
                Height = DesiredImageHeight != null && !double.IsPositiveInfinity((double) DesiredImageHeight) ? (int?) Convert.ToInt32(DesiredImageHeight) : null,
                EnableImageEnhancers = EnableServerImageEnhancers
            };

            if ((imageType == ImageType.Primary && DownloadPrimaryImageAtExactSize)
                || (imageType != ImageType.Primary && DownloadImagesAtExactSize)) {
                imageOptions.Width = DesiredImageWidth != null && !double.IsPositiveInfinity((double) DesiredImageWidth) ? (int?) Convert.ToInt32(DesiredImageWidth) : null;
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

        public Size Size
        {
            get { return new Size(ActualWidth + 2 * HomeViewModel.TileMargin, ActualHeight + 2 * HomeViewModel.TileMargin); }
        }
    }
}