
namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface ISupportsThemeMedia
    {
    }

    public interface ISupportsItemThemeMedia : ISupportsThemeMedia
    {
        string ThemeMediaItemId { get; }
    }
}
