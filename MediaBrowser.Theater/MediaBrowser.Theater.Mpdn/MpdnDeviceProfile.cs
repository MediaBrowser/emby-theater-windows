using System.Xml.Serialization;
using MediaBrowser.Model.Dlna;

namespace MediaBrowser.Theater.Mpdn
{
    [XmlRoot("Profile")]
    public class MpdnDeviceProfile : DeviceProfile
    {
        public MpdnDeviceProfile()
        {
            Name = "Media Browser Theater";

            MaxStreamingBitrate = 8000000;
            MaxStaticBitrate = 80000000;
            
            // MPDN only supports direct play, not streaming
            DirectPlayProfiles = new[] {
                new DirectPlayProfile {
                    Container = "avi,mkv,ts,mp4,mov,m4v,asf,wmv,mpg,mpeg,flv,3gp",
                    Type = DlnaProfileType.Video
                },
                new DirectPlayProfile {
                    Container = "mp3,mp4,flac,wma,asf,ogg,m4a,aac,wav,oga",
                    Type = DlnaProfileType.Audio
                },
                new DirectPlayProfile {
                    Container = "jpeg,png,gif,bmp,tiff",
                    Type = DlnaProfileType.Photo,
                }
            };
        }
    }
}