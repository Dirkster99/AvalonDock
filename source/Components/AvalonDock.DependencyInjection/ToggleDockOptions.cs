namespace AvalonDock.DependencyInjection
{
	/// <summary>
	/// Configuration options for the Toggle Docking Manager.
	/// Register via <see cref="DockLayoutBuilder.ConfigureToggleDock"/>
	/// and resolve in your window to apply settings.
	/// </summary>
	public class ToggleDockOptions
	{
		/// <summary>Gets or sets the sidebar toggle button size. Default is 25.</summary>
		public double ButtonSize { get; set; } = 25;

		/// <summary>Gets or sets the default width for side-docked panes. Default is 250.</summary>
		public double DefaultDockWidth { get; set; } = 250;

		/// <summary>Gets or sets the default height for bottom-docked panes. Default is 200.</summary>
		public double DefaultDockHeight { get; set; } = 200;

		/// <summary>Gets or sets the layout priority mode name. Default is "BottomFullWidth".</summary>
		public string LayoutPriority { get; set; } = "BottomFullWidth";

		/// <summary>Gets or sets a value indicating whether the Minimize button appears in pane headers. Default is true.</summary>
		public bool ShowHeaderMinimizeButton { get; set; } = true;

		/// <summary>Gets or sets a value indicating whether the Options (three-dot) button appears in pane headers. Default is true.</summary>
		public bool ShowHeaderOptionsButton { get; set; } = true;
	}
}