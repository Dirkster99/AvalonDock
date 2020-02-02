/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	public class DockingManagerOverlayArea : OverlayArea
	{
		#region fields
		private DockingManager _manager;
		#endregion fields

		#region Constructors

		internal DockingManagerOverlayArea(IOverlayWindow overlayWindow, DockingManager manager)
			: base(overlayWindow)
		{
			_manager = manager;

			base.SetScreenDetectionArea(new Rect(
				_manager.PointToScreenDPI(new Point()),
				_manager.TransformActualSizeToAncestor()));
		}

		#endregion
	}
}
