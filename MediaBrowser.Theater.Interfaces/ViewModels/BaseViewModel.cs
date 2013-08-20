using MediaBrowser.Theater.Interfaces.Reflection;
using System.ComponentModel;

namespace MediaBrowser.Theater.Interfaces.ViewModels
{
    /// <summary>
    /// Represents a base ViewModel
    /// </summary>
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
