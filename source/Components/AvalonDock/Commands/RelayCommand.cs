/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Input;

namespace AvalonDock.Commands
{
	/// <inheritdoc />
	/// <summary>
	/// A command whose sole purpose is to  relay its functionality to other
	/// objects by invoking delegates.
	/// The default return value for the <see cref="ICommand.CanExecute"/> method is <c>true</c>.
	/// 
	/// Source: <see href="http://www.codeproject.com/Articles/31837/Creating-an-Internationalized-Wizard-in-WPF"/>
	/// </summary>
	/// <seealso cref="ICommand"/>
	internal class RelayCommand : ICommand
	{
		#region fields
		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;
		#endregion fields

		#region Constructors
		
		/// <summary>
		/// Class constructor from <see cref="Action{T}"/> parameter.
		/// </summary>
		/// <param name="execute">The action to execute when the command is activated.</param>
		public RelayCommand(Action<object> execute) : this(execute, null)
		{
		}

		/// <summary>
		/// Class constructor from <see cref="Action{T}"/> parameter plus
		/// canExecute predicate to decide whether command should currently
		/// be available or not.
		/// </summary>
		/// <param name="execute">The action to execute when the command is activated.</param>
		/// <param name="canExecute">The predicate to determine if this command can be executed.</param>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		#endregion Constructors

		#region ICommand Members
		
		/// <inheritdoc />
		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		/// <inheritdoc />
		/// <remarks>Returns <c>true</c>, if no predicate is set for the check.</remarks>
		public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

		/// <inheritdoc />
		public void Execute(object parameter) => _execute(parameter);

		#endregion ICommand Members
	}
}
