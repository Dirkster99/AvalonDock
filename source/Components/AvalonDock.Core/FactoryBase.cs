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
		/// <inheritdoc/>
		public IDictionary<string, Func<object?>>? ContextLocator { get; set; }

		/// <inheritdoc/>
		public IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

		/// <inheritdoc/>
		public abstract IRootDock CreateRootDock();

		/// <inheritdoc/>
		public abstract IDocumentDock CreateDocumentDock();

		/// <inheritdoc/>
		public abstract IToolDock CreateToolDock();

		/// <inheritdoc/>
		public abstract IList<T> CreateList<T>(params T[] items);

		/// <inheritdoc/>
		public abstract IRootDock CreateLayout();

		/// <inheritdoc/>
		public virtual void InitLayout(IDockable layout)
		{
			InitDockable(layout, null);
		}

		/// <inheritdoc/>
		public virtual void MoveDockable(IDock source, IDock target, IDockable dockable)
		{
			source.VisibleDockables?.Remove(dockable);
			target.VisibleDockables?.Add(dockable);
			dockable.Owner = target;
			target.ActiveDockable = dockable;
			OnDockableMoved(dockable, source, target);
		}

		/// <inheritdoc/>
		public virtual void PinDockable(IDockable dockable)
		{
			OnDockablePinned(dockable);
		}

		/// <inheritdoc/>
		public virtual void FloatDockable(IDockable dockable)
		{
			OnDockableFloated(dockable);
		}

		/// <inheritdoc/>
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

		/// <summary>Recursively initializes a dockable and its children.</summary>
		/// <param name="dockable">The dockable to initialize.</param>
		/// <param name="owner">The parent dockable that owns this one.</param>
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