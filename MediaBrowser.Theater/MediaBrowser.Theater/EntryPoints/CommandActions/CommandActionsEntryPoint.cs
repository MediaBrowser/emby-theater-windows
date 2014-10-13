using System;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.System;

namespace MediaBrowser.Theater.EntryPoints.CommandActions
{
    public class CommandActionsEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly ICommandManager _commandManager;
        private readonly ICommandRouter _commandRouter;

        public CommandActionsEntryPoint(ICommandManager commandManager, ICommandRouter commandRouter)
        {
            _commandManager = commandManager;
            _commandRouter = commandRouter;
        }

        public void Dispose()
        {
            _commandManager.CommandReceived -= commandManager_CommandReceived;
        }

        public void Run()
        {
            _commandManager.CommandReceived += commandManager_CommandReceived;
        }

        private void commandManager_CommandReceived(object sender, CommandEventArgs commandEventArgs)
        {
            commandEventArgs.Handled = _commandRouter.RouteCommand(commandEventArgs.Command, commandEventArgs.Args);
        }
    }
}