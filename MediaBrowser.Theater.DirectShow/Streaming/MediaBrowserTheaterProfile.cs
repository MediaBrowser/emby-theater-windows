using MediaBrowser.Model.Dlna;
using System.Xml.Serialization;

namespace MediaBrowser.Theater.DirectShow.Streaming
{
    [XmlRoot("Profile")]
    public class MediaBrowserTheaterProfile : DeviceProfile
    {
        public MediaBrowserTheaterProfile()
        {
            Name = "Media Browser Theater";

            MaxStreamingBitrate = 8000000;
            MaxStaticBitrate = 8000000;

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
                    Container = "ts",
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
                    Container = "avi",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "mpeg1video,mpeg2video,mpeg4,h264,vc1",
                    AudioCodec = "ac3,dca,mp2,mp3,pcm"
                },

                new DirectPlayProfile
                {
                    Container = "mpeg",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "mpeg1video,mpeg2video",
                    AudioCodec = "ac3,dca,mp2,mp3,pcm"
                },

                new DirectPlayProfile
                {
                    Container = "mkv",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "mpeg1video,mpeg2video,mpeg4,h264,vc1",
                    AudioCodec = "ac3,dca,aac,mp2,mp3,pcm"
                },

                new DirectPlayProfile
                {
                    Container = "ts",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "mpeg1video,mpeg2video,h264,vc1",
                    AudioCodec = "ac3,dca,mp2,mp3,aac"
                },

                new DirectPlayProfile
                {
                    Container = "mp4,mov",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "h264,mpeg4",
                    AudioCodec = "ac3,aac,mp2,mp3"
                },

                new DirectPlayProfile
                {
                    Container = "asf",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "vc1",
                    AudioCodec = "wmav2,wmapro"
                },

                new DirectPlayProfile
                {
                    Container = "asf",
                    Type = DlnaProfileType.Video,
                    VideoCodec = "mpeg2video",
                    AudioCodec = "mp2,ac3"
                },

                new DirectPlayProfile
                {
                    Container = "mp3",
                    AudioCodec = "mp2,mp3",
                    Type = DlnaProfileType.Audio
                },

                new DirectPlayProfile
                {
                    Container = "mp4",
                    AudioCodec = "mp4",
                    Type = DlnaProfileType.Audio
                },

                new DirectPlayProfile
                {
                    Container = "flac",
                    AudioCodec = "flac",
                    Type = DlnaProfileType.Audio
                },

                new DirectPlayProfile
                {
                    Container = "asf",
                    AudioCodec = "wmav2,wmapro,wmavoice",
                    Type = DlnaProfileType.Audio
                },

                new DirectPlayProfile
                {
                    Container = "ogg",
                    AudioCodec = "vorbis",
                    Type = DlnaProfileType.Audio
                },

                new DirectPlayProfile
                {
                    Type = DlnaProfileType.Photo,

                    Container = "jpeg,png,gif,bmp,tiff"
                }
            };
        }
    }
}
