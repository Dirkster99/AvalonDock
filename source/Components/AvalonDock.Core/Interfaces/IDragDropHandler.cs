#nullable disable
namespace AvalonDock.Core
{
	/// <summary>
	/// Defines the possible drop positions in the docking layout.
	/// </summary>
	public enum DockPosition
	{
		/// <summary>No valid position.</summary>
		None,

		/// <summary>Dock to the left side.</summary>
		Left,

		/// <summary>Dock to the right side.</summary>
		Right,

		/// <summary>Dock to the top.</summary>
		Top,

		/// <summary>Dock to the bottom.</summary>
		Bottom,

		/// <summary>Dock as a tabbed document in the center.</summary>
		Center,

		/// <summary>Float as an independent window.</summary>
		Float
	}

	/// <summary>
	/// UI-agnostic interface for drag-and-drop decisions in the docking system.
	/// Separates the pure logic (can drop? where?) from WPF hit-testing and rendering.
	/// </summary>
	/// <remarks>
	/// <para>The WPF <c>DragService</c> handles actual mouse tracking and overlay rendering.</para>
	/// <para>This interface enables testing drag/drop rules without a WPF host.</para>
	/// </remarks>
	public interface IDragDropHandler
	{
		/// <summary>Determines whether the specified content can be dropped at the given position.</summary>
		/// <param name="contentId">Content ID of the item being dragged.</param>
		/// <param name="targetContentId">Content ID of the drop target (or null for root).</param>
		/// <param name="position">The proposed drop position.</param>
		/// <returns>True if the drop is allowed.</returns>
		bool CanDrop(string contentId, string targetContentId, DockPosition position);

		/// <summary>Gets or sets a value indicating whether drag-and-drop operations are enabled.</summary>
		bool IsDragDropEnabled { get; set; }
	}
}