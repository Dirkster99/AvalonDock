/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Controls
{
	/// <summary>
	/// Describes a specific drop target area inside a given type of drop area. A drop area
	/// is a specific type of control that can be identified by the <see cref="DropAreaType"/>.
	/// </summary>
	public enum DropTargetType
	{
		#region DockingManager
		/// <summary>
		/// The specific drop area is the left of the <see cref="DockingManager"/> control.
		/// </summary>
		DockingManagerDockLeft,

		/// <summary>
		/// The specific drop area is the top of the <see cref="DockingManager"/> control.
		/// </summary>
		DockingManagerDockTop,

		/// <summary>
		/// The specific drop area is the right of the <see cref="DockingManager"/> control.
		/// </summary>
		DockingManagerDockRight,

		/// <summary>
		/// The specific drop area is the bottom of the <see cref="DockingManager"/> control.
		/// </summary>
		DockingManagerDockBottom,
		#endregion DockingManager

		#region DocumentPane
		/// <summary>
		/// The specific drop area is the left of a <see cref="LayoutDocumentPaneControl"/> control.
		/// </summary>
		DocumentPaneDockLeft,

		/// <summary>
		/// The specific drop area is the top of a <see cref="LayoutDocumentPaneControl"/> control.
		/// </summary>
		DocumentPaneDockTop,

		/// <summary>
		/// The specific drop area is the right of a <see cref="LayoutDocumentPaneControl"/> control.
		/// </summary>
		DocumentPaneDockRight,

		/// <summary>
		/// The specific drop area is the bottom of a <see cref="LayoutDocumentPaneControl"/> control.
		/// </summary>
		DocumentPaneDockBottom,

		/// <summary>
		/// The specific drop area is a <see cref="LayoutDocumentPaneControl"/> control (the dropped control
		/// and the drop target will be part of a tabbed display).
		/// </summary>
		DocumentPaneDockInside,
		#endregion DocumentPane

		/// <summary>
		/// The specific drop area is a <see cref="LayoutDocumentPaneGroupControl"/> control (the dropped control
		/// and the drop target will be part of a tabbed display).
		/// </summary>
		DocumentPaneGroupDockInside,

		#region AnchorablePane
		/// <summary>
		/// The specific drop area is the left of a <see cref="LayoutAnchorablePaneControl"/> control.
		/// </summary>
		AnchorablePaneDockLeft,

		/// <summary>
		/// The specific drop area is the top of a <see cref="LayoutAnchorablePaneControl"/> control.
		/// </summary>
		AnchorablePaneDockTop,

		/// <summary>
		/// The specific drop area is the right of a <see cref="LayoutAnchorablePaneControl"/> control.
		/// </summary>
		AnchorablePaneDockRight,

		/// <summary>
		/// The specific drop area is the bottom of a <see cref="LayoutAnchorablePaneControl"/> control.
		/// </summary>
		AnchorablePaneDockBottom,

		/// <summary>
		/// The specific drop area is a <see cref="LayoutAnchorablePaneControl"/> control (the dropped control
		/// and the drop target will be part of a tabbed display).
		/// </summary>
		AnchorablePaneDockInside,
		#endregion AnchorablePane

		#region DocumentPaneDockAsAnchorable
		DocumentPaneDockAsAnchorableLeft,
		DocumentPaneDockAsAnchorableTop,
		DocumentPaneDockAsAnchorableRight,
		DocumentPaneDockAsAnchorableBottom,
		#endregion DocumentPaneDockAsAnchorable
	}
}
