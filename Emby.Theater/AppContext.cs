using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Emby.Theater
{
    public class AppContext : ApplicationContext
    {
        private readonly TheaterServer _server;
        private readonly Process _electronProcess;

        public AppContext(TheaterServer server, Process electronProcess)
        {
            server.StartServer(Dispatcher.CurrentDispatcher);
            _server = server;
            _electronProcess = electronProcess;

            Application.ApplicationExit += Application_ApplicationExit;
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
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

            try
            {
                _server.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
