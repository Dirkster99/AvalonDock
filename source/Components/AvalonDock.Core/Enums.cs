namespace AvalonDock.Core
{
	/// <summary>
	/// Represents the current docking state of a dockable element.
	/// </summary>
	public enum DockState
	{
		Docked,
		AutoHidden,
		Float,
		Hidden
	}

	/// <summary>
	/// Represents the alignment/side for a tool dock.
	/// </summary>
	public enum DockAlignment
	{
		Left,
		Right,
		Top,
		Bottom
	}
}