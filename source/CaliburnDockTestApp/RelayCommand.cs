using System;
using System.Diagnostics;
using System.Windows.Input;

/// <summary>
/// Used where Caliburn.Micro needs to be interfaced to ICommand.
/// </summary>
public class RelayCommand<T> : ICommand
{
	#region Fields

	private readonly Action<T> _execute;
	private readonly Predicate<T> _canExecute;

	#endregion // Fields

	#region Constructors

	public RelayCommand(Action<T> execute)
		: this(execute, null)
	{
	}

	public RelayCommand(Action<T> execute, Predicate<T> canExecute)
	{
		if (execute == null)
			throw new ArgumentNullException(nameof(execute));

		_execute = execute;
		_canExecute = canExecute;
	}
	#endregion // Constructors

	#region ICommand Members

	[DebuggerStepThrough]
	public virtual bool CanExecute(T parameter)
	{
		return _canExecute == null || _canExecute(parameter);
	}

	public event EventHandler CanExecuteChanged;

	public virtual void Execute(T parameter)
	{
		_execute(parameter);
	}

	bool ICommand.CanExecute(object parameter)
	{
		if (parameter != null && typeof(T).IsAssignableFrom(parameter.GetType()) == false)
			throw new ArgumentException($"RelayCommand.CanExecute: Invalid type {parameter.GetType().FullName} - Expecting {typeof(T).FullName}");
		return CanExecute((T)parameter);
	}

	void ICommand.Execute(object parameter)
	{
		Execute((T)parameter);
	}

	/// <summary>
	/// Method used to raise the <see cref="CanExecuteChanged"/> event
	/// to indicate that the return value of the <see cref="CanExecute"/>
	/// method has changed.
	/// </summary>
	public void RaiseCanExecuteChanged()
	{
		CommandManager.InvalidateRequerySuggested();
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
	#endregion // ICommand Members
}

public class RelayCommand : RelayCommand<object>
{
	public RelayCommand(Action<object> execute)
	: this(execute, null)
	{ }

	public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		: base(execute, canExecute)
	{ }
}