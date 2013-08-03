using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for NavigationBar.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Gets or sets the current player.
        /// </summary>
        /// <value>The current player.</value>
        private readonly IInternalMediaPlayer _mediaPlayer;

        /// <summary>
        /// Gets or sets the current position timer.
        /// </summary>
        /// <value>The current position timer.</value>
        private Timer CurrentPositionTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationBar" /> class.
        /// </summary>
        public NavigationBar(IPlaybackManager playbackManager, IImageManager imageManager, IApiClient apiClient, IInternalMediaPlayer mediaPlayer)
        {
            _playbackManager = playbackManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _mediaPlayer = mediaPlayer;
            InitializeComponent();

            Loaded += NavigationBar_Loaded;
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

            _playbackManager.PlaybackCompleted += PlaybackManager_PlaybackCompleted;

            CurrentPositionSlider.PreviewMouseUp += CurrentPositionSlider_PreviewMouseUp;

            LoadPlayer();
        }

        private void LoadPlayer()
        {
            _mediaPlayer.PlayStateChanged += CurrentPlayer_PlayStateChanged;

            ResetButtonVisibilities(_mediaPlayer);

            Dispatcher.InvokeAsync(() =>
            {
                var runtime = _mediaPlayer.CurrentMedia.RunTimeTicks ?? 0;
                CurrentPositionSlider.Maximum = runtime;

                TxtDuration.Text = GetTimeString(runtime);
            });

            CurrentPositionTimer = new Timer(CurrentPositionTimerCallback, null, 250, 250);
        }

        /// <summary>
        /// Handles the Click event of the PreviousChapterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void PreviousChapterButton_Click(object sender, RoutedEventArgs e)
        {
            await _mediaPlayer.GoToPreviousChapter();
        }

        /// <summary>
        /// Handles the Click event of the NextChapterButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void NextChapterButton_Click(object sender, RoutedEventArgs e)
        {
            await _mediaPlayer.GoToNextChapter();
        }

        /// <summary>
        /// Handles the Click event of the PauseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            await _mediaPlayer.Pause();
        }

        /// <summary>
        /// Handles the Click event of the PlayButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await _mediaPlayer.UnPause();
        }

        /// <summary>
        /// Handles the Click event of the StopButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await _mediaPlayer.Stop();
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the PlaybackManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStartEventArgs" /> instance containing the event data.</param>
        void PlaybackManager_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            //var player = _mediaPlayer;

            //if (e.Player == player)
            //{
            //    if (CurrentPositionTimer != null)
            //    {
            //        CurrentPositionTimer.Dispose();
            //    }

            //    player.PlayStateChanged -= CurrentPlayer_PlayStateChanged;
            //    CurrentPlayer = null;
            //    ResetButtonVisibilities(null);

            //    Dispatcher.InvokeAsync(() =>
            //    {
            //        TxtCurrentPosition.Text = string.Empty;
            //        UpdateNowPlayingImage();
            //    });
            //}
        }

        /// <summary>
        /// Handles the Click event of the VolumeUpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void VolumeUpButton_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.VolumeStepUp();
        }

        /// <summary>
        /// Handles the Click event of the VolumeDownButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void VolumeDownButton_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.VolumeStepDown();
        }

        /// <summary>
        /// Handles the Click event of the MuteButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playbackManager.IsMuted)
            {
                _playbackManager.UnMute();
            }
            else
            {
                _playbackManager.Mute();
            }
        }

        /// <summary>
        /// Currents the position timer callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CurrentPositionTimerCallback(object state)
        {
            var time = string.Empty;

            var ticks = _mediaPlayer.CurrentPositionTicks;

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
            ResetButtonVisibilities(_mediaPlayer);
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
                MuteButton.Visibility = player != null ? Visibility.Visible : Visibility.Collapsed;
                VolumeUpButton.Visibility = player != null ? Visibility.Visible : Visibility.Collapsed;
                VolumeDownButton.Visibility = player != null ? Visibility.Visible : Visibility.Collapsed;

                var isSeekable = player != null && player.CanSeek && player.CurrentMedia != null;
                SeekGrid.Visibility = isSeekable ? Visibility.Visible : Visibility.Collapsed;

                var canSeekChapters = isSeekable && player.CurrentMedia.Chapters != null && player.CurrentMedia.Chapters.Count > 1;

                NextChapterButton.Visibility = canSeekChapters ? Visibility.Visible : Visibility.Collapsed;
                PreviousChapterButton.Visibility = canSeekChapters ? Visibility.Visible : Visibility.Collapsed;

            }, DispatcherPriority.Background);
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
            await _mediaPlayer.Seek(Convert.ToInt64(CurrentPositionSlider.Value));
        }
    }
}
