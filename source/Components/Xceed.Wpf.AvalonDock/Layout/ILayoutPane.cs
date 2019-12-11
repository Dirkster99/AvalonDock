/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	public interface ILayoutPane : ILayoutContainer, ILayoutElementWithVisibility
	{
		void MoveChild(int oldIndex, int newIndex);

		void RemoveChildAt(int childIndex);
	}
}
