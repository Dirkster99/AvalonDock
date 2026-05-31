using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// High-level service for managing the dock layout tree.
	/// Provides a clean API for common operations (opening documents,
	/// managing anchorables, tracking active content) so that ViewModels
	/// don't need to interact with low-level layout nodes directly.
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

		/// <summary>Finds the first open document matching the predicate.</summary>
		/// <typeparam name="T">The concrete document type.</typeparam>
		/// <param name="predicate">A predicate to match against.</param>
		/// <returns>The matching document, or null if not found.</returns>
		T? FindDocument<T>(Func<T, bool> predicate)
			where T : class, IDockable;

		/// <summary>Opens a document if not already open, otherwise activates it.</summary>
		/// <typeparam name="T">The concrete document type.</typeparam>
		/// <param name="predicate">Predicate to find an existing instance.</param>
		/// <param name="factory">Factory to create a new instance if not found.</param>
		/// <returns>The existing or newly created document.</returns>
		T OpenOrActivateDocument<T>(Func<T, bool> predicate, Func<T> factory)
			where T : class, IDockable;

		/// <summary>Gets a registered anchorable/tool by type.</summary>
		/// <typeparam name="T">The concrete anchorable type.</typeparam>
		/// <returns>The anchorable instance, or null if not registered.</returns>
		T? GetAnchorable<T>()
			where T : class, IToolbox;

		/// <summary>Docks (shows) the specified anchorable if it is currently auto-hidden.</summary>
		/// <param name="anchorable">The anchorable to show.</param>
		void ShowAnchorable(IDockable anchorable);

		/// <summary>Auto-hides (minimizes) the specified anchorable if it is currently docked.</summary>
		/// <param name="anchorable">The anchorable to hide.</param>
		void HideAnchorable(IDockable anchorable);

		/// <summary>Gets a value indicating whether the specified anchorable is currently docked (not auto-hidden).</summary>
		/// <param name="anchorable">The anchorable to query.</param>
		/// <returns><c>true</c> if the anchorable is docked; <c>false</c> if auto-hidden or not found.</returns>
		bool IsAnchorableOpen(IDockable anchorable);

		/// <summary>Gets a value indicating whether any anchorable on the specified side is docked.</summary>
		/// <param name="side">The side to query.</param>
		/// <returns><c>true</c> if any anchorable on the side is docked; otherwise <c>false</c>.</returns>
		bool IsSideOpen(ToolboxSide side);

		/// <summary>
		/// Raised when the docked/auto-hidden state of any anchorable changes.
		/// Subscribers should re-read <see cref="IsSideOpen"/> and <see cref="IsAnchorableOpen"/> values.
		/// </summary>
		event EventHandler? AnchorableStateChanged;
	}
}