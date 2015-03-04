using System;

namespace MediaBrowser.Theater.Api.System
{
    public static class ProgressExtensions
    {
        public static IProgress<double> Slice(this IProgress<double> progress, double min, double max)
        {
            return new Progress<double>(p => progress.Report(min + p*(max - min)));
        }
    }
}