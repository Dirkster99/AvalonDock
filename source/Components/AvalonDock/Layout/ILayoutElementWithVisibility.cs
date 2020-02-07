/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>Interface definition for a layout element that can update its visibility (IsVisible) property.</summary>
	public interface ILayoutElementWithVisibility
	{
		/// <summary>Invoke this to update the visibility (IsVisible) property of this layout element.</summary>
		void ComputeVisibility();
	}
}
