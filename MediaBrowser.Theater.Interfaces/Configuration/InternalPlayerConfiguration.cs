
namespace MediaBrowser.Theater.Interfaces.Configuration
{
    /// <summary>
    /// Class InternalPlayerConfiguration
    /// </summary>
    public class InternalPlayerConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable reclock].
        /// </summary>
        /// <value><c>true</c> if [enable reclock]; otherwise, <c>false</c>.</value>
        public bool EnableReclock { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable madvr].
        /// </summary>
        /// <value><c>true</c> if [enable madvr]; otherwise, <c>false</c>.</value>
        public bool EnableMadvr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable xy sub filter].
        /// </summary>
        /// <value><c>true</c> if [enable xy sub filter]; otherwise, <c>false</c>.</value>
        public bool EnableXySubFilter { get; set; }

        public VideoConfiguration VideoConfig { get; set; }

        public AudioConfiguration AudioConfig { get; set; }

        public InternalPlayerConfiguration()
        {
            //set defaults if necessary
            VideoConfig = new VideoConfiguration();
            AudioConfig = new AudioConfiguration();
        }
    }

    //add configuration values here as necessary
    public class VideoConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating overridden video HWA mode.
        /// </summary>
        /// <value><c>> -1</c> if user has overridden; otherwise, <c>-1</c>.</value>
        private int _hwaMode = -1;

        public int HwaMode
        {
            get
            {
                return _hwaMode;
            }
            set
            {
                _hwaMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value enabling madVR smooth motion.
        /// </summary>
        /// <value><c>true</c> to enable; otherwise, <c>false</c>.</value>
        private bool _useMadVrSmoothMotion = true;
        public bool UseMadVrSmoothMotion
        {
            get
            {
                return _useMadVrSmoothMotion;
            }
            set
            {
                _useMadVrSmoothMotion = value;
            }
        }

        /// <summary>
        /// Gets or sets madVR smooth motion mode.
        /// </summary>
        /// <value><c>avoidJudder</c>, <c>almostAlways</c> or <c>always</c>.</value>
        private string _madVrSmoothMotionMode = "avoidJudder";
        public string MadVrSmoothMotionMode
        {
            get
            {
                return _madVrSmoothMotionMode;
            }
            set
            {
                _madVrSmoothMotionMode = value;
            }
        }
    }

    //add configuration values here as necessary
    public class AudioConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable audio bitstreaming].
        /// </summary>
        /// <value><c>true</c> if [enable audio bitstreaming]; otherwise, <c>false</c>.</value>
        public bool EnableAudioBitstreaming { get; set; }
    }
}
