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
		IDictionary<string, Func<object?>>? ContextLocator { get; set; }

		IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

		IRootDock CreateRootDock();

		IDocumentDock CreateDocumentDock();

		IToolDock CreateToolDock();

		IList<T> CreateList<T>(params T[] items);

		IRootDock CreateLayout();

		void InitLayout(IDockable layout);

		void MoveDockable(IDock source, IDock target, IDockable dockable);

		void PinDockable(IDockable dockable);

		void FloatDockable(IDockable dockable);

		void CloseDockable(IDockable dockable);
	}
}