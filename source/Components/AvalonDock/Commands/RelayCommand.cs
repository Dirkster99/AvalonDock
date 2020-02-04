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
	/// <summary>
	/// A command whose sole purpose is to  relay its functionality to other
	/// objects by invoking delegates.
	/// The default return value for the CanExecute method is 'true'.
	/// 
	/// Source: http://www.codeproject.com/Articles/31837/Creating-an-Internationalized-Wizard-in-WPF
	/// </summary>
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
		/// <param name="execute"></param>
		public RelayCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Class constructor from <see cref="Action{T}"/> parameter plus
		/// canExecute predicate to decide whether command should currently
		/// be available or not.
		/// </summary>
		/// <param name="execute"></param>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}
		#endregion Constructors

		#region ICommand Members
		/// <summary>
		/// Eventhandler to re-evaluate whether this command can execute or not
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		/// <summary>
		/// Determine whether this pre-requisites to execute this command is given or not.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute == null ? true : _canExecute(parameter);
		}

		/// <summary>
		/// Execute the command method managed in this class.
		/// </summary>
		/// <param name="parameter"></param>
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		#endregion ICommand Members
	}
}
