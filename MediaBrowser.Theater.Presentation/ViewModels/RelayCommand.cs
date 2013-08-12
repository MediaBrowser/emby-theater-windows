using System;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class RelayCommand
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// The _action
        /// </summary>
        private readonly Action<object> _action;
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
