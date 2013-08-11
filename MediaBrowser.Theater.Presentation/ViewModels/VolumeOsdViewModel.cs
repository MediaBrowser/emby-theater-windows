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
                OnPropertyChanged("Volume");
                _volume = value;
            }
        }

        private bool _isMuted;
        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                OnPropertyChanged("IsMuted");
                _isMuted = value;
            }
        }

        private bool _isRecentlyUpdated;
        public bool IsRecentlyUpdated
        {
            get { return _isRecentlyUpdated; }
            set
            {
                OnPropertyChanged("IsRecentlyUpdated");
                _isRecentlyUpdated = value;
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
