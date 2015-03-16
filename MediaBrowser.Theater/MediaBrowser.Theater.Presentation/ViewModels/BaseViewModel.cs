using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.Annotations;
using MediaBrowser.Theater.Presentation.Reflection;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public sealed class ViewModel
    {
        public static TimeSpan GetCloseDelay(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(CloseDelayProperty);
        }

        public static void SetCloseDelay(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(CloseDelayProperty, value);
        }

        public static readonly DependencyProperty CloseDelayProperty =
            DependencyProperty.RegisterAttached("CloseDelay", typeof(TimeSpan), typeof(ViewModel), new PropertyMetadata(TimeSpan.Zero, OnCloseDelayChanged));

        private static void OnCloseDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Control;
            if (control == null) {
                return;
            }

            if (control.DataContext == null) {
                RoutedEventHandler loaded = null;
                loaded = (sender, args) => {
                    SetDelay(control, (TimeSpan)e.NewValue);
                    control.Loaded -= loaded;
                };

                control.Loaded += loaded;
            }

            SetDelay(control, (TimeSpan) e.NewValue);
        }

        private static void SetDelay(Control control, TimeSpan delay)
        {
            var viewModel = control.DataContext as IHasCloseDelay;
            if (viewModel == null) {
                return;
            }

            viewModel.CloseDelay = delay;
        }
    }

    /// <summary>
    ///     The base class for view models.
    /// </summary>
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public abstract class BaseViewModel
        : IHasActivityStatus, IHasCloseDelay, IViewModel
    {
        private static readonly Task Completed = Task.FromResult<object>(null);
        private TimeSpan _closeDelay;

        private bool _isActive;
        private bool _isInitialized;

        /// <summary>
        ///     Gets or sets a value indicating if this view model is active.
        /// </summary>
        bool IHasActivityStatus.IsActive
        {
            get { return IsActive; }
            set { IsActive = value; }
        }

        /// <summary>
        ///     Gets a value indicating if this view model is active.
        /// </summary>
        public virtual bool IsActive
        {
            get { return _isActive; }
            protected set
            {
                if (value.Equals(_isActive))
                {
                    return;
                }
                _isActive = value;
                OnPropertyChanged();
            }
        }

        TimeSpan IHasCloseDelay.CloseDelay
        {
            get { return _closeDelay; }
            set { _closeDelay = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets a value indicating if this view model has been initialized.
        /// </summary>
        public virtual bool IsInitialized
        {
            get { return _isInitialized; }
            protected set
            {
                if (value.Equals(_isInitialized)) {
                    return;
                }
                _isInitialized = value;
                OnPropertyChanged();
                OnInitialized();
            }
        }

        /// <summary>
        ///     Initializes this view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task Initialize()
        {
            IsInitialized = true;
            return Completed;
        }

        /// <summary>
        ///     An event which is triggered when this view model is initialized.
        /// </summary>
        public event EventHandler Initialized;

        protected virtual void OnInitialized()
        {
            EventHandler handler = Initialized;
            if (handler != null) {
                Action execute = () => handler(this, EventArgs.Empty);
                execute.OnUiThread();
            }
        }

        /// <summary>
        ///     An event which is trigged when this view model is closed.
        /// </summary>
        public event EventHandler Closed;

        protected virtual void OnClosed()
        {
            EventHandler handler = Closed;
            if (handler != null) {
                Action execute = () => handler(this, EventArgs.Empty);
                execute.OnUiThread();
            }
        }

        /// <summary>
        ///     Notifies listeners that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property which has changed.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Action execute = () => {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            };

            execute.OnUiThread();
        }

        /// <summary>
        ///     Marks this view model as closed.
        /// </summary>
        /// <returns>A task which represents the asynchronous operation.</returns>
        public async Task Close()
        {
            IsActive = false;

            if (_closeDelay != TimeSpan.Zero) {
                await Task.Delay(_closeDelay);
            }

            OnClosed();
        }
    }
}