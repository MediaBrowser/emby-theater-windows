using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Threading;
using MediaBrowser.Theater.Interfaces.ViewModels;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Represents a view model that contains multiple items.
    /// This should be used if you want to display a button or list item that holds more than one item, 
    /// and cycle through them periodically.
    /// </summary>
    public class RotatingCollectionViewModel : BaseViewModel, IDisposable
    {
        private int RotationPeriodMs { get; set; }

        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }
        
        public RotatingCollectionViewModel(IApiClient apiClient, IImageManager imageManager, int rotationPeriodMs = 8000, int rotationDevaiationMs = 0)
            : base()
        {
            ImageManager = imageManager;
            ApiClient = apiClient;

            if (rotationDevaiationMs > 0)
            {
                rotationPeriodMs += new Random(Guid.NewGuid().GetHashCode()).Next(0 - rotationDevaiationMs, rotationDevaiationMs);
            }

            RotationPeriodMs = rotationPeriodMs;
        }

        /// <summary>
        /// Gets the timer that updates the current item
        /// </summary>
        private Timer _currentItemTimer;

        private readonly object _timerLock = new object();

        private BaseItemDto[] _items;
        /// <summary>
        /// Gets or sets the list of items
        /// </summary>
        public BaseItemDto[] Items
        {
            get { return _items; }
            set
            {
                _items = value ?? new BaseItemDto[] { };
                OnPropertyChanged("Items");
                CurrentItemIndex = Items.Length == 0 ? -1 : 0;

                ReloadTimer();
            }
        }

        private int _currentItemIndex;
        /// <summary>
        /// Gets or sets the index of the current item
        /// </summary>
        public int CurrentItemIndex
        {
            get { return _currentItemIndex; }
            set
            {
                _currentItemIndex = value;
                OnPropertyChanged("CurrentItemIndex");
                OnPropertyChanged("CurrentItem");
                OnPropertyChanged("NextItem");
            }
        }

        /// <summary>
        /// Gets the current item
        /// </summary>
        public BaseItemDto CurrentItem
        {
            get { return CurrentItemIndex == -1 ? null : Items[CurrentItemIndex]; }
        }

        /// <summary>
        /// Gets the next item
        /// </summary>
        public BaseItemDto NextItem
        {
            get
            {
                if (CurrentItem == null || CurrentItemIndex == -1)
                {
                    return null;
                }
                var index = CurrentItemIndex + 1;

                if (index >= Items.Length)
                {
                    index = 0;
                }

                return Items[index];
            }
        }

        /// <summary>
        /// Disposes the timer
        /// </summary>
        private void DisposeTimer()
        {
            lock (_timerLock)
            {
                if (_currentItemTimer != null)
                {
                    _currentItemTimer.Dispose();
                    _currentItemTimer = null;
                }
            }
        }

        /// <summary>
        /// Reloads the timer
        /// </summary>
        private void ReloadTimer()
        {
            // Don't bother unless there's at least two items
            if (Items.Length > 1)
            {
                lock (_timerLock)
                {
                    if (_currentItemTimer == null)
                    {
                        _currentItemTimer = new Timer(OnTimerFired, null, TimeSpan.FromMilliseconds(RotationPeriodMs), TimeSpan.FromMilliseconds(RotationPeriodMs));
                    }
                    else
                    {
                        _currentItemTimer.Change(TimeSpan.FromMilliseconds(RotationPeriodMs), TimeSpan.FromMilliseconds(RotationPeriodMs));
                    }
                }
            }
        }

        public void StopTimer()
        {
            DisposeTimer();
        }

        public void StartTimer()
        {
            ReloadTimer();
        }

        private void OnTimerFired(object state)
        {
            var newIndex = CurrentItemIndex + 1;

            if (newIndex >= Items.Length)
            {
                newIndex = 0;
            }

            CurrentItemIndex = newIndex;
        }

        /// <summary>
        /// Disposes the collection
        /// </summary>
        public void Dispose()
        {
            DisposeTimer();
        }
    }
}
