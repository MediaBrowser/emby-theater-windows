using System.IO;
using System.Text;
using CefSharp;

namespace Emby.Theater.App.ServerDiscovery
{
    public class ServerDiscoveryJsHandler : IResourceHandler
    {
        public Stream GetResponse(IResponse response, out long responseLength, out string redirectUrl)
        {
            var path = GetType().Namespace + ".serverdiscovery.js";

            using (var stream = GetType().Assembly.GetManifestResourceStream(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    var js = reader.ReadToEnd();

                    response.MimeType = "text/javascript";

                    var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(js));
                    responseLength = responseStream.Length;
                    redirectUrl = null;
                    return responseStream;
                }
            }
        }

        public bool ProcessRequestAsync(IRequest request, ICallback callback)
        {
            callback.Continue();
            return true;
        }
    }
}
