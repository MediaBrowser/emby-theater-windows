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

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public static class BaseItemDtoPlayInfoExtensions
    {
        public static double GetPlayedPercent(this BaseItemDto item)
        {
            if (item == null) {
                return 0;
            }

            if (item.IsFolder) {
                return item.UserData.PlayedPercentage ?? 0;
            }

            if (item.RunTimeTicks.HasValue) {
                if (item.UserData != null && item.UserData.PlaybackPositionTicks > 0) {
                    if (item.UserData.PlaybackPositionTicks == item.RunTimeTicks) {
                        return 100;
                    }

                    double percent = item.UserData.PlaybackPositionTicks / (double)item.RunTimeTicks.Value;
                    return percent * 100;
                }
            }

            return 0;
        }
    }

    public class PlayButtonViewModel : BaseViewModel
    {
        private BaseItemDto _firstItem;
        private readonly ICommand _play;
        private string _displayName;
        private ImageViewerViewModel _image;

        public PlayButtonViewModel(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, ISessionManager sessionManager, int? defaultBackgroundImageIndex = null)
            : this(item, playbackManager, connectionManager, imageManager, sessionManager, defaultBackgroundImageIndex != null ? GetImage(item, defaultBackgroundImageIndex.Value, ImageType.Backdrop, connectionManager.GetApiClient(item)) : null)
        {
        }

        public PlayButtonViewModel(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, ISessionManager sessionManager, string defaultBackgroundImage = null)
        {
            var loading = Load(item, playbackManager, connectionManager, imageManager, sessionManager, defaultBackgroundImage);
            _play = new RelayCommand(async o => {
                var media = await loading;
                await playbackManager.Play(media);
            });
        }

        private async Task<IList<Media>> Load(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, ISessionManager sessionManager, string defaultBackgroundImage = null)
        {
            var items = await GetItems(connectionManager, sessionManager, item);
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

        private async Task<IList<BaseItemDto>> GetItems(IConnectionManager connectionManager, ISessionManager sessionManager, BaseItemDto item)
        {
            var queryParams = new ChildrenQueryParams {
                Recursive = true,
                IncludeItemTypes = new[] { "Movie", "Episode", "Audio" },
                SortOrder = SortOrder.Ascending,
                SortBy = new[] { "SortName" }
            };

            if (item.IsType("Series") || item.IsType("Season") || item.IsType("BoxSet")) {
                var response = (await ItemChildren.Get(connectionManager, sessionManager, item, queryParams));
                var children = response.Items;

                int lastWatched = -1;
                for (int i = 0; i < children.Length; i++) {
                    var percent = children[i].GetPlayedPercent();
                    if (percent >= 100 || children[i].UserData.Played) {
                        lastWatched = i;
                    } else if (percent > 0) {
                        lastWatched = i - 1;
                    }
                }

                var start = lastWatched + 1;
                if (start > 0 && start < children.Length - 1) {
                    children = children.Skip(start).ToArray();
                }

                return children;
            }
           
            if (item.IsFolder || item.IsGenre || item.IsPerson || item.IsStudio) {
                return (await ItemChildren.Get(connectionManager, sessionManager, item, queryParams)).Items;
            }

            return new List<BaseItemDto> { item };
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
