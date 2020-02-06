/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.ObjectModel;

namespace AvalonDock.Layout
{
	/// <summary>Interface can be used to query and manipulate the child layout elements
	/// directly under the layout root. This interface is implemented by the <see cref="LayoutRoot"/> class.
	///
	/// It represents the root of the layout model (see Layout property in the <see cref="DockingManager"/>).</summary>
	public interface ILayoutRoot
	{
		/// <summary>Gets the <see cref="DockingManager"/> (visual root) of the docking system.</summary>
		DockingManager Manager { get; }

		/// <summary>Gets the <see cref="LayoutPanel"/> that acts as a root of this layout root.</summary>
		LayoutPanel RootPanel { get; }

		/// <summary>Gets the top side <see cref="LayoutAnchorSide"/> where a layout Anchorable
		/// can be/or has been anchored in this layout root.</summary>
		LayoutAnchorSide TopSide { get; }

		/// <summary>Gets the left side <see cref="LayoutAnchorSide"/> where a layout Anchorable
		/// can be/or has been anchored in this layout root.</summary>
		LayoutAnchorSide LeftSide { get; }

		/// <summary>Gets the right side <see cref="LayoutAnchorSide"/> where a layout Anchorable
		/// can be/or has been anchored in this layout root.</summary>
		LayoutAnchorSide RightSide { get; }

		/// <summary>Gets the bottom side <see cref="LayoutAnchorSide"/> where a layout Anchorable
		/// can be/or has been anchored in this layout root.</summary>
		LayoutAnchorSide BottomSide { get; }

		/// <summary>Gets the currently active <see cref="LayoutContent"/>.</summary>
		LayoutContent ActiveContent { get; set; }

		/// <summary>Gets a collection of <see cref="LayoutFloatingWindow"/> items that are currently displayed as floating document or floating layout Anchoreable.</summary>
		ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; }

		/// <summary>Gets a collection of <see cref="LayoutAnchorable"/> items that are currently not displayed.</summary>
		ObservableCollection<LayoutAnchorable> Hidden { get; }

		/// <summary>This method can be used to traverse the tree of layout objects and
		/// remove empty unused elements, such as, <see cref="LayoutAnchorablePanes"/> without child elements.</summary>
		void CollectGarbage();
	}
}
