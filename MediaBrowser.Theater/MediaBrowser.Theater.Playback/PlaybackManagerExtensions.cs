using System;
using System.Collections.Generic;
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

        public static Task Play(this IPlaybackManager playbackManager, Media media, bool enqueueIfAlreadyPlaying = false)
        {
            return playbackManager.Play(new[] { media }, enqueueIfAlreadyPlaying);
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