using System.Collections.Generic;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Defines the contract for overlay Window.
	/// </summary>
	internal interface IOverlayWindow
	{
		/// <summary>
		/// Gets the get Targets.
		/// </summary>
		/// <returns>The requested value.</returns>
		IEnumerable<IDropTarget> GetTargets();

		/// <summary>
		/// Executes the drag Enter operation.
		/// </summary>
		/// <param name="floatingWindow">The floating Window.</param>
		void DragEnter(LayoutFloatingWindowControl floatingWindow);

		/// <summary>
		/// Executes the drag Leave operation.
		/// </summary>
		/// <param name="floatingWindow">The floating Window.</param>
		void DragLeave(LayoutFloatingWindowControl floatingWindow);

		/// <summary>
		/// Executes the drag Enter operation.
		/// </summary>
		/// <param name="area">The area.</param>
		void DragEnter(IDropArea area);

		/// <summary>
		/// Executes the drag Leave operation.
		/// </summary>
		/// <param name="area">The area.</param>
		void DragLeave(IDropArea area);

		/// <summary>
		/// Executes the drag Enter operation.
		/// </summary>
		/// <param name="target">The target.</param>
		void DragEnter(IDropTarget target);

		/// <summary>
		/// Executes the drag Leave operation.
		/// </summary>
		/// <param name="target">The target.</param>
		void DragLeave(IDropTarget target);

		/// <summary>
		/// Executes the drag Drop operation.
		/// </summary>
		/// <param name="target">The target.</param>
		void DragDrop(IDropTarget target);
	}
}