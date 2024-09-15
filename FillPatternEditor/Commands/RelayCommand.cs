using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FillPatternEditor.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute; // Action to execute the command
        private readonly Func<object, bool> _canExecute; // Function to check if the command can execute

        /// <summary>
        /// Gets the name of the command, used for display purposes.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Occurs when changes affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="canExecute">The function to determine if the command can execute (optional).</param>
        public RelayCommand(Action execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class with a command name.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="canExecute">The function to determine if the command can execute (optional).</param>
        public RelayCommand(Action execute, string commandName, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            CommandName = commandName;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        /// <returns>True if the command can execute, otherwise false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
