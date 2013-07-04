using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Presentation.Extensions
{
    /// <summary>
    /// Class AspectRatioHelper
    /// </summary>
    public static class AspectRatioHelper
    {
        /// <summary>
        /// Gets the aspect ratio.
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="medianPrimaryImageAspectRatio">The median primary image aspect ratio.</param>
        /// <returns>System.Double.</returns>
        public static double GetAspectRatio(ImageType imageType, double medianPrimaryImageAspectRatio)
        {
            switch (imageType)
            {
                case ImageType.Art:
                    return 1.777777777777778;
                case ImageType.Backdrop:
                    return 1.777777777777778;
                case ImageType.Banner:
                    return 5.414285714285714;
                case ImageType.Disc:
                    return 1;
                case ImageType.Logo:
                    return 1.777777777777778;
                case ImageType.Primary:
                    return medianPrimaryImageAspectRatio;
                case ImageType.Thumb:
                    return 1.777777777777778;
                default:
                    return 1;
            }
        }
    }
}
