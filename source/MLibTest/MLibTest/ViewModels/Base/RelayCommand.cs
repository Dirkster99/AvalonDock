namespace MLibTest.ViewModels.Base
{
	using System;
	using System.Diagnostics;
	using System.Windows.Input;

	/// <summary>
	/// A command whose sole purpose is to relay its functionality to other
	/// objects by invoking delegates. The default return value for the <see cref="ICommand.CanExecute"/>
	/// method is <c>true</c>.
	/// 
	/// Source: <see href="http://www.codeproject.com/Articles/31837/Creating-an-Internationalized-Wizard-in-WPF"/>
	/// </summary>
	internal class RelayCommand<T> : ICommand
	{
		#region fields
		private readonly Action<T> mExecute = null;
		private readonly Predicate<T> mCanExecute = null;
		#endregion fields

		#region Constructors
		/// <summary>Class constructor.</summary>
		/// <param name="execute"></param>
		public RelayCommand(Action<T> execute) : this(execute, null)
		{
		}

		/// <summary>Creates a new command.</summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action<T> execute, Predicate<T> canExecute)
		{
			this.mExecute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.mCanExecute = canExecute;
		}

		#endregion Constructors

		#region events
		/// <summary>Event handler to re-evaluate whether this command can execute or not.</summary>
		public event EventHandler CanExecuteChanged
		{
			add { if (this.mCanExecute != null) CommandManager.RequerySuggested += value; }
			remove { if (this.mCanExecute != null) CommandManager.RequerySuggested -= value; }
		}
		#endregion

		#region methods
		/// <summary>Determine whether this pre-requisites to execute this command are given or not.</summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public bool CanExecute(object parameter) => mCanExecute?.Invoke((T)parameter) ?? true;

		/// <summary>Execute the command method managed in this class.</summary>
		/// <param name="parameter"></param>
		public void Execute(object parameter) => this.mExecute((T)parameter);

		#endregion methods
	}

	/// <summary>
	/// A command whose sole purpose is to relay its functionality to other
	/// objects by invoking delegates. The default return value for the <see cref="CanExecute"/>
	/// method is <c>true</c>.
	/// </summary>
	internal class RelayCommand : ICommand
	{
		#region Fields
		private readonly Action mExecute;
		private readonly Func<bool> mCanExecute;
		#endregion Fields

		#region Constructors

		/// <summary>Creates a new command that can always execute.</summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action execute)
		  : this(execute, null)
		{
		}

		/// <summary> Copy constructor.</summary>
		/// <param name="inputRC">Source command to copy</param>
		public RelayCommand(RelayCommand inputRC) : this(inputRC.mExecute, inputRC.mCanExecute)
		{
		}

		/// <summary> Creates a new command. </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			this.mExecute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.mCanExecute = canExecute;
		}

		#endregion Constructors

		#region Events
		/// <summary>Event handler to re-evaluate whether this command can execute or not.</summary>
		public event EventHandler CanExecuteChanged
		{
			add { if (this.mCanExecute != null) CommandManager.RequerySuggested += value; }
			remove { if (this.mCanExecute != null) CommandManager.RequerySuggested -= value; }
		}
		#endregion Events

		#region Methods
		/// <summary>
		/// Execute the attached CanExecute method delegate (or always return <c>true</c>)
		/// to determine whether the command managed in this object can execute or not.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public bool CanExecute(object parameter) => mCanExecute?.Invoke() ?? true;

		/// <summary>Return the attached delegate method for execution.</summary>
		/// <param name="parameter"></param>
		public void Execute(object parameter) => this.mExecute();

		#endregion Methods
	}
}
