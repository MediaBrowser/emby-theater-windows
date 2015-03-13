using MediaBrowser.Model.Dlna;

namespace MediaBrowser.Theater.DirectShow
{
    public class LocalPlayer : DefaultLocalPlayer
    {
        public override bool CanAccessUrl(string url, bool requiresCustomRequestHeaders)
        {
            return base.CanAccessUrl(url, requiresCustomRequestHeaders);
        }
    }
}
