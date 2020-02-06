/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	interface ILayoutPreviousContainer
	{
		ILayoutContainer PreviousContainer { get; set; }

		string PreviousContainerId { get; set; }
	}
}
