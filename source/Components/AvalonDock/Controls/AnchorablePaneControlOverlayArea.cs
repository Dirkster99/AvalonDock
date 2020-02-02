/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	public class AnchorablePaneControlOverlayArea : OverlayArea
	{
		#region fields
		private LayoutAnchorablePaneControl _anchorablePaneControl;
		#endregion fields

		#region constructors

		internal AnchorablePaneControlOverlayArea(
			IOverlayWindow overlayWindow,
			LayoutAnchorablePaneControl anchorablePaneControl)
			: base(overlayWindow)
		{

			_anchorablePaneControl = anchorablePaneControl;
			base.SetScreenDetectionArea(new Rect(
				_anchorablePaneControl.PointToScreenDPI(new Point()),
				_anchorablePaneControl.TransformActualSizeToAncestor()));

		}

		#endregion
	}
}
