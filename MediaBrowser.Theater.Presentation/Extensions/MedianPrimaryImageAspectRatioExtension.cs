using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Theater.Presentation.Extensions
{
    /// <summary>
    /// Class MedianPrimaryImageAspectRatioExtension
    /// </summary>
    public static class MedianPrimaryImageAspectRatioExtension
    {
        /// <summary>
        /// Gets the median primary image aspect ratio.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="System.ArgumentNullException">items</exception>
        public static double MedianPrimaryImageAspectRatio(this IEnumerable<BaseItemDto> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            var values = items.Select(i => i.PrimaryImageAspectRatio ?? 0).Where(i => i > 0).ToList();

            if (values.Count > 0)
            {
                return values.Median();
            }

            return 1;
        }
    }
}
