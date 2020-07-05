namespace AvalonDock.Commands
{
	/// <summary>
	/// Interface IExecuteWithObjectAndResult
	/// </summary>
	internal interface IExecuteWithObjectAndResult
	{
		#region Public Methods

		/// <summary>
		/// Executes a Func and returns the result.
		/// </summary>
		/// <param name="parameter">A parameter passed as an object,
		/// to be casted to the appropriate type.</param>
		/// <returns>The result of the operation.</returns>
		object ExecuteWithObject(object parameter);

		#endregion Public Methods
	}
}