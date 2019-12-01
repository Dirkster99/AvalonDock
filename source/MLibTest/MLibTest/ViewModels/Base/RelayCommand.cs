namespace MLibTest.ViewModels.Base
{
	using System;
	using System.Diagnostics;
	using System.Windows.Input;

	/// <summary>
	/// A command whose sole purpose is to 
	/// relay its functionality to other
	/// objects by invoking delegates. The
	/// default return value for the CanExecute
	/// method is 'true'.
	/// 
	/// Source: http://www.codeproject.com/Articles/31837/Creating-an-Internationalized-Wizard-in-WPF
	/// </summary>
	internal class RelayCommand<T> : ICommand
	{
		#region Fields
		private readonly Action<T> mExecute = null;
		private readonly Predicate<T> mCanExecute = null;
		#endregion // Fields

		#region Constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="execute"></param>
		public RelayCommand(Action<T> execute)
		  : this(execute, null)
		{
		}

		/// <summary>
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action<T> execute, Predicate<T> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.mExecute = execute;
			this.mCanExecute = canExecute;
		}

		#endregion // Constructors

		#region events
		/// <summary>
		/// Eventhandler to re-evaluate whether this command can execute or not
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (this.mCanExecute != null)
					CommandManager.RequerySuggested += value;
			}

			remove
			{
				if (this.mCanExecute != null)
					CommandManager.RequerySuggested -= value;
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Determine whether this pre-requisites to execute this command are given or not.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return this.mCanExecute == null ? true : this.mCanExecute((T)parameter);
		}

		/// <summary>
		/// Execute the command method managed in this class.
		/// </summary>
		/// <param name="parameter"></param>
		public void Execute(object parameter)
		{
			this.mExecute((T)parameter);
		}
		#endregion methods
	}

	/// <summary>
	/// A command whose sole purpose is to 
	/// relay its functionality to other
	/// objects by invoking delegates. The
	/// default return value for the CanExecute
	/// method is 'true'.
	/// </summary>
	internal class RelayCommand : ICommand
	{
		#region Fields
		private readonly Action mExecute;
		private readonly Func<bool> mCanExecute;
		#endregion Fields

		#region Constructors

		/// <summary>
		/// Creates a new command that can always execute.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action execute)
		  : this(execute, null)
		{
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="inputRC"></param>
		public RelayCommand(RelayCommand inputRC)
		  : this(inputRC.mExecute, inputRC.mCanExecute)
		{
		}

		/// <summary>
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.mExecute = execute;
			this.mCanExecute = canExecute;
		}

		#endregion Constructors

		#region Events
		/// <summary>
		/// Eventhandler to re-evaluate whether this command can execute or not
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (this.mCanExecute != null)
					CommandManager.RequerySuggested += value;
			}

			remove
			{
				if (this.mCanExecute != null)
					CommandManager.RequerySuggested -= value;
			}
		}
		#endregion Events

		#region Methods
		/// <summary>
		/// Execute the attached CanExecute methode delegate (or always return true)
		/// to determine whether the command managed in this object can execute or not.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return this.mCanExecute == null ? true : this.mCanExecute();
		}

		/// <summary>
		/// Return the attached delegate method.
		/// </summary>
		/// <param name="parameter"></param>
		public void Execute(object parameter)
		{
			this.mExecute();
		}
		#endregion Methods
	}
}
