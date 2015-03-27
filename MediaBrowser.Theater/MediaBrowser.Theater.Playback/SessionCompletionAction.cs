namespace MediaBrowser.Theater.Playback
{
    public struct SessionCompletionAction
    {
        public NavigationDirection Direction { get; set; }
        public int Index { get; set; }
    }

    public static class PlaySequenceNavigationExtensions
    {
        public static bool MoveNext(this IPlaySequence sequence, SessionCompletionAction action)
        {
            switch (action.Direction) {
                case NavigationDirection.Forward:
                    return sequence.Next();
                case NavigationDirection.Backward:
                    return sequence.Previous();
                case NavigationDirection.Skip:
                    return sequence.SkipTo(action.Index);
            }

            return false;
        }
    }
}