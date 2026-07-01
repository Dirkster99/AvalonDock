using System;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout group base.
	/// </summary>
	[Serializable]
	public abstract class LayoutGroupBase : LayoutElement, Core.Serialization.ISerializableLayoutContainer
	{
		/// <summary>
		/// Occurs when the children collection changed event is raised.
		/// </summary>
		[field: NonSerialized]
		[field: XmlIgnore]
		public event EventHandler ChildrenCollectionChanged;

		/// <summary>
		/// Occurs when the children tree changed event is raised.
		/// </summary>
		[field: NonSerialized]
		[field: XmlIgnore]
		public event EventHandler<ChildrenTreeChangedEventArgs> ChildrenTreeChanged;

		/// <summary>
		/// Raises the children tree changed.
		/// </summary>
		internal void RaiseChildrenTreeChanged()
		{
			OnChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
			var parentGroup = Parent as LayoutGroupBase;
			if (parentGroup != null)
				parentGroup.RaiseChildrenTreeChanged();
		}

		/// <summary>
		/// Executes the on children collection changed operation.
		/// </summary>
		protected virtual void OnChildrenCollectionChanged()
		{
			if (ChildrenCollectionChanged != null)
				ChildrenCollectionChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Executes the on children tree changed operation.
		/// </summary>
		/// <param name="change">The change.</param>
		protected virtual void OnChildrenTreeChanged(ChildrenTreeChange change)
		{
			if (ChildrenTreeChanged != null)
				ChildrenTreeChanged(this, new ChildrenTreeChangedEventArgs(change));
		}

		/// <summary>
		/// Executes the notify children tree changed operation.
		/// </summary>
		/// <param name="change">The change.</param>
		protected void NotifyChildrenTreeChanged(ChildrenTreeChange change)
		{
			OnChildrenTreeChanged(change);
			var parentGroup = Parent as LayoutGroupBase;
			if (parentGroup != null)
				parentGroup.NotifyChildrenTreeChanged(ChildrenTreeChange.TreeChanged);
		}
	}
}