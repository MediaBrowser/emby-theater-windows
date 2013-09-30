using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.ViewModels;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class MediaStreamViewModel : BaseViewModel
    {
        private SelectableMediaStream _mediaStream;
        public SelectableMediaStream MediaStream
        {
            get { return _mediaStream; }

            set
            {
                var changed = _mediaStream != value;

                _mediaStream = value;

                if (changed)
                {
                    OnPropertyChanged("MediaStream");
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Type");
                }
            }
        }

        public string Name
        {
            get { return _mediaStream == null ? null : _mediaStream.Name; }
        }

        public MediaStreamType Type
        {
            get { return _mediaStream == null ? MediaStreamType.Video : _mediaStream.Type; }
        }
    }
}
