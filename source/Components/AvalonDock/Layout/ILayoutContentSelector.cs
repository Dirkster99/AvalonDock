/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	public interface ILayoutContentSelector
	{
		#region Properties

		int SelectedContentIndex
		{
			get; set;
		}

		LayoutContent SelectedContent
		{
			get;
		}

		#endregion Properties

		#region Methods

		int IndexOf(LayoutContent content);

		#endregion Methods
	}
}
