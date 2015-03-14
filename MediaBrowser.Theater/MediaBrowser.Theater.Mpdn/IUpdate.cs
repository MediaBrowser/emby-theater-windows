using System;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;

namespace MediaBrowser.Theater.Mpdn
{
    public interface IUpdate
    {
        Version Version { get; }
        UpdateType Type { get; }
        Task Install(IProgress<double> progress, IHttpClient httpClient);
    }
}