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
	/// The <see cref="IOverlayWindow"/> interface is implemented by custom controls that realize an
	/// <see cref="OverlayWindow"/> (UI) control to show docking buttons when the user
	/// drags a:
	/// - documents (<see cref="LayoutDocumentControl"/>) or
	/// - tool window (<see cref="LayoutAnchorableControl"/>) to a new docking location.
	/// 
	/// Docking buttons are (depending on the state of the framework) usually shown on the
	/// center, left, right, top, and bottom side of an <see cref="OverlayWindow"/>.
	/// </summary>
	/// <seealso cref="OverlayWindow"/>
	/// <seealso cref="LayoutAnchorControl"/>
	/// <seealso cref="LayoutDocumentControl"/>
	internal interface IOverlayWindow
	{
		/// <summary>
		/// Gets all <see cref="IDropTarget"/> areas in which a given floating element can be dropped into this window.
		/// The emitted information is used to draw preview elements (drop buttons in center, top, left, right, bottom)
		/// to show each drop target and realize the drop itself when the drag operation ends at a given drop target.
		/// </summary>
		/// <returns>All the applicable targets.</returns>
		/// <seealso cref="IDropTarget"/>
		IEnumerable<IDropTarget> GetTargets();

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if the mouse enters (hovers over)
		/// the control that implements the <see cref="IOverlayWindow"/> interface. The implementing
		/// control can use this method call to decide whether dropping the dragged
		/// <see cref="LayoutFloatingWindowControl"/> should be enabled and whether to show or hide
		/// the corresponding drop target buttons.
		/// </summary>
		/// <param name="floatingWindow">The applicable window.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragEnter(LayoutFloatingWindowControl floatingWindow);

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if the mouse leaves (does no longer hover over)
		/// the control that implements the <see cref="IOverlayWindow"/> interface. The implementing
		/// control can use this point in time to hide the corresponding drop target buttons and free
		/// resources that may have been allocated to show and support a drop operation of the
		/// dragged <see cref="LayoutFloatingWindowControl"/> .
		/// </summary>
		/// <param name="floatingWindow">The applicable window.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragLeave(LayoutFloatingWindowControl floatingWindow);

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if the mouse enters a drop area (which can be a part of a FrameworkElement that implements
		/// the <see cref="IOverlayWindow"/> interface.
		/// 
		/// A drop area can contain multiple drop targets (eg.: the <see cref="DockingManager"/> contains side panels
		/// (top, bottom, left, right) and each of them is an individual drop target.
		/// 
		/// The implementing control can use this method call to decide whether the drop area should
		/// be enabled as such and whether there are any drop targets that are available as a specific
		/// docking position.
		/// </summary>
		/// <param name="area">The drop area.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="DockingManager"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragEnter(IDropArea area);

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if the mouse leaves a drop area (which can be a part of a <see cref="System.Windows.FrameworkElement"/> that implements
		/// the <see cref="IOverlayWindow"/> interface.
		/// 
		/// A drop area can contain multiple drop targets (eg.: the <see cref="DockingManager"/> contains side panels
		/// (top, bottom, left, right) and each of them is an individual drop target.
		/// 
		/// The implementing control can use this method call to disable the drop area and hide
		/// its drop targets (<see cref="IDropTarget"/>) that may have been available as a specific
		/// docking position.
		/// </summary>
		/// <param name="area">The drop area.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="DockingManager"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragLeave(IDropArea area);

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if the mouse enters a drop target (which is typically a part of a <see cref="IDropArea"/>).
		/// 
		/// A drop target is an individual/specific docking position
		/// where a user can drop an item to dock it.
		/// 
		/// The implementing control can use this method call to show a preview (Highlighting PreviewBox Geometry)
		/// of the final docking position while the user hovers the mouse of a drop target (drop target button).
		/// </summary>
		/// <param name="target">The applicable target.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragEnter(IDropTarget target);

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if the mouse leaves a drop target (which is typically a part of a <see cref="IDropArea"/>).
		/// 
		/// A drop target is an individual/specific docking position
		/// where a user can drop an item to dock it.
		/// 
		/// The implementing control can use this method call to hide a preview (Highlighting PreviewBox Geometry)
		/// of the final docking position while the drags the mouse away from a drop target (drop target button).
		/// </summary>
		/// <param name="target">The applicable target.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragLeave(IDropTarget target);

		/// <summary>
		/// This method is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>,
		/// if a drag & drop operation is completed to display the <see cref="LayoutFloatingWindowControl"/> in a
		/// new docking position.
		/// 
		/// The implementing control can use this method call to:
		/// - hide available drop areas and their current drop targets preview (Highlighting PreviewBox Geometry)
		/// - and re-positioning the dragged control to complete the new docking position.
		/// </summary>
		/// <param name="target">The applicable target.</param>
		/// <seealso cref="DragService"/>
		/// <seealso cref="LayoutFloatingWindowControl"/>
		void DragDrop(IDropTarget target);
	}
}
