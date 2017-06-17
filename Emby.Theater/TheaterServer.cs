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
using Emby.Theater.DirectShowPlayer;
using MediaBrowser.Common;
using MediaBrowser.Model.Logging;
using SocketHttpListener.Net;

namespace Emby.Theater
{
    public class TheaterServer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ITheaterConfigurationManager _config;

        private readonly Process _electronProcess;
        private HttpListener _listener;
        private DirectShowPlayerBridge _dsPlayerBridge;
        private readonly ApplicationHost _appHost;

        public TheaterServer(ILogger logger, ITheaterConfigurationManager config, Process electronProcess, ApplicationHost appHost)
        {
            _logger = logger;
            _config = config;
            _electronProcess = electronProcess;
            _appHost = appHost;
        }

        public void StartServer(Dispatcher context)
        {
            //var serverPort = GetRandomUnusedPort();
            var serverPort = 8154;

            var listener = new HttpListener(new PatternsLogger(_logger), (string)null);

            listener.Prefixes.Add("http://localhost:" + serverPort + "/");
            listener.OnContext = ProcessContext;

            listener.Start();
            _listener = listener;

            _dsPlayerBridge = new DirectShowPlayerBridge(_appHost.LogManager, _config.CommonApplicationPaths,
                _appHost.GetIsoManager(), _appHost.GetZipClient(), _appHost.GetHttpClient(), _config,
                _appHost.JsonSerializer, context);
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
                if (string.Equals(localPath, "windowstate-maximized", StringComparison.OrdinalIgnoreCase))
                {
                    _dsPlayerBridge.HandleWindowSizeChanged();
                }
                else if (string.Equals(localPath, "windowstate-normal", StringComparison.OrdinalIgnoreCase))
                {
                    _dsPlayerBridge.HandleWindowSizeChanged();
                }
                else if (string.Equals(localPath, "windowstate-minimized", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (string.Equals(localPath, "windowstate-fullscreen", StringComparison.OrdinalIgnoreCase))
                {
                    _dsPlayerBridge.HandleWindowSizeChanged();
                }
                else if (string.Equals(localPath, "windowsize", StringComparison.OrdinalIgnoreCase))
                {
                    _dsPlayerBridge.HandleWindowSizeChanged();
                }
                else if (string.Equals(localPath, "runatstartup-true", StringComparison.OrdinalIgnoreCase))
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
                else if (localPath.StartsWith("directshowplayer", StringComparison.OrdinalIgnoreCase))
                {
                    _dsPlayerBridge.ProcessRequest(context, localPath);
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
                else if (localPath.StartsWith("mediafilterinfo", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var bytes = Encoding.UTF8.GetBytes(_dsPlayerBridge.MediaFilterPath);
                        context.Response.ContentLength64 = bytes.Length;
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else if (localPath.StartsWith("openmadvr", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Process.Start(Path.Combine(_dsPlayerBridge.MediaFilterPath, "madVR"));
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
            try
            {
                _electronProcess.CloseMainWindow();
            }
            catch (Exception ex)
            {

            }

            try
            {
                _electronProcess.WaitForExit(2000);
            }
            catch (Exception ex)
            {

            }

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
