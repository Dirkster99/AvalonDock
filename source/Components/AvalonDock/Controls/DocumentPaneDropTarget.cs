using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the document pane drop target.
	/// </summary>
	internal class DocumentPaneDropTarget : DropTarget<LayoutDocumentPaneControl>
	{
		private LayoutDocumentPaneControl _targetPane;
		private int _tabIndex = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentPaneDropTarget"/> class.
		/// </summary>
		/// <param name="paneControl">The pane control.</param>
		/// <param name="detectionRect">The detection rectangle.</param>
		/// <param name="type">The drop target type.</param>
		internal DocumentPaneDropTarget(
			LayoutDocumentPaneControl paneControl,
			Rect detectionRect,
			DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentPaneDropTarget"/> class.
		/// </summary>
		/// <param name="paneControl">The pane control.</param>
		/// <param name="detectionRect">The detection rectangle.</param>
		/// <param name="type">The drop target type.</param>
		/// <param name="tabIndex">The tab index.</param>
		internal DocumentPaneDropTarget(
			LayoutDocumentPaneControl paneControl,
			Rect detectionRect,
			DropTargetType type,
			int tabIndex)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
			_tabIndex = tabIndex;
		}

		/// <inheritdoc/>
		protected override void Drop(LayoutDocumentFloatingWindow floatingWindow)
		{
			var targetModel = (ILayoutDocumentPane)_targetPane.Model;
			var documentActive = floatingWindow.Descendents().OfType<LayoutDocument>().FirstOrDefault();

			var paneGroup = targetModel.Parent as LayoutDocumentPaneGroup;
			var requiredOrientation = Type == DropTargetType.DocumentPaneDockBottom || Type == DropTargetType.DocumentPaneDockTop ?
				System.Windows.Controls.Orientation.Vertical : System.Windows.Controls.Orientation.Horizontal;
			var allowMixedOrientation = targetModel.Root.Manager.AllowMixedOrientation;

			if (paneGroup == null)
			{
				var targetModelAsPositionableElement = (ILayoutPositionableElement)targetModel;
				var layoutGroup = (ILayoutGroup)targetModel.Parent;
				paneGroup = new LayoutDocumentPaneGroup
				{
					Orientation = requiredOrientation,
					DockWidth = targetModelAsPositionableElement.DockWidth,
					DockHeight = targetModelAsPositionableElement.DockHeight,
				};

				paneGroup.Children.Add(targetModel);
				layoutGroup.InsertChildAt(0, paneGroup);
			}
			else if (allowMixedOrientation && paneGroup.Orientation != requiredOrientation && Type != DropTargetType.DocumentPaneDockInside)
			{
				var targetModelAsPositionableElement = (ILayoutPositionableElement)targetModel;
				var newGroup = new LayoutDocumentPaneGroup
				{
					Orientation = requiredOrientation,
					DockWidth = targetModelAsPositionableElement.DockWidth,
					DockHeight = targetModelAsPositionableElement.DockHeight,
				};
				
				paneGroup.ReplaceChild(targetModel, newGroup);
				newGroup.Children.Add(targetModel);
				paneGroup = newGroup;
			}

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockBottom:
					{
						if (!allowMixedOrientation && paneGroup.Orientation != System.Windows.Controls.Orientation.Vertical)
						{
							paneGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
						}

						var targetIndex = paneGroup.IndexOfChild(targetModel);
						var insertToIndex = targetIndex < 0 ? paneGroup.Children.Count : targetIndex + 1;
						if (insertToIndex > paneGroup.Children.Count)
						{
							insertToIndex = paneGroup.Children.Count;
						}

						var documentsToMove = floatingWindow.Children.ToArray();
						for (int i = 0; i < documentsToMove.Length; i++)
						{
							var floatingChild = documentsToMove[i];
							paneGroup.InsertChildAt(insertToIndex + i, floatingChild);
						}
					}

					break;

				case DropTargetType.DocumentPaneDockTop:
					{
						if (!allowMixedOrientation && paneGroup.Orientation != System.Windows.Controls.Orientation.Vertical)
						{
							paneGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
						}

						var insertToIndex = paneGroup.IndexOfChild(targetModel);
						if (insertToIndex < 0)
						{
							insertToIndex = 0;
						}

						var documentsToMove = floatingWindow.Children.ToArray();
						for (int i = 0; i < documentsToMove.Length; i++)
						{
							var floatingChild = documentsToMove[i];
							paneGroup.InsertChildAt(insertToIndex + i, floatingChild);
						}
					}

					break;

				case DropTargetType.DocumentPaneDockLeft:
					{
						if (!allowMixedOrientation && paneGroup.Orientation != System.Windows.Controls.Orientation.Horizontal)
						{
							paneGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
						}

						var insertToIndex = paneGroup.IndexOfChild(targetModel);
						if (insertToIndex < 0)
						{
							insertToIndex = 0;
						}

						var documentsToMove = floatingWindow.Children.ToArray();
						for (int i = 0; i < documentsToMove.Length; i++)
						{
							var floatingChild = documentsToMove[i];
							paneGroup.InsertChildAt(insertToIndex + i, floatingChild);
						}
					}

					break;

				case DropTargetType.DocumentPaneDockRight:
					{
						if (!allowMixedOrientation && paneGroup.Orientation != System.Windows.Controls.Orientation.Horizontal)
						{
							paneGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
						}

						var targetIndex = paneGroup.IndexOfChild(targetModel);
						var insertToIndex = targetIndex < 0 ? paneGroup.Children.Count : targetIndex + 1;
						if (insertToIndex > paneGroup.Children.Count)
						{
							insertToIndex = paneGroup.Children.Count;
						}

						var documentsToMove = floatingWindow.Children.ToArray();
						for (int i = 0; i < documentsToMove.Length; i++)
						{
							var floatingChild = documentsToMove[i];
							paneGroup.InsertChildAt(insertToIndex + i, floatingChild);
						}
					}

					break;

				case DropTargetType.DocumentPaneDockInside:
					{
						var paneModel = targetModel as LayoutDocumentPane;
						var layoutDocumentPaneGroup = floatingWindow.RootPanel as LayoutDocumentPaneGroup;

						// A LayoutFloatingDocumentWindow can contain multiple instances of both Anchorables or Documents
						// and we should drop these back into the DocumentPane if they are available
						var allowedDropTypes = new[] { typeof(LayoutDocument), typeof(LayoutAnchorable) };

						int i = _tabIndex == -1 ? 0 : _tabIndex;
						foreach (var anchorableToImport in
							layoutDocumentPaneGroup.Descendents().OfType<LayoutContent>()
								.Where(item => allowedDropTypes.Any(dropType => dropType.IsInstanceOfType(item))).ToArray())
						{
							paneModel.Children.Insert(i, anchorableToImport);
							i++;
						}
					}

					break;
			}

			if (documentActive != null)
			{
				documentActive.IsActive = true;
			}

			base.Drop(floatingWindow);
		}

		/// <inheritdoc/>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockBottom:
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

				case DropTargetType.DocumentPaneDockTop:
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

				case DropTargetType.DocumentPaneDockLeft:
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

				case DropTargetType.DocumentPaneDockRight:
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

				case DropTargetType.DocumentPaneDockInside:
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

							// BD: 17.08.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
							// anchorableToImport.SetCanCloseInternal(true);
							paneModel.Children.Insert(i, anchorableToImport);
							i++;
							anchorableToActivate = anchorableToImport;
						}

						anchorableToActivate.IsActive = true;
					}

					break;
			}

			base.Drop(floatingWindow);
		}

		/// <inheritdoc/>
		public override Geometry GetPreviewPath(
			OverlayWindow overlayWindow,
			LayoutFloatingWindow floatingWindowModel)
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
				case DropTargetType.DocumentPaneDockTop:
				case DropTargetType.DocumentPaneDockLeft:
				case DropTargetType.DocumentPaneDockRight:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						if (OverlayPreviewRules.TryComputePanePreviewRect(
							Type,
							targetScreenRect.Width,
							targetScreenRect.Height,
							out var left,
							out var top,
							out var width,
							out var height))
						{
							targetScreenRect = new Rect(
								targetScreenRect.Left + left,
								targetScreenRect.Top + top,
								width,
								height);
						}

						return new RectangleGeometry(targetScreenRect);
					}
			}

			return null;
		}
	}
}