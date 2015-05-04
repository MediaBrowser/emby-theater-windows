using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class ItemCommandsManager : IItemCommandsManager
    {
        private readonly IApplicationHost _host;

        public ItemCommandsManager(IApplicationHost host)
        {
            _host = host;
        }

        public async Task<IEnumerable<IItemCommand>> GetCommands(BaseItemDto item)
        {
            var commands = _host.GetExports<IItemCommand>().ToList();

            foreach (var c in commands) {
                await c.Initialize(item);
            }

            return commands.Where(c => c.IsEnabled).OrderBy(c => c.SortOrder);
        }

        public void ShowItemCommandsMenu(BaseItemDto item)
        {
            throw new NotImplementedException();
        }
    }
}