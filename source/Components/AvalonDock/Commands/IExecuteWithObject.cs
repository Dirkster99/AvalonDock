namespace AvalonDock.Commands
{
	/// <summary>
	/// Defines the contract for execute With Object.
	/// </summary>
	internal interface IExecuteWithObject
	{
		/// <summary>
		/// Gets the target.
		/// </summary>
		object Target
		{
			get;
		}

		/// <summary>
		/// Executes the execute With Object operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		void ExecuteWithObject(object parameter);

		/// <summary>
		/// Executes the mark For Deletion operation.
		/// </summary>
		void MarkForDeletion();
	}
}