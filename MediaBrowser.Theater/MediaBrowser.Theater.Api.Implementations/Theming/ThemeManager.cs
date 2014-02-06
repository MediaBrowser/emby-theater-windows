using System;
using System.Collections.Generic;
using System.Reflection;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Theater.Api.Theming
{
    public class ThemeManager
        : IThemeManager
    {
        private readonly ILogger _logger;

        public ThemeManager(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<ThemeInfo> FindInstalledThemes()
        {
            throw new NotImplementedException();
        }

        public Assembly LoadSelectedTheme()
        {
            throw new NotImplementedException();
        }
    }
}