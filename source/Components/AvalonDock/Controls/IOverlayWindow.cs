/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;

namespace AvalonDock.Controls
{
	internal interface IOverlayWindow
	{
		IEnumerable<IDropTarget> GetTargets();

		void DragEnter(LayoutFloatingWindowControl floatingWindow);
		void DragLeave(LayoutFloatingWindowControl floatingWindow);

		void DragEnter(IDropArea area);
		void DragLeave(IDropArea area);

		void DragEnter(IDropTarget target);
		void DragLeave(IDropTarget target);
		void DragDrop(IDropTarget target);
	}
}
