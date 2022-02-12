/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Defines relevant methods and property for a drop target in the AvalonDock library.
	///
	/// A drop target is an individual/specific docking position where a user can drop an item to dock it.
	///
	/// This drag & drop operation (which can result in new docking position) can include interaction elements
	/// such as a preview of the new docking position while the user hovers with the mouse
	/// (and the item being dragged) over a specific enabled drop target.
	///
	/// This interface defines the means for implementing these interaction elements.
	/// </summary>
	internal interface IDropTarget
	{
		#region Properties

		/// <summary>Gets the type of a drop target that descries a specific docking position inside a <see cref="DropAreaType"/>.</summary>
		DropTargetType Type { get; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets a <see cref="Geometry"/> that is used to highlight/preview the docking position
		/// of this drop target for a <paramref name="floatingWindowModel"/> being docked inside an
		/// <paramref name="overlayWindow"/>.
		/// </summary>
		/// <param name="overlayWindow"></param>
		/// <param name="floatingWindowModel"></param>
		/// <returns>The geometry of the preview/highlighting WPF figure path.</returns>
		Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel);

		/// <summary>Determines whether the <paramref name="dragPoint"/> is part of this drop target or not.</summary>
		/// <param name="dragPoint">The point to test.</param>
		/// <returns><c>true</c> if it is inside the target.</returns>
		bool HitTestScreen(Point dragPoint);

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		void Drop(LayoutFloatingWindow floatingWindow);

		/// <summary>
		/// Method is invoked to signal that the mouse is starting to hover
		/// (while dragging a <see cref="LayoutFloatingWindow"/>) over this drop target.
		/// </summary>
		void DragEnter();

		/// <summary>
		/// Method is invoked to signal that the mouse is no longer hovering
		/// (while dragging a <see cref="LayoutFloatingWindow"/>) over this drop target.
		/// </summary>
		void DragLeave();

		#endregion Methods
	}
}