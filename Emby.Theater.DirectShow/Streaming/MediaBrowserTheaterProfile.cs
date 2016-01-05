using MediaBrowser.Model.Dlna;
using System.Xml.Serialization;

namespace Emby.Theater.DirectShow.Streaming
{
    [XmlRoot("Profile")]
    public class MediaBrowserTheaterProfile : DeviceProfile
    {
        public MediaBrowserTheaterProfile()
        {
            Name = "Emby Theater";

            MaxStreamingBitrate = 8000000;
            MaxStaticBitrate = 80000000;

            TranscodingProfiles = new[]
            {
                new TranscodingProfile
                {
                    Container = "mp3",
                    Type = DlnaProfileType.Audio,
                    AudioCodec = "mp3",
                    Context = EncodingContext.Static
                },
                new TranscodingProfile
                {
                    Container = "mp3",
                    Type = DlnaProfileType.Audio,
                    AudioCodec = "mp3",
                    Context = EncodingContext.Streaming
                },
                //new TranscodingProfile
                //{
                //    Protocol = "hls",
                //    Container = "ts",
                //    VideoCodec = "h264",
                //    AudioCodec = "aac,ac3,mp3",
                //    Type = DlnaProfileType.Video
                //},
                new TranscodingProfile
                {
                    Container = "mp4",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "h264",
                    AudioCodec = "aac,ac3,mp3",
                    Context = EncodingContext.Streaming
                },
                new TranscodingProfile
                {
                    Container = "mp4",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "h264",
                    AudioCodec = "aac",
                    Context = EncodingContext.Streaming
                },
                new TranscodingProfile
                {
                    Container = "jpeg",
                    Type = DlnaProfileType.Photo
                }
            };

            DirectPlayProfiles = new[]
            {
                new DirectPlayProfile
                {
                    Container = "avi,mkv,ts,mp4,mov,m4v,asf,wmv,mpg,mpeg,flv,3gp,mpegts,m2ts,vob,webm",
                    Type = DlnaProfileType.Video
                },

                new DirectPlayProfile
                {
                    Container = "mp3,mp4,flac,wma,asf,ogg,m4a,aac,wav,oga",
                    Type = DlnaProfileType.Audio
                },

                new DirectPlayProfile
                {
                    Type = DlnaProfileType.Photo,

                    Container = "jpeg,png,gif,bmp,tiff"
                }
            };

            SubtitleProfiles = new[]
            {
                new SubtitleProfile
                {
                    Format = "srt",
                    Method = SubtitleDeliveryMethod.External
                }
            };
        }
    }
}
