namespace AvalonDock.DependencyInjection
{
	/// <summary>
	/// Configuration options that apply to any docking manager, regardless of which one is used.
	/// Register via <see cref="DockLayoutBuilder.ConfigureDocking"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// These are applied to the layout built by <c>IDockLayoutService</c>, so a docking manager bound to
	/// that layout picks them up by itself - there is nothing to copy over in the code-behind of the
	/// window.
	/// </para>
	/// <para>
	/// <see cref="ToggleDockOptions"/> derives from this class, so an application that configures the
	/// toggle docking manager sets these on the very same options object.
	/// </para>
	/// </remarks>
	public class DockingOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether content may be torn off into floating windows.
		/// Default is <see langword="true"/>.
		/// </summary>
		/// <remarks>
		/// When <see langword="false"/> no floating window is created at all: dragging content out of its
		/// pane and the Float command both do nothing, and a loaded layout has its floating content
		/// docked back.
		/// </remarks>
		public bool AllowFloatingWindows { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether anchorables may be moved into standalone top level
		/// windows - the "Window" view mode. Default is <see langword="true"/>.
		/// </summary>
		/// <remarks>
		/// When <see langword="false"/> the menu entry offering the mode is disabled and no such window
		/// is created, not even for a layout that was saved with one open.
		/// </remarks>
		public bool AllowDetachedWindows { get; set; } = true;
	}
}