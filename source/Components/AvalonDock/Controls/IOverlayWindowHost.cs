/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;
using System.Windows;

namespace AvalonDock.Controls
{
	internal interface IOverlayWindowHost
	{
		#region Properties

		DockingManager Manager
		{
			get;
		}

		#endregion

		#region Methods

		bool HitTest(Point dragPoint);

		IOverlayWindow ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow);

		void HideOverlayWindow();

		IEnumerable<IDropArea> GetDropAreas(LayoutFloatingWindowControl draggingWindow);

		#endregion
	}
}
