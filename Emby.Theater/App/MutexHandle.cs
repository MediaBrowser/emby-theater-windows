using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MediaBrowser.Model.Logging;

namespace Emby.Theater.App
{
    public class MutexHandle : IDisposable
    {
        private Mutex _mutex;
        private bool _hasHandle;
        private ILogger _logger;

        public MutexHandle(Mutex mutes, bool hasHandle, ILogger logger)
        {
            _mutex = mutes;
            _hasHandle = hasHandle;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_hasHandle)
            {
                _logger.Info("Releasing mutex");
                _mutex.ReleaseMutex();
                _logger.Info("Mutex released");
            }
            GC.SuppressFinalize(this);
        }
    }
}
