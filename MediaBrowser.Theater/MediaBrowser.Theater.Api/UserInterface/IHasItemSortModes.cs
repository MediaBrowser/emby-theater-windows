using System.Collections.Generic;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IItemListSortMode
    {
        string DisplayName { get; }
        object GetSortKey(BaseItemDto item);
        object GetIndexKey(BaseItemDto item);
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public interface IHasItemSortModes
    {
        IItemListSortMode SortMode { get; set; }
        SortDirection SortDirection { get; set; }
        IEnumerable<IItemListSortMode> AvailableSortModes { get; }
    }
}