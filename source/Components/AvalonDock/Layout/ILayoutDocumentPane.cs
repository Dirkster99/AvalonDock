/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout elements that behave like a <see cref="LayoutDocumentPane"/>
	/// or an equivalent pane container such as <see cref="LayoutDocumentPaneGroup"/>.
	/// </summary>
	public interface ILayoutDocumentPane : ILayoutPanelElement, ILayoutPane
	{
	}
}