namespace AvalonDock.Controls
{
	/// <summary>
	/// Describes a specific drop target area inside a given type of drop area. A drop area
	/// is a specific type of control that can be identified by its drop area type.
	/// </summary>
	/// <remarks>
	/// This enumeration is UI-agnostic and lives in <c>AvalonDock.Core</c> so that drop-zone
	/// geometry rules can be shared across WPF and other UI front ends.
	/// </remarks>
	public enum DropTargetType
	{
		/// <summary>The specific drop area is the left of the <c>DockingManager</c> control.</summary>
		DockingManagerDockLeft,

		/// <summary>The specific drop area is the top of the <c>DockingManager</c> control.</summary>
		DockingManagerDockTop,

		/// <summary>The specific drop area is the right of the <c>DockingManager</c> control.</summary>
		DockingManagerDockRight,

		/// <summary>The specific drop area is the bottom of the <c>DockingManager</c> control.</summary>
		DockingManagerDockBottom,

		/// <summary>The specific drop area is the left of a <c>LayoutDocumentPaneControl</c> control.</summary>
		DocumentPaneDockLeft,

		/// <summary>The specific drop area is the top of a <c>LayoutDocumentPaneControl</c> control.</summary>
		DocumentPaneDockTop,

		/// <summary>The specific drop area is the right of a <c>LayoutDocumentPaneControl</c> control.</summary>
		DocumentPaneDockRight,

		/// <summary>The specific drop area is the bottom of a <c>LayoutDocumentPaneControl</c> control.</summary>
		DocumentPaneDockBottom,

		/// <summary>The specific drop area is a <c>LayoutDocumentPaneControl</c> control (the dropped control
		/// and the drop target will be part of a tabbed display).</summary>
		DocumentPaneDockInside,

		/// <summary>The specific drop area is a <c>LayoutDocumentPaneGroupControl</c> control (the dropped control
		/// and the drop target will be part of a tabbed display).</summary>
		DocumentPaneGroupDockInside,

		/// <summary>The specific drop area is the left of a <c>LayoutAnchorablePaneControl</c> control.</summary>
		AnchorablePaneDockLeft,

		/// <summary>The specific drop area is the top of a <c>LayoutAnchorablePaneControl</c> control.</summary>
		AnchorablePaneDockTop,

		/// <summary>The specific drop area is the right of a <c>LayoutAnchorablePaneControl</c> control.</summary>
		AnchorablePaneDockRight,

		/// <summary>The specific drop area is the bottom of a <c>LayoutAnchorablePaneControl</c> control.</summary>
		AnchorablePaneDockBottom,

		/// <summary>The specific drop area is a <c>LayoutAnchorablePaneControl</c> control (the dropped control
		/// and the drop target will be part of a tabbed display).</summary>
		AnchorablePaneDockInside,

		/// <summary>The specific drop area docks a document pane as an anchorable to the left.</summary>
		DocumentPaneDockAsAnchorableLeft,

		/// <summary>The specific drop area docks a document pane as an anchorable to the top.</summary>
		DocumentPaneDockAsAnchorableTop,

		/// <summary>The specific drop area docks a document pane as an anchorable to the right.</summary>
		DocumentPaneDockAsAnchorableRight,

		/// <summary>The specific drop area docks a document pane as an anchorable to the bottom.</summary>
		DocumentPaneDockAsAnchorableBottom,
	}
}