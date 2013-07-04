using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Theater.Presentation.Extensions
{
    /// <summary>
    /// Class MedianExtension
    /// </summary>
    public static class MedianExtension
    {
        /// <summary>
        /// Medians the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="System.InvalidOperationException">Sequence contains no elements</exception>
        public static double Median(this IEnumerable<double> source)
        {
            var list = source.ToList();

            if (list.Count != 0)
            {
                var midpoint = (list.Count - 1) / 2;
                var sorted = list.OrderBy(n => n);
                var median = sorted.ElementAt(midpoint);

                if (list.Count % 2 == 0)
                {
                    median = (median + sorted.ElementAt(midpoint + 1)) / 2;
                }

                return median;
            }

            throw new InvalidOperationException("Sequence contains no elements");
        }
    }
}
