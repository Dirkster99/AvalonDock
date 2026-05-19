namespace AvalonDock.Core
{
	/// <summary>
	/// Represents the current docking state of a dockable element.
	/// </summary>
	public enum DockState
	{
		/// <summary>Content is docked in the main layout.</summary>
		Docked,

		/// <summary>Content is auto-hidden to a sidebar.</summary>
		AutoHidden,

		/// <summary>Content is floating in an independent window.</summary>
		Float,

		/// <summary>Content is hidden and not visible.</summary>
		Hidden
	}

	/// <summary>
	/// Represents the alignment/side for a tool dock.
	/// </summary>
	public enum DockAlignment
	{
		/// <summary>Aligned to the left side.</summary>
		Left,

		/// <summary>Aligned to the right side.</summary>
		Right,

		/// <summary>Aligned to the top.</summary>
		Top,

		/// <summary>Aligned to the bottom.</summary>
		Bottom
	}
}