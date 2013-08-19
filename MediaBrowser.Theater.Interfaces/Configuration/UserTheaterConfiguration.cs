namespace MediaBrowser.Theater.Interfaces.Configuration
{
    public class UserTheaterConfiguration
    {
        public string NavigationTransition { get; set; }
        
        public string Theme { get; set; }
        public string HomePage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationConfiguration" /> class.
        /// </summary>
        public UserTheaterConfiguration()
        {
            NavigationTransition = "horizontal slide";
        }
    }
}
