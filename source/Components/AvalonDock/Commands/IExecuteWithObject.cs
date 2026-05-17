namespace AvalonDock.Commands
{
	/// <summary>
	/// Interface IExecuteWithObject
	/// </summary>
	internal interface IExecuteWithObject
	{
		/// <summary>
		/// The target of the WeakAction.
		/// </summary>
		/// <value>The target.</value>
		object Target
		{
			get;
		}

		/// <summary>
		/// Executes an action.
		/// </summary>
		/// <param name="parameter">A parameter passed as an object,
		/// to be casted to the appropriate type.</param>
		void ExecuteWithObject(object parameter);

		/// <summary>
		/// Deletes all references, which notifies the cleanup method
		/// that this entry must be deleted.
		/// </summary>
		void MarkForDeletion();
	}
}