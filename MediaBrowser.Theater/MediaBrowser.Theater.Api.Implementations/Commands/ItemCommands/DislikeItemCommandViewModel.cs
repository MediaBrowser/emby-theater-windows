﻿using System.ComponentModel;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public class DislikeItemCommandViewModel : BaseViewModel
    {
        private readonly PropertyChangedEventHandler _isLikedChanged;
        private readonly BaseItemDto _item;
        private readonly PropertyChangedEventHandler _userDataChanged;
        private UserItemDataDto _data;
        private bool _isDisliked;

        public DislikeItemCommandViewModel(BaseItemDto item)
        {
            _item = item;
            _data = item.UserData;

            _isLikedChanged = (s, e) => {
                if (e.PropertyName == "Likes") {
                    UpdateIsLiked(item);
                }
            };

            _userDataChanged = (s, e) => {
                if (e.PropertyName == "UserData") {
                    _data.PropertyChanged -= _isLikedChanged;
                    _data = item.UserData;
                    _data.PropertyChanged += _isLikedChanged;

                    UpdateIsLiked(item);
                }
            };

            _data.PropertyChanged += _isLikedChanged;
            _item.PropertyChanged += _userDataChanged;

            UpdateIsLiked(item);
        }

        public bool IsDisliked
        {
            get { return _isDisliked; }
            private set
            {
                if (Equals(_isDisliked, value)) {
                    return;
                }

                _isDisliked = value;
                OnPropertyChanged();
            }
        }

        protected override void OnClosed()
        {
            _item.PropertyChanged -= _userDataChanged;
            _data.PropertyChanged -= _isLikedChanged;

            base.OnClosed();
        }

        private void UpdateIsLiked(BaseItemDto item)
        {
            IsDisliked = item.UserData.Likes == false;
        }
    }
}