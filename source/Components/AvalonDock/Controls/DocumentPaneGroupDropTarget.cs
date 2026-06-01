using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the document pane group drop target.
	/// </summary>
	internal class DocumentPaneGroupDropTarget : DropTarget<LayoutDocumentPaneGroupControl>
	{
		private LayoutDocumentPaneGroupControl _targetPane;

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentPaneGroupDropTarget"/> class.
		/// </summary>
		/// <param name="paneControl">The pane control.</param>
		/// <param name="detectionRect">The detection rectangle.</param>
		/// <param name="type">The drop target type.</param>
		internal DocumentPaneGroupDropTarget(
			LayoutDocumentPaneGroupControl paneControl,
			Rect detectionRect,
			DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		/// <inheritdoc/>
		protected override void Drop(LayoutDocumentFloatingWindow floatingWindow)
		{
			ILayoutPane targetModel = _targetPane.Model as ILayoutPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneGroupDockInside:
					{
						var paneGroupModel = targetModel as LayoutDocumentPaneGroup;
						var paneModel = paneGroupModel as LayoutDocumentPaneGroup;
						var sourceModel = floatingWindow.RootPanel as LayoutDocumentPaneGroup;

						paneModel.Children.Insert(0, sourceModel);
					}

					break;
			}

			base.Drop(floatingWindow);
		}

		/// <inheritdoc/>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			ILayoutPane targetModel = _targetPane.Model as ILayoutPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneGroupDockInside:
					{
						var paneGroupModel = targetModel as LayoutDocumentPaneGroup;
						var paneModel = paneGroupModel.Children[0] as LayoutDocumentPane;
						var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

						int i = 0;
						foreach (var anchorableToImport in layoutAnchorablePaneGroup.Descendents().OfType<LayoutAnchorable>().ToArray())
						{
							// BD: 18.07.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
							// anchorableToImport.SetCanCloseInternal(true);
							paneModel.Children.Insert(i, anchorableToImport);
							i++;
						}
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
				case DropTargetType.DocumentPaneGroupDockInside:
					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						return new RectangleGeometry(targetScreenRect);
					}
			}

			return null;
		}
	}
}