using System;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.Api.UserInterface.ViewModels
{
    public abstract class NotificationViewModel
        : BaseViewModel
    {
        private readonly TimeSpan _duration;

        protected NotificationViewModel(TimeSpan duration)
        {
            _duration = duration;
        }

        public override bool IsActive
        {
            get { return base.IsActive; }
            set
            {
                base.IsActive = value;
                if (value) {
                    Task.Run(async () => {
                        await Task.Delay(_duration);
                        IsActive = false;
                    });
                }
            }
        }
    }
}