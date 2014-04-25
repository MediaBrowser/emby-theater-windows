namespace MediaBrowser.Theater.Interfaces.Configuration
{
    public class UserTheaterConfiguration
    {
        public string Theme { get; set; }
        public string HomePage { get; set; }
        public string Screensaver { get; set;  }
        public bool ShowBackButton { get; set; }
        public bool ShowExternalDiscApp { get; set; }

        public UserTheaterConfiguration()
        {
            ShowBackButton = true;
            ShowExternalDiscApp = true;
            Screensaver = "Backdrop";
        }
    }
}
