using MediaBrowser.Model.Dto;
using System.ComponentModel;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Controls.Details
{
    /// <summary>
    /// Class BaseDetailsControl
    /// </summary>
    public abstract class BaseDetailsControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDetailsControl" /> class.
        /// </summary>
        protected BaseDetailsControl()
        {
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        
        /// <summary>
        /// The _item
        /// </summary>
        private BaseItemDto _item;
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                _item = value;
                OnPropertyChanged("Item");
                OnItemChanged();
            }
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected abstract void OnItemChanged();
    }
}
