/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	[Serializable]
	public abstract class LayoutGroupBase : LayoutElement
	{
		#region Events

		[field: NonSerialized]
		[field: XmlIgnore]
		public event EventHandler ChildrenCollectionChanged;

		[field: NonSerialized]
		[field: XmlIgnore]
		public event EventHandler<ChildrenTreeChangedEventArgs> ChildrenTreeChanged;

		#endregion Events

		#region Internal Methods

		protected virtual void OnChildrenCollectionChanged()
		{
			if (ChildrenCollectionChanged != null)
				ChildrenCollectionChanged(this, EventArgs.Empty);
		}

		protected void NotifyChildrenTreeChanged(ChildrenTreeChange change)
		{
			OnChildrenTreeChanged(change);
			var parentGroup = Parent as LayoutGroupBase;
			if (parentGroup != null)
				parentGroup.NotifyChildrenTreeChanged(ChildrenTreeChange.TreeChanged);
		}

		internal void RaiseChildrenTreeChanged()
		{
			OnChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
			var parentGroup = Parent as LayoutGroupBase;
			if (parentGroup != null)
				parentGroup.RaiseChildrenTreeChanged();
		}

		protected virtual void OnChildrenTreeChanged(ChildrenTreeChange change)
		{
			if (ChildrenTreeChanged != null)
				ChildrenTreeChanged(this, new ChildrenTreeChangedEventArgs(change));
		}

		#endregion Internal Methods
	}
}
