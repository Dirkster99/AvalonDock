using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the docking manager drop target.
	/// </summary>
	internal class DockingManagerDropTarget : DropTarget<DockingManager>
	{
		private DockingManager _manager;

		/// <summary>
		/// Initializes a new instance of the <see cref="DockingManagerDropTarget"/> class.
		/// </summary>
		/// <param name="manager">The manager.</param>
		/// <param name="detectionRect">The detection rectangle.</param>
		/// <param name="type">The drop target type.</param>
		internal DockingManagerDropTarget(
			DockingManager manager,
			Rect detectionRect,
			DropTargetType type)
			: base(manager, detectionRect, type)
		{
			_manager = manager;
		}

		/// <inheritdoc/>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			switch (Type)
			{
				case DropTargetType.DockingManagerDockLeft:
					{
						if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Horizontal &&
							_manager.Layout.RootPanel.Children.Count == 1)
							_manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

						if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
						{
							var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
							if (layoutAnchorablePaneGroup != null &&
								layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal)
							{
								var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
								for (int i = 0; i < childrenToTransfer.Length; i++)
									_manager.Layout.RootPanel.Children.Insert(i, childrenToTransfer[i]);
							}
							else
							{
								_manager.Layout.RootPanel.Children.Insert(0, floatingWindow.RootPanel);
							}
						}
						else
						{
							var newOrientedPanel = new LayoutPanel()
							{
								Orientation = System.Windows.Controls.Orientation.Horizontal
							};

							newOrientedPanel.Children.Add(floatingWindow.RootPanel);
							newOrientedPanel.Children.Add(_manager.Layout.RootPanel);

							_manager.Layout.RootPanel = newOrientedPanel;
						}
					}

					break;

				case DropTargetType.DockingManagerDockRight:
					{
						if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Horizontal &&
							_manager.Layout.RootPanel.Children.Count == 1)
							_manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

						if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
						{
							var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
							if (layoutAnchorablePaneGroup != null &&
								layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal)
							{
								var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
								for (int i = 0; i < childrenToTransfer.Length; i++)
									_manager.Layout.RootPanel.Children.Add(childrenToTransfer[i]);
							}
							else
							{
								_manager.Layout.RootPanel.Children.Add(floatingWindow.RootPanel);
							}
						}
						else
						{
							var newOrientedPanel = new LayoutPanel()
							{
								Orientation = System.Windows.Controls.Orientation.Horizontal
							};

							newOrientedPanel.Children.Add(floatingWindow.RootPanel);
							newOrientedPanel.Children.Insert(0, _manager.Layout.RootPanel);

							_manager.Layout.RootPanel = newOrientedPanel;
						}
					}

					break;

				case DropTargetType.DockingManagerDockTop:
					{
						if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Vertical &&
							_manager.Layout.RootPanel.Children.Count == 1)
							_manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

						if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
						{
							var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
							if (layoutAnchorablePaneGroup != null &&
								layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)
							{
								var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
								for (int i = 0; i < childrenToTransfer.Length; i++)
									_manager.Layout.RootPanel.Children.Insert(i, childrenToTransfer[i]);
							}
							else
							{
								_manager.Layout.RootPanel.Children.Insert(0, floatingWindow.RootPanel);
							}
						}
						else
						{
							var newOrientedPanel = new LayoutPanel()
							{
								Orientation = System.Windows.Controls.Orientation.Vertical
							};

							newOrientedPanel.Children.Add(floatingWindow.RootPanel);
							newOrientedPanel.Children.Add(_manager.Layout.RootPanel);

							_manager.Layout.RootPanel = newOrientedPanel;
						}
					}

					break;

				case DropTargetType.DockingManagerDockBottom:
					{
						if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Vertical &&
							_manager.Layout.RootPanel.Children.Count == 1)
							_manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

						if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
						{
							var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
							if (layoutAnchorablePaneGroup != null &&
								layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)
							{
								var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
								for (int i = 0; i < childrenToTransfer.Length; i++)
									_manager.Layout.RootPanel.Children.Add(childrenToTransfer[i]);
							}
							else
							{
								_manager.Layout.RootPanel.Children.Add(floatingWindow.RootPanel);
							}
						}
						else
						{
							var newOrientedPanel = new LayoutPanel()
							{
								Orientation = System.Windows.Controls.Orientation.Vertical
							};

							newOrientedPanel.Children.Add(floatingWindow.RootPanel);
							newOrientedPanel.Children.Insert(0, _manager.Layout.RootPanel);

							_manager.Layout.RootPanel = newOrientedPanel;
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
			var anchorableFloatingWindowModel = floatingWindowModel as LayoutAnchorableFloatingWindow;
			var layoutAnchorablePane = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElement;
			var layoutAnchorablePaneWithActualSize = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElementWithActualSize;

			var targetScreenRect = TargetElement.GetScreenArea();

			// Preferred dock size used by the outer-edge rules: width for Left/Right, height for Top/Bottom.
			var preferredSize = Type == DropTargetType.DockingManagerDockTop || Type == DropTargetType.DockingManagerDockBottom
				? (layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight)
				: (layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth);

			if (OverlayPreviewRules.TryComputeManagerPreviewRect(
				Type,
				targetScreenRect.Width,
				targetScreenRect.Height,
				preferredSize,
				out var left,
				out var top,
				out var width,
				out var height))
			{
				var previewBoxRect = new Rect(
					targetScreenRect.Left - overlayWindow.Left + left,
					targetScreenRect.Top - overlayWindow.Top + top,
					width,
					height);

				return new RectangleGeometry(previewBoxRect);
			}

			throw new InvalidOperationException();
		}
	}
}