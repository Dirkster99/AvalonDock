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
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (target is null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			if (dockable is null)
			{
				throw new ArgumentNullException(nameof(dockable));
			}

			source.VisibleDockables?.Remove(dockable);
			target.VisibleDockables?.Add(dockable);
			dockable.Owner = target;
			target.ActiveDockable = dockable;
			OnDockableMoved(dockable, source, target);
		}

		/// <inheritdoc/>
		public virtual void PinDockable(IDockable dockable)
		{
			if (dockable is null)
			{
				throw new ArgumentNullException(nameof(dockable));
			}

			if (!dockable.CanPin)
			{
				return;
			}

			var root = FindRoot(dockable);

			if (dockable.DockState == DockState.AutoHidden)
			{
				root?.PinnedDockables?.Remove(dockable);

				// The owner is left alone while a dockable sits on the sidebar, so unpinning knows
				// where it came from.
				if (dockable.Owner is IDock owner)
				{
					owner.VisibleDockables?.Add(dockable);
					owner.ActiveDockable = dockable;
				}

				dockable.DockState = DockState.Docked;
			}
			else
			{
				root?.FloatingDockables?.Remove(dockable);
				Detach(dockable);

				if (root != null)
				{
					root.PinnedDockables ??= CreateList<IDockable>();
					root.PinnedDockables.Add(dockable);
				}

				dockable.DockState = DockState.AutoHidden;
			}

			OnDockablePinned(dockable);
		}

		/// <inheritdoc/>
		public virtual void FloatDockable(IDockable dockable)
		{
			if (dockable is null)
			{
				throw new ArgumentNullException(nameof(dockable));
			}

			if (!dockable.CanFloat || dockable.DockState == DockState.Float)
			{
				return;
			}

			var root = FindRoot(dockable);
			root?.PinnedDockables?.Remove(dockable);
			Detach(dockable);

			if (root != null)
			{
				root.FloatingDockables ??= CreateList<IDockable>();
				root.FloatingDockables.Add(dockable);
			}

			dockable.DockState = DockState.Float;
			OnDockableFloated(dockable);
		}

		/// <inheritdoc/>
		public virtual void CloseDockable(IDockable dockable)
		{
			if (dockable is null)
			{
				throw new ArgumentNullException(nameof(dockable));
			}

			if (!dockable.OnClose())
			{
				return;
			}

			var root = FindRoot(dockable);
			root?.PinnedDockables?.Remove(dockable);
			root?.FloatingDockables?.Remove(dockable);
			Detach(dockable);

			dockable.DockState = DockState.Hidden;
			OnDockableClosed(dockable);
		}

		/// <summary>Walks up the owner chain to the root of the layout a dockable belongs to.</summary>
		/// <param name="dockable">The dockable to start from.</param>
		/// <returns>The root dock, or <c>null</c> if the dockable is not part of a rooted layout.</returns>
		protected static IRootDock? FindRoot(IDockable dockable)
		{
			for (IDockable? current = dockable; current != null; current = current.Owner)
			{
				if (current is IRootDock root)
				{
					return root;
				}
			}

			return null;
		}

		/// <summary>Takes a dockable out of the visible children of the dock that owns it.</summary>
		/// <param name="dockable">The dockable to take out.</param>
		/// <remarks>
		/// <see cref="IDockable.Owner"/> is deliberately kept: it is the only record of where the
		/// dockable belongs once it is on the sidebar or in a floating window, and putting it back
		/// needs that record.
		/// </remarks>
		protected static void Detach(IDockable dockable)
		{
			if (!(dockable.Owner is IDock owner))
			{
				return;
			}

			owner.VisibleDockables?.Remove(dockable);
			if (owner.ActiveDockable == dockable)
			{
				owner.ActiveDockable = owner.DefaultDockable;
			}
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