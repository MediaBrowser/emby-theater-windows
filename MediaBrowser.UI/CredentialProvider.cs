using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Interfaces.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.UI
{
    public class CredentialProvider : ICredentialProvider
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IJsonSerializer _json;
        private readonly ILogger _logger;

        public CredentialProvider(ITheaterConfigurationManager config, IJsonSerializer json, ILogger logger)
        {
            _config = config;
            _json = json;
            _logger = logger;
        }

        private string Path
        {
            get { return System.IO.Path.Combine(_config.CommonApplicationPaths.ConfigurationDirectoryPath, "servers.json"); }
        }

        private ServerCredentials _servers;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        public async Task<ServerCredentials> GetServerCredentials()
        {
            if (_servers == null)
            {
                await _asyncLock.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (_servers == null)
                    {
                        try
                        {
                            _servers = _json.DeserializeFromFile<ServerCredentials>(Path);
                        }
                        catch (IOException)
                        {
                            _servers = new ServerCredentials();
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Error reading saved credentials", ex);
                            _servers = new ServerCredentials();
                        }
                    }
                }
                finally
                {
                    _asyncLock.Release();
                }
            }
            return _servers;
        }

        public async Task SaveServerCredentials(ServerCredentials configuration)
        {
            var path = Path;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            await _asyncLock.WaitAsync().ConfigureAwait(false);

            try
            {
                _json.SerializeToFile(configuration, path);
            }
            finally
            {
                _asyncLock.Release();
            }
        }
    }
}
