/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a group control that hosts a <see cref="LayoutDocumentPaneGroup"/> model.
	///
	/// This Grid based control can host multiple other controls in its Children collection
	/// (<see cref="LayoutAnchorableControl"/>, <see cref="LayoutDocumentControl"/> etc).
	/// </summary>
	public class LayoutDocumentPaneGroupControl : LayoutGridControl<ILayoutDocumentPane>, ILayoutControl
	{
		private readonly LayoutDocumentPaneGroup _model;

		internal LayoutDocumentPaneGroupControl(LayoutDocumentPaneGroup model)
			: base(model, model.Orientation)
		{
			_model = model;
		}

		protected override void OnFixChildrenDockLengths()
		{
			if (_model.Orientation == Orientation.Horizontal)
			{
				for (int i = 0; i < _model.Children.Count; i++)
				{
					var childModel = _model.Children[i] as ILayoutPositionableElement;
					if (!childModel.DockWidth.IsStar)
					{
						childModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
					}
				}
			}
			else
			{
				for (int i = 0; i < _model.Children.Count; i++)
				{
					var childModel = _model.Children[i] as ILayoutPositionableElement;
					if (!childModel.DockHeight.IsStar)
					{
						childModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
					}
				}
			}
		}
	}
}