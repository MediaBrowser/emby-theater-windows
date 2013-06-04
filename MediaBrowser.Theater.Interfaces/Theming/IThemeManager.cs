using System.Collections.Generic;

namespace MediaBrowser.Theater.Interfaces.Theming
{
    public interface IThemeManager
    {
        void AddParts(IEnumerable<ITheme> themes);

        IEnumerable<ITheme> Themes { get; }

        ITheme CurrentTheme { get; }

        void SetCurrentTheme(ITheme theme);
    }
}
