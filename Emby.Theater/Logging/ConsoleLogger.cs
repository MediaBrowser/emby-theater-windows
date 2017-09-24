using System;
using MediaBrowser.Model.Logging;

namespace Emby.Theater.Logging
{
    public class ConsoleLogger : IConsoleLogger
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
