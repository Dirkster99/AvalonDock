using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Abstract factory for creating and managing dockable layout elements.
	/// Inspired by wieslawsoltes/Dock's FactoryBase pattern.
	/// </summary>
	public interface IFactory
	{
		/// <summary>Gets or sets a dictionary mapping content IDs to context factories.</summary>
		IDictionary<string, Func<object?>>? ContextLocator { get; set; }

		/// <summary>Gets or sets a dictionary mapping content IDs to dockable factories.</summary>
		IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

		/// <summary>Creates a new root dock instance.</summary>
		/// <returns>A new root dock.</returns>
		IRootDock CreateRootDock();

		/// <summary>Creates a new document dock instance.</summary>
		/// <returns>A new document dock.</returns>
		IDocumentDock CreateDocumentDock();

		/// <summary>Creates a new tool dock instance.</summary>
		/// <returns>A new tool dock.</returns>
		IToolDock CreateToolDock();

		/// <summary>Creates a list initialized with the given items.</summary>
		/// <typeparam name="T">The type of list items.</typeparam>
		/// <param name="items">The items to populate the list with.</param>
		/// <returns>A new list containing the specified items.</returns>
		IList<T> CreateList<T>(params T[] items);

		/// <summary>Creates the default dock layout.</summary>
		/// <returns>The root dock of the created layout.</returns>
		IRootDock CreateLayout();

		/// <summary>Initializes a layout tree, setting up owners and context.</summary>
		/// <param name="layout">The root dockable of the layout to initialize.</param>
		void InitLayout(IDockable layout);

		/// <summary>Moves a dockable from one dock to another.</summary>
		/// <param name="source">The source dock.</param>
		/// <param name="target">The target dock.</param>
		/// <param name="dockable">The dockable to move.</param>
		void MoveDockable(IDock source, IDock target, IDockable dockable);

		/// <summary>Pins a dockable to prevent auto-hiding.</summary>
		/// <param name="dockable">The dockable to pin.</param>
		void PinDockable(IDockable dockable);

		/// <summary>Floats a dockable into an independent window.</summary>
		/// <param name="dockable">The dockable to float.</param>
		void FloatDockable(IDockable dockable);

		/// <summary>Closes a dockable, removing it from the layout.</summary>
		/// <param name="dockable">The dockable to close.</param>
		void CloseDockable(IDockable dockable);
	}
}