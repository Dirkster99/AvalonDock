/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Layout
{
	public interface ILayoutGroup : ILayoutContainer
	{
		int IndexOfChild(ILayoutElement element);
		void InsertChildAt(int index, ILayoutElement element);
		void RemoveChildAt(int index);
		void ReplaceChildAt(int index, ILayoutElement element);

		event EventHandler ChildrenCollectionChanged;
	}
}
