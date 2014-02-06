using System;
using System.Collections.Generic;

namespace MediaBrowser.Theater.Api.Theming
{
    public class ThemeInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public string Author { get; set; }
        public string IconUrl { get; set; }
        public List<string> ThumbImageUrls { get; set; }
        public List<string> ScreenshotUrls { get; set; }
    }
}