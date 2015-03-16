using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public static class PlaybackManagerExtensions
    {
        public static async Task Play(this IPlaybackManager playbackManager, IEnumerable<Media> media, bool enqueueIfAlreadyPlaying = false)
        {
            var queued = enqueueIfAlreadyPlaying && await playbackManager.AccessSession(session => {
                playbackManager.Queue.AddRange(media);
                session.Play();
            });

            if (!queued) {
                await playbackManager.StopPlayback();

                playbackManager.Queue.Clear();
                playbackManager.Queue.AddRange(media);
                playbackManager.BeginPlayback();
            }
        }

        public static async Task Play(this IPlaybackManager playbackManager, Media media, bool enqueueIfAlreadyPlaying = false, bool? enqueueIntros = null)
        {
            if (enqueueIntros ?? (!enqueueIfAlreadyPlaying && ShouldPlayIntros(media))) {
                var intros = await playbackManager.GetIntros(media.Item);
                await playbackManager.Play(new[] { media }.Concat(intros.Select(i => Media.Create(i))));
            } else {
                await playbackManager.Play(new[] { media }, enqueueIfAlreadyPlaying);
            }
        }

        private static bool ShouldPlayIntros(Media media)
        {
            return (media.Options.StartPositionTicks ?? 0) == 0 &&
                   media.Item.IsVideo;
        }

        public static Task<bool> AccessSession(this IPlaybackManager playbackManager, Action<IPlaybackSession> action)
        {
            return playbackManager.AccessSession(s => {
                action(s);
                return Task.FromResult(0);
            });
        }
    }
}