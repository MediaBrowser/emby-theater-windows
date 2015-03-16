using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Theater.Playback
{
    public class PlayableMedia
    {
        public string Path { get; set; }
        public MediaSourceInfo Source { get; set; }
        public StreamInfo StreamInfo { get; set; }
        public IIsoMount IsoMount { get; set; }
        public Media Media { get; set; }
    }
}