using MediaBrowser.ApiInteraction.Cryptography;
using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace MediaBrowser.UI.Sync
{
    public class LocalAssetManagerFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly IApplicationPaths _appPaths;
        private readonly IJsonSerializer _json;
        private readonly ILogger _logger;

        public LocalAssetManagerFactory(IFileSystem fileSystem, IApplicationPaths appPaths, IJsonSerializer json, ILogger logger)
        {
            _fileSystem = fileSystem;
            _appPaths = appPaths;
            _json = json;
            _logger = logger;
        }

        public LocalAssetManager GetLocalAssetManager()
        {
            var userActionRepo = new UserActionRepository();
            var itemRepo = new ItemRepository(_appPaths, _json);
            var fileStorage = new FileStorage(_fileSystem, _appPaths);

            return new LocalAssetManager(userActionRepo, itemRepo, fileStorage, new CryptographyProvider(), _logger);
        }
    }
}
