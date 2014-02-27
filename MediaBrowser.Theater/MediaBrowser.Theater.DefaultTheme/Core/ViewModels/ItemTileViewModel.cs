using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;

        private readonly BaseItemDto _item;

        public ItemTileViewModel(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, IPlaybackManager playbackManager, BaseItemDto item)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _navigator = navigator;
            _playbackManager = playbackManager;
            _item = item;

            DisplayNameGenerator = i => i.Name;
            PreferredImageTypes = new[] { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };

            serverEvents.UserDataChanged += serverEvents_UserDataChanged;
        }

        void serverEvents_UserDataChanged(object sender, UserDataChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public string DisplayName { get { return DisplayNameGenerator(_item); } }
        public Func<BaseItemDto, string> DisplayNameGenerator { get; set; }
        public string Creator {
            get { return _item.AlbumArtist; }
        }
        public bool HasCreator { get { return !string.IsNullOrEmpty(_item.AlbumArtist); } }
        public ImageViewerViewModel Image { get; private set; }
        public ImageType[] PreferredImageTypes { get; set; }
        public bool IsPlayed { get { return _item.UserData.Played; } }

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
            get
            {
                if (_item.IsFolder) {
                    return _item.PlayedPercentage ?? 0;
                }

                if (_item.RunTimeTicks.HasValue)
                {
                    if (_item.UserData != null && _item.UserData.PlaybackPositionTicks > 0)
                    {
                        double percent = _item.UserData.PlaybackPositionTicks;
                        percent /= _item.RunTimeTicks.Value;

                        return percent * 100;
                    }
                }

                return 0;
            }
        }

        public ICommand PlayCommand { get; private set; }
        public ICommand GoToDetailsCommand { get; private set; }
        public ICommand PlayTrailerCommand { get; private set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
