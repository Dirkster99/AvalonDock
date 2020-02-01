/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;

namespace AvalonDock.Controls
{
	/// <summary>
	/// The IOverlayWindow interface is implemented by custom controls that realize an
	/// <see cref="OverlayWindow"/> (UI) control to show docking buttons when the user
	/// drags a:
	/// - documents (<see cref="LayoutDocumentControl"/>) or
	/// - tool window (<see cref="LayoutAnchorableControl"/>) to a new docking location.
	/// 
	/// Docking buttons are (depending on the state of the framework) usually shown on the
	/// center, left, right, top, and bottom side of an <see cref="OverlayWindow"/>.
	/// </summary>
	internal interface IOverlayWindow
	{
		/// <summary>
		/// Gets all <see cref="IDropTarget"/> areas in which a given floating element can be dropped into this window.
		/// The emitted information is used to draw preview elements (drop buttons in center, top, left, right, bottom)
		/// to show each drop target and realize the drop itself when the drag operation ends at a given drop target.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDropTarget> GetTargets();

		/// <summary>
		/// This method is invoked by the AvalonDock framework, if the mouse enters (hovers over)
		/// the control that implements the <see cref="IOverlayWindow"/> interface. The implementing
		/// control can use this point in time to decide whether dropping the dragged
		/// <see cref="LayoutFloatingWindowControl"/> should be enabled and whether to show or hide
		/// the corresponding drop target buttons.
		/// </summary>
		/// <param name="floatingWindow"></param>
		void DragEnter(LayoutFloatingWindowControl floatingWindow);

		/// <summary>
		/// This method is invoked by the AvalonDock framework, if the mouse leaves (does no longer hover over)
		/// the control that implements the <see cref="IOverlayWindow"/> interface. The implementing
		/// control can use this point in time to hide the corresponding drop target buttons and free
		/// resources that may have been allocated to show and support a drop operation of the
		/// dragged <see cref="LayoutFloatingWindowControl"/> .
		/// </summary>
		/// <param name="floatingWindow"></param>
		void DragLeave(LayoutFloatingWindowControl floatingWindow);


		void DragEnter(IDropArea area);

		void DragLeave(IDropArea area);

		void DragEnter(IDropTarget target);

		void DragLeave(IDropTarget target);

		void DragDrop(IDropTarget target);
	}
}
