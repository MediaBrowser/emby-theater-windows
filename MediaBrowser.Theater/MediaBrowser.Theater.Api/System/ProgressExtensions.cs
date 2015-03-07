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

        public static IDisposableProgress<double> SlicePercent(this IProgress<double> progress, double min, double max)
        {
            return new ProgressProxy<double>(p => progress.Report(min + (p / 100.0) * (max - min)));
        }

        public static IDisposableProgress<double> Clamp(this IProgress<double> progress, double min = 0, double max = 1)
        {
            return new ProgressProxy<double>(p => progress.Report(Math.Max(min, Math.Min(p, max))));
        }

        public static IDisposableProgress<T> Disposable<T>(this IProgress<T> progress)
        {
            return new ProgressProxy<T>(progress.Report);
        }

        public static IDisposableProgress<TIn> Select<TIn, TOut>(this IProgress<TOut> progress, Func<TIn, TOut> selector)
        {
            return new ProgressProxy<TIn>(p => progress.Report(selector(p)));
        }

        public static IDisposableProgress<T> Select<T>(this IProgress<T> progress, Func<T, T> selector)
        {
            return new ProgressProxy<T>(p => progress.Report(selector(p)));
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