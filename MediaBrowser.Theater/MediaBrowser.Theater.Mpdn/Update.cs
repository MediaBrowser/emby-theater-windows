using System;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;

namespace MediaBrowser.Theater.Mpdn
{
    public class Update : IUpdate
    {
        public static readonly IUpdate Unavailable = new Update(UpdateType.Unavailable);
        public static readonly IUpdate UpToDate = new Update(UpdateType.UpToDate);

        public Version Version
        {
            get { return new Version(0, 0, 0); }
        }

        public UpdateType Type { get; private set; }

        public Task Install(IProgress<double> progress, IHttpClient httpClient)
        {
            progress.Report(0);
            return Task.FromResult(0);
        }

        public Update(UpdateType type)
        {
            Type = type;
        }
    }
}