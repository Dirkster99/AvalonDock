using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Defines the contract for drop Target.
	/// </summary>
	internal interface IDropTarget
	{
		/// <summary>
		/// Gets the type.
		/// </summary>
		DropTargetType Type { get; }

		/// <summary>
		/// Gets the get Preview Path.
		/// </summary>
		/// <param name="overlayWindow">The overlay Window.</param>
		/// <param name="floatingWindowModel">The floating Window Model.</param>
		/// <returns>The requested value.</returns>
		Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel);

		/// <summary>
		/// Executes the hit Test Screen operation.
		/// </summary>
		/// <param name="dragPoint">The drag Point.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		bool HitTestScreen(Point dragPoint);

		/// <summary>
		/// Gets this target's detection area in screen coordinates (the inverse of the transform
		/// <see cref="HitTestScreen"/> applies to incoming points), so a caller can compute a screen
		/// point guaranteed to land on this specific target. Test-only surface (see
		/// TestInternalsVisibleTo.cs): lets DevFlow integration tests target a precise
		/// <see cref="DropTargetType"/> compass indicator instead of guessing screen offsets.
		/// </summary>
		Rect GetScreenBounds();

		/// <summary>
		/// Executes the drop operation.
		/// </summary>
		/// <param name="floatingWindow">The floating Window.</param>
		void Drop(LayoutFloatingWindow floatingWindow);

		/// <summary>
		/// Executes the drag Enter operation.
		/// </summary>
		void DragEnter();

		/// <summary>
		/// Executes the drag Leave operation.
		/// </summary>
		void DragLeave();
	}
}