//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using MediaBrowser.Model.Entities;
//
//namespace MediaBrowser.Theater.Api.Playback
//{
//    /// <summary>
//    /// Class SelectableMediaStream
//    /// </summary>
//    public class SelectableMediaStream : INotifyPropertyChanged
//    {
//        private bool _isActive;
//        private int _index;
//        private string _name;
//        private MediaStreamType _type;
//        private string _identifier;
//        private string _path;
//
//        /// <summary>
//        /// Gets or sets the index.
//        /// </summary>
//        /// <value>The index.</value>
//        public int Index
//        {
//            get { return _index; }
//            set
//            {
//                if (Equals(_index, value)) {
//                    return;
//                }
//
//                _index = value;
//                OnPropertyChanged();
//            }
//        }
//
//        /// <summary>
//        /// Gets or sets the name.
//        /// </summary>
//        /// <value>The name.</value>
//        public string Name
//        {
//            get { return _name; }
//            set
//            {
//                if (Equals(_name, value)) {
//                    return;
//                }
//
//                _name = value;
//                OnPropertyChanged();
//            }
//        }
//
//        /// <summary>
//        /// Gets or sets a value indicating whether this instance is active.
//        /// </summary>
//        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
//        public bool IsActive
//        {
//            get { return _isActive; }
//            set
//            {
//                if (Equals(_isActive, value)) {
//                    return;
//                }
//
//                _isActive = value;
//                OnPropertyChanged();
//            }
//        }
//
//        /// <summary>
//        /// Gets or sets the type.
//        /// </summary>
//        /// <value>The type.</value>
//        public MediaStreamType Type
//        {
//            get { return _type; }
//            set
//            {
//                if (Equals(_type, value)) {
//                    return;
//                }
//
//                _type = value;
//                OnPropertyChanged();
//            }
//        }
//
//        /// <summary>
//        /// Gets or sets the identifier.
//        /// </summary>
//        /// <value>The identifier.</value>
//        public string Identifier
//        {
//            get { return _identifier; }
//            set
//            {
//                if (Equals(_identifier, value)) {
//                    return;
//                }
//
//                _identifier = value;
//                OnPropertyChanged();
//            }
//        }
//
//        /// <summary>
//        /// Gets or sets the Path.
//        /// </summary>
//        /// <value>The Path - used for external subtitles .</value>
//        public string Path
//        {
//            get { return _path; }
//            set
//            {
//                if (Equals(_path, value)) {
//                    return;
//                }
//                
//                _path = value;
//                OnPropertyChanged();
//            }
//        }
//
//        public event PropertyChangedEventHandler PropertyChanged;
//
//        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            PropertyChangedEventHandler handler = PropertyChanged;
//            if (handler != null) {
//                handler(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }
//    }
//}
