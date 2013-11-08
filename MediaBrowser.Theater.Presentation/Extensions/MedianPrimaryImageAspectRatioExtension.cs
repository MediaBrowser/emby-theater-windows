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
                var result = values.Median();

                // If really close to 2:3 (poster image), just return 2:3
                if (Math.Abs(0.66666666667 - result) <= .05)
                {
                    return 0.66666666667;
                }

                // If really close to 16:9 (episode image), just return 16:9
                if (Math.Abs(1.777777778 - result) <= .05)
                {
                    return 1.777777778;
                }

                // If really close to 1 (square image), just return 1
                if (Math.Abs(1 - result) <= .05)
                {
                    return 1;
                }

                // If really close to 4:3 (poster image), just return 2:3
                if (Math.Abs(1.33333333333 - result) <= .05)
                {
                    return 1.33333333333;
                }
            }

            return 1;
        }
    }
}
