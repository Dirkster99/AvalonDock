/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout panel element.
	/// </summary>
	public interface ILayoutPanelElement : ILayoutElement
	{
		/// <summary>
		/// Gets a value indicating whether this instance is visible.
		/// </summary>
		bool IsVisible { get; }
	}
}