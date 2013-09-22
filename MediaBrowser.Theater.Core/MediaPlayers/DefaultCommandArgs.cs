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

            if (string.Equals(filename, "mpc-hc.exe"))
            {
                return "{PATH} /play /close /fullscreen";
            }

            return string.Empty;
        }
    }
}
