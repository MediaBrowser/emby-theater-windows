using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Api.Library
{
    public struct DisplayNameFormat
    {
        public readonly static DisplayNameFormat Default = new DisplayNameFormat(false, false);

        public bool IncludeParentName { get; set; }
        public bool ShortFormat { get; set; }

        public DisplayNameFormat(bool includeParentName, bool shortFormat)
            : this()
        {
            IncludeParentName = includeParentName;
            ShortFormat = shortFormat;
        }
    }

    public static class ItemDisplayName
    {
        public static string GetDisplayName(this BaseItemDto item, DisplayNameFormat? displayFormatOptions = null)
        {
            if (item == null) {
                return null;
            }

            var options = displayFormatOptions ?? DisplayNameFormat.Default;
            string format = null;

            if (item.IsType("Episode")) {
                if (options.IncludeParentName) {
                    format = "{4} ";
                }

                if (item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue) {
                    format += "S{3}, E{1}";

                    if (item.IndexNumberEnd.HasValue) {
                        format += "-{2}";
                    }
                }

                if (!options.ShortFormat) {
                    if (string.IsNullOrEmpty(format)) {
                        format = "{0}";
                    } else {
                        format += " - {0}";
                    }
                }
            }

            if (item.IsType("Track")) {
                if (options.ShortFormat) {
                    format = "{0}";
                } else {
                    format = "{1}. {0}";
                }

                if (options.IncludeParentName) {
                    format += " by {6}";
                }
            }

            if (item.IsType("Album")) {
                format = "{0}";

                if (options.IncludeParentName) {
                    format += " by {6}";
                }
            }

            if (string.IsNullOrEmpty(format)) {
                format = "{0}";
            }

            return string.Format(format,
                                 item.Name,
                                 item.IndexNumber,
                                 item.IndexNumberEnd,
                                 item.ParentIndexNumber,
                                 item.SeriesName,
                                 item.Album,
                                 item.AlbumArtist);
        }
    }
}
