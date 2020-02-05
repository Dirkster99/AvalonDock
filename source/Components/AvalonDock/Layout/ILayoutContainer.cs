/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;

namespace AvalonDock.Layout
{
	public interface ILayoutContainer : ILayoutElement
	{
		#region Properties

		IEnumerable<ILayoutElement> Children
		{
			get;
		}

		int ChildrenCount
		{
			get;
		}

		#endregion Properties

		#region Methods

		void RemoveChild(ILayoutElement element);

		void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);

		#endregion Methods
	}
}
