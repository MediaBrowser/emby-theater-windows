// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api
{
    public class AsyncSemaphore
    {
        private static readonly Task Completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            _currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return Completed;
                }

                var waiter = new TaskCompletionSource<bool>();
                _waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                    toRelease = _waiters.Dequeue();
                else
                    ++_currentCount;
            }
            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }

    public class AsyncLock
    {
        private readonly Task<Releaser> _releaser;
        private readonly AsyncSemaphore _semaphore;

        public AsyncLock()
        {
            _semaphore = new AsyncSemaphore(1);
            _releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            Task wait = _semaphore.WaitAsync();
            return wait.IsCompleted
                ? _releaser
                : wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser(AsyncLock toRelease)
            {
                _toRelease = toRelease;
            }

            public void Dispose()
            {
                if (_toRelease != null)
                    _toRelease._semaphore.Release();
            }
        }
    }
}