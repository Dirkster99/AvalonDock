/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows.Controls;
using System.Windows;
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
		#region fields
		private LayoutDocumentPaneGroup _model;
		#endregion fields

		#region Constructors

		internal LayoutDocumentPaneGroupControl(LayoutDocumentPaneGroup model)
			: base(model, model.Orientation)
		{
			_model = model;
		}

		#endregion Constructors

		#region Overrides

		protected override void OnFixChildrenDockLengths()
		{
			#region Setup DockWidth/Height for children
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
			#endregion Setup DockWidth/Height for children
		}

		#endregion Overrides
	}
}
