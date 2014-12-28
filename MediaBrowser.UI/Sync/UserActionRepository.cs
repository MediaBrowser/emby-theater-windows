using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.Model.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.UI.Sync
{
    public class UserActionRepository : IUserActionRepository
    {
        private List<UserAction> _actions = new List<UserAction>();

        public Task Create(UserAction action)
        {
            _actions.Add(action);
            return Task.FromResult(true);
        }

        public Task Delete(UserAction action)
        {
            _actions = _actions.Where(i => !string.Equals(i.Id, action.Id, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(true);
        }

        public Task<IEnumerable<UserAction>> Get(string serverId)
        {
            return Task.FromResult<IEnumerable<UserAction>>(_actions.ToList());
        }
    }
}
