using System;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Presentation
{
    public static class ImageTypeExtensions
    {
        public static float GetAspectRatio(this ImageType imageType)
        {
            switch (imageType) {
                case ImageType.Primary:
                    return 1000f/1426f;
                case ImageType.Art:
                case ImageType.Backdrop:
                case ImageType.Screenshot:
                case ImageType.Thumb:
                case ImageType.Box:
                case ImageType.BoxRear:
                case ImageType.Chapter:
                case ImageType.Menu:
                    return 16f/9f;
                case ImageType.Banner:
                    return 1000f/185f;
                case ImageType.Logo:
                    return 800f/310f;
                case ImageType.Disc:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException("imageType");
            }
        }
    }
}