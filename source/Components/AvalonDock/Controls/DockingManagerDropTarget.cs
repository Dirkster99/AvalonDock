/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a <see cref="DockingManager"/> drop target
	/// on which other items (<see cref="LayoutDocument"/> or <see cref="LayoutAnchorable"/>) can be dropped.
	/// 
	/// The resulting drop targets are usually the 4 outer drop target buttons
	/// re-presenting a <see cref="LayoutAnchorSideControl"/> shown as overlay
	/// on the <see cref="DockingManager"/> when the user drags an item over it.
	/// </summary>
	internal class DockingManagerDropTarget : DropTarget<DockingManager>
	{
		#region fields
		private DockingManager _manager;
		#endregion  fields

		#region Constructors
		/// <summary>
		/// Class constructor 
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal DockingManagerDropTarget(DockingManager manager,
										  Rect detectionRect,
										  DropTargetType type)
			: base(manager, detectionRect, type)
		{
			_manager = manager;
		}
		#endregion Constructors

		#region Overrides

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the LayoutAnchorable <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			switch (Type)
			{
				case DropTargetType.DockingManagerDockLeft:
				#region DropTargetType.DockingManagerDockLeft
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
							_manager.Layout.RootPanel.Children.Insert(0, floatingWindow.RootPanel);
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
				#endregion DropTargetType.DockingManagerDockLeft

				case DropTargetType.DockingManagerDockRight:
				#region DropTargetType.DockingManagerDockRight
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
							_manager.Layout.RootPanel.Children.Add(floatingWindow.RootPanel);
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
				#endregion DropTargetType.DockingManagerDockRight

				case DropTargetType.DockingManagerDockTop:
				#region DropTargetType.DockingManagerDockTop
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
							_manager.Layout.RootPanel.Children.Insert(0, floatingWindow.RootPanel);
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
				#endregion DropTargetType.DockingManagerDockTop

				case DropTargetType.DockingManagerDockBottom:
				#region DropTargetType.DockingManagerDockBottom
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
							_manager.Layout.RootPanel.Children.Add(floatingWindow.RootPanel);
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
				#endregion DropTargetType.DockingManagerDockBottom
			}

			base.Drop(floatingWindow);
		}

		/// <summary>
		/// Gets a <see cref="Geometry"/> that is used to highlight/preview the docking position
		/// of this drop target for a <paramref name="floatingWindowModel"/> being docked inside an
		/// <paramref name="overlayWindow"/>.
		/// </summary>
		/// <param name="overlayWindow"></param>
		/// <param name="floatingWindowModel"></param>
		/// <returns>The geometry of the preview/highlighting WPF figure path.</returns>
		public override Geometry GetPreviewPath(OverlayWindow overlayWindow,
												LayoutFloatingWindow floatingWindowModel)
		{
			var anchorableFloatingWindowModel = floatingWindowModel as LayoutAnchorableFloatingWindow;
			var layoutAnchorablePane = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElement;
			var layoutAnchorablePaneWithActualSize = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElementWithActualSize;

			var targetScreenRect = TargetElement.GetScreenArea();

			switch (Type)
			{
				case DropTargetType.DockingManagerDockLeft:
				{
					var desideredWidth = layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth;
					var previewBoxRect = new Rect(
						targetScreenRect.Left - overlayWindow.Left,
						targetScreenRect.Top - overlayWindow.Top,
						Math.Min(desideredWidth, targetScreenRect.Width / 2.0),
						targetScreenRect.Height);

					return new RectangleGeometry(previewBoxRect);
				}

				case DropTargetType.DockingManagerDockTop:
				{
					var desideredHeight = layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight;
					var previewBoxRect = new Rect(
						targetScreenRect.Left - overlayWindow.Left,
						targetScreenRect.Top - overlayWindow.Top,
						targetScreenRect.Width,
						Math.Min(desideredHeight, targetScreenRect.Height / 2.0));

					return new RectangleGeometry(previewBoxRect);
				}

				case DropTargetType.DockingManagerDockRight:
				{
					var desideredWidth = layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth;
					var previewBoxRect = new Rect(
						targetScreenRect.Right - overlayWindow.Left - Math.Min(desideredWidth, targetScreenRect.Width / 2.0),
						targetScreenRect.Top - overlayWindow.Top,
						Math.Min(desideredWidth, targetScreenRect.Width / 2.0),
						targetScreenRect.Height);

					return new RectangleGeometry(previewBoxRect);
				}

				case DropTargetType.DockingManagerDockBottom:
				{
					var desideredHeight = layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight;
					var previewBoxRect = new Rect(
						targetScreenRect.Left - overlayWindow.Left,
						targetScreenRect.Bottom - overlayWindow.Top - Math.Min(desideredHeight, targetScreenRect.Height / 2.0),
						targetScreenRect.Width,
						Math.Min(desideredHeight, targetScreenRect.Height / 2.0));

					return new RectangleGeometry(previewBoxRect);
				}
			}

			throw new InvalidOperationException();
		}
		#endregion Overrides
	}
}
