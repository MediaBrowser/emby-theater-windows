using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WPFLocalizeExtension.Extensions;

namespace MediaBrowser.Theater.Presentation
{
    public static class LocalizationExtensions
    {
        public static string Localize(this string key)
        {
            string localized;
            
            var locExtension = new LocExtension(key);
            locExtension.ResolveLocalizedValue(out localized);
            return localized;
        }

        public static string LocalizeFormat(this string key, params object[] arguments)
        {
            var localized = key.Localize();
            return string.Format(localized, arguments);
        }

        public static string Localize(this DayOfWeek day, CultureInfo culture = null)
        {
            return (culture ?? CultureInfo.CurrentUICulture).DateTimeFormat.DayNames[(int) day];
        }

        public static string ToLocalizedList(this IEnumerable<string> items) 
        {
            var itemList = items as IList<string> ?? items.ToList();

            if (itemList.Count == 0) {
                return string.Empty;
            }

            if (itemList.Count == 1) {
                return itemList[0];
            }

            if (itemList.Count == 2) {
                return "MediaBrowser.Theater.Presentation:Strings:Lists_TwoItems".LocalizeFormat(itemList[0], itemList[1]);
            }

            var middle = CreateMiddleItems(itemList.Skip(1).Take(itemList.Count - 2));
            var end = "MediaBrowser.Theater.Presentation:Strings:Lists_End".LocalizeFormat(middle, itemList[itemList.Count - 1]);
            var complete = "MediaBrowser.Theater.Presentation:Strings:Lists_Start".LocalizeFormat(itemList[0], end);

            return complete;
        }

        private static string CreateMiddleItems(IEnumerable<string> items)
        {
            var itemList = items as IList<string> ?? items.ToList();

            if (itemList.Count == 1) {
                return itemList[0];
            }

            var rest = CreateMiddleItems(itemList.Skip(1));
            return "MediaBrowser.Theater.Presentation:Strings:Lists_Middle".LocalizeFormat(itemList[0], rest);
        }
    }
}
