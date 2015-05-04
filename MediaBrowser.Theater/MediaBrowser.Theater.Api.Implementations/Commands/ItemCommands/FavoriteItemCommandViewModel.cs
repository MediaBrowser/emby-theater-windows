using System.ComponentModel;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class FavoriteItemCommandViewModel : BaseViewModel
    {
        private readonly PropertyChangedEventHandler _isLikedChanged;
        private readonly BaseItemDto _item;
        private readonly PropertyChangedEventHandler _userDataChanged;
        private UserItemDataDto _data;
        private bool _isFavorite;

        public FavoriteItemCommandViewModel(BaseItemDto item)
        {
            _item = item;
            _data = item.UserData;

            _isLikedChanged = (s, e) => {
                if (e.PropertyName == "IsFavorite") {
                    UpdateIsFavorite(item);
                }
            };

            _userDataChanged = (s, e) => {
                if (e.PropertyName == "UserData") {
                    _data.PropertyChanged -= _isLikedChanged;
                    _data = item.UserData;
                    _data.PropertyChanged += _isLikedChanged;

                    UpdateIsFavorite(item);
                }
            };

            _data.PropertyChanged += _isLikedChanged;
            _item.PropertyChanged += _userDataChanged;

            UpdateIsFavorite(item);
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            private set
            {
                if (Equals(_isFavorite, value)) {
                    return;
                }

                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        protected override void OnClosed()
        {
            _item.PropertyChanged -= _userDataChanged;
            _data.PropertyChanged -= _isLikedChanged;

            base.OnClosed();
        }

        private void UpdateIsFavorite(BaseItemDto item)
        {
            IsFavorite = item.UserData.IsFavorite;
        }
    }
}