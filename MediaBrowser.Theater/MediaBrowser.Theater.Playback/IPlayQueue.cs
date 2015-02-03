using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlayQueue : INotifyCollectionChanged, IList<Media>
    {
        event Action<RepeatMode> RepeatModeChanged;
        event Action<SortMode> SortModeChanged;
        RepeatMode RepeatMode { get; set; }
        SortMode SortMode { get; set; }
        IPlaySequence GetPlayOrder();
    }
}