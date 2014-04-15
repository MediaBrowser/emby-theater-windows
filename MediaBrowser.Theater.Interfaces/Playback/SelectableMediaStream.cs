using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.ViewModels;

namespace MediaBrowser.Theater.Interfaces.Playback
{
    /// <summary>
    /// Class SelectableMediaStream
    /// </summary>
    public class SelectableMediaStream : BaseViewModel
    {
        private bool _isActive;

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                OnPropertyChanged("IsActive");
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public MediaStreamType Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        /// <value>The Path - used for external subtitles .</value>
        public string Path { get; set; }
    }
}
