/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	public class OverlayWindowDropTarget : IOverlayWindowDropTarget
	{
		#region fields
		private IOverlayWindowArea _overlayArea;
		private Rect _screenDetectionArea;
		private OverlayWindowDropTargetType _type;
		#endregion fields

		#region Constructors

		internal OverlayWindowDropTarget(IOverlayWindowArea overlayArea, OverlayWindowDropTargetType targetType, FrameworkElement element)
		{
			_overlayArea = overlayArea;
			_type = targetType;
			_screenDetectionArea = new Rect(element.TransformToDeviceDPI(new Point()), element.TransformActualSizeToAncestor());
		}

		#endregion


		#region IOverlayWindowDropTarget

		Rect IOverlayWindowDropTarget.ScreenDetectionArea
		{
			get
			{
				return _screenDetectionArea;
			}

		}

		OverlayWindowDropTargetType IOverlayWindowDropTarget.Type
		{
			get
			{
				return _type;
			}
		}

		#endregion
	}
}
