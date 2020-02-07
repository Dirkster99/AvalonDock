/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>Interface definition for a layout pane that can be identified by a unique id.</summary>
	interface ILayoutPaneSerializable
	{
		/// <summary>Gets/sets the unique id for this layout pane.</summary>
		string Id { get; set; }
	}
}
