using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Interfaces.Configuration;
using System.IO;

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
        private readonly object _syncLock = new object();
        public ServerCredentialConfiguration GetServerCredentials()
        {
            if (_servers == null)
            {
                lock (_syncLock)
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
            }
            return _servers;
        }

        public void SaveServerCredentials(ServerCredentialConfiguration configuration)
        {
            var path = Path;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            lock (_syncLock)
            {
                _json.SerializeToFile(configuration, path);
            }
        }
    }
}
