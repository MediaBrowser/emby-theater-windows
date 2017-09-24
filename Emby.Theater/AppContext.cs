using System.Windows.Forms;
using System.Windows.Threading;

namespace Emby.Theater
{
    public class AppContext : ApplicationContext
    {
        private readonly TheaterServer _server;

        public AppContext(TheaterServer server)
        {
            server.StartServer(Dispatcher.CurrentDispatcher);
            _server = server;
        }
    }
}
