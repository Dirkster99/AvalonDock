/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>
	/// Defines an API to layout model that can manipluate its child entries by moving
	/// them from one index to the other or by removing a particular child from its child collection.
	/// </summary>
	public interface ILayoutPane : ILayoutContainer, ILayoutElementWithVisibility
	{
		/// <summary>Move a child entry from the <paramref name="oldIndex"/> to the <paramref name="newIndex"/>.</summary>
		/// <param name="oldIndex"></param>
		/// <param name="newIndex"></param>
		void MoveChild(int oldIndex, int newIndex);

		/// <summary>Remove a child entry from the collection of children at the <paramref name="childIndex"/>.</summary>
		/// <param name="childIndex"></param>
		void RemoveChildAt(int childIndex);
	}
}
