namespace MediaBrowser.Theater.Playback
{
    public struct PlaybackCapabilities
    {
        public bool CanPlay { get; set; }
        public bool CanPause { get; set; }
        public bool CanStop { get; set; }
        public bool CanSeek { get; set; }
        public bool CanSkip { get; set; }
        public bool CanChangeStreams { get; set; }
        public bool CanChangeVolume { get; set; }
    }
}