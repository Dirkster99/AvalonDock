using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// High-level service for managing the dock layout tree.
	/// Provides the core primitives (layout, documents, anchorables, events)
	/// while higher-level operations are available as extension methods
	/// in <see cref="DockLayoutServiceExtensions"/>.
	/// </summary>
	/// <remarks>
	/// <para>Register via DI with <c>AddDockLayoutService()</c>.
	/// The service auto-builds the layout tree from registered <see cref="IToolbox"/> instances
	/// and exposes the <see cref="Layout"/> property for binding to
	/// <c>DockingManager.DockLayout</c>.</para>
	/// </remarks>
	public interface IDockLayoutService
	{
		/// <summary>Gets the root of the MVVM layout tree. Bind to <c>DockLayout</c> on the DockingManager.</summary>
		IRootDock Layout { get; }

		/// <summary>Gets or sets the currently active dockable.</summary>
		IDockable? ActiveDockable { get; set; }

		/// <summary>Gets the currently open documents.</summary>
		IEnumerable<IDockable> Documents { get; }

		/// <summary>Gets the registered anchorables/tools.</summary>
		IEnumerable<IDockable> Anchorables { get; }

		/// <summary>Opens a document and makes it active. If already open, activates it.</summary>
		/// <param name="document">The document dockable to open.</param>
		void OpenDocument(IDockable document);

		/// <summary>Closes a document, removing it from the layout.</summary>
		/// <param name="document">The document dockable to close.</param>
		void CloseDocument(IDockable document);

		/// <summary>
		/// Raised when the docked/auto-hidden state of any anchorable changes
		/// (i.e. when any <see cref="IToolbox.IsOpen"/> value changes).
		/// Subscribers should re-query visibility via the extension methods
		/// <see cref="DockLayoutServiceExtensions.IsSideOpen"/> and
		/// <see cref="DockLayoutServiceExtensions.IsAnchorableOpen"/>.
		/// </summary>
		event EventHandler? AnchorableStateChanged;
	}
}