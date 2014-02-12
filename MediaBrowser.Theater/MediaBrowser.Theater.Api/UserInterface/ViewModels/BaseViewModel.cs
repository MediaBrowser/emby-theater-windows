using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Annotations;

namespace MediaBrowser.Theater.Api.UserInterface.ViewModels
{
    public interface IRequiresInitialization
    {
        bool IsInitialized { get; }
        Task Initialize();
    }

    public interface IHasActivityStatus
    {
        bool IsActive { get; set; }
        bool IsClosed { get; set; }
    }

    /// <summary>
    ///     The base class for view models.
    /// </summary>
    public abstract class BaseViewModel
        : INotifyPropertyChanged, IRequiresInitialization, IHasActivityStatus
    {
        private static readonly Task Completed = Task.FromResult<object>(null);

        private bool _isActive;
        private bool _isClosed;
        private bool _isInitialized;

        /// <summary>
        ///     Gets or sets a value indicating if this view model has been closed.
        /// </summary>
        public virtual bool IsClosed
        {
            get { return _isClosed; }
            set
            {
                if (value.Equals(_isClosed)) {
                    return;
                }
                _isClosed = value;
                OnPropertyChanged();

                if (IsClosed) {
                    IsActive = false;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating if this view model is active.
        /// </summary>
        public virtual bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value.Equals(_isActive)) {
                    return;
                }
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets a value indicating if this view model has been initialized.
        /// </summary>
        public virtual bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                if (value.Equals(_isInitialized)) {
                    return;
                }
                _isInitialized = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Initializes this view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task Initialize()
        {
            if (IsClosed) {
                throw new InvalidOperationException("Cannot initalise a view model which has been closed.");
            }

            IsInitialized = true;
            return Completed;
        }

        /// <summary>
        ///     Notifies listeners that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property which has changed.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Action action = () => {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            };

            action.OnUiThread();
        }
    }
}