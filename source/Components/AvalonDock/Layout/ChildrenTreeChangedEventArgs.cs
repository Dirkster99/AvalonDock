using System;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Defines a way for communicating a type of change if the tree of
	/// has been changed due to an insert/remove operation.
	/// </summary>
	public enum ChildrenTreeChange
	{
		/// <summary>Direct insert/remove operation has been perfomed to the group</summary>
		DirectChildrenChanged,

		/// <summary>An element below in the hierarchy as been added/removed</summary>
		TreeChanged
	}

	/// <summary>
	/// Provides event data for children tree changed operations.
	/// </summary>
	public class ChildrenTreeChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ChildrenTreeChangedEventArgs"/> class.
		/// </summary>
		/// <param name="change">The change.</param>
		public ChildrenTreeChangedEventArgs(ChildrenTreeChange change)
		{
			Change = change;
		}

		/// <summary>
		/// Gets the change.
		/// </summary>
		public ChildrenTreeChange Change { get; private set; }
	}
}