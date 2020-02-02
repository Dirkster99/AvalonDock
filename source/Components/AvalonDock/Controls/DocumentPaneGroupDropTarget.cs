/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	internal class DocumentPaneGroupDropTarget : DropTarget<LayoutDocumentPaneGroupControl>
	{
		#region Constructors

		internal DocumentPaneGroupDropTarget(LayoutDocumentPaneGroupControl paneControl, Rect detectionRect, DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		#endregion

		#region fields
		private LayoutDocumentPaneGroupControl _targetPane;
		#endregion fields

		#region Overrides

		protected override void Drop(LayoutDocumentFloatingWindow floatingWindow)
		{
			ILayoutPane targetModel = _targetPane.Model as ILayoutPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneGroupDockInside:
					#region DropTargetType.DocumentPaneGroupDockInside
					{
						var paneGroupModel = targetModel as LayoutDocumentPaneGroup;
						var paneModel = paneGroupModel as LayoutDocumentPaneGroup;
						var sourceModel = floatingWindow.RootPanel as LayoutDocumentPaneGroup;

						paneModel.Children.Insert(0, sourceModel);
					}
					break;
					#endregion
			}
			base.Drop(floatingWindow);
		}

		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			ILayoutPane targetModel = _targetPane.Model as ILayoutPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneGroupDockInside:
					#region DropTargetType.DocumentPaneGroupDockInside
					{
						var paneGroupModel = targetModel as LayoutDocumentPaneGroup;
						var paneModel = paneGroupModel.Children[0] as LayoutDocumentPane;
						var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

						int i = 0;
						foreach (var anchorableToImport in layoutAnchorablePaneGroup.Descendents().OfType<LayoutAnchorable>().ToArray())
						{
							anchorableToImport.SetCanCloseInternal(true);

							paneModel.Children.Insert(i, anchorableToImport);
							i++;
						}
					}
					break;
					#endregion
			}

			base.Drop(floatingWindow);
		}

		public override System.Windows.Media.Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel)
		{
			switch (Type)
			{
				case DropTargetType.DocumentPaneGroupDockInside:
					#region DropTargetType.DocumentPaneGroupDockInside
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						return new RectangleGeometry(targetScreenRect);
					}
					#endregion
			}

			return null;
		}

		#endregion
	}
}
