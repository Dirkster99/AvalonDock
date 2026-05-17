namespace AvalonDock.Core
{
	/// <summary>
	/// Event callbacks for factory lifecycle operations.
	/// </summary>
	public abstract partial class FactoryBase
	{
		protected virtual void OnDockableMoved(IDockable dockable, IDock source, IDock target)
		{
		}

		protected virtual void OnDockablePinned(IDockable dockable)
		{
		}

		protected virtual void OnDockableFloated(IDockable dockable)
		{
		}

		protected virtual void OnDockableClosed(IDockable dockable)
		{
		}

		protected virtual void OnDockableAdded(IDockable dockable, IDock parent)
		{
		}

		protected virtual void OnActiveDockableChanged(IDock dock, IDockable? dockable)
		{
		}
	}
}