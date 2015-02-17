using System;
using System.IO;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Theater.Playback
{
    public static class DriveInfoExtensions
    {
        private static BaseItemDto AsBaseItemDto(DriveInfo drive)
        {
            string path;
            VideoType videoType;

            // check if there is a DVD
            if (Directory.Exists(drive.Name + @"\VIDEO_TS")) {
                path = drive.Name + @"\VIDEO_TS";
                videoType = VideoType.Dvd;
            } else if (Directory.Exists(drive.Name + @"\BDMV")) {
                path = drive.Name + @"\BDMV";
                videoType = VideoType.BluRay;
            } else {
                throw new ApplicationException("The disc in the player contains neither a DVD or BLueray directory");
            }

            return new BaseItemDto {
                Id = null,
                Type = "movie",
                VideoType = videoType,
                Path = path,
                Name = drive.VolumeLabel ?? String.Empty,
                MediaType = "Video"
            };
        }
    }
}