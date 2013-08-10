using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Header
{
    /// <summary>
    /// Interaction logic for VolumeOsd.xaml
    /// </summary>
    public partial class VolumeOsd : UserControl
    {
        internal static IPlaybackManager PlaybackManager { get; set; }

        private Timer _overlayTimer;
        private readonly object _timerLock = new object();
        
        public VolumeOsd()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            PlaybackManager.VolumeChanged += PlaybackManager_VolumeChanged;

            Unloaded += VolumeOsd_Unloaded;
        }

        void VolumeOsd_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeTimer();
        }

        void PlaybackManager_VolumeChanged(object sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                VolumeSlider.Value = PlaybackManager.Volume;
                MuteBar.Visibility = PlaybackManager.IsMuted ? Visibility.Visible : Visibility.Collapsed;
            });

            ShowOsd();
        }

        private void ShowOsd()
        {
            Dispatcher.InvokeAsync(() => MainPanel.Visibility = Visibility.Visible);

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
            Dispatcher.InvokeAsync(() => MainPanel.Visibility = Visibility.Collapsed);
            
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
