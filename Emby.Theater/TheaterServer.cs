using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Emby.Theater.App;
using Emby.Theater.Configuration;
using MediaBrowser.Model.Logging;
using SocketHttpListener.Net;

namespace Emby.Theater
{
    public class TheaterServer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ITheaterConfigurationManager _config;

        private HttpListener _listener;
        private readonly ApplicationHost _appHost;

        public TheaterServer(ILogger logger, ApplicationHost appHost)
        {
            _logger = logger;
            _config = (ITheaterConfigurationManager)appHost.ConfigurationManager;
            _appHost = appHost;
        }

        public void StartServer(Dispatcher context)
        {
            //var serverPort = GetRandomUnusedPort();
            var serverPort = 8154;

            var listener = new HttpListener(_logger, 
                _appHost.CryptographyProvider, 
                _appHost.SocketFactory, 
                _appHost.NetworkManager, 
                _appHost.TextEncoding, 
                _appHost.MemoryStreamFactory,
                _appHost.FileSystemManager,
                _appHost.EnvironmentInfo);

            listener.Prefixes.Add("http://localhost:" + serverPort + "/");
            listener.OnContext = ProcessContext;

            listener.Start();
            _listener = listener;
        }

        private void ProcessContext(HttpListenerContext context)
        {
            Task.Run(() => InitTask(context));
        }

        private void InitTask(HttpListenerContext context)
        {
            try
            {
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                HandleError(ex, context);
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var localPath = request.Url.LocalPath.Trim('/');

            var logRequest = !string.Equals(localPath, "directshowplayer/refresh", StringComparison.OrdinalIgnoreCase);

            if (logRequest)
            {
                _logger.Info("Http {0} {1}", request.HttpMethod, localPath);
            }

            try
            {
                if (string.Equals(localPath, "runatstartup-true", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_config.Configuration.RunAtStartup)
                    {
                        _config.Configuration.RunAtStartup = true;
                        _config.SaveConfiguration();
                    }
                }
                else if (string.Equals(localPath, "runatstartup-false", StringComparison.OrdinalIgnoreCase))
                {
                    if (_config.Configuration.RunAtStartup)
                    {
                        _config.Configuration.RunAtStartup = false;
                        _config.SaveConfiguration();
                    }
                }
                else if (localPath.StartsWith("fileexists", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var path = reader.ReadToEnd();
                        var exists = File.Exists(path);
                        var bytes = Encoding.UTF8.GetBytes(exists.ToString().ToLower());
                        context.Response.ContentLength64 = bytes.Length;
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else if (localPath.StartsWith("directoryexists", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var path = reader.ReadToEnd();
                        var exists = Directory.Exists(path);
                        var bytes = Encoding.UTF8.GetBytes(exists.ToString().ToLower());
                        context.Response.ContentLength64 = bytes.Length;
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            finally
            {
                if (logRequest)
                {
                    _logger.Info("Http Completed {0} {1}", request.HttpMethod, localPath);
                }

                response.Close();
            }
        }

        private void HandleError(Exception ex, HttpListenerContext context)
        {
        }

        public void Dispose()
        {
            if (_listener != null)
            {
                foreach (var prefix in _listener.Prefixes.ToList())
                {
                    _listener.Prefixes.Remove(prefix);
                }

                _listener.Close();
            }
        }
    }
}
