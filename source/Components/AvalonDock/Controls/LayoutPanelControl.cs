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
#if WINDOWS_APP_SDK
							// WinUI 3: the dispatched children refresh runs after this grid is
							// arranged but before the new pane controls are, so the model's
							// ActualWidth is still 0 here. Converting now would pin the pane at
							// its minimum width. Leave it star — matches Uno, where this code is
							// only reached before the grid is arranged (early return above).
							if (childPositionableModelWidthActualSize.ActualWidth == 0) continue;
#endif
							var childDockMinWidth = (_model.Children[i] as ILayoutPositionableElement).CalculatedDockMinWidth();
							var widthToSet = Math.Max(childPositionableModelWidthActualSize.ActualWidth, childDockMinWidth);

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
#if WINDOWS_APP_SDK
							// See the DockWidth branch above — never pin a not-yet-arranged pane.
							if (childPositionableModelWidthActualSize.ActualHeight == 0) continue;
#endif
							var childDockMinHeight = (_model.Children[i] as ILayoutPositionableElement).CalculatedDockMinHeight();
							var heightToSet = Math.Max(childPositionableModelWidthActualSize.ActualHeight, childDockMinHeight);
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