/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

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
	/// Defines an event to communicating a type of change if the tree of
	/// has been changed due to an insert/remove operation.
	/// </summary>
	public class ChildrenTreeChangedEventArgs : EventArgs
	{
		/// <summary>Class constructor</summary>
		public ChildrenTreeChangedEventArgs(ChildrenTreeChange change)
		{
			Change = change;
		}

		/// <summary>Gets the type of <see cref="ChildrenTreeChange"/> for this event.</summary>
		public ChildrenTreeChange Change { get; private set; }
	}
}
