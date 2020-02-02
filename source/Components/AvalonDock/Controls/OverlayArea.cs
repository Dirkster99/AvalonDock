/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	public abstract class OverlayArea : IOverlayWindowArea
	{
		#region fields
		private IOverlayWindow _overlayWindow;
		private Rect? _screenDetectionArea;
		#endregion fields

		#region Constructors

		internal OverlayArea(IOverlayWindow overlayWindow)
		{
			_overlayWindow = overlayWindow;
		}

		#endregion

		#region Internal Methods

		protected void SetScreenDetectionArea(Rect rect)
		{
			_screenDetectionArea = rect;
		}

		#endregion

		#region IOverlayWindowArea

		Rect IOverlayWindowArea.ScreenDetectionArea
		{
			get
			{
				return _screenDetectionArea.Value;
			}
		}

		#endregion
	}
}
