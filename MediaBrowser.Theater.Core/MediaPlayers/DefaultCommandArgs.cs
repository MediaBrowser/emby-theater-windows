using System;
using System.Collections.Generic;
using System.IO;

namespace MediaBrowser.Theater.Core.MediaPlayers
{
    public static class DefaultCommandArgs
    {
        public static string GetDefaultArgs(string playerPath)
        {
            var filename = Path.GetFileName(playerPath);

            if (string.IsNullOrEmpty(filename))
            {
                return string.Empty;
            }

            // MEDIA PLAYERS
            if (string.Equals(filename, "mpc-hc.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "{PATH} /play /close /fullscreen";
            }

            if (string.Equals(filename, "vlc.exe", StringComparison.OrdinalIgnoreCase))
            {
                return GetVlcArgs();
            }
            
            // GAMES
            if (string.Equals(filename, "1964.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "-f -g {PATH}";
            }

            if (string.Equals(filename, "dolphin.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "--exec={PATH}";
            }

            if (string.Equals(filename, "epsxe.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "-nogui -loadbin {PATH}";
            }

            if (string.Equals(filename, "fusion.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "-fullscreen {PATH}";
            }

            if (string.Equals(filename, "mupen64.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "-g {PATH} -nogui";
            }

            // They add the release version to the filename ie: pcsx2-r5350.exe
            if (filename.ToLowerInvariant().Contains("pcsx2"))
            {
                return "--nogui --fullscreen {PATH}";
            }

            if (string.Equals(filename, "zsnesw.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "-m {PATH}";
            }

            return "{PATH}";
        }

        private static string GetVlcArgs()
        {
            var args = new List<string>();

            args.Add("{PATH}");

            // Play in fullscreen
            args.Add("--fullscreen");
            // Keep the window on top of others
            args.Add("--video-on-top");
            // Start a new instance
            args.Add("--no-one-instance");
            // Close the player when playback finishes
            args.Add("--play-and-exit");
            // Disable system screen saver during playback
            args.Add("--disable-screensaver");

            // Keep the ui minimal
            args.Add("--qt-minimal-view");
            args.Add("--no-video-deco");
            args.Add("--no-playlist-tree");

            // OSD marquee font
            args.Add("--freetype-outline-thickness=6");

            // Disable the new version notification for this session
            args.Add("--no-qt-updates-notif");

            // Map the stop button on the remote to close the player
            args.Add("--global-key-quit=\"Media Stop\"");

            args.Add("--global-key-play=\"Media Play\"");
            args.Add("--global-key-pause=\"Media Pause\"");
            args.Add("--global-key-play-pause=\"Media Play Pause\"");

            args.Add("--global-key-vol-down=\"Volume Down\"");
            args.Add("--global-key-vol-up=\"Volume Up\"");
            args.Add("--global-key-vol-mute=\"Mute\"");

            args.Add("--key-nav-up=\"Up\"");
            args.Add("--key-nav-down=\"Down\"");
            args.Add("--key-nav-left=\"Left\"");
            args.Add("--key-nav-right=\"Right\"");
            args.Add("--key-nav-activate=\"Enter\"");

            args.Add("--global-key-jump-long=\"Media Prev Track\"");
            args.Add("--global-key-jump+long=\"Media Next Track\"");

            return string.Join(" ", args.ToArray());
        }
    }
}
