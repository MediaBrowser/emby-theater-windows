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
    }
}
