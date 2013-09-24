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
            if (string.Equals(filename, "mpc-hc.exe"))
            {
                return "{PATH} /play /close /fullscreen";
            }

            // GAMES
            if (string.Equals(filename, "mupen64.exe"))
            {
                return "-g {PATH} -nogui";
            }

            if (string.Equals(filename, "1964.exe"))
            {
                return "-f -g {PATH}";
            }

            if (string.Equals(filename, "dolphin.exe"))
            {
                return "--exec={PATH}";
            }

            if (string.Equals(filename, "fusion.exe"))
            {
                return "-fullscreen {PATH}";
            }

            if (string.Equals(filename, "epsxe.exe"))
            {
                return "-nogui -loadbin {PATH}";
            }

            // They add the release version to the filename ie: pcsx2-r5350.exe
            if (filename.ToLowerInvariant().Contains("pcsx2"))
            {
                return "--nogui --fullscreen {PATH}";
            }

            // BOOKS

            return string.Empty;
        }
    }
}
