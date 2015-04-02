using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.Library;
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
                    double percent = item.UserData.PlaybackPositionTicks;
                    percent /= item.RunTimeTicks.Value;

                    return percent * 100;
                }
            }

            return 0;
        }
    }

    public class PlayButtonViewModel : BaseViewModel
    {
        private readonly ImageViewerViewModel _imageViewer;
        private readonly BaseItemDto _firstItem;
        private readonly ICommand _play;
        private string _displayName;

        public PlayButtonViewModel(BaseItemDto item, IPlaybackManager playbackManager, IConnectionManager connectionManager, IImageManager imageManager, string backgroundImageTag = null)
        {
            var items = GetItems(item);
            var media = Enumerable.Concat(items.Take(1).Select(Media.Resume),
                                          items.Skip(1).Select(Media.Create));

            _firstItem = items.First();
            _play = new RelayCommand(o => playbackManager.Play(media));
            backgroundImageTag = FindImageTag(_firstItem, backgroundImageTag);
            
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

            if (backgroundImageTag != null) {
                _imageViewer = new ImageViewerViewModel(imageManager, new[] { new ImageViewerImage { Url = FindImageTag(_firstItem, backgroundImageTag) } }) {
                    ImageStretch = System.Windows.Media.Stretch.UniformToFill,
                };
            } else {
                _imageViewer = null;
            }
        }

        private string FindImageTag(BaseItemDto item, string defaultTag)
        {
            if (item.IsType("Episode")) {
                return item.ScreenshotImageTags.FirstOrDefault() ?? defaultTag;
            }

            return defaultTag;
        }

        private string GetImage(BaseItemDto item, string tag, IApiClient api)
        {
            var imageOptions = new ImageOptions {
                Tag = tag,
                EnableImageEnhancers = false
            };

            return api.GetImageUrl(item, imageOptions);
        }

        private IList<BaseItemDto> GetItems(BaseItemDto item)
        {
            throw new NotImplementedException();
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
            get { return _imageViewer; }
        }

        public ICommand PlayCommand
        {
            get { return _play; }
        }
    }
}
