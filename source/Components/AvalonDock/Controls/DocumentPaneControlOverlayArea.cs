/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	public class DocumentPaneControlOverlayArea : OverlayArea
	{
		#region fields
		private LayoutDocumentPaneControl _documentPaneControl;
		#endregion fields

		#region Constructors

		internal DocumentPaneControlOverlayArea(
			IOverlayWindow overlayWindow,
			LayoutDocumentPaneControl documentPaneControl)
			: base(overlayWindow)
		{
			_documentPaneControl = documentPaneControl;
			base.SetScreenDetectionArea(new Rect(_documentPaneControl.PointToScreenDPI(new Point()), _documentPaneControl.TransformActualSizeToAncestor()));
		}

		#endregion
	}
}
