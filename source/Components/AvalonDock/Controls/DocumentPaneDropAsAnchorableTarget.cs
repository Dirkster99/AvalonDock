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
			if (!FindParentLayoutDocumentPane(targetModel, out parentGroup, out parentGroupPanel))
			{
				parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;
				var dropTargetObject = parentGroup.Parent as LayoutDocumentFloatingWindow;
				if (parentGroup != null && dropTargetObject != null)
					DropLayoutAnchorable(floatingWindow);

				base.Drop(floatingWindow);
				return;
			}

			// We found  a parentGroup and a parentGroupPanel o lets go ahead and insert it
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
		/// Dropping a <see cref="LayoutAnchorableFloatingWindow"/> source into a
		/// <see cref="LayoutDocumentFloatingWindow"/> target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		private void DropLayoutAnchorable(LayoutAnchorableFloatingWindow floatingWindow)
		{
			ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;

			switch (Type)
			{
				case DropTargetType.DocumentPaneDockAsAnchorableBottom:
					#region DropTargetType.DocumentPaneDockAsAnchorableBottom
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Vertical };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
							newParentModel.Children.Add(newLayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Vertical)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Vertical;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex + 1, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(targetModel);
								newChildGroup.Children.Add(newLayoutDocumentPane);
							}
						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableBottom

				case DropTargetType.DocumentPaneDockAsAnchorableTop:
					#region DropTargetType.DocumentPaneDockAsAnchorableTop
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Vertical };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(newLayoutDocumentPane);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Vertical)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Vertical;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(newLayoutDocumentPane);
								newChildGroup.Children.Add(targetModel);
							}
						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableTop

				case DropTargetType.DocumentPaneDockAsAnchorableLeft:
					#region DropTargetType.DocumentPaneDockAsAnchorableLeft
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Horizontal };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(newLayoutDocumentPane);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Horizontal)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Horizontal;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(newLayoutDocumentPane);
								newChildGroup.Children.Add(targetModel);
							}

						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);
					}
					break;
				#endregion DropTargetType.DocumentPaneDockAsAnchorableLeft

				case DropTargetType.DocumentPaneDockAsAnchorableRight:
					#region DropTargetType.DocumentPaneDockAsAnchorableRight
					{
						var parentModel = targetModel.Parent as LayoutDocumentPaneGroup;
						var newLayoutDocumentPane = new LayoutDocumentPane();

						if (parentModel == null)
						{
							var parentContainer = targetModel.Parent as ILayoutContainer;
							var newParentModel = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Horizontal };
							parentContainer.ReplaceChild(targetModel, newParentModel);
							newParentModel.Children.Add(targetModel as LayoutDocumentPane);
							newParentModel.Children.Add(newLayoutDocumentPane);
						}
						else
						{
							var manager = parentModel.Root.Manager;
							if (!manager.AllowMixedOrientation || parentModel.Orientation == System.Windows.Controls.Orientation.Horizontal)
							{
								parentModel.Orientation = System.Windows.Controls.Orientation.Horizontal;
								int targetPaneIndex = parentModel.IndexOfChild(targetModel);
								parentModel.Children.Insert(targetPaneIndex + 1, newLayoutDocumentPane);
							}
							else
							{
								LayoutDocumentPaneGroup newChildGroup = new LayoutDocumentPaneGroup();
								newChildGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
								parentModel.ReplaceChild(targetModel, newChildGroup);
								newChildGroup.Children.Add(targetModel);
								newChildGroup.Children.Add(newLayoutDocumentPane);
							}
						}

						foreach (var cntToTransfer in floatingWindow.RootPanel.Descendents().OfType<LayoutAnchorable>().ToArray())
							newLayoutDocumentPane.Children.Add(cntToTransfer);

					}
					break;
					#endregion DropTargetType.DocumentPaneDockAsAnchorableRight
			}
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

			LayoutDocumentPaneGroup parentGroup;
			LayoutPanel parentGroupPanel;
			FrameworkElement documentPaneControl;
			if (!FindParentLayoutDocumentPane(targetModel, out parentGroup, out parentGroupPanel))
			{
				parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;
				if (parentGroup != null && parentGroup.Parent is LayoutFloatingWindow)
				{
					documentPaneControl = manager.FindLogicalChildren<FrameworkElement>()
						.OfType<ILayoutControl>().First(d => parentGroup != null ? d.Model == parentGroup :
																				   d.Model == parentGroup.Parent) as FrameworkElement;
					targetScreenRect = documentPaneControl.GetScreenArea();

					return PreviewDocumentPaneDockAsAnchorable(overlayWindow, targetScreenRect, this.Type);
				}

				return null;
			}

			documentPaneControl = manager.FindLogicalChildren<FrameworkElement>().OfType<ILayoutControl>().First(d => parentGroup != null ? d.Model == parentGroup : d.Model == parentGroupPanel) as FrameworkElement;
			targetScreenRect = documentPaneControl.GetScreenArea();

			return PreviewDocumentPaneDockAsAnchorable(overlayWindow, targetScreenRect, Type);
		}

		/// <summary>
		/// Show a top, bottom, left, or right preview box for an associated <see cref="DropTargetType"/>.
		/// </summary>
		/// <param name="overlayWindow"></param>
		/// <param name="targetScreenRect"></param>
		/// <param name="targetType">Drop Target Types that result in a preview Geometry being returned are:
		/// <see cref="DropTargetType.DocumentPaneDockAsAnchorableBottom"/>,
		/// <see cref="DropTargetType.DocumentPaneDockAsAnchorableTop"/>,
		/// <see cref="DropTargetType.DocumentPaneDockAsAnchorableLeft"/>,
		/// <see cref="DropTargetType.DocumentPaneDockAsAnchorableRight"/>, other drop targets result in null being returned.</param>
		/// <returns>The <see cref="Geometry"/> object that represents the preview area or null.</returns>
		private static Geometry PreviewDocumentPaneDockAsAnchorable(
			OverlayWindow overlayWindow,
			Rect targetScreenRect,
			DropTargetType targetType)
		{
			switch (targetType)
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
