using System;
using System.Windows.Input;

namespace SerializationTestApp.Base
{
    public class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Func<bool> _canExecuteAction;
        private readonly Action _executeAction;

        public Command(Action executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = () => true;
        }

        public Command(Action executeAction, Func<bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object? parameter = null)
        {
            return _canExecuteAction == null || _canExecuteAction();
        }

        public void Execute(object? parameter = null)
        {
            _executeAction?.Invoke();
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class Command<T> : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action<T?> _executeAction;
        private readonly Func<T?, bool> _canExecuteAction;

        public Command(Action<T?> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = _ => true;
        }

        public Command(Action<T?> executeAction, Func<T?, bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object? parameter)
        {
            return
                _canExecuteAction == null ||
                parameter == null ||
                parameter is T t && _canExecuteAction(t);
        }

        public void Execute(object? parameter)
        {
            _executeAction?.Invoke((T?)parameter);
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}