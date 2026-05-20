/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AvalonDock.Controls;
using AvalonDock.Core;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Defines the dock Layout Priority values.
	/// </summary>
	public enum DockLayoutPriority
	{
		/// <summary>
		/// The bottom Full Width option.
		/// </summary>
		BottomFullWidth,

		/// <summary>
		/// The sides Full Height option.
		/// </summary>
		SidesFullHeight,

		/// <summary>
		/// The default option.
		/// </summary>
		Default
	}

	/// <summary>
	/// Represents the toggle Docking Manager.
	/// </summary>
	public class ToggleDockingManager : DockingManager
	{
		/// <summary>
		/// The left Top Bar field.
		/// </summary>
		internal ToggleDockButtonBar _leftTopBar;

		/// <summary>
		/// The left Bottom Bar field.
		/// </summary>
		internal ToggleDockButtonBar _leftBottomBar;

		/// <summary>
		/// The right Top Bar field.
		/// </summary>
		internal ToggleDockButtonBar _rightTopBar;

		/// <summary>
		/// The right Bottom Bar field.
		/// </summary>
		internal ToggleDockButtonBar _rightBottomBar;

		/// <summary>
		/// The bottom Left Bar field.
		/// </summary>
		internal ToggleDockButtonBar _bottomLeftBar;

		/// <summary>
		/// The bottom Right Bar field.
		/// </summary>
		internal ToggleDockButtonBar _bottomRightBar;

		/// <summary>
		/// The injected Left Dock Panel field.
		/// </summary>
		internal DockPanel _injectedLeftDockPanel;

		/// <summary>
		/// The injected Right Dock Panel field.
		/// </summary>
		internal DockPanel _injectedRightDockPanel;

		/// <summary>
		/// The left Separator field.
		/// </summary>
		internal FrameworkElement _leftSeparator;

		/// <summary>
		/// The right Separator field.
		/// </summary>
		internal FrameworkElement _rightSeparator;

		/// <summary>
		/// <see cref="LayoutPriority"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayoutPriorityProperty =
			DependencyProperty.Register(nameof(LayoutPriority), typeof(DockLayoutPriority), typeof(ToggleDockingManager),
				new PropertyMetadata(DockLayoutPriority.BottomFullWidth));

		/// <summary>
		/// <see cref="ButtonSize"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ButtonSizeProperty =
			DependencyProperty.Register(nameof(ButtonSize), typeof(double), typeof(ToggleDockingManager),
				new PropertyMetadata(25.0));

		/// <summary>
		/// <see cref="ShowHeaderMinimizeButton"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowHeaderMinimizeButtonProperty =
			DependencyProperty.Register(nameof(ShowHeaderMinimizeButton), typeof(bool), typeof(ToggleDockingManager),
				new PropertyMetadata(true));

		/// <summary>
		/// <see cref="ShowHeaderOptionsButton"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowHeaderOptionsButtonProperty =
			DependencyProperty.Register(nameof(ShowHeaderOptionsButton), typeof(bool), typeof(ToggleDockingManager),
				new PropertyMetadata(true));

		/// <summary>
		/// <see cref="DefaultDockWidth"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DefaultDockWidthProperty =
			DependencyProperty.Register(nameof(DefaultDockWidth), typeof(double), typeof(ToggleDockingManager),
				new PropertyMetadata(250.0));

		/// <summary>
		/// <see cref="DefaultDockHeight"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DefaultDockHeightProperty =
			DependencyProperty.Register(nameof(DefaultDockHeight), typeof(double), typeof(ToggleDockingManager),
				new PropertyMetadata(200.0));

		/// <summary>
		/// Gets or sets the layout Priority.
		/// </summary>
		public DockLayoutPriority LayoutPriority
		{
			get => (DockLayoutPriority)GetValue(LayoutPriorityProperty);
			set => SetValue(LayoutPriorityProperty, value);
		}

		/// <summary>
		/// Gets or sets the button Size.
		/// </summary>
		public double ButtonSize
		{
			get => (double)GetValue(ButtonSizeProperty);
			set => SetValue(ButtonSizeProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether show Header Minimize Button.
		/// </summary>
		public bool ShowHeaderMinimizeButton
		{
			get => (bool)GetValue(ShowHeaderMinimizeButtonProperty);
			set => SetValue(ShowHeaderMinimizeButtonProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether show Header Options Button.
		/// </summary>
		public bool ShowHeaderOptionsButton
		{
			get => (bool)GetValue(ShowHeaderOptionsButtonProperty);
			set => SetValue(ShowHeaderOptionsButtonProperty, value);
		}

		/// <summary>
		/// Gets or sets the default Dock Width.
		/// </summary>
		public double DefaultDockWidth
		{
			get => (double)GetValue(DefaultDockWidthProperty);
			set => SetValue(DefaultDockWidthProperty, value);
		}

		/// <summary>
		/// Gets or sets the default Dock Height.
		/// </summary>
		public double DefaultDockHeight
		{
			get => (double)GetValue(DefaultDockHeightProperty);
			set => SetValue(DefaultDockHeightProperty, value);
		}

		/// <summary>
		/// Initializes static members of the <see cref="ToggleDockingManager"/> class.
		/// </summary>
		static ToggleDockingManager()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleDockingManager), new FrameworkPropertyMetadata(typeof(ToggleDockingManager)));
			BorderThicknessProperty.OverrideMetadata(typeof(ToggleDockingManager), new FrameworkPropertyMetadata(new Thickness(0)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ToggleDockingManager"/> class.
		/// </summary>
		public ToggleDockingManager()
		{
			LayoutUpdateStrategy = new ToggleLayoutStrategy();
			Loaded += ToggleDockingManager_Loaded;
			ActiveContentChanged += (s, e) => RefreshButtonStates();
		}

		/// <inheritdoc/>
		protected override void OnThemeChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnThemeChanged(e);
			// Re-apply toggle anchorable style after theme changes
			ApplyToggleAnchorableStyle();
		}

		private void ApplyToggleAnchorableStyle()
		{
			var toggleStyle = TryFindResource("ToggleAnchorablePaneControlStyle") as System.Windows.Style;
			if (toggleStyle != null)
			{
				AnchorablePaneControlStyle = toggleStyle;
			}
		}

		/// <summary>
		/// Executes the toggle Anchorable operation.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <param name="zone">The zone.</param>
		public void ToggleAnchorable(LayoutAnchorable anchorable, DockZone zone)
		{
			if (anchorable.IsAutoHidden)
			{
				// Hide any currently docked anchorable in the SAME bar only
				HideDockedInBar(GetBarForZone(zone));
				DockFromAutoHide(anchorable, zone);
				FixSplitOrientation(anchorable, zone);

				switch (LayoutPriority)
				{
					case DockLayoutPriority.BottomFullWidth:
						EnsureBottomFullWidth();
						break;
					case DockLayoutPriority.SidesFullHeight:
						EnsureSidesFullHeight();
						break;
				}

				// Focus the newly docked anchorable so the button shows as focused
				ActiveContent = anchorable.Content;
			}
			else
			{
				AutoHideFromDock(anchorable, zone);
			}

			RefreshButtonStates();
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}

		/// <summary>
		/// Executes the move Anchorable To Zone operation.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <param name="targetZone">The target Zone.</param>
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
				AutoHideFromDock(anchorable, currentZone);

			// Remove from current layout parent
			if (anchorable.Parent is LayoutAnchorGroup oldGroup)
				oldGroup.RemoveChild(anchorable);

			// Add to the target side in the layout model (always create a fresh group
			// so DockFromAutoHide won't reuse a stale PreviousContainer from another zone)
			var targetSide = GetLayoutSideForZone(targetZone);
			var targetGroup = new LayoutAnchorGroup();
			targetSide.Children.Add(targetGroup);
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

		/// <inheritdoc/>
		internal override void ExecuteAutoHideCommand(LayoutAnchorable anchorable)
		{
			if (anchorable == null) return;
			var zone = GetAnchorableZone(anchorable);
			ToggleAnchorable(anchorable, zone);
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		internal override void StartDraggingFloatingWindowForContent(LayoutContent contentModel, bool startDrag = true)
		{
			if (contentModel is LayoutAnchorable anchorable)
			{
				ToggleDockDragOverlay.StartDragFromPane(anchorable, this);
				return;
			}

			base.StartDraggingFloatingWindowForContent(contentModel, startDrag);
		}

		private void ToggleDockingManager_Loaded(object sender, RoutedEventArgs e)
		{
			ApplyToggleAnchorableStyle();
			SetupToggleDockButtonBars();
			OpenDefaultToolboxes();
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}

		private void OpenDefaultToolboxes()
		{
			if (Layout == null) return;

			foreach (var anc in Layout.Descendents().OfType<LayoutAnchorable>().ToList())
			{
				if (anc.Content is IToolbox toolbox && toolbox.IsOpenByDefault)
				{
					ToggleAnchorable(anc, toolbox.Zone);
				}
			}
		}

		private void UpdatePinButtonsToMinimize()
		{
			foreach (var title in FindVisualChildren<AnchorablePaneTitle>(this))
			{
				foreach (var btn in FindVisualChildren<System.Windows.Controls.Button>(title))
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

				// Hide the old DropDownButton (pin menu)
				foreach (var dd in FindVisualChildren<DropDownButton>(title))
					dd.Visibility = Visibility.Collapsed;

				// Inject three-dot menu button if not already present
				var grid = FindVisualChildren<Grid>(title).FirstOrDefault();
				if (grid != null && !grid.Children.OfType<System.Windows.Controls.Button>().Any(b => b.Name == "PART_ToggleMenu"))
				{
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
					int newCol = grid.ColumnDefinitions.Count - 1;

					// Move minimize button (PART_AutoHidePin) to the last column
					var autoHideBtn = grid.Children.OfType<System.Windows.Controls.Button>().FirstOrDefault(b => b.Name == "PART_AutoHidePin");
					if (autoHideBtn != null)
						Grid.SetColumn(autoHideBtn, newCol);

					// Put three-dot menu in the old auto-hide column (2)
					var menuBtn = CreateThreeDotMenuButton(title);
					Grid.SetColumn(menuBtn, 2);
					grid.Children.Add(menuBtn);
				}
			}
		}

		private System.Windows.Controls.Button CreateThreeDotMenuButton(AnchorablePaneTitle title)
		{
			var btn = new System.Windows.Controls.Button
			{
				Name = "PART_ToggleMenu",
				Content = new System.Windows.Controls.TextBlock
				{
					Text = "⋯",
					FontSize = 14,
					FontWeight = FontWeights.Bold,
					Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					Margin = new Thickness(0, -2, 0, 0)
				},
				Width = 20,
				Height = 20,
				Padding = new Thickness(0),
				Margin = new Thickness(2, 0, 2, 0),
				Focusable = false,
				Style = (Style)FindResource(ToolBar.ButtonStyleKey),
				ToolTip = "Options"
			};

			btn.Click += (s, e) =>
			{
				var anchorable = title.Model;
				if (anchorable == null) return;

				var menu = BuildToggleContextMenu(anchorable);
				menu.PlacementTarget = btn;
				menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
				menu.IsOpen = true;
			};

			return btn;
		}

		private ContextMenu BuildToggleContextMenu(LayoutAnchorable anchorable)
		{
			var menu = new ContextMenu();

			// "Move To" submenu
			var moveToItem = new MenuItem { Header = "Move To" };
			foreach (DockZone zone in Enum.GetValues(typeof(DockZone)))
			{
				var zoneLabel = System.Text.RegularExpressions.Regex.Replace(zone.ToString(), "(\\B[A-Z])", " $1");
				var z = zone;
				var mi = new MenuItem { Header = zoneLabel };
				mi.Click += (s, e) => MoveAnchorableToZone(anchorable, z);
				moveToItem.Items.Add(mi);
			}

			menu.Items.Add(moveToItem);

			menu.Items.Add(new Separator());

			// "View Mode" submenu
			var viewModeItem = new MenuItem { Header = "View Mode" };

			var floatItem = new MenuItem { Header = "Float" };
			floatItem.Click += (s, e) =>
			{
				if (anchorable.IsAutoHidden)
					anchorable.ToggleSingleAutoHide();
				var layoutItem = GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
				layoutItem?.FloatCommand?.Execute(null);
			};
			viewModeItem.Items.Add(floatItem);

			var dockedItem = new MenuItem { Header = "Docked" };
			dockedItem.Click += (s, e) =>
			{
				if (anchorable.IsAutoHidden)
				{
					var zone = GetAnchorableZone(anchorable);
					ToggleAnchorable(anchorable, zone);
				}
			};
			viewModeItem.Items.Add(dockedItem);

			var hiddenItem = new MenuItem { Header = "Hidden" };
			hiddenItem.Click += (s, e) =>
			{
				var layoutItem = GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
				layoutItem?.HideCommand?.Execute(null);
			};
			viewModeItem.Items.Add(hiddenItem);

			menu.Items.Add(viewModeItem);

			return menu;
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

		private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
			where T : DependencyObject
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
		/// Executes the fix Split Orientation operation.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <param name="zone">The zone.</param>
		private void FixSplitOrientation(LayoutAnchorable anchorable, DockZone zone)
		{
			var pane = anchorable.Parent as LayoutAnchorablePane;
			if (pane == null) return;

			var parentPanel = pane.Parent as LayoutPanel;
			if (parentPanel == null) return;

			var desiredOrientation = IsBottomZone(zone) ? Orientation.Horizontal : Orientation.Vertical;

			// Case 1: There's an adjacent LayoutAnchorablePaneGroup sibling — insert into it.
			// Only join a group that is contiguous with this pane (same side of the document).
			var paneIdx = parentPanel.Children.IndexOf(pane);
			LayoutAnchorablePaneGroup existingGroup = null;
			if (paneIdx > 0 && parentPanel.Children[paneIdx - 1] is LayoutAnchorablePaneGroup leftNeighbor
				&& leftNeighbor.Orientation == desiredOrientation)
				existingGroup = leftNeighbor;
			else if (paneIdx < parentPanel.Children.Count - 1 && parentPanel.Children[paneIdx + 1] is LayoutAnchorablePaneGroup rightNeighbor
				&& rightNeighbor.Orientation == desiredOrientation)
				existingGroup = rightNeighbor;

			if (existingGroup != null)
			{
				parentPanel.Children.Remove(pane);
				// "Top" sub-zones (LeftTop, RightTop) go first; "Bottom" sub-zones append
				if (zone == DockZone.LeftTop || zone == DockZone.RightTop)
					existingGroup.Children.Insert(0, pane);
				else
					existingGroup.Children.Add(pane);
				return;
			}

			// Case 2: Multiple contiguous LayoutAnchorablePane siblings on the same side —
			// wrap them in a group. Only group panes that are adjacent (no document pane
			// or other element between them), so left-side and right-side panes aren't
			// accidentally merged.
			paneIdx = parentPanel.Children.IndexOf(pane);
			var contiguousPanes = new List<LayoutAnchorablePane> { pane };

			// Scan backward from the pane
			for (int i = paneIdx - 1; i >= 0; i--)
			{
				if (parentPanel.Children[i] is LayoutAnchorablePane adjPane)
					contiguousPanes.Insert(0, adjPane);
				else break;
			}

			// Scan forward from the pane
			for (int i = paneIdx + 1; i < parentPanel.Children.Count; i++)
			{
				if (parentPanel.Children[i] is LayoutAnchorablePane adjPane)
					contiguousPanes.Add(adjPane);
				else break;
			}

			if (contiguousPanes.Count < 2) return;
			if (parentPanel.Orientation == desiredOrientation) return;

			var group = new LayoutAnchorablePaneGroup { Orientation = desiredOrientation };
			int firstIdx = parentPanel.Children.IndexOf(contiguousPanes[0]);

			for (int i = contiguousPanes.Count - 1; i >= 0; i--)
				parentPanel.Children.Remove(contiguousPanes[i]);

			foreach (var sp in contiguousPanes)
				group.Children.Add(sp);

			parentPanel.Children.Insert(Math.Min(firstIdx, parentPanel.Children.Count), group);
		}

		private static bool IsBottomZone(DockZone zone)
		{
			return zone == DockZone.BottomLeft || zone == DockZone.BottomRight;
		}

		private static bool IsLeftZone(DockZone zone)
		{
			return zone == DockZone.LeftTop || zone == DockZone.LeftBottom;
		}

		private static bool IsRightZone(DockZone zone)
		{
			return zone == DockZone.RightTop || zone == DockZone.RightBottom;
		}

		private static AnchorSide ZoneToAnchorSide(DockZone zone)
		{
			if (IsLeftZone(zone)) return AnchorSide.Left;
			if (IsRightZone(zone)) return AnchorSide.Right;
			return AnchorSide.Bottom;
		}

		/// <summary>
		/// Executes the ensure Bottom Full Width operation.
		/// </summary>
		private void EnsureBottomFullWidth()
		{
			var root = Layout as LayoutRoot;
			if (root == null) return;
			var rootPanel = root.RootPanel;
			if (rootPanel == null) return;

			// If root is horizontal, check if any bottom panes are nested inside it
			if (rootPanel.Orientation == Orientation.Horizontal)
			{
				// Look for bottom anchorable panes that are children of sub-vertical panels
				// within this horizontal root. If found, we need to lift them out.
				var bottomPanes = new List<LayoutAnchorablePane>();
				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Vertical)
					{
						foreach (var pane in child.Children.OfType<LayoutAnchorablePane>().ToList())
						{
							// Check if this pane is at the end of a vertical sub-panel → it's a bottom pane
							if (child.Children.IndexOf(pane) == child.Children.Count - 1 && child.Children.Count > 1)
								bottomPanes.Add(pane);
						}
					}
				}

				if (bottomPanes.Count > 0)
				{
					// Wrap root in vertical, move bottom panes to outermost level
					var vPanel = new LayoutPanel { Orientation = Orientation.Vertical };
					root.RootPanel = vPanel;

					// Clean bottom panes from their current parents
					foreach (var bp in bottomPanes)
						((ILayoutContainer)bp.Parent).RemoveChild(bp);

					vPanel.Children.Add(rootPanel);
					foreach (var bp in bottomPanes)
						vPanel.Children.Add(bp);
				}
			}
			else if (rootPanel.Orientation == Orientation.Vertical)
			{
				// Root is already vertical. Check if bottom panes are in the right place.
				// Bottom panes should be direct children at the end, not nested in horizontal sub-panels.
				var bottomPanesToLift = new List<LayoutAnchorablePane>();
				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Horizontal)
					{
						foreach (var subChild in child.Children.OfType<LayoutPanel>().ToList())
						{
							if (subChild.Orientation == Orientation.Vertical)
							{
								foreach (var pane in subChild.Children.OfType<LayoutAnchorablePane>().ToList())
								{
									if (subChild.Children.IndexOf(pane) == subChild.Children.Count - 1 && subChild.Children.Count > 1)
										bottomPanesToLift.Add(pane);
								}
							}
						}
					}
				}

				foreach (var bp in bottomPanesToLift)
				{
					((ILayoutContainer)bp.Parent).RemoveChild(bp);
					rootPanel.Children.Add(bp);
				}
			}
		}

		/// <summary>
		/// Executes the ensure Sides Full Height operation.
		/// </summary>
		private void EnsureSidesFullHeight()
		{
			var root = Layout as LayoutRoot;
			if (root == null) return;
			var rootPanel = root.RootPanel;
			if (rootPanel == null) return;

			if (rootPanel.Orientation == Orientation.Vertical)
			{
				// Root is vertical. Side panes may be nested in horizontal sub-panels.
				var leftPanes = new List<LayoutAnchorablePane>();
				var rightPanes = new List<LayoutAnchorablePane>();

				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Horizontal)
					{
						foreach (var pane in child.Children.OfType<LayoutAnchorablePane>().ToList())
						{
							int idx = child.Children.IndexOf(pane);
							if (idx == 0 && child.Children.Count > 1)
								leftPanes.Add(pane);
							else if (idx == child.Children.Count - 1 && child.Children.Count > 1)
								rightPanes.Add(pane);
						}
					}
				}

				if (leftPanes.Count > 0 || rightPanes.Count > 0)
				{
					var hPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
					root.RootPanel = hPanel;

					foreach (var lp in leftPanes)
						((ILayoutContainer)lp.Parent).RemoveChild(lp);
					foreach (var rp in rightPanes)
						((ILayoutContainer)rp.Parent).RemoveChild(rp);

					foreach (var lp in leftPanes)
						hPanel.Children.Add(lp);
					hPanel.Children.Add(rootPanel);
					foreach (var rp in rightPanes)
						hPanel.Children.Add(rp);
				}
			}
			else if (rootPanel.Orientation == Orientation.Horizontal)
			{
				// Root is already horizontal. Check if side panes are nested in vertical sub-panels.
				var leftPanesToLift = new List<LayoutAnchorablePane>();
				var rightPanesToLift = new List<LayoutAnchorablePane>();

				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Vertical)
					{
						foreach (var subChild in child.Children.OfType<LayoutPanel>().ToList())
						{
							if (subChild.Orientation == Orientation.Horizontal)
							{
								foreach (var pane in subChild.Children.OfType<LayoutAnchorablePane>().ToList())
								{
									int idx = subChild.Children.IndexOf(pane);
									if (idx == 0 && subChild.Children.Count > 1)
										leftPanesToLift.Add(pane);
									else if (idx == subChild.Children.Count - 1 && subChild.Children.Count > 1)
										rightPanesToLift.Add(pane);
								}
							}
						}
					}
				}

				foreach (var lp in leftPanesToLift)
				{
					((ILayoutContainer)lp.Parent).RemoveChild(lp);
					rootPanel.Children.Insert(0, lp);
				}

				foreach (var rp in rightPanesToLift)
				{
					((ILayoutContainer)rp.Parent).RemoveChild(rp);
					rootPanel.Children.Add(rp);
				}
			}
		}

		/// <summary>
		/// Executes the dock From Auto Hide operation.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <param name="zone">The zone.</param>
		private void DockFromAutoHide(LayoutAnchorable anchorable, DockZone zone)
		{
			var parentGroup = anchorable.Parent as LayoutAnchorGroup;
			if (parentGroup == null) return;
			var parentSide = parentGroup.Parent as LayoutAnchorSide;
			if (parentSide == null) return;

			var previousContainer = ((ILayoutPreviousContainer)parentGroup).PreviousContainer as LayoutAnchorablePane;
			if (previousContainer != null && previousContainer.Root == null)
				previousContainer = null;

			if (previousContainer == null)
			{
				var side = ZoneToAnchorSide(zone);
				previousContainer = new LayoutAnchorablePane
				{
					DockMinWidth = anchorable.AutoHideMinWidth,
					DockMinHeight = anchorable.AutoHideMinHeight
				};

				// Apply default dock dimensions from the manager
				if (side == AnchorSide.Left || side == AnchorSide.Right)
					previousContainer.DockWidth = new GridLength(DefaultDockWidth);
				else
					previousContainer.DockHeight = new GridLength(DefaultDockHeight);

				var root = parentGroup.Root as LayoutRoot;
				var rootPanel = root.RootPanel;

				switch (side)
				{
					case AnchorSide.Right:
						if (rootPanel.Orientation == Orientation.Horizontal)
						{
							if (zone == DockZone.RightTop)
							{
								// Insert before existing right-side panes/groups (after the document content)
								int insertIdx = rootPanel.Children.Count;
								while (insertIdx > 0 &&
									   (rootPanel.Children[insertIdx - 1] is LayoutAnchorablePane ||
										rootPanel.Children[insertIdx - 1] is LayoutAnchorablePaneGroup))
								{
									insertIdx--;
								}

								rootPanel.Children.Insert(insertIdx, previousContainer);
							}
							else
							{
								rootPanel.Children.Add(previousContainer);
							}
						}
						else
						{
							var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
							root.RootPanel = panel;
							panel.Children.Add(rootPanel);
							panel.Children.Add(previousContainer);
						}

						break;
					case AnchorSide.Left:
						if (rootPanel.Orientation == Orientation.Horizontal)
						{
							if (zone == DockZone.LeftBottom)
							{
								// Insert after existing left-side panes/groups (before the document content)
								int insertIdx = 0;
								while (insertIdx < rootPanel.Children.Count &&
									   (rootPanel.Children[insertIdx] is LayoutAnchorablePane ||
										rootPanel.Children[insertIdx] is LayoutAnchorablePaneGroup))
								{
									insertIdx++;
								}

								rootPanel.Children.Insert(insertIdx, previousContainer);
							}
							else
							{
								rootPanel.Children.Insert(0, previousContainer);
							}
						}
						else
						{
							var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
							root.RootPanel = panel;
							panel.Children.Add(previousContainer);
							panel.Children.Add(rootPanel);
						}

						break;
					case AnchorSide.Bottom:
						if (rootPanel.Orientation == Orientation.Vertical)
						{
							rootPanel.Children.Add(previousContainer);
						}
						else
						{
							var panel = new LayoutPanel { Orientation = Orientation.Vertical };
							root.RootPanel = panel;
							panel.Children.Add(rootPanel);
							panel.Children.Add(previousContainer);
						}

						break;
				}
			}

			parentGroup.Children.Remove(anchorable);
			previousContainer.Children.Add(anchorable);

			if (parentGroup.Children.Count == 0)
				parentSide.Children.Remove(parentGroup);
		}

		/// <summary>
		/// Executes the auto Hide From Dock operation.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <param name="zone">The zone.</param>
		private void AutoHideFromDock(LayoutAnchorable anchorable, DockZone zone)
		{
			var parentPane = anchorable.Parent as LayoutAnchorablePane;
			if (parentPane == null) return;
			var root = anchorable.Root;
			if (root == null) return;

			var side = ZoneToAnchorSide(zone);
			var newAnchorGroup = new LayoutAnchorGroup();
			((ILayoutPreviousContainer)newAnchorGroup).PreviousContainer = parentPane;

			parentPane.Children.Remove(anchorable);
			newAnchorGroup.Children.Add(anchorable);

			switch (side)
			{
				case AnchorSide.Right: root.RightSide?.Children.Add(newAnchorGroup); break;
				case AnchorSide.Left: root.LeftSide?.Children.Add(newAnchorGroup); break;
				case AnchorSide.Bottom: root.BottomSide?.Children.Add(newAnchorGroup); break;
			}
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

			// Collect all anchorables from each side
			var leftAnchorables = CollectAnchorables(Layout.LeftSide);
			var rightAnchorables = CollectAnchorables(Layout.RightSide);
			var bottomAnchorables = CollectAnchorables(Layout.BottomSide);

			// Distribute anchorables to zones based on IToolbox.Zone
			var leftTop = new List<LayoutAnchorable>();
			var leftBottom = new List<LayoutAnchorable>();
			foreach (var anc in leftAnchorables)
			{
				if (anc.Content is IToolbox t && t.Zone == DockZone.LeftBottom)
					leftBottom.Add(anc);
				else
					leftTop.Add(anc);
			}

			var rightTop = new List<LayoutAnchorable>();
			var rightBottom = new List<LayoutAnchorable>();
			foreach (var anc in rightAnchorables)
			{
				if (anc.Content is IToolbox t && t.Zone == DockZone.RightBottom)
					rightBottom.Add(anc);
				else
					rightTop.Add(anc);
			}

			var bottomLeft = new List<LayoutAnchorable>();
			var bottomRight = new List<LayoutAnchorable>();
			foreach (var anc in bottomAnchorables)
			{
				if (anc.Content is IToolbox t && t.Zone == DockZone.BottomRight)
					bottomRight.Add(anc);
				else
					bottomLeft.Add(anc);
			}

			// Create 6 button bars with automation IDs for UI test discoverability
			_leftTopBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.LeftTop };
			System.Windows.Automation.AutomationProperties.SetAutomationId(_leftTopBar, "ToggleDockBar_LeftTop");
			_leftTopBar.SetAnchorables(leftTop, DockZone.LeftTop);

			_leftBottomBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.LeftBottom };
			System.Windows.Automation.AutomationProperties.SetAutomationId(_leftBottomBar, "ToggleDockBar_LeftBottom");
			_leftBottomBar.SetAnchorables(leftBottom, DockZone.LeftBottom);

			_rightTopBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.RightTop };
			System.Windows.Automation.AutomationProperties.SetAutomationId(_rightTopBar, "ToggleDockBar_RightTop");
			_rightTopBar.SetAnchorables(rightTop, DockZone.RightTop);

			_rightBottomBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.RightBottom };
			System.Windows.Automation.AutomationProperties.SetAutomationId(_rightBottomBar, "ToggleDockBar_RightBottom");
			_rightBottomBar.SetAnchorables(rightBottom, DockZone.RightBottom);

			_bottomLeftBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.BottomLeft };
			System.Windows.Automation.AutomationProperties.SetAutomationId(_bottomLeftBar, "ToggleDockBar_BottomLeft");
			_bottomLeftBar.SetAnchorables(bottomLeft, DockZone.BottomLeft);

			_bottomRightBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Zone = DockZone.BottomRight };
			System.Windows.Automation.AutomationProperties.SetAutomationId(_bottomRightBar, "ToggleDockBar_BottomRight");
			_bottomRightBar.SetAnchorables(bottomRight, DockZone.BottomRight);

			// Inject into the template grid
			var rootGrid = FindTemplateRootGrid();
			if (rootGrid == null) return;

			// Left sidebar: [LeftTop] — separator — [LeftBottom] — gap — [BottomLeft]
			var leftPanel = new DockPanel();
			DockPanel.SetDock(_leftTopBar, Dock.Top);
			DockPanel.SetDock(_bottomLeftBar, Dock.Bottom);

			var leftSep = CreateSeparator();
			_leftSeparator = leftSep;
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
			_rightSeparator = rightSep;
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
				Background = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99))
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
			{
				foreach (var anchorable in group.Children)
				{
					result.Add(anchorable);
				}
			}

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

		/// <summary>
		/// Executes the hide Docked In Bar operation.
		/// </summary>
		/// <param name="bar">The bar.</param>
		private void HideDockedInBar(ToggleDockButtonBar bar)
		{
			if (bar == null) return;
			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable != null && !btn.Anchorable.IsAutoHidden)
					AutoHideFromDock(btn.Anchorable, bar.Zone);
			}
		}

		private void RefreshButtonStates()
		{
			var activeContent = ActiveContent;
			RefreshBarStates(_leftTopBar, activeContent);
			RefreshBarStates(_leftBottomBar, activeContent);
			RefreshBarStates(_rightTopBar, activeContent);
			RefreshBarStates(_rightBottomBar, activeContent);
			RefreshBarStates(_bottomLeftBar, activeContent);
			RefreshBarStates(_bottomRightBar, activeContent);
		}

		private static void RefreshBarStates(ToggleDockButtonBar bar, object activeContent)
		{
			if (bar == null) return;
			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable != null)
				{
					btn.IsChecked = !btn.Anchorable.IsAutoHidden;
					btn.IsAnchorableFocused = !btn.Anchorable.IsAutoHidden
						&& activeContent != null
						&& activeContent == btn.Anchorable.Content;
				}
			}
		}

		/// <summary>
		/// Gets the get Anchorable Zone.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <returns>The requested value.</returns>
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

		/// <summary>
		/// Gets the get Bar For Zone.
		/// </summary>
		/// <param name="zone">The zone.</param>
		/// <returns>The requested value.</returns>
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

		/// <summary>
		/// Gets the get Layout Side For Zone.
		/// </summary>
		/// <param name="zone">The zone.</param>
		/// <returns>The requested value.</returns>
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
	}
}