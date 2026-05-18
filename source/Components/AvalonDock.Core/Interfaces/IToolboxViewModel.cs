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
	/// Interface for toolbox ViewModels that participate in the toggle docking system.
	/// Implement this interface on your ViewModel and register it via DI
	/// to have it automatically placed in the ToggleDockingManager layout.
	/// </summary>
	/// <remarks>
	/// <para>The <see cref="Icon"/> property accepts any object that WPF can render:
	/// <c>ImageSource</c>, <c>UIElement</c> (Viewbox, Path), <c>DrawingImage</c>, or a string
	/// for font-icon glyphs.</para>
	/// <para>Set <see cref="IsOpenByDefault"/> to <c>true</c> to have the toolbox
	/// toggled open when the application starts.</para>
	/// </remarks>
	public interface IToolboxViewModel
	{
		/// <summary>Gets the display title shown in the pane header.</summary>
		string Title { get; }

		/// <summary>Gets the unique content identifier used for layout serialization.</summary>
		string ContentId { get; }

		/// <summary>Gets the tooltip text shown on the sidebar toggle button.</summary>
		string? ToolTipText { get; }

		/// <summary>Gets which side of the dock this toolbox belongs to.</summary>
		ToolboxSide Side { get; }

		/// <summary>Gets whether this toolbox should be toggled open on startup.</summary>
		bool IsOpenByDefault { get; }

		/// <summary>
		/// Gets the icon content for the sidebar toggle button.
		/// Accepts any WPF-renderable object: ImageSource, UIElement (Viewbox, Path), DrawingImage, or null.
		/// </summary>
		object? Icon { get; }
	}
}