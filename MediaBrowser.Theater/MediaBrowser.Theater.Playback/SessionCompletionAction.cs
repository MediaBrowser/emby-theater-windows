using System.Threading.Tasks;

namespace MediaBrowser.Theater.Playback
{
    public struct SessionCompletionAction
    {
        public NavigationDirection Direction { get; set; }
        public int Index { get; set; }
    }

    public static class PlaySequenceNavigationExtensions
    {
        public static Task<bool> MoveNext<T>(this IPlaySequence<T> sequence, SessionCompletionAction action)
        {
            switch (action.Direction) {
                case NavigationDirection.Forward:
                    return sequence.Next();
                case NavigationDirection.Backward:
                    return sequence.Previous();
                case NavigationDirection.Skip:
                    return sequence.SkipTo(action.Index);
            }

            return Task.FromResult(false);
        }
    }
}