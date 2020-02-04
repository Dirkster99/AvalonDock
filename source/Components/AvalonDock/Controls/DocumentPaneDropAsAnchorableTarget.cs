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
	/// Implements a <see cref="LayoutDocumentPaneControl"/> drop target on which other items
	/// on which other items (<see cref="LayoutDocument"/> or <see cref="LayoutAnchorable"/>) can be dropped.
	/// </summary>
	internal class DocumentPaneDropAsAnchorableTarget : DropTarget<LayoutDocumentPaneControl>
	{
		#region fields
		private LayoutDocumentPaneControl _targetPane;
		private int _tabIndex = -1;
		#endregion fields

		#region Constructors
		/// <summary>
		/// Class constructor from parameters without a specific tabindex as dock position.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal DocumentPaneDropAsAnchorableTarget(LayoutDocumentPaneControl paneControl,
													Rect detectionRect,
													DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		/// <summary>
		/// Class constructor from parameters with a specific tabindex as dock position.
		/// This constructor can be used to drop a document at a specific tab index.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		/// <param name="tabIndex"></param>
		internal DocumentPaneDropAsAnchorableTarget(LayoutDocumentPaneControl paneControl,
													Rect detectionRect,
													DropTargetType type,
													int tabIndex)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
			_tabIndex = tabIndex;
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
			ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
			LayoutDocumentPaneGroup parentGroup;
			LayoutPanel parentGroupPanel;
			FindParentLayoutDocumentPane(targetModel, out parentGroup, out parentGroupPanel);

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockAsAnchorableBottom:
				#region DropTargetType.DocumentPaneDockAsAnchorableBottom
				{
					if (parentGroupPanel != null &&
						parentGroupPanel.ChildrenCount == 1)
						parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

					if (parentGroupPanel != null &&
						parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
					{
						parentGroupPanel.Children.Insert(
							parentGroupPanel.IndexOfChild(parentGroup != null ? parentGroup : targetModel) + 1,
							floatingWindow.RootPanel);
					}
					else if (parentGroupPanel != null)
					{
						var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
						parentGroupPanel.ReplaceChild(parentGroup != null ? parentGroup : targetModel, newParentPanel);
						newParentPanel.Children.Add(parentGroup != null ? parentGroup : targetModel);
						newParentPanel.Children.Add(floatingWindow.RootPanel);
					}
					else
					{
						throw new NotImplementedException();
					}


				}
				break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableBottom

				case DropTargetType.DocumentPaneDockAsAnchorableTop:
				#region DropTargetType.DocumentPaneDockAsAnchorableTop
				{
					if (parentGroupPanel != null &&
						parentGroupPanel.ChildrenCount == 1)
						parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

					if (parentGroupPanel != null &&
						parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
					{
						parentGroupPanel.Children.Insert(
							parentGroupPanel.IndexOfChild(parentGroup != null ? parentGroup : targetModel),
							floatingWindow.RootPanel);
					}
					else if (parentGroupPanel != null)
					{
						var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
						parentGroupPanel.ReplaceChild(parentGroup != null ? parentGroup : targetModel, newParentPanel);
						newParentPanel.Children.Add(parentGroup != null ? parentGroup : targetModel);
						newParentPanel.Children.Insert(0, floatingWindow.RootPanel);
					}
					else
					{
						throw new NotImplementedException();
					}

				}
				break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableTop

				case DropTargetType.DocumentPaneDockAsAnchorableLeft:
				#region DropTargetType.DocumentPaneDockAsAnchorableLeft
				{
					if (parentGroupPanel != null &&
						parentGroupPanel.ChildrenCount == 1)
						parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

					if (parentGroupPanel != null &&
						parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
					{
						parentGroupPanel.Children.Insert(
							parentGroupPanel.IndexOfChild(parentGroup != null ? parentGroup : targetModel),
							floatingWindow.RootPanel);
					}
					else if (parentGroupPanel != null)
					{
						var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
						parentGroupPanel.ReplaceChild(parentGroup != null ? parentGroup : targetModel, newParentPanel);
						newParentPanel.Children.Add(parentGroup != null ? parentGroup : targetModel);
						newParentPanel.Children.Insert(0, floatingWindow.RootPanel);
					}
					else
					{
						throw new NotImplementedException();
					}
				}
				break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableLeft

				case DropTargetType.DocumentPaneDockAsAnchorableRight:
				#region DropTargetType.DocumentPaneDockAsAnchorableRight
				{
					if (parentGroupPanel != null &&
						parentGroupPanel.ChildrenCount == 1)
						parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

					if (parentGroupPanel != null &&
						parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
					{
						parentGroupPanel.Children.Insert(
							parentGroupPanel.IndexOfChild(parentGroup != null ? parentGroup : targetModel) + 1,
							floatingWindow.RootPanel);
					}
					else if (parentGroupPanel != null)
					{
						var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
						parentGroupPanel.ReplaceChild(parentGroup != null ? parentGroup : targetModel, newParentPanel);
						newParentPanel.Children.Add(parentGroup != null ? parentGroup : targetModel);
						newParentPanel.Children.Add(floatingWindow.RootPanel);
					}
					else
					{
						throw new NotImplementedException();
					}
				}
				break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableRight
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
		public override Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel)
		{
			Rect targetScreenRect;
			ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
			var manager = targetModel.Root.Manager;

			//ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
			LayoutDocumentPaneGroup parentGroup;
			LayoutPanel parentGroupPanel;
			if (!FindParentLayoutDocumentPane(targetModel, out parentGroup, out parentGroupPanel))
				return null;

			//if (targetModel.Parent is LayoutDocumentPaneGroup)
			//{
			//    var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;
			//    var documentPaneGroupControl = manager.FindLogicalChildren<LayoutDocumentPaneGroupControl>().First(d => d.Model == parentGroup);
			//    targetScreenRect = documentPaneGroupControl.GetScreenArea();
			//}
			//else
			//{
			//    var documentPaneControl = manager.FindLogicalChildren<LayoutDocumentPaneControl>().First(d => d.Model == targetModel);
			//    targetScreenRect = documentPaneControl.GetScreenArea();
			//}

			//var parentPanel = targetModel.FindParent<LayoutPanel>();
			var documentPaneControl = manager.FindLogicalChildren<FrameworkElement>().OfType<ILayoutControl>().First(d => parentGroup != null ? d.Model == parentGroup : d.Model == parentGroupPanel) as FrameworkElement;
			targetScreenRect = documentPaneControl.GetScreenArea();

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockAsAnchorableBottom:
				{
					targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
					targetScreenRect.Offset(0.0, targetScreenRect.Height - targetScreenRect.Height / 3.0);
					targetScreenRect.Height /= 3.0;
					return new RectangleGeometry(targetScreenRect);
				}

				case DropTargetType.DocumentPaneDockAsAnchorableTop:
				{
					targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
					targetScreenRect.Height /= 3.0;
					return new RectangleGeometry(targetScreenRect);
				}

				case DropTargetType.DocumentPaneDockAsAnchorableRight:
				{
					targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
					targetScreenRect.Offset(targetScreenRect.Width - targetScreenRect.Width / 3.0, 0.0);
					targetScreenRect.Width /= 3.0;
					return new RectangleGeometry(targetScreenRect);
				}

				case DropTargetType.DocumentPaneDockAsAnchorableLeft:
				{
					targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
					targetScreenRect.Width /= 3.0;
					return new RectangleGeometry(targetScreenRect);
				}
			}

			return null;
		}
		#endregion Overrides

		#region Private Methods

		private bool FindParentLayoutDocumentPane(ILayoutDocumentPane documentPane, out LayoutDocumentPaneGroup containerPaneGroup, out LayoutPanel containerPanel)
		{
			containerPaneGroup = null;
			containerPanel = null;

			if (documentPane.Parent is LayoutPanel)
			{
				containerPaneGroup = null;
				containerPanel = documentPane.Parent as LayoutPanel;
				return true;
			}
			else if (documentPane.Parent is LayoutDocumentPaneGroup)
			{
				var currentDocumentPaneGroup = documentPane.Parent as LayoutDocumentPaneGroup;
				while (!(currentDocumentPaneGroup.Parent is LayoutPanel))
				{
					currentDocumentPaneGroup = currentDocumentPaneGroup.Parent as LayoutDocumentPaneGroup;

					if (currentDocumentPaneGroup == null)
						break;
				}

				if (currentDocumentPaneGroup == null)
					return false;

				containerPaneGroup = currentDocumentPaneGroup;
				containerPanel = currentDocumentPaneGroup.Parent as LayoutPanel;
				return true;
			}

			return false;
		}

		#endregion Private Methods
	}
}
