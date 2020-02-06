using System;
using System.Windows.Input;

namespace AvalonDock.MVVMTestApp
{
	internal class RelayCommand : ICommand
	{
		#region fields

		readonly Action<object> _execute;
		readonly Predicate<object> _canExecute;

		#endregion fields

		#region Constructors

		public RelayCommand(Action<object> execute) : this(execute, null)
		{
		}

		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}
		#endregion Constructors

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			return _canExecute?.Invoke(parameter) ?? true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		#endregion ICommand Members
	}
}
