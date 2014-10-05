using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Interfaces.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.UI
{
    public class CredentialProvider : ICredentialProvider
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IJsonSerializer _json;

        public CredentialProvider(ITheaterConfigurationManager config, IJsonSerializer json)
        {
            _config = config;
            _json = json;
        }

        private string Path
        {
            get { return System.IO.Path.Combine(_config.CommonApplicationPaths.ConfigurationDirectoryPath, "servers.json"); }
        }

        private ServerCredentialConfiguration _servers;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        public async Task<ServerCredentialConfiguration> GetServerCredentials()
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
                            _servers = _json.DeserializeFromFile<ServerCredentialConfiguration>(Path);
                        }
                        catch (IOException)
                        {
                            _servers = new ServerCredentialConfiguration();
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

        public async Task SaveServerCredentials(ServerCredentialConfiguration configuration)
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
