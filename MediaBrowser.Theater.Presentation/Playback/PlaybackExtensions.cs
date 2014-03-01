using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Linq;

namespace MediaBrowser.Theater.Presentation.Playback
{
    /// <summary>
    /// Class PlaybackExtensions
    /// </summary>
    public static class PlaybackExtensions
    {
        public static void GoToNextTrack(this IMediaPlayer player)
        {
            var index = player.CurrentPlaylistIndex;

            if (index < player.CurrentPlayOptions.Items.Count - 1)
            {
                player.ChangeTrack(index + 1);
            }
        }

        public static void GoToPreviousTrack(this IMediaPlayer player)
        {
            var index = player.CurrentPlaylistIndex;

            if (index > 0)
            {
                player.ChangeTrack(index - 1);
            }
        }

        /// <summary>
        /// Goes to next chapter.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>Task.</returns>
        public static void GoToNextChapter(this IMediaPlayer player)
        {
            var current = player.CurrentPositionTicks;

            var chapter = player.CurrentMedia.Chapters.FirstOrDefault(c => c.StartPositionTicks > current);

            if (chapter != null)
            {
                player.Seek(chapter.StartPositionTicks);
            }
        }

        /// <summary>
        /// Goes to previous chapter.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>Task.</returns>
        public static void GoToPreviousChapter(this IMediaPlayer player)
        {
            var current = player.CurrentPositionTicks;

            var ticksPerTenSeconds = TimeSpan.FromSeconds(10).Ticks;

            var chapter = player.CurrentMedia.Chapters.LastOrDefault(c => c.StartPositionTicks < current - ticksPerTenSeconds);

            if (chapter != null)
            {
                player.Seek(chapter.StartPositionTicks);
            }
        }

        /// <summary>
        /// Skips the forward.
        /// </summary>
        /// <param name="player">The player.</param>
        ///<param name="seconds"> Number of seconds to skip forward, defaults to 10.</param>
        /// <returns>Task.</returns>
        public static void SkipForward(this IMediaPlayer player, int seconds = 10)
        {
            var current = player.CurrentPositionTicks ?? 0;

            current += TimeSpan.FromSeconds(seconds).Ticks;

            if (current < 0)
            {
                current = 0;
            }

            player.Seek(current);
        }

        /// <summary>
        /// Skips the backward.
        /// </summary>
        /// <param name="player">The player.</param>
        ///<param name="seconds"> Number of seconds to skip forward, defaults to 10.</param>
        /// <returns>Task.</returns>
        public static void SkipBackward(this IMediaPlayer player, int seconds = 10)
        {
            var current = player.CurrentPositionTicks ?? 0;

            current -= TimeSpan.FromSeconds(seconds).Ticks;

            if (current < 0)
            {
                current = 0;
            }

            player.Seek(current);
        }
    }
}
