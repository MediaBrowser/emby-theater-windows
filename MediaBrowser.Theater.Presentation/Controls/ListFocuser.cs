using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// Class ListFocuser
    /// </summary>
    public class ListFocuser
    {
        /// <summary>
        /// The _list box
        /// </summary>
        private readonly ListBox _listBox;

        /// <summary>
        /// The _index
        /// </summary>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFocuser"/> class.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        public ListFocuser(ListBox listBox)
        {
            _listBox = listBox;
        }

        /// <summary>
        /// Focuses the after containers generated.
        /// </summary>
        /// <param name="index">The index.</param>
        public void FocusAfterContainersGenerated(int index)
        {
            _index = index;

            _listBox.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            _listBox.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        /// <summary>
        /// Handles the StatusChanged event of the ItemContainerGenerator control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        async void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            var container = (ItemContainerGenerator)sender;

            if (container.Status == GeneratorStatus.ContainersGenerated)
            {
                await _listBox.Dispatcher.InvokeAsync(() =>
                {
                    var item = _listBox.ItemContainerGenerator.ContainerFromIndex(_index) as ListBoxItem;

                    if (item != null)
                    {
                        item.Focus();
                        _listBox.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    }
                });
            }
        }
    }
}
