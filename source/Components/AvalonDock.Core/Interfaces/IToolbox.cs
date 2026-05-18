namespace AvalonDock.Core
{
	/// <summary>
	/// Defines the side of the dock where a toolbox should be placed.
	/// </summary>
	public enum ToolboxSide
	{
		/// <summary>Docked to the left sidebar.</summary>
		Left,

		/// <summary>Docked to the right sidebar.</summary>
		Right,

		/// <summary>Docked to the bottom panel.</summary>
		Bottom
	}

	/// <summary>
	/// Contract for toolbox (anchorable tool) view models that participate in the toggle docking system.
	/// Extends <see cref="IDockable"/> with placement and icon metadata.
	/// </summary>
	/// <remarks>
	/// <para>Implement this interface on your ViewModel and register it via DI
	/// to have it automatically placed in the ToggleDockingManager layout.</para>
	/// <para>The <see cref="Icon"/> property accepts any object that WPF can render:
	/// <c>ImageSource</c>, <c>UIElement</c> (Viewbox, Path), <c>DrawingImage</c>, or a string
	/// for font-icon glyphs.</para>
	/// <para>For a ready-to-use base class, see <c>AvalonDock.Mvvm.ToolboxBase</c>.</para>
	/// </remarks>
	public interface IToolbox : IDockable
	{
		/// <summary>Gets or sets the tooltip text shown on the sidebar toggle button.</summary>
		string? ToolTipText { get; set; }

		/// <summary>Gets or sets which side of the dock this toolbox belongs to.</summary>
		ToolboxSide Side { get; set; }

		/// <summary>Gets or sets whether this toolbox should be toggled open on startup.</summary>
		bool IsOpenByDefault { get; set; }

		/// <summary>
		/// Gets or sets the icon content for the sidebar toggle button.
		/// Accepts any WPF-renderable object: ImageSource, UIElement (Viewbox, Path), DrawingImage, or null.
		/// </summary>
		object? Icon { get; set; }
	}
}