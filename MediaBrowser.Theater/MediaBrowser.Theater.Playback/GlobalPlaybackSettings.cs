namespace MediaBrowser.Theater.Playback
{
    public class GlobalPlaybackSettings
    {
        private readonly AudioSettings _audio;

        public GlobalPlaybackSettings()
        {
            _audio = new AudioSettings();
        }

        public AudioSettings Audio
        {
            get { return _audio; }
        }
    }
}