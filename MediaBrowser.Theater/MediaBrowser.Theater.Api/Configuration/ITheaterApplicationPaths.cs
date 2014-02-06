using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;

namespace MediaBrowser.Theater.Api.Configuration
{
    public interface ITheaterApplicationPaths
        : IApplicationPaths
    {
        string ThemesPath { get; }
    }
}
