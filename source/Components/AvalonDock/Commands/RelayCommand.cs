using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Represents the relay Command.
	/// </summary>
	/// <typeparam name="T">The t type.</typeparam>
	internal class RelayCommand<T> : ICommand
	{
		private readonly WeakFunc<T, bool> _canExecute;

		/// <summary>
		/// The execute field.
		/// </summary>
		private readonly WeakAction<T> _execute;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
		/// </summary>
		/// <param name="execute">The execute.</param>
		public RelayCommand(Action<T> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
		/// </summary>
		/// <param name="execute">The execute.</param>
		/// <param name="canExecute">The can Execute.</param>
		public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}

			_execute = new WeakAction<T>(execute);

			if (canExecute != null)
			{
				_canExecute = new WeakFunc<T, bool>(canExecute);
			}
		}

		/// <summary>
		/// Occurs when can Execute Changed.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (_canExecute != null)
				{
					CommandManager.RequerySuggested += value;
				}
			}

			remove
			{
				if (_canExecute != null)
				{
					CommandManager.RequerySuggested -= value;
				}
			}
		}

		/// <summary>
		/// Executes the can Execute operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
			{
				return true;
			}

			if (_canExecute.IsStatic || _canExecute.IsAlive)
			{
				if (parameter == null && typeof(T).IsValueType)
				{
					return _canExecute.Execute(default);
				}

				if (parameter == null || parameter is T)
				{
					return _canExecute.Execute((T)parameter);
				}
			}

			return false;
		}

		/// <summary>
		/// Executes the execute operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		public virtual void Execute(object parameter)
		{
			var val = parameter;

			if (parameter != null
				&& parameter.GetType() != typeof(T))
			{
				if (typeof(T).IsEnum)
				{
					val = Enum.Parse(typeof(T), parameter.ToString());
				}
				else if (parameter is IConvertible)
				{
					val = Convert.ChangeType(parameter, typeof(T), null);
				}
			}

			if (CanExecute(val)
				&& _execute != null
				&& (_execute.IsStatic || _execute.IsAlive))
			{
				if (val == null)
				{
					if (typeof(T).IsValueType)
					{
						_execute.Execute(default);
					}
					else
					{
						// ReSharper disable ExpressionIsAlwaysNull
						_execute.Execute((T)val);
						// ReSharper restore ExpressionIsAlwaysNull
					}
				}
				else
				{
					_execute.Execute((T)val);
				}
			}
		}

		/// <summary>
		/// Executes the raise Can Execute Changed operation.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "The this keyword is used in the Silverlight version")]
		[SuppressMessage(
			"Microsoft.Design",
			"CA1030:UseEventsWhereAppropriate",
			Justification = "This cannot be an event")]
		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}
}