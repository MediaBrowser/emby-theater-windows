using System.Threading.Async;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public sealed class LockedPlaybackSessionAccessor : IPlaybackSessionAccessor
    {
        private readonly IPlaybackSession _session;
        private readonly AsyncSemaphore _lock;

        public LockedPlaybackSessionAccessor(IPlaybackSession session, AsyncSemaphore semaphore)
        {
            _session = session;
            _lock = semaphore;
        }

        public void Dispose()
        {
            _lock.Release();
        }

        public IPlaybackSession Session
        {
            get { return _session; }
        }

        public static async Task<IPlaybackSessionAccessor> Wrap(IPlaybackSession session, AsyncSemaphore semaphore)
        {
            await semaphore.Wait();
            return new LockedPlaybackSessionAccessor(session, semaphore);
        }
    }
}