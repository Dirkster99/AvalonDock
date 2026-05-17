namespace AvalonDock.Core
{
	/// <summary>
	/// Manages layout state save/restore operations.
	/// </summary>
	public interface IDockState
	{
		void Save(IRootDock rootDock);

		void Restore(IRootDock rootDock);
	}
}