/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using AvalonDock.Controls;
using AvalonDock.Layout;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvalonDock
{
	/// <summary>
	/// Identifies one of the 6 sidebar button positions / dock zones.
	/// </summary>
	public enum DockZone
	{
		LeftTop,
		LeftBottom,
		RightTop,
		RightBottom,
		BottomLeft,
		BottomRight
	}

	/// <summary>
	/// A docking manager with Rider-style toggle-dock behavior.
	/// Two sidebars (left + right), each with 3 button sections separated by separators:
	/// [SideTop] — separator — [SideBottom] — gap — [BottomSide].
	/// Clicking a button toggles the corresponding anchorable. Exclusivity is per-bar.
	/// Two tools from the same side (one top, one bottom) can coexist as a vertical split.
	/// </summary>
	public class ToggleDockingManager : DockingManager
	{
		#region fields

		internal ToggleDockButtonBar _leftTopBar;
		internal ToggleDockButtonBar _leftBottomBar;
		internal ToggleDockButtonBar _rightTopBar;
		internal ToggleDockButtonBar _rightBottomBar;
		internal ToggleDockButtonBar _bottomLeftBar;
		internal ToggleDockButtonBar _bottomRightBar;
		internal DockPanel _injectedLeftDockPanel;
		internal DockPanel _injectedRightDockPanel;

		#endregion fields

		#region Constructors

		static ToggleDockingManager()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleDockingManager), new FrameworkPropertyMetadata(typeof(ToggleDockingManager)));
		}

		public ToggleDockingManager()
		{
			Loaded += ToggleDockingManager_Loaded;
		}

		#endregion Constructors

		#region Public Methods

		/// <summary>
		/// Toggles an anchorable in the specified zone. Exclusivity is per button-bar:
		/// only one anchorable per bar is docked at a time. But two bars on the same side
		/// (e.g. LeftTop + LeftBottom) can both have an active dock — they split the space.
		/// </summary>
		public void ToggleAnchorable(LayoutAnchorable anchorable, DockZone zone)
		{
			if (anchorable.IsAutoHidden)
			{
				// Hide any currently docked anchorable in the SAME bar only
				HideDockedInBar(GetBarForZone(zone));
				anchorable.ToggleSingleAutoHide();
				FixSplitOrientation(anchorable, zone);
			}
			else
			{
				anchorable.ToggleSingleAutoHide();
			}

			RefreshButtonStates();
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}

		/// <summary>
		/// Moves an anchorable to a different zone. Updates button bars and layout model.
		/// If there's already a docked anchorable on the same side (different sub-zone),
		/// both coexist as a vertical/horizontal split.
		/// </summary>
		public void MoveAnchorableToZone(LayoutAnchorable anchorable, DockZone targetZone)
		{
			if (anchorable == null) return;

			var currentZone = GetAnchorableZone(anchorable);

			// If already in the target zone, just toggle on
			if (currentZone == targetZone)
			{
				if (anchorable.IsAutoHidden)
					ToggleAnchorable(anchorable, targetZone);
				return;
			}

			// Ensure auto-hidden first
			if (!anchorable.IsAutoHidden)
				anchorable.ToggleSingleAutoHide();

			// Remove from current layout parent
			if (anchorable.Parent is LayoutAnchorGroup oldGroup)
				oldGroup.RemoveChild(anchorable);

			// Add to the target side in the layout model
			var targetSide = GetLayoutSideForZone(targetZone);
			LayoutAnchorGroup targetGroup;
			if (targetSide.Children.Count > 0)
				targetGroup = targetSide.Children.First();
			else
			{
				targetGroup = new LayoutAnchorGroup();
				targetSide.Children.Add(targetGroup);
			}
			targetGroup.Children.Add(anchorable);

			// Remove from all button bars
			RemoveFromAllBars(anchorable);

			// Add to the target button bar
			var targetBar = GetBarForZone(targetZone);
			if (targetBar != null)
			{
				var btn = new ToggleDockButton { Anchorable = anchorable, Zone = targetZone };
				targetBar.Items.Add(btn);
			}

			// Toggle it on
			ToggleAnchorable(anchorable, targetZone);
		}

		#endregion Public Methods

		#region Internal Methods

		/// <summary>
		/// Overrides the pin/auto-hide button behavior. The pin button
		/// acts as a "minimize" button that sends the anchorable back to the sidebar.
		/// </summary>
		internal override void ExecuteAutoHideCommand(LayoutAnchorable anchorable)
		{
			if (anchorable == null) return;
			var zone = GetAnchorableZone(anchorable);
			ToggleAnchorable(anchorable, zone);
		}

		/// <summary>
		/// Overrides pane title bar drag to use the toggle overlay instead of floating windows.
		/// </summary>
		internal override void StartDraggingFloatingWindowForPane(LayoutAnchorablePane paneModel)
		{
			if (paneModel == null) return;
			var anchorable = paneModel.Children.OfType<LayoutAnchorable>().FirstOrDefault();
			if (anchorable != null)
			{
				ToggleDockDragOverlay.StartDragFromPane(anchorable, this);
				return;
			}
			base.StartDraggingFloatingWindowForPane(paneModel);
		}

		/// <summary>
		/// Overrides content drag to use the toggle overlay for anchorables.
		/// </summary>
		internal override void StartDraggingFloatingWindowForContent(LayoutContent contentModel, bool startDrag = true)
		{
			if (contentModel is LayoutAnchorable anchorable)
			{
				ToggleDockDragOverlay.StartDragFromPane(anchorable, this);
				return;
			}
			base.StartDraggingFloatingWindowForContent(contentModel, startDrag);
		}

		#endregion Internal Methods

		#region Private Methods

		private void ToggleDockingManager_Loaded(object sender, RoutedEventArgs e)
		{
			SetupToggleDockButtonBars();
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}

		private void UpdatePinButtonsToMinimize()
		{
			foreach (var btn in FindVisualChildren<System.Windows.Controls.Button>(this))
			{
				if (btn.Name == "PART_AutoHidePin")
				{
					btn.ToolTip = "Minimize";
					var border = btn.Content as Border;
					if (border == null)
					{
						border = new Border { Background = Brushes.Transparent };
						btn.Content = border;
					}
					border.Child = CreateMinimizeIcon();
				}
				else if (btn.Name == "PART_HidePin")
				{
					btn.Visibility = Visibility.Collapsed;
				}
			}
		}

		private static UIElement CreateMinimizeIcon()
		{
			return new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M2,11 L11,11"),
				Stroke = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
				StrokeThickness = 1.5,
				Width = 13, Height = 13,
				Stretch = Stretch.None,
				SnapsToDevicePixels = true
			};
		}

		private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
		{
			if (parent == null) yield break;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T t) yield return t;
				foreach (var c in FindVisualChildren<T>(child))
					yield return c;
			}
		}

		/// <summary>
		/// After docking an anchorable, ensures that if multiple panes share the same side,
		/// they are split with the correct orientation:
		/// Left/Right → Vertical (top/bottom), Bottom → Horizontal (left/right).
		/// </summary>
		private void FixSplitOrientation(LayoutAnchorable anchorable, DockZone zone)
		{
			var pane = anchorable.Parent as LayoutAnchorablePane;
			if (pane == null) return;

			var parentPanel = pane.Parent as LayoutPanel;
			if (parentPanel == null) return;

			// Determine the desired orientation for panes on this side
			var desiredOrientation = IsBottomZone(zone) ? Orientation.Horizontal : Orientation.Vertical;

			// Find all anchorable panes that are siblings in the same panel
			var siblingPanes = parentPanel.Children.OfType<LayoutAnchorablePane>().ToList();
			if (siblingPanes.Count < 2) return; // nothing to fix if only one pane

			// Check if the parent panel already has the correct orientation
			if (parentPanel.Orientation == desiredOrientation) return;

			// If there are exactly 2 anchorable panes and possibly other children (like document pane),
			// we need to group the anchorable panes into a LayoutAnchorablePaneGroup with the right orientation
			var group = new LayoutAnchorablePaneGroup { Orientation = desiredOrientation };
			int firstIdx = -1;

			// Remove sibling panes from parent and add to group
			foreach (var sp in siblingPanes)
			{
				int idx = parentPanel.Children.IndexOf(sp);
				if (firstIdx < 0) firstIdx = idx;
			}

			// Remove in reverse order to preserve indices
			for (int i = siblingPanes.Count - 1; i >= 0; i--)
				parentPanel.Children.Remove(siblingPanes[i]);

			foreach (var sp in siblingPanes)
				group.Children.Add(sp);

			parentPanel.Children.Insert(Math.Min(firstIdx, parentPanel.Children.Count), group);
		}

		private static bool IsBottomZone(DockZone zone)
		{
			return zone == DockZone.BottomLeft || zone == DockZone.BottomRight;
		}

		private void SetupToggleDockButtonBars()
		{
			RemoveToggleDockButtonBars();

			// Hide the standard auto-hide side panels
			if (LeftSidePanel != null) LeftSidePanel.Visibility = Visibility.Collapsed;
			if (RightSidePanel != null) RightSidePanel.Visibility = Visibility.Collapsed;
			if (TopSidePanel != null) TopSidePanel.Visibility = Visibility.Collapsed;
			if (BottomSidePanel != null) BottomSidePanel.Visibility = Visibility.Collapsed;

			AutoHideAllDockedAnchorables();

			// Collect anchorables
			var leftAnchorables = CollectAnchorables(Layout.LeftSide);
			var rightAnchorables = CollectAnchorables(Layout.RightSide);
			var bottomAnchorables = CollectAnchorables(Layout.BottomSide);

			// Split bottom anchorables: first half → BottomLeft, second half → BottomRight
			var bottomLeft = bottomAnchorables.Take((bottomAnchorables.Count + 1) / 2).ToList();
			var bottomRight = bottomAnchorables.Skip((bottomAnchorables.Count + 1) / 2).ToList();

			// Create 6 button bars
			_leftTopBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.LeftTop };
			_leftTopBar.SetAnchorables(leftAnchorables, DockZone.LeftTop);

			_leftBottomBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.LeftBottom };
			// LeftBottom starts empty — user drags buttons there

			_rightTopBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.RightTop };
			_rightTopBar.SetAnchorables(rightAnchorables, DockZone.RightTop);

			_rightBottomBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.RightBottom };
			// RightBottom starts empty

			_bottomLeftBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.BottomLeft };
			_bottomLeftBar.SetAnchorables(bottomLeft, DockZone.BottomLeft);

			_bottomRightBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.BottomRight };
			_bottomRightBar.SetAnchorables(bottomRight, DockZone.BottomRight);

			// Inject into the template grid
			var rootGrid = FindTemplateRootGrid();
			if (rootGrid == null) return;

			// Left sidebar: [LeftTop] — separator — [LeftBottom] — gap — [BottomLeft]
			var leftPanel = new DockPanel();
			DockPanel.SetDock(_leftTopBar, Dock.Top);
			DockPanel.SetDock(_bottomLeftBar, Dock.Bottom);

			var leftSep = CreateSeparator();
			DockPanel.SetDock(leftSep, Dock.Top);
			DockPanel.SetDock(_leftBottomBar, Dock.Top);

			leftPanel.Children.Add(_leftTopBar);
			leftPanel.Children.Add(leftSep);
			leftPanel.Children.Add(_leftBottomBar);
			leftPanel.Children.Add(_bottomLeftBar);
			leftPanel.Children.Add(new Border()); // fill gap

			Grid.SetRow(leftPanel, 0);
			Grid.SetRowSpan(leftPanel, 3);
			Grid.SetColumn(leftPanel, 0);

			// Right sidebar: [RightTop] — separator — [RightBottom] — gap — [BottomRight]
			var rightPanel = new DockPanel();
			DockPanel.SetDock(_rightTopBar, Dock.Top);
			DockPanel.SetDock(_bottomRightBar, Dock.Bottom);

			var rightSep = CreateSeparator();
			DockPanel.SetDock(rightSep, Dock.Top);
			DockPanel.SetDock(_rightBottomBar, Dock.Top);

			rightPanel.Children.Add(_rightTopBar);
			rightPanel.Children.Add(rightSep);
			rightPanel.Children.Add(_rightBottomBar);
			rightPanel.Children.Add(_bottomRightBar);
			rightPanel.Children.Add(new Border()); // fill gap

			Grid.SetRow(rightPanel, 0);
			Grid.SetRowSpan(rightPanel, 3);
			Grid.SetColumn(rightPanel, 2);

			rootGrid.Children.Add(leftPanel);
			rootGrid.Children.Add(rightPanel);

			_injectedLeftDockPanel = leftPanel;
			_injectedRightDockPanel = rightPanel;
		}

		private static FrameworkElement CreateSeparator()
		{
			return new Border
			{
				Height = 1,
				Margin = new Thickness(4, 6, 4, 6),
				Background = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC))
			};
		}

		private void RemoveToggleDockButtonBars()
		{
			if (_injectedLeftDockPanel != null)
			{
				(VisualTreeHelper.GetParent(_injectedLeftDockPanel) as Grid)?.Children.Remove(_injectedLeftDockPanel);
				_injectedLeftDockPanel = null;
			}
			if (_injectedRightDockPanel != null)
			{
				(VisualTreeHelper.GetParent(_injectedRightDockPanel) as Grid)?.Children.Remove(_injectedRightDockPanel);
				_injectedRightDockPanel = null;
			}
			_leftTopBar = null;
			_leftBottomBar = null;
			_rightTopBar = null;
			_rightBottomBar = null;
			_bottomLeftBar = null;
			_bottomRightBar = null;
		}

		private Grid FindTemplateRootGrid()
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(this); i++)
			{
				var child = VisualTreeHelper.GetChild(this, i);
				if (child is Border border)
				{
					for (int j = 0; j < VisualTreeHelper.GetChildrenCount(border); j++)
					{
						if (VisualTreeHelper.GetChild(border, j) is Grid grid)
							return grid;
					}
				}
				if (child is Grid g)
					return g;
			}
			return null;
		}

		private static List<LayoutAnchorable> CollectAnchorables(LayoutAnchorSide side)
		{
			var result = new List<LayoutAnchorable>();
			if (side == null) return result;
			foreach (var group in side.Children)
				foreach (var anchorable in group.Children)
					result.Add(anchorable);
			return result;
		}

		private void AutoHideAllDockedAnchorables()
		{
			var docked = Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.Where(a => a.Parent is LayoutAnchorablePane && !a.IsFloating)
				.ToList();
			foreach (var a in docked)
				a.ToggleSingleAutoHide();
		}

		/// <summary>Hides any docked anchorable that belongs to the specified button bar.</summary>
		private void HideDockedInBar(ToggleDockButtonBar bar)
		{
			if (bar == null) return;
			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable != null && !btn.Anchorable.IsAutoHidden)
					btn.Anchorable.ToggleSingleAutoHide();
			}
		}

		private void RefreshButtonStates()
		{
			RefreshBarStates(_leftTopBar);
			RefreshBarStates(_leftBottomBar);
			RefreshBarStates(_rightTopBar);
			RefreshBarStates(_rightBottomBar);
			RefreshBarStates(_bottomLeftBar);
			RefreshBarStates(_bottomRightBar);
		}

		private static void RefreshBarStates(ToggleDockButtonBar bar)
		{
			if (bar == null) return;
			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable != null)
					btn.IsChecked = !btn.Anchorable.IsAutoHidden;
			}
		}

		/// <summary>Determines which zone an anchorable belongs to by checking all button bars.</summary>
		private DockZone GetAnchorableZone(LayoutAnchorable anchorable)
		{
			if (_leftTopBar?.ContainsAnchorable(anchorable) == true) return DockZone.LeftTop;
			if (_leftBottomBar?.ContainsAnchorable(anchorable) == true) return DockZone.LeftBottom;
			if (_rightTopBar?.ContainsAnchorable(anchorable) == true) return DockZone.RightTop;
			if (_rightBottomBar?.ContainsAnchorable(anchorable) == true) return DockZone.RightBottom;
			if (_bottomLeftBar?.ContainsAnchorable(anchorable) == true) return DockZone.BottomLeft;
			if (_bottomRightBar?.ContainsAnchorable(anchorable) == true) return DockZone.BottomRight;

			// Fallback
			if (anchorable.Parent is LayoutAnchorGroup group && group.Parent is LayoutAnchorSide side)
			{
				switch (side.Side)
				{
					case AnchorSide.Left: return DockZone.LeftTop;
					case AnchorSide.Right: return DockZone.RightTop;
					case AnchorSide.Bottom: return DockZone.BottomLeft;
				}
			}
			return DockZone.LeftTop;
		}

		internal ToggleDockButtonBar GetBarForZone(DockZone zone)
		{
			switch (zone)
			{
				case DockZone.LeftTop: return _leftTopBar;
				case DockZone.LeftBottom: return _leftBottomBar;
				case DockZone.RightTop: return _rightTopBar;
				case DockZone.RightBottom: return _rightBottomBar;
				case DockZone.BottomLeft: return _bottomLeftBar;
				case DockZone.BottomRight: return _bottomRightBar;
				default: return _leftTopBar;
			}
		}

		/// <summary>Maps a DockZone to its LayoutAnchorSide in the layout model.</summary>
		private LayoutAnchorSide GetLayoutSideForZone(DockZone zone)
		{
			switch (zone)
			{
				case DockZone.LeftTop:
				case DockZone.LeftBottom:
					return Layout.LeftSide;
				case DockZone.RightTop:
				case DockZone.RightBottom:
					return Layout.RightSide;
				case DockZone.BottomLeft:
				case DockZone.BottomRight:
					return Layout.BottomSide;
				default:
					return Layout.LeftSide;
			}
		}

		private void RemoveFromAllBars(LayoutAnchorable anchorable)
		{
			RemoveFromBar(_leftTopBar, anchorable);
			RemoveFromBar(_leftBottomBar, anchorable);
			RemoveFromBar(_rightTopBar, anchorable);
			RemoveFromBar(_rightBottomBar, anchorable);
			RemoveFromBar(_bottomLeftBar, anchorable);
			RemoveFromBar(_bottomRightBar, anchorable);
		}

		private static void RemoveFromBar(ToggleDockButtonBar bar, LayoutAnchorable anchorable)
		{
			if (bar == null) return;
			for (int i = bar.Items.Count - 1; i >= 0; i--)
			{
				if (bar.Items[i] is ToggleDockButton btn && btn.Anchorable == anchorable)
				{
					bar.Items.RemoveAt(i);
					return;
				}
			}
		}

		#endregion Private Methods
	}
}
