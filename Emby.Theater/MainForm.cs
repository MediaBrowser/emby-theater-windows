using Emby.Theater.App;
using Emby.Theater.Configuration;
using MediaBrowser.Model.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emby.Theater.Common;
using Emby.Theater.DirectShowPlayer;
using Emby.Theater.Window;
using HttpListener = SocketHttpListener.Net.HttpListener;
using HttpListenerContext = SocketHttpListener.Net.HttpListenerContext;

namespace Emby.Theater
{
    public partial class MainForm : MainBaseForm
    {
        private readonly ILogger _logger;
        private readonly ITheaterConfigurationManager _config;
        private readonly ApplicationHost _appHost;

        private WindowSync _windowSync;
        private readonly Process _electronProcess;
        private HttpListener _listener;
        private DirectShowPlayerBridge _dsPlayerBridge;

        public MainForm(ILogger logger, ITheaterConfigurationManager config, ApplicationHost appHost, Process electronProcess)
        {
            _logger = logger;
            _config = config;
            _appHost = appHost;
            _electronProcess = electronProcess;
            InitializeComponent();

            SetWindowState();
        }

        private void SetWindowState()
        {
            var win = this;

            win.WindowState = FormWindowState.Normal;
            win.StartPosition = FormStartPosition.CenterScreen;
            win.MinimumSize = new Size(720, 480);
            win.Width = 1280;
            win.Height = 720;
            FormBorderStyle = FormBorderStyle.None;

            RECT rect = new RECT();
            var electronWindowHandle = _electronProcess.MainWindowHandle;
            NativeWindowMethods.GetWindowRect(electronWindowHandle, ref rect);

            if (win.WindowState == FormWindowState.Normal)
            {
                win.Top = rect.Top;
                win.Left = rect.Left;
                win.Width = Math.Abs(rect.Right - rect.Left);
                win.Height = Math.Abs(rect.Bottom - rect.Top);
            }
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int serverPort = StartServer();

            _dsPlayerBridge = new DirectShowPlayerBridge(_appHost.LogManager, this, _config.CommonApplicationPaths,
                _appHost.GetIsoManager(), _appHost.GetZipClient(), _appHost.GetHttpClient(), _config,
                _appHost.JsonSerializer);

            _windowSync = new WindowSync(this, _electronProcess.MainWindowHandle, _logger);
        }

        protected override void OnClosing(CancelEventArgs e)
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

            base.OnClosing(e);
        }

        private int StartServer()
        {
            //var serverPort = GetRandomUnusedPort();
            var serverPort = 8154;
 
            var listener = new HttpListener(new PatternsLogger(_logger), (string)null);

            listener.Prefixes.Add("http://localhost:" + serverPort + "/");
            listener.OnContext = ProcessContext;

            listener.Start();
            _listener = listener;

            return serverPort;
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void ProcessContext(HttpListenerContext context)
        {
            Task.Factory.StartNew(() => InitTask(context));
        }

        private void InitTask(HttpListenerContext context)
        {
            try
            {
                var task = this.ProcessRequestAsync(context);
                task.ContinueWith(x => HandleError(x.Exception, context), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);

                //if (task.Status == TaskStatus.Created)
                //{
                //    task.RunSynchronously();
                //}
            }
            catch (Exception ex)
            {
                HandleError(ex, context);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var localPath = request.Url.LocalPath.Trim('/');
            _logger.Info("Http {0} {1}", request.HttpMethod, localPath);

            try
            {
                if (string.Equals(localPath, "windowstate-maximized", StringComparison.OrdinalIgnoreCase))
                {
                    _windowSync.OnElectronWindowStateChanged("maximized");
                }
                else if (string.Equals(localPath, "windowstate-normal", StringComparison.OrdinalIgnoreCase))
                {
                    _windowSync.OnElectronWindowStateChanged("normal");
                }
                else if (string.Equals(localPath, "windowstate-minimized", StringComparison.OrdinalIgnoreCase))
                {
                    _windowSync.OnElectronWindowStateChanged("minimized");
                }
                else if (string.Equals(localPath, "windowstate-fullscreen", StringComparison.OrdinalIgnoreCase))
                {
                    _windowSync.OnElectronWindowStateChanged("fullscreen");
                }
                else if (string.Equals(localPath, "windowsize", StringComparison.OrdinalIgnoreCase))
                {
                    _windowSync.OnElectronWindowSizeChanged();
                }
                else if (localPath.StartsWith("directshowplayer", StringComparison.OrdinalIgnoreCase))
                {
                    await _dsPlayerBridge.ProcessRequest(context, localPath).ConfigureAwait(false);
                }
                else if (localPath.StartsWith("fileexists", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var path = reader.ReadToEnd();
                        var bytes = Encoding.UTF8.GetBytes(File.Exists(path).ToString().ToLower());
                        context.Response.ContentLength64 = bytes.Length;
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else if (localPath.StartsWith("directoryexists", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var path = reader.ReadToEnd();
                        var bytes = Encoding.UTF8.GetBytes(Directory.Exists(path).ToString().ToLower());
                        context.Response.ContentLength64 = bytes.Length;
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            finally
            {
                response.Close();
            }
        }

        private void HandleError(Exception ex, HttpListenerContext context)
        {
        }
    }
}
