using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FillPatternEditor.Commands
{
    /// <summary>
    /// Represents an asynchronous command that implements ICommand and allows asynchronous execution.
    /// </summary>
    public class AsyncCommand : IAsyncCommand, ICommand
    {
        private bool _isExecuting;
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private readonly IErrorHandler _errorHandler;

        /// <summary>
        /// Occurs when changes affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the AsyncCommand class.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <param name="canExecute">The function to determine whether the command can execute.</param>
        /// <param name="errorHandler">Optional error handler for async execution.</param>
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null, IErrorHandler errorHandler = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        public bool CanExecute()
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (!CanExecute())
                return;

            try
            {
                _isExecuting = true;
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter) => CanExecute();

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync().FireAndForgetSafeAsync(_errorHandler);
        }
    }

    /// <summary>
    /// An interface representing an asynchronous command.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
    }

    /// <summary>
    /// Provides a mechanism for handling errors in asynchronous execution.
    /// </summary>
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }

    /// <summary>
    /// Extension method to safely fire and forget an asynchronous task.
    /// </summary>
    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task, IErrorHandler handler = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handler?.HandleError(ex);
            }
        }
    }
}
