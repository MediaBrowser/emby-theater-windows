using System.Collections.Generic;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    public interface IPlaybackManager
    {
        void AddParts(IEnumerable<IPlaybackManager> mediaPlayers);

        IEnumerable<IPlaybackManager> MediaPlayers { get; }
    }
}
