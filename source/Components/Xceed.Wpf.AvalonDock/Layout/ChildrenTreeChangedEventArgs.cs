/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Layout
{
	public enum ChildrenTreeChange
	{
		/// <summary>
		/// Direct insert/remove operation has been perfomed to the group
		/// </summary>
		DirectChildrenChanged,

		/// <summary>
		/// An element below in the hierarchy as been added/removed
		/// </summary>
		TreeChanged
	}

	public class ChildrenTreeChangedEventArgs : EventArgs
	{
		public ChildrenTreeChangedEventArgs(ChildrenTreeChange change)
		{
			Change = change;
		}

		public ChildrenTreeChange Change
		{
			get; private set;
		}
	}
}
