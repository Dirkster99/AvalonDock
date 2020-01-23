/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.ObjectModel;

namespace AvalonDock.Layout
{
	public interface ILayoutRoot
	{
		DockingManager Manager
		{
			get;
		}

		LayoutPanel RootPanel
		{
			get;
		}

		LayoutAnchorSide TopSide
		{
			get;
		}
		LayoutAnchorSide LeftSide
		{
			get;
		}
		LayoutAnchorSide RightSide
		{
			get;
		}
		LayoutAnchorSide BottomSide
		{
			get;
		}

		LayoutContent ActiveContent
		{
			get; set;
		}

		ObservableCollection<LayoutFloatingWindow> FloatingWindows
		{
			get;
		}
		ObservableCollection<LayoutAnchorable> Hidden
		{
			get;
		}

		void CollectGarbage();
	}
}
