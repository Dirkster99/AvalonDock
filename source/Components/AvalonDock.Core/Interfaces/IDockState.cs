namespace AvalonDock.Core
{
	/// <summary>
	/// Manages layout state save/restore operations.
	/// </summary>
	public interface IDockState
	{
		/// <summary>Saves the current layout state of the root dock.</summary>
		/// <param name="rootDock">The root dock whose state to save.</param>
		void Save(IRootDock rootDock);

		/// <summary>Restores a previously saved layout state.</summary>
		/// <param name="rootDock">The root dock to restore state into.</param>
		void Restore(IRootDock rootDock);
	}
}