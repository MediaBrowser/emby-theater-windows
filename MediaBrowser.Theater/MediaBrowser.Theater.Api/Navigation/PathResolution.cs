using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Navigation
{
    public class PathResolution
    {
        private readonly Func<INavigationPath, Task<INavigationContext>> _handler;

        public PathResolution(Func<INavigationPath, Task<INavigationContext>> handler, IEnumerable<INavigationPath> excessPath, INavigationPath matchingPath)
        {
            _handler = handler;
            ExcessPath = excessPath;
            MatchingPath = matchingPath;
        }

        public Task<INavigationContext> Execute()
        {
            return _handler(MatchingPath);
        }

        public IEnumerable<INavigationPath> ExcessPath { get; private set; }
        public INavigationPath MatchingPath { get; private set; }
    }
}