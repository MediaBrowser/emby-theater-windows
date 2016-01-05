using System.IO;
using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System.Text;
using CefSharp;

namespace Emby.Theater.App.ServerDiscovery
{
    class ServerDiscoveryHandler : IResourceHandler
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;

        public ServerDiscoveryHandler(ILogger logger, IJsonSerializer json)
        {
            _logger = logger;
            _json = json;
        }

        private Stream _stream;

        public async void FindServers(ICallback callback)
        {
            // Get the timeout value from the query string
            var result = await new ServerLocator(_logger).FindServers(1000).ConfigureAwait(false);

            var json = _json.SerializeToString(result);

            _stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            callback.Continue();
        }

        public Stream GetResponse(IResponse response, out long responseLength, out string redirectUrl)
        {
            responseLength = _stream.Length;
            redirectUrl = null;
            return _stream;
        }

        public bool ProcessRequestAsync(IRequest request, ICallback callback)
        {
            FindServers(callback);
            return true;
        }
    }
}
