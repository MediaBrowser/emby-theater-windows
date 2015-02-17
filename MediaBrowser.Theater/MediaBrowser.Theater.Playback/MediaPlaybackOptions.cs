namespace MediaBrowser.Theater.Playback
{
    public struct MediaPlaybackOptions
    {
        public bool Resume { get; set; }
        public long? StartPositionTicks { get; set; }
    }
}