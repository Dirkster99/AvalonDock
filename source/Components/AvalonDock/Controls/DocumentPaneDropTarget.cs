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
	internal class DocumentPaneDropTarget : DropTarget<LayoutDocumentPaneControl>
	{
		#region Members

		private LayoutDocumentPaneControl _targetPane;
		private int _tabIndex = -1;

		#endregion

		#region Constructors

		internal DocumentPaneDropTarget(LayoutDocumentPaneControl paneControl, Rect detectionRect, DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		internal DocumentPaneDropTarget(LayoutDocumentPaneControl paneControl, Rect detectionRect, DropTargetType type, int tabIndex)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
			_tabIndex = tabIndex;
		}

		#endregion

		#region Overrides

		protected override void Drop(LayoutDocumentFloatingWindow floatingWindow)
		{
			ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
			LayoutDocument documentActive = floatingWindow.Descendents().OfType<LayoutDocument>().FirstOrDefault();

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockBottom:
					#region DropTargetType.DocumentPaneDockBottom
				{
					var parentModel = targetModel.Parent as ILayoutGroup;
					var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
					int insertToIndex = parentModel.IndexOfChild(targetModel);

					if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Vertical &&
					    parentModel.ChildrenCount == 1)
						parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Vertical;

					if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Vertical)
					{
						var layoutDocumentPaneGroup = floatingWindow.RootPanel as LayoutDocumentPaneGroup;
						if (layoutDocumentPaneGroup != null &&
						    (layoutDocumentPaneGroup.Children.Count == 1 ||
						     layoutDocumentPaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical))
						{
							var documentsToMove = layoutDocumentPaneGroup.Children.ToArray();
							for (int i = 0; i < documentsToMove.Length; i++)
								parentModel.InsertChildAt(insertToIndex + 1 + i, documentsToMove[i]);
						}
						else
							parentModel.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
					}
					else
					{
						var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
						var newOrientedPanel = new LayoutDocumentPaneGroup()
						{
							Orientation = System.Windows.Controls.Orientation.Vertical,
							DockWidth = targetModelAsPositionableElement.DockWidth,
							DockHeight = targetModelAsPositionableElement.DockHeight,
						};

						parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
						newOrientedPanel.Children.Add(targetModel);
						newOrientedPanel.Children.Add(floatingWindow.RootPanel);

					}
				}
					break;

				#endregion
				case DropTargetType.DocumentPaneDockTop:
					#region DropTargetType.DocumentPaneDockTop
				{
					var parentModel = targetModel.Parent as ILayoutGroup;
					var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
					int insertToIndex = parentModel.IndexOfChild(targetModel);

					if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Vertical &&
					    parentModel.ChildrenCount == 1)
						parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Vertical;

					if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Vertical)
					{
						var layoutDocumentPaneGroup = floatingWindow.RootPanel as LayoutDocumentPaneGroup;
						if (layoutDocumentPaneGroup != null &&
						    (layoutDocumentPaneGroup.Children.Count == 1 ||
						     layoutDocumentPaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical))
						{
							var documentsToMove = layoutDocumentPaneGroup.Children.ToArray();
							for (int i = 0; i < documentsToMove.Length; i++)
								parentModel.InsertChildAt(insertToIndex + i, documentsToMove[i]);
						}
						else
							parentModel.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
					}
					else
					{
						var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
						var newOrientedPanel = new LayoutDocumentPaneGroup()
						{
							Orientation = System.Windows.Controls.Orientation.Vertical,
							DockWidth = targetModelAsPositionableElement.DockWidth,
							DockHeight = targetModelAsPositionableElement.DockHeight,
						};

						parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
						//the floating window must be added after the target modal as it could be raise a CollectGarbage call
						newOrientedPanel.Children.Add(targetModel);
						newOrientedPanel.Children.Insert(0, floatingWindow.RootPanel);

					}
				}
					break;
				#endregion
				case DropTargetType.DocumentPaneDockLeft:
					#region DropTargetType.DocumentPaneDockLeft
				{
					var parentModel = targetModel.Parent as ILayoutGroup;
					var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
					int insertToIndex = parentModel.IndexOfChild(targetModel);

					if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Horizontal &&
					    parentModel.ChildrenCount == 1)
						parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Horizontal;

					if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Horizontal)
					{
						var layoutDocumentPaneGroup = floatingWindow.RootPanel as LayoutDocumentPaneGroup;
						if (layoutDocumentPaneGroup != null &&
						    (layoutDocumentPaneGroup.Children.Count == 1 ||
						     layoutDocumentPaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal))
						{
							var documentsToMove = layoutDocumentPaneGroup.Children.ToArray();
							for (int i = 0; i < documentsToMove.Length; i++)
								parentModel.InsertChildAt(insertToIndex + i, documentsToMove[i]);
						}
						else
							parentModel.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
					}
					else
					{
						var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
						var newOrientedPanel = new LayoutDocumentPaneGroup()
						{
							Orientation = System.Windows.Controls.Orientation.Horizontal,
							DockWidth = targetModelAsPositionableElement.DockWidth,
							DockHeight = targetModelAsPositionableElement.DockHeight,
						};

						parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
						//the floating window must be added after the target modal as it could be raise a CollectGarbage call
						newOrientedPanel.Children.Add(targetModel);
						newOrientedPanel.Children.Insert(0, floatingWindow.RootPanel);

					}
				}
					break;
				#endregion
				case DropTargetType.DocumentPaneDockRight:
					#region DropTargetType.DocumentPaneDockRight
				{
					var parentModel = targetModel.Parent as ILayoutGroup;
					var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
					int insertToIndex = parentModel.IndexOfChild(targetModel);

					if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Horizontal &&
					    parentModel.ChildrenCount == 1)
						parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Horizontal;

					if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Horizontal)
					{
						var layoutDocumentPaneGroup = floatingWindow.RootPanel as LayoutDocumentPaneGroup;
						if (layoutDocumentPaneGroup != null &&
						    (layoutDocumentPaneGroup.Children.Count == 1 ||
						     layoutDocumentPaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal))
						{
							var documentToMove = layoutDocumentPaneGroup.Children.ToArray();
							for (int i = 0; i < documentToMove.Length; i++)
								parentModel.InsertChildAt(insertToIndex + 1 + i, documentToMove[i]);
						}
						else
							parentModel.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
					}
					else
					{
						var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
						var newOrientedPanel = new LayoutDocumentPaneGroup()
						{
							Orientation = System.Windows.Controls.Orientation.Horizontal,
							DockWidth = targetModelAsPositionableElement.DockWidth,
							DockHeight = targetModelAsPositionableElement.DockHeight,
						};

						parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
						newOrientedPanel.Children.Add(targetModel);
						newOrientedPanel.Children.Add(floatingWindow.RootPanel);

					}
				}
					break;
				#endregion

				case DropTargetType.DocumentPaneDockInside:
					#region DropTargetType.DocumentPaneDockInside
				{
					var paneModel = targetModel as LayoutDocumentPane;
					var layoutDocumentPaneGroup = floatingWindow.RootPanel as LayoutDocumentPaneGroup;

					int i = _tabIndex == -1 ? 0 : _tabIndex;
					foreach (var anchorableToImport in
						layoutDocumentPaneGroup.Descendents().OfType<LayoutDocument>().ToArray())
					{
						paneModel.Children.Insert(i, anchorableToImport);
						i++;
					}
				}
					break;
				#endregion
			}

			documentActive.IsActive = true;

			base.Drop(floatingWindow);
		}

		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockBottom:
					#region DropTargetType.DocumentPaneDockBottom
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Vertical };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
							newParentModel.Children.Add(newLayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Vertical)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Vertical;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex + 1, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(targetModel);
								newChildGroup.Children.Add(newLayoutDocumentPane);
							}
						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
				#endregion
				case DropTargetType.DocumentPaneDockTop:
					#region DropTargetType.DocumentPaneDockTop
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Vertical };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(newLayoutDocumentPane);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Vertical)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Vertical;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(newLayoutDocumentPane);
								newChildGroup.Children.Add(targetModel);
							}
						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
				#endregion
				case DropTargetType.DocumentPaneDockLeft:
					#region DropTargetType.DocumentPaneDockLeft
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Horizontal };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(newLayoutDocumentPane);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Horizontal)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Horizontal;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(newLayoutDocumentPane);
								newChildGroup.Children.Add(targetModel);
							}

						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
				#endregion
				case DropTargetType.DocumentPaneDockRight:
					#region DropTargetType.DocumentPaneDockRight
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Horizontal };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
							newParentModel.Children.Add(newLayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Horizontal)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Horizontal;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex + 1, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(targetModel);
								newChildGroup.Children.Add(newLayoutDocumentPane);
							}
						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
				#endregion
				case DropTargetType.DocumentPaneDockInside:
					#region DropTargetType.DocumentPaneDockInside
					{
						var paneModel = targetModel as LayoutDocumentPane;
						var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

						bool checkPreviousContainer = true;
						int i = 0;
						if (_tabIndex != -1)
						{
							i = _tabIndex;
							checkPreviousContainer = false;
						}
						LayoutAnchorable anchorableToActivate = null;

						foreach (var anchorableToImport in layoutAnchorablePaneGroup.Descendents().OfType<LayoutAnchorable>().ToArray())
						{
							if (checkPreviousContainer)
							{
								var previousContainer = ((ILayoutPreviousContainer)anchorableToImport).PreviousContainer;
								if (object.ReferenceEquals(previousContainer, targetModel) && (anchorableToImport.PreviousContainerIndex != -1))
								{
									i = anchorableToImport.PreviousContainerIndex;
								}
								checkPreviousContainer = false;
							}

							anchorableToImport.SetCanCloseInternal(true);

							paneModel.Children.Insert(i, anchorableToImport);
							i++;
							anchorableToActivate = anchorableToImport;
						}

						anchorableToActivate.IsActive = true;
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
				case DropTargetType.DocumentPaneDockInside:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						if (_tabIndex == -1)
						{
							return new RectangleGeometry(targetScreenRect);
						}
						else
						{
							var translatedDetectionRect = new Rect(DetectionRects[0].TopLeft, DetectionRects[0].BottomRight);
							translatedDetectionRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

							var pathFigure = new PathFigure();
							pathFigure.StartPoint = targetScreenRect.BottomRight;
							pathFigure.Segments.Add(new LineSegment() { Point = new Point(targetScreenRect.Right, translatedDetectionRect.Bottom) });
							pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.BottomRight });
							pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.TopRight });
							pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.TopLeft });
							pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.BottomLeft });
							pathFigure.Segments.Add(new LineSegment() { Point = new Point(targetScreenRect.Left, translatedDetectionRect.Bottom) });
							pathFigure.Segments.Add(new LineSegment() { Point = targetScreenRect.BottomLeft });
							pathFigure.IsClosed = true;
							pathFigure.IsFilled = true;
							pathFigure.Freeze();
							return new PathGeometry(new PathFigure[] { pathFigure });
						}
					}
				case DropTargetType.DocumentPaneDockBottom:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
						targetScreenRect.Offset(0.0, targetScreenRect.Height / 2.0);
						targetScreenRect.Height /= 2.0;
						return new RectangleGeometry(targetScreenRect);
					}
				case DropTargetType.DocumentPaneDockTop:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
						targetScreenRect.Height /= 2.0;
						return new RectangleGeometry(targetScreenRect);
					}
				case DropTargetType.DocumentPaneDockLeft:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
						targetScreenRect.Width /= 2.0;
						return new RectangleGeometry(targetScreenRect);
					}
				case DropTargetType.DocumentPaneDockRight:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
						targetScreenRect.Offset(targetScreenRect.Width / 2.0, 0.0);
						targetScreenRect.Width /= 2.0;
						return new RectangleGeometry(targetScreenRect);
					}
			}

			return null;
		}

		#endregion
	}
}
