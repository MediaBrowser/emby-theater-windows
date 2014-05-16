using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IViewModel : INotifyPropertyChanged, IRequiresInitialization
    {
        /// <summary>
        ///     Gets a value indicating if this view model is active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        ///     An event which is triggered when this view model is initialized.
        /// </summary>
        event EventHandler Initialized;

        /// <summary>
        ///     An event which is trigged when this view model is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        ///     Marks this view model as closed.
        /// </summary>
        /// <returns>A task which represents the asynchronous operation.</returns>
        Task Close();
    }
}