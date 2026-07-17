using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Anchorable Pane Group Control.
	/// </summary>
	public class LayoutAnchorablePaneGroupControl : LayoutGridControl<ILayoutAnchorablePane>, ILayoutControl
	{
		private LayoutAnchorablePaneGroup _model;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorablePaneGroupControl"/> class.
		/// </summary>
		/// <param name="model">The model.</param>
		internal LayoutAnchorablePaneGroupControl(LayoutAnchorablePaneGroup model)
			: base(model, model.Orientation)
		{
			_model = model;
		}

		/// <inheritdoc/>
		protected override void OnFixChildrenDockLengths()
		{
			if (_model.Orientation == Orientation.Horizontal)
			{
				// Setup DockWidth for children
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
				// Setup DockHeight for children
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