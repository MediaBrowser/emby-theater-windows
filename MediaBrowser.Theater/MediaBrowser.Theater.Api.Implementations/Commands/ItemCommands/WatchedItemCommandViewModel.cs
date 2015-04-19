using System.ComponentModel;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class WatchedItemCommandViewModel : BaseViewModel
    {
        private readonly PropertyChangedEventHandler _isPlayedChanged;
        private readonly BaseItemDto _item;
        private readonly PropertyChangedEventHandler _userDataChanged;
        private UserItemDataDto _data;
        private bool _isPlayed;

        public WatchedItemCommandViewModel(BaseItemDto item)
        {
            _item = item;
            _data = item.UserData;

            _isPlayedChanged = (s, e) => {
                if (e.PropertyName == "Played") {
                    UpdateIsPlayed(item);
                }
            };

            _userDataChanged = (s, e) => {
                if (e.PropertyName == "UserData") {
                    _data.PropertyChanged -= _isPlayedChanged;
                    _data = item.UserData;
                    _data.PropertyChanged += _isPlayedChanged;

                    UpdateIsPlayed(item);
                }
            };

            _data.PropertyChanged += _isPlayedChanged;
            _item.PropertyChanged += _userDataChanged;

            UpdateIsPlayed(item);
        }

        public bool IsPlayed
        {
            get { return _isPlayed; }
            private set
            {
                if (Equals(_isPlayed, value)) {
                    return;
                }

                _isPlayed = value;
                OnPropertyChanged();
            }
        }

        protected override void OnClosed()
        {
            _item.PropertyChanged -= _userDataChanged;
            _data.PropertyChanged -= _isPlayedChanged;

            base.OnClosed();
        }

        private void UpdateIsPlayed(BaseItemDto item)
        {
            IsPlayed = item.UserData.Played;
        }
    }
}