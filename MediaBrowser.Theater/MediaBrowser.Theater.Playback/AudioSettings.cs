using System;

namespace MediaBrowser.Theater.Playback
{
    public class AudioSettings
    {
        private bool _isMuted;
        private decimal _volume;

        public AudioSettings()
        {
            _isMuted = false;
            _volume = 100;
        }

        public decimal Volume
        {
            get { return _volume; }
            set
            {
                value = Math.Max(0, Math.Min(value, 100));
                if (Equals(_volume, value)) {
                    return;
                }

                _volume = value;
                OnVolumeChanged(this);
            }
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (Equals(_isMuted, value)) {
                    return;
                }

                _isMuted = value;
                OnVolumeChanged(this);
            }
        }

        public event Action<AudioSettings> VolumeChanged;

        protected virtual void OnVolumeChanged(AudioSettings obj)
        {
            Action<AudioSettings> handler = VolumeChanged;
            if (handler != null) {
                handler(obj);
            }
        }

        public void StepVolumeUp()
        {
            IsMuted = false;
            Volume += 2;
        }

        public void StepVolumeDown()
        {
            IsMuted = false;
            Volume -= 2;
        }
    }
}