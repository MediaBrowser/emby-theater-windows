using System;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Presentation
{
    public static class ImageTypeExtensions
    {
        public static float GetAspectRatio(this ImageType imageType, string itemType)
        {
            switch (imageType) {
                case ImageType.Primary:
                    return PrimaryImageAspectRatio(itemType);
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

        private static float PrimaryImageAspectRatio(string itemType)
        {
            switch (itemType) {
                case "Episode":
                    return 16f/9f;
                case "MusicAlbum":
                case "MusicArtist":
                case "Audio":
                    return 1;
                default:
                    return 1000f / 1426f;
            }
        }
    }
}