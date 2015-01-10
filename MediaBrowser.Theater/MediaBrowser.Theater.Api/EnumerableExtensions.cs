using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Theater.Api
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            var rnd = new Random();
            return items.OrderBy(i => rnd.Next());
        }
    }
}