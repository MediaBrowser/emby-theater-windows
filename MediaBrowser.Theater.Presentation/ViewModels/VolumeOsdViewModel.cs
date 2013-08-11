using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Threading;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class VolumeOsdViewModel : BaseViewModel, IDisposable
    {
        private readonly IPlaybackManager _playbackManager;

        private Timer _overlayTimer;
        private readonly object _timerLock = new object();
        
        private float _volume;
        public float Volume
        {
            get { return _volume; }
            set
            {
                var changed = !_volume.Equals(value);
                _volume = value;

                if (changed)
                {
                    OnPropertyChanged("Volume");
                }
            }
        }

        private bool _isMuted;
        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                var changed = !_isMuted.Equals(value);
                
                _isMuted = value;

                if (changed)
                {
                    OnPropertyChanged("IsMuted");
                }
            }
        }

        private bool _isRecentlyUpdated;
        public bool IsRecentlyUpdated
        {
            get { return _isRecentlyUpdated; }
            set
            {
                var changed = !_isRecentlyUpdated.Equals(value);
                _isRecentlyUpdated = value;

                if (changed)
                {
                    OnPropertyChanged("IsRecentlyUpdated");
                }
            }
        }

        public VolumeOsdViewModel(IPlaybackManager playbackManager)
        {
            _playbackManager = playbackManager;

            _playbackManager.VolumeChanged += _playbackManager_VolumeChanged;
        }

        public void Dispose()
        {
            _playbackManager.VolumeChanged -= _playbackManager_VolumeChanged;

            DisposeTimer();
        }

        void _playbackManager_VolumeChanged(object sender, EventArgs e)
        {
            Volume = _playbackManager.Volume;
            IsMuted = _playbackManager.IsMuted;

            IsRecentlyUpdated = true;

            lock (_timerLock)
            {
                if (_overlayTimer == null)
                {
                    _overlayTimer = new Timer(TimerCallback, null, 3000, Timeout.Infinite);
                }
                else
                {
                    _overlayTimer.Change(3000, Timeout.Infinite);
                }
            }
        }

        private void TimerCallback(object state)
        {
            IsRecentlyUpdated = false;

            DisposeTimer();
        }

        private void DisposeTimer()
        {
            lock (_timerLock)
            {
                if (_overlayTimer != null)
                {
                    _overlayTimer.Dispose();
                    _overlayTimer = null;
                }
            }
        }
    }
}
