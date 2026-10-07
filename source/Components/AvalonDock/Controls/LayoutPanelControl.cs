using System;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Panel Control.
	/// </summary>
	public class LayoutPanelControl : LayoutGridControl<ILayoutPanelElement>, ILayoutControl
	{
		private readonly LayoutPanel _model;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutPanelControl"/> class.
		/// </summary>
		/// <param name="model">The model.</param>
		internal LayoutPanelControl(LayoutPanel model)
			: base(model, model.Orientation)
		{
			_model = model;
		}

		/// <inheritdoc/>
		protected override void OnFixChildrenDockLengths()
		{
			if (ActualWidth == 0.0 || ActualHeight == 0.0) return;

			if (_model.Orientation == Orientation.Horizontal)
			{
				if (_model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>())
				{
					for (var i = 0; i < _model.Children.Count; i++)
					{
						if (_model.Children[i] as ILayoutContainer != null &&
							((_model.Children[i] as ILayoutContainer).IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
							 (_model.Children[i] as ILayoutContainer).ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>()))
						{
							// Keep set values (from XML for instance)
							if (!(_model.Children[i] as ILayoutPositionableElement).DockWidth.IsStar) (_model.Children[i] as ILayoutPositionableElement).DockWidth = new GridLength(1.0, GridUnitType.Star);
						}
						else if (_model.Children[i] as ILayoutPositionableElement != null && (_model.Children[i] as ILayoutPositionableElement).DockWidth.IsStar)
						{
							var childPositionableModelWidthActualSize = _model.Children[i] as ILayoutPositionableElement as ILayoutPositionableElementWithActualSize;
							var childDockMinWidth = (_model.Children[i] as ILayoutPositionableElement).CalculatedDockMinWidth();

							// Freezing a Star pane to its rendered width is only meaningful once that
							// width is actually known. On the LibreWPF backend a Star column renders
							// at the content minimum instead of expanding, so the measured width of a
							// pane just docked from a floating window is its minimum and freezing to
							// it collapses the pane into a title-bar sliver. Prefer the floating
							// window's size (kept on the model) and fall back to the rendered width
							// only when no floating size is available.
							var widthToSet = childPositionableModelWidthActualSize.FloatingWidth > childDockMinWidth
								? childPositionableModelWidthActualSize.FloatingWidth
								: childPositionableModelWidthActualSize.ActualWidth;

							widthToSet = Math.Min(widthToSet, ActualWidth / 2.0);
							widthToSet = Math.Max(widthToSet, childDockMinWidth);
							(_model.Children[i] as ILayoutPositionableElement).DockWidth = new GridLength(widthToSet, GridUnitType.Pixel);
						}
					}
				}
				else
				{
					for (var i = 0; i < _model.Children.Count; i++)
					{
						var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;
						if (!childPositionableModel.DockWidth.IsStar)
						{
							// Keep set values (from XML for instance)
							if (!childPositionableModel.DockWidth.IsStar) childPositionableModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
						}
					}
				}
			}
			else // Vertical
			{
				if (_model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>())
				{
					for (var i = 0; i < _model.Children.Count; i++)
					{
						if (_model.Children[i] is ILayoutContainer childContainerModel &&
							(childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
							 childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>()))
						{
							// Keep set values (from XML for instance)
							if (!(_model.Children[i] as ILayoutPositionableElement).DockHeight.IsStar) (_model.Children[i] as ILayoutPositionableElement).DockHeight = new GridLength(1.0, GridUnitType.Star);
						}
						else if (_model.Children[i] as ILayoutPositionableElement != null && (_model.Children[i] as ILayoutPositionableElement).DockHeight.IsStar)
						{
							var childPositionableModelWidthActualSize = _model.Children[i] as ILayoutPositionableElement as ILayoutPositionableElementWithActualSize;
							var childDockMinHeight = (_model.Children[i] as ILayoutPositionableElement).CalculatedDockMinHeight();

							// See the horizontal branch: prefer the floating window's size when the
							// rendered height is the collapsed minimum (LibreWPF Star-row behavior).
							var heightToSet = childPositionableModelWidthActualSize.FloatingHeight > childDockMinHeight
								? childPositionableModelWidthActualSize.FloatingHeight
								: childPositionableModelWidthActualSize.ActualHeight;

							heightToSet = Math.Min(heightToSet, ActualHeight / 2.0);
							heightToSet = Math.Max(heightToSet, childDockMinHeight);
							(_model.Children[i] as ILayoutPositionableElement).DockHeight = new GridLength(heightToSet, GridUnitType.Pixel);
						}
					}
				}
				else
				{
					for (var i = 0; i < _model.Children.Count; i++)
					{
						var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;
						if (!childPositionableModel.DockHeight.IsStar)
						{
							// Keep set values (from XML for instance)
							if (!childPositionableModel.DockHeight.IsStar) childPositionableModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
						}
					}
				}
			}
		}
	}
}