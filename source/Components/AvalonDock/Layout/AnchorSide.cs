/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>
	/// Determines the side to which a <see cref="LayoutAnchorSide"/> model is anchored in the
	/// <see cref="DockingManager.Layout"/> root dependency property.
	/// </summary>
	public enum AnchorSide
	{
		/// <summary>The object is anchored to the left side.</summary>
		Left,

		/// <summary>The object is anchored to the top side.</summary>
		Top,

		/// <summary>The object is anchored to the right side.</summary>
		Right,

		/// <summary>The object is anchored to the bottom side.</summary>
		Bottom
	}
}
