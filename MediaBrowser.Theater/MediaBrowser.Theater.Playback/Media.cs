using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public class Media
    {
        public BaseItemDto Item { get; set; }
        public MediaPlaybackOptions Options { get; set; }

        public static implicit operator Media(BaseItemDto item)
        {
            return new Media {
                Item = item,
                Options = new MediaPlaybackOptions {
                    Resume = false
                }
            };
        }
    }

    public struct MediaPlaybackOptions
    {
        public bool Resume { get; set; }
    }
}
