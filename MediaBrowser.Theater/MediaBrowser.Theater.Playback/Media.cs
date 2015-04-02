using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public class Media
    {
        public BaseItemDto Item { get; set; }
        public MediaPlaybackOptions Options { get; set; }

        public static implicit operator Media(BaseItemDto item)
        {
            return Create(item);
        }

        public static Media Create(BaseItemDto item)
        {
            return Create(item, null);
        }

        public static Media Create(BaseItemDto item, MediaPlaybackOptions? options)
        {
            return new Media {
                Item = item,
                Options = options ?? new MediaPlaybackOptions()
            };
        }

        public static Media Resume(BaseItemDto item)
        {
            return Create(item, new MediaPlaybackOptions {
                StartPositionTicks = item.UserData.PlaybackPositionTicks
            });
        }
    }
}
