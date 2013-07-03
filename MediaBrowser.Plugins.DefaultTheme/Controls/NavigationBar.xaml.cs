using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for NavigationBar.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        internal static IPlaybackManager PlaybackManager { get; set; }

        /// <summary>
        /// Gets or sets the current player.
        /// </summary>
        /// <value>The current player.</value>
        private IMediaPlayer CurrentPlayer { get; set; }

        /// <summary>
        /// Gets or sets the current position timer.
        /// </summary>
        /// <value>The current position timer.</value>
        private Timer CurrentPositionTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationBar" /> class.
        /// </summary>
        public NavigationBar()
        {
            InitializeComponent();

            Loaded += NavigationBar_Loaded;
        }

        /// <summary>
        /// The _is mouse idle
        /// </summary>
        private bool _isMouseIdle = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse idle.
        /// </summary>
        /// <value><c>true</c> if this instance is mouse idle; otherwise, <c>false</c>.</value>
        public bool IsMouseIdle
        {
            get { return _isMouseIdle; }
            set
            {
                var changed = _isMouseIdle != value;

                _isMouseIdle = value;

                if (changed)
                {
                    Dispatcher.InvokeAsync(() => NavBarGrid.Visibility = value ? Visibility.Collapsed : Visibility.Visible);
                }
            }
        }
        
        /// <summary>
        /// Handles the Loaded event of the NavigationBar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void NavigationBar_Loaded(object sender, RoutedEventArgs e)
        {
            MuteButton.Click += MuteButton_Click;

            VolumeDownButton.PreviewMouseDown += VolumeDownButton_Click;
            VolumeUpButton.PreviewMouseDown += VolumeUpButton_Click;

            StopButton.Click += StopButton_Click;
            PlayButton.Click += PlayButton_Click;
            PauseButton.Click += PauseButton_Click;

            NextChapterButton.Click += NextChapterButton_Click;
            PreviousChapterButton.Click += PreviousChapterButton_Click;

            PlaybackManager.PlaybackStarted += PlaybackManager_PlaybackStarted;
            PlaybackManager.PlaybackCompleted += PlaybackManager_PlaybackCompleted;

            CurrentPositionSlider.PreviewMouseUp += CurrentPositionSlider_PreviewMouseUp;
        }
        
        /// <summary>
        /// Handles the Click event of the PreviousChapterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void PreviousChapterButton_Click(object sender, RoutedEventArgs e)
        {
            //await CurrentPlayer.GoToPreviousChapter();
        }

        /// <summary>
        /// Handles the Click event of the NextChapterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void NextChapterButton_Click(object sender, RoutedEventArgs e)
        {
            //await CurrentPlayer.GoToNextChapter();
        }

        /// <summary>
        /// Handles the Click event of the PauseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            await CurrentPlayer.Pause();
        }

        /// <summary>
        /// Handles the Click event of the PlayButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await CurrentPlayer.UnPause();
        }

        /// <summary>
        /// Handles the Click event of the StopButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await CurrentPlayer.Stop();
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStartEventArgs" /> instance containing the event data.</param>
        void PlaybackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            if (e.Player == CurrentPlayer)
            {
                if (CurrentPositionTimer != null)
                {
                    CurrentPositionTimer.Dispose();
                }

                CurrentPlayer.PlayStateChanged -= CurrentPlayer_PlayStateChanged;
                CurrentPlayer = null;
                ResetButtonVisibilities(null);

                Dispatcher.InvokeAsync(() => TxtCurrentPosition.Text = string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the VolumeUpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void VolumeUpButton_Click(object sender, RoutedEventArgs e)
        {
            await CurrentPlayer.SetVolume(CurrentPlayer.Volume + 3);
        }

        /// <summary>
        /// Handles the Click event of the VolumeDownButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void VolumeDownButton_Click(object sender, RoutedEventArgs e)
        {
            await CurrentPlayer.SetVolume(CurrentPlayer.Volume - 3);
        }

        /// <summary>
        /// Handles the Click event of the MuteButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPlayer.CanControlVolume)
            {
                if (CurrentPlayer.IsMuted)
                {
                    await CurrentPlayer.Mute();
                }
                else
                {
                    await CurrentPlayer.UnMute();
                }
            }
        }

        /// <summary>
        /// Handles the PlaybackStarted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStartEventArgs" /> instance containing the event data.</param>
        void PlaybackManager_PlaybackStarted(object sender, PlaybackStartEventArgs e)
        {
            if (e.Player is IInternalMediaPlayer)
            {
                CurrentPlayer = e.Player;
                CurrentPlayer.PlayStateChanged += CurrentPlayer_PlayStateChanged;

                ResetButtonVisibilities(e.Player);

                Dispatcher.InvokeAsync(() =>
                {
                    var runtime = e.Player.CurrentMedia.RunTimeTicks ?? 0;
                    CurrentPositionSlider.Maximum = runtime;

                    TxtDuration.Text = GetTimeString(runtime);

                });

                CurrentPositionTimer = new Timer(CurrentPositionTimerCallback, null, 250, 250);
            }
        }

        /// <summary>
        /// Currents the position timer callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CurrentPositionTimerCallback(object state)
        {
            var time = string.Empty;

            var ticks = CurrentPlayer.CurrentPositionTicks;

            if (ticks.HasValue)
            {
                time = GetTimeString(ticks.Value);
            }

            Dispatcher.InvokeAsync(() =>
            {
                TxtCurrentPosition.Text = time;

                if (!_isPositionSliderDragging)
                {
                    CurrentPositionSlider.Value = ticks ?? 0;
                }
            });
        }

        /// <summary>
        /// Gets the time string.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>System.String.</returns>
        private string GetTimeString(long ticks)
        {
            var timespan = TimeSpan.FromTicks(ticks);

            return timespan.TotalHours >= 1 ? timespan.ToString("hh':'mm':'ss") : timespan.ToString("mm':'ss");
        }

        /// <summary>
        /// Handles the PlayStateChanged event of the CurrentPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void CurrentPlayer_PlayStateChanged(object sender, EventArgs e)
        {
            ResetButtonVisibilities(CurrentPlayer);
        }

        /// <summary>
        /// Resets the button visibilities.
        /// </summary>
        /// <param name="player">The player.</param>
        private async void ResetButtonVisibilities(IMediaPlayer player)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                PlayButton.Visibility = player != null && player.PlayState == PlayState.Paused ? Visibility.Visible : Visibility.Collapsed;
                PauseButton.Visibility = player != null && player.CanPause && player.PlayState == PlayState.Playing ? Visibility.Visible : Visibility.Collapsed;

                StopButton.Visibility = player != null ? Visibility.Visible : Visibility.Collapsed;
                MuteButton.Visibility = player != null && player.CanControlVolume ? Visibility.Visible : Visibility.Collapsed;
                VolumeUpButton.Visibility = player != null && player.CanControlVolume ? Visibility.Visible : Visibility.Collapsed;
                VolumeDownButton.Visibility = player != null && player.CanControlVolume ? Visibility.Visible : Visibility.Collapsed;

                var isSeekabke = player != null && player.CanSeek && player.CurrentMedia != null;
                SeekGrid.Visibility = isSeekabke ? Visibility.Visible : Visibility.Collapsed;

                var canSeekChapters = isSeekabke && player.CurrentMedia.Chapters != null && player.CurrentMedia.Chapters.Count > 1;

                NextChapterButton.Visibility = canSeekChapters ? Visibility.Visible : Visibility.Collapsed;
                PreviousChapterButton.Visibility = canSeekChapters ? Visibility.Visible : Visibility.Collapsed;
            });
        }
                
        /// <summary>
        /// The is position slider dragging
        /// </summary>
        private bool _isPositionSliderDragging;

        /// <summary>
        /// Handles the DragStarted event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragStartedEventArgs" /> instance containing the event data.</param>
        private void CurrentPositionSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isPositionSliderDragging = true;
        }

        /// <summary>
        /// Handles the DragCompleted event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragCompletedEventArgs" /> instance containing the event data.</param>
        private void CurrentPositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isPositionSliderDragging = false;

            //await CurrentPlayer.Seek(Convert.ToInt64(CurrentPositionSlider.Value));
        }

        /// <summary>
        /// Handles the PreviewMouseUp event of the CurrentPositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        async void CurrentPositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            await CurrentPlayer.Seek(Convert.ToInt64(CurrentPositionSlider.Value));
        }
    }
}
