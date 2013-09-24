using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using System.Collections.Generic;

namespace MediaBrowser.Theater.DirectShow
{
    public class PlayableItem
    {
        public string PlayablePath { get; set; }

        public BaseItemDto OriginalItem { get; set; }

        public bool IsVideo
        {
            get { return OriginalItem.IsVideo; }
        }

        public List<MediaStream> MediaStreams
        {
            get { return OriginalItem.MediaStreams; }
        }

        public IIsoMount IsoMount { get; set; }
    }
}
