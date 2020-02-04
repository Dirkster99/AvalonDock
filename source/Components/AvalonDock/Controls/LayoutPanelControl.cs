/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a <see cref="Grid"/> based panel base class
	/// that hosts a <see cref="LayoutPanel"/> as its model.
	/// </summary>
	public class LayoutPanelControl : LayoutGridControl<ILayoutPanelElement>, ILayoutControl
	{
		#region fields
		private LayoutPanel _model;
		#endregion fields

		#region Constructors

		internal LayoutPanelControl(LayoutPanel model)
			: base(model, model.Orientation)
		{
			_model = model;

		}

		#endregion

		#region Overrides

		protected override void OnFixChildrenDockLengths()
		{
			if (ActualWidth == 0.0 || ActualHeight == 0.0) return;

			var modelAsPositionableElement = _model as ILayoutPositionableElementWithActualSize;
			#region Setup DockWidth/Height for children
			if (_model.Orientation == Orientation.Horizontal)
			{
				if (_model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>())
				{
					for (var i = 0; i < _model.Children.Count; i++)
					{
						var childContainerModel = _model.Children[i] as ILayoutContainer;
						var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;

						if (childContainerModel != null &&
							(childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
							 childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>()))
						{
							// Keep set values (from XML for instance)
							if(!childPositionableModel.DockWidth.IsStar) childPositionableModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
						}
						else if (childPositionableModel != null && childPositionableModel.DockWidth.IsStar)
						{
							var childPositionableModelWidthActualSize = childPositionableModel as ILayoutPositionableElementWithActualSize;
							var childDockMinWidth = childPositionableModel.CalculatedDockMinWidth();
							var widthToSet = Math.Max(childPositionableModelWidthActualSize.ActualWidth, childDockMinWidth);

							widthToSet = Math.Min(widthToSet, ActualWidth / 2.0);
							widthToSet = Math.Max(widthToSet, childDockMinWidth);

							childPositionableModel.DockWidth = new GridLength(widthToSet, GridUnitType.Pixel);
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
							if(!childPositionableModel.DockWidth.IsStar) childPositionableModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
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
						var childContainerModel = _model.Children[i] as ILayoutContainer;
						var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;

						if (childContainerModel != null &&
							(childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
							 childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>()))
						{
							// Keep set values (from XML for instance)
							if(!childPositionableModel.DockHeight.IsStar) childPositionableModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
						}
						else if (childPositionableModel != null && childPositionableModel.DockHeight.IsStar)
						{
							var childPositionableModelWidthActualSize = childPositionableModel as ILayoutPositionableElementWithActualSize;
							var childDockMinHeight = childPositionableModel.CalculatedDockMinHeight();
							var heightToSet = Math.Max(childPositionableModelWidthActualSize.ActualHeight, childDockMinHeight);
							heightToSet = Math.Min(heightToSet, ActualHeight / 2.0);
							heightToSet = Math.Max(heightToSet, childDockMinHeight);

							childPositionableModel.DockHeight = new GridLength(heightToSet, GridUnitType.Pixel);
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
							if(!childPositionableModel.DockHeight.IsStar) childPositionableModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
						}
					}
				}
			}
			#endregion
		}

		#endregion
	}
}
