/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	internal interface IDropTarget
	{
		#region Properties

		DropTargetType Type
		{
			get;
		}

		#endregion

		#region Methods

		Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindow);

		bool HitTest(Point dragPoint);

		void Drop(LayoutFloatingWindow floatingWindow);

		void DragEnter();

		void DragLeave();

		#endregion
	}
}
