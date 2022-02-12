/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;
using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>
	/// This interface is implemented by a class that can display an <see cref="IOverlayWindow"/>,
	/// which in turn is used to arrange the docking layout of a document or tool window in AvalonDock.
	/// </summary>
	internal interface IOverlayWindowHost
	{
		#region Properties

		/// <summary>
		/// Gets the <see cref="DockingManager"/> control that should be invoked to do the actual docking
		/// if docking into this <see cref="IOverlayWindowHost"/> should be performed.
		/// </summary>
		DockingManager Manager { get; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Determines whether the given screen coordinates are part of the <see cref="IOverlayWindowHost"/>
		/// window or not.
		/// </summary>
		/// <param name="dragPoint">The point to test.</param>
		/// <returns><c>true</c> if inside window, otherwise <c>false</c>.</returns>
		bool HitTestScreen(Point dragPoint);

		/// <summary>
		/// Is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>
		/// to actually show the <see cref="IOverlayWindow"/> for a given <see cref="LayoutFloatingWindowControl"/>
		/// on an <see cref="IOverlayWindowHost"/> (where the <see cref="IOverlayWindowHost"/> is the drop target
		/// and the <see cref="LayoutFloatingWindowControl"/> the dragged item).
		/// </summary>
		/// <param name="draggingWindow">The control being dragged.</param>
		/// <returns>The window encapsulating the dragged item.</returns>
		IOverlayWindow ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow);

		/// <summary>
		/// Is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>
		/// to hide the <see cref="IOverlayWindow"/> for this <see cref="IOverlayWindowHost"/>.
		/// </summary>
		void HideOverlayWindow();

		/// <summary>
		/// Is invoked by the <see cref="DragService"/> of a <see cref="LayoutFloatingWindowControl"/>
		/// to enumerate and show all overlay buttons in an <see cref="IOverlayWindow"/> when the
		/// floating window is dragged over an <see cref="IOverlayWindowHost"/>.
		/// </summary>
		/// <param name="draggingWindow">The window to examine.</param>
		/// <returns>The associated drop areas.</returns>
		IEnumerable<IDropArea> GetDropAreas(LayoutFloatingWindowControl draggingWindow);

		#endregion Methods
	}
}