using System;

namespace MediaBrowser.Theater.Api.System
{
    public interface IDisposableProgress<in T> : IProgress<T>, IDisposable { }

    public static class ProgressExtensions
    {
        public static IDisposableProgress<double> Slice(this IProgress<double> progress, double min, double max)
        {
            return new ProgressProxy<double>(p => progress.Report(min + p*(max - min)));
        }

        public static IDisposableProgress<double> Clamp(this IProgress<double> progress, double min = 0, double max = 1)
        {
            return new ProgressProxy<double>(p => progress.Report(Math.Max(min, Math.Min(p, max))));
        }

        public static IDisposableProgress<T> Disposable<T>(this IProgress<T> progress)
        {
            return new ProgressProxy<T>(progress.Report);
        }

        private class ProgressProxy<T> : IDisposableProgress<T>
        {
            private Action<T> _action;

            public ProgressProxy(Action<T> action)
            {
                _action = action;
            }

            public void Report(T value)
            {
                if (_action != null) {
                    _action(value);
                }
            }

            public void Dispose()
            {
                _action = null;
            }
        }
    }
}