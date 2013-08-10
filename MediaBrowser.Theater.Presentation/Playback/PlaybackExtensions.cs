using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Presentation.Playback
{
    /// <summary>
    /// Class PlaybackExtensions
    /// </summary>
    public static class PlaybackExtensions
    {
        /// <summary>
        /// The null task result
        /// </summary>
        private static readonly Task NullTaskResult = Task.FromResult(true);

        /// <summary>
        /// Goes to next chapter.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>Task.</returns>
        public static Task GoToNextChapter(this IMediaPlayer player)
        {
            var current = player.CurrentPositionTicks;

            var chapter = player.CurrentMedia.Chapters.FirstOrDefault(c => c.StartPositionTicks > current);

            return chapter != null ? player.Seek(chapter.StartPositionTicks) : NullTaskResult;
        }

        /// <summary>
        /// Goes to previous chapter.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>Task.</returns>
        public static Task GoToPreviousChapter(this IMediaPlayer player)
        {
            var current = player.CurrentPositionTicks;

            var ticksPerTenSeconds = TimeSpan.FromSeconds(10).Ticks;

            var chapter = player.CurrentMedia.Chapters.LastOrDefault(c => c.StartPositionTicks < current - ticksPerTenSeconds);

            return chapter != null ? player.Seek(chapter.StartPositionTicks) : NullTaskResult;
        }

        /// <summary>
        /// Skips the forward.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>Task.</returns>
        public static Task SkipForward(this IMediaPlayer player)
        {
            var current = player.CurrentPositionTicks ?? 0;

            current += TimeSpan.FromSeconds(10).Ticks;

            if (current < 0)
            {
                current = 0;
            }

            return player.Seek(current);
        }

        /// <summary>
        /// Skips the backward.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>Task.</returns>
        public static Task SkipBackward(this IMediaPlayer player)
        {
            var current = player.CurrentPositionTicks ?? 0;

            current -= TimeSpan.FromSeconds(10).Ticks;

            if (current < 0)
            {
                current = 0;
            }

            return player.Seek(current);
        }
    }
}
