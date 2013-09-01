using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IItemPage
    {
        BaseItemDto PageItem { get; }

        ViewType ViewType { get; }
    }
}
