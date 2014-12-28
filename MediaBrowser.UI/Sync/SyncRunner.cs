using MediaBrowser.ApiInteraction.Sync;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.IO;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.UI.Sync
{
    public class SyncRunner
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IApplicationPaths _appPaths;
        private readonly IJsonSerializer _json;

        public SyncRunner(IConnectionManager connectionManager, ILogger logger, IFileSystem fileSystem, IApplicationPaths appPaths, IJsonSerializer json)
        {
            _connectionManager = connectionManager;
            _logger = logger;
            _fileSystem = fileSystem;
            _appPaths = appPaths;
            _json = json;
        }

        public Task Run(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var manager = new LocalAssetManagerFactory(_fileSystem, _appPaths, _json).GetLocalAssetManager();

            var sync = new MultiServerSync(_connectionManager, _logger, manager);

            return sync.Sync(progress, cancellationToken);
        }
    }
}
