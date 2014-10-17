using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.ComponentModel;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class ServerInfoViewModel : BaseViewModel
    {
        public string Name
        {
            get { return _item == null ? null : _item.Name; }
        }

        private ServerInfo _item;

        public ServerInfo Server
        {
            get { return _item; }

            set
            {
                var changed = _item != value;

                _item = value;

                if (changed)
                {
                    OnPropertyChanged("Server");
                    OnPropertyChanged("Name");
                }
            }
        }
    }
}
