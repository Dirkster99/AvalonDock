/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Layout
{
	/// <summary>Interface definition for a group of layout elements that can act as a container.</summary>
	public interface ILayoutGroup : ILayoutContainer
	{
		/// <summary>Notifies subscribers when the collection of children has changed.</summary>
		event EventHandler ChildrenCollectionChanged;

		/// <summary>Gets The zero-based index of the item within the children collection, if found; otherwise, -1.</summary>
		/// <param name="element">The element whos index should be returned.</param>
		/// <returns>The zero based index of the child or -1.</returns>
		int IndexOfChild(ILayoutElement element);

		/// <summary>Inserts a new child element at the zero based index position as indicated by the parameters.</summary>
		/// <param name="index">The index position for inserting the new element.</param>
		/// <param name="element">The element to be inserted.</param>
		void InsertChildAt(int index, ILayoutElement element);

		/// <summary>Removes a child element at the zero based index position.</summary>
		/// <param name="index">The index position of the element to be removed.</param>
		void RemoveChildAt(int index);

		/// <summary>Replaces a child element with a new instance at the indicated zero based index position.</summary>
		/// <param name="index">The index position of the element to be replaced.</param>
		/// <param name="element">The new element to be replaced over an existing element.</param>
		void ReplaceChildAt(int index, ILayoutElement element);
	}
}
