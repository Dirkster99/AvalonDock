using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Abstract base factory implementing core orchestration logic for dock layout management.
	/// Concrete implementations provide MVVM-framework-specific object creation.
	/// </summary>
	public abstract partial class FactoryBase : IFactory
	{
		public IDictionary<string, Func<object?>>? ContextLocator { get; set; }

		public IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

		public abstract IRootDock CreateRootDock();

		public abstract IDocumentDock CreateDocumentDock();

		public abstract IToolDock CreateToolDock();

		public abstract IList<T> CreateList<T>(params T[] items);

		public abstract IRootDock CreateLayout();

		public virtual void InitLayout(IDockable layout)
		{
			InitDockable(layout, null);
		}

		public virtual void MoveDockable(IDock source, IDock target, IDockable dockable)
		{
			source.VisibleDockables?.Remove(dockable);
			target.VisibleDockables?.Add(dockable);
			dockable.Owner = target;
			target.ActiveDockable = dockable;
			OnDockableMoved(dockable, source, target);
		}

		public virtual void PinDockable(IDockable dockable)
		{
			OnDockablePinned(dockable);
		}

		public virtual void FloatDockable(IDockable dockable)
		{
			OnDockableFloated(dockable);
		}

		public virtual void CloseDockable(IDockable dockable)
		{
			if (!dockable.OnClose())
			{
				return;
			}

			if (dockable.Owner is IDock dock)
			{
				dock.VisibleDockables?.Remove(dockable);
				if (dock.ActiveDockable == dockable)
				{
					dock.ActiveDockable = dock.DefaultDockable;
				}
			}

			OnDockableClosed(dockable);
		}

		protected virtual void InitDockable(IDockable dockable, IDockable? owner)
		{
			dockable.Owner = owner;
			dockable.Factory = this;

			if (ContextLocator != null
				&& dockable.Id != null
				&& ContextLocator.TryGetValue(dockable.Id, out var contextFactory))
			{
				dockable.Context = contextFactory();
			}

			if (dockable is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					InitDockable(child, dockable);
				}
			}
		}
	}
}