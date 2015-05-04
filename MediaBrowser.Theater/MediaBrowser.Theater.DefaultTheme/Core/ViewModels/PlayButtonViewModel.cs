using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.ViewModels;
using MediaBrowser.Theater.Api.Commands.ItemCommands;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class PlayButtonViewModel : BaseViewModel
    {
        private BaseItemDto _firstItem;
        private readonly ICommand _play;
        private string _displayName;
        private ImageViewerViewModel _image;

        public bool AutoFocus { get; set; }

        public PlayButtonViewModel(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, ISessionManager sessionManager, int? defaultBackgroundImageIndex = null)
            : this(item, playbackManager, connectionManager, imageManager, sessionManager, defaultBackgroundImageIndex != null ? GetImage(item, defaultBackgroundImageIndex.Value, ImageType.Backdrop, connectionManager.GetApiClient(item)) : null)
        {
        }

        public PlayButtonViewModel(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, ISessionManager sessionManager, string defaultBackgroundImage = null)
        {
            AutoFocus = true;
            var loading = Load(item, playbackManager, connectionManager, imageManager, sessionManager, defaultBackgroundImage);
            _play = new RelayCommand(async o => {
                var media = await loading;
                await playbackManager.Play(media);
            });
        }

        private async Task<IList<Media>> Load(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, ISessionManager sessionManager, string defaultBackgroundImage = null)
        {
            var items = await item.GetSmartPlayItems(connectionManager, sessionManager);
            var media = Enumerable.Concat(items.Take(1).Select(Media.Resume),
                                          items.Skip(1).Select(Media.Create))
                                  .ToList();

            FirstItem = items.First();
            var imageUrl = FindImageUrl(_firstItem, defaultBackgroundImage, connectionManager.GetApiClient(_firstItem));

            var isPlayable = IsPlayable(item);
            var resumable = _firstItem.CanResume && IsInProgress;

            if (isPlayable) {
                // single playable item
                DisplayName = resumable ? "Resume" : "Play";
            } else {
                // folder
                var name = item.GetDisplayName(new DisplayNameFormat(false, true));
                DisplayName = resumable ? string.Format("Resume {0}", name) : string.Format("Play {0}", name);
            }

            if (imageUrl != null) {
                Image = new ImageViewerViewModel(imageManager, new[] { new ImageViewerImage { Url = imageUrl } }) {
                    ImageStretch = System.Windows.Media.Stretch.UniformToFill,
                };

                Image.StartRotating();
            } else {
                Image = null;
            }

            return media;
        }

        private static string FindImageUrl(BaseItemDto item, string backgroundImage, IApiClient api)
        {
            if (item.IsType("Episode") && item.ScreenshotImageTags != null && item.ScreenshotImageTags.Count > 0) {
                return GetImage(item, 0, ImageType.Screenshot, api) ?? backgroundImage;
            }

            return backgroundImage;
        }

        private static string GetImage(BaseItemDto item, int? index, ImageType type, IApiClient api)
        {
            var imageOptions = new ImageOptions {
                ImageIndex = index,
                ImageType = type,
                EnableImageEnhancers = false,
                
            };

            return api.GetImageUrl(item, imageOptions);
        }

        

        private BaseItemDto FirstItem
        {
            get { return _firstItem; }
            set
            {
                if (Equals(_firstItem, value)) {
                    return;
                }

                _firstItem = value;
                OnPropertyChanged();
                OnPropertyChanged("IsInProgress");
                OnPropertyChanged("PlayedPercent");
            }
        }

        public bool IsInProgress
        {
            get
            {
                var percent = PlayedPercent;
                return percent > 0 && percent < 100;
            }
        }

        public double PlayedPercent
        {
            get { return _firstItem.GetPlayedPercent(); }
        }

        private bool IsPlayable(BaseItemDto item)
        {
            return !item.IsFolder && !item.IsGenre && !item.IsPerson && !item.IsStudio;
        }

        public string DisplayName
        {
            get { return _displayName; }
            private set
            {
                if (value == _displayName) {
                    return;
                }
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public ImageViewerViewModel Image
        {
            get { return _image; }
            private set
            {
                if (Equals(value, _image)) {
                    return;
                }
                _image = value;
                OnPropertyChanged();
            }
        }

        public ICommand PlayCommand
        {
            get { return _play; }
        }
    }
}
