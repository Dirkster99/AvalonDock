/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

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
	/// A docking manager with VSCode/Rider-style toggle-dock behavior.
	/// Sidebar button bars replace the auto-hide flyout overlay. Clicking a button toggles
	/// the corresponding anchorable panel in-place. Only one anchorable per section is active at a time.
	/// </summary>
	public class ToggleDockingManager : DockingManager
	{
		#region fields

		private ToggleDockButtonBar _leftTopButtonBar;
		private ToggleDockButtonBar _leftBottomButtonBar;
		private ToggleDockButtonBar _rightTopButtonBar;
		private DockPanel _injectedLeftDockPanel;

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
		/// Toggles an anchorable in the specified section. Ensures only one anchorable
		/// per section is docked at a time (exclusive toggle).
		/// </summary>
		public void ToggleAnchorable(LayoutAnchorable anchorable, AnchorSide section)
		{
			if (anchorable.IsAutoHidden)
			{
				// First, hide any currently docked anchorable in this section
				HideDockedInSection(section);
				// Then dock this one
				anchorable.ToggleSingleAutoHide();
			}
			else
			{
				// Hide it (send back to auto-hide)
				anchorable.ToggleSingleAutoHide();
			}

			// Refresh button states
			RefreshButtonStates();

			// Update pin buttons to show "Minimize" tooltip after layout settles
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}

		/// <summary>
		/// Toggles an entire button group. When activating, docks all anchorables
		/// in the group as tabs in a single pane. When deactivating, sends all back to auto-hide.
		/// </summary>
		public void ToggleGroup(ToggleDockButtonGroup group)
		{
			if (group == null || group.Anchorables.Count == 0) return;

			var section = group.Section;
			bool anyDocked = group.Anchorables.Any(a => !a.IsAutoHidden);

			if (anyDocked)
			{
				// Send all back to auto-hide
				foreach (var anc in group.Anchorables.Where(a => !a.IsAutoHidden).ToList())
					anc.ToggleSingleAutoHide();
			}
			else
			{
				// Hide any currently docked anchorable in this section
				HideDockedInSection(section);

				// Dock the first one
				var first = group.Anchorables.First();
				first.ToggleSingleAutoHide();

				// Add the rest into the same pane (as tabs)
				var pane = first.Parent as LayoutAnchorablePane;
				if (pane != null)
				{
					foreach (var anc in group.Anchorables.Skip(1).ToList())
					{
						// Move from auto-hide side to the docked pane
						if (anc.Parent is LayoutAnchorGroup anchorGroup)
						{
							anchorGroup.RemoveChild(anc);
							pane.Children.Add(anc);
						}
					}
				}
			}

			RefreshButtonStates();
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}

		#endregion Public Methods

		#region Internal Methods

		/// <summary>
		/// Overrides the pin/auto-hide button behavior. In toggle mode, the pin button
		/// acts as a "minimize" button that sends the anchorable back to the sidebar.
		/// </summary>
		internal override void ExecuteAutoHideCommand(LayoutAnchorable anchorable)
		{
			if (anchorable == null) return;

			// Determine which section this anchorable belongs to
			var section = GetAnchorableSection(anchorable);

			// Use the toggle logic (which handles exclusive activation and button state refresh)
			ToggleAnchorable(anchorable, section);
		}

		#endregion Internal Methods

		#region Public Methods — Drag Support

		/// <summary>
		/// Moves an anchorable from its current section to a new section.
		/// Removes it from the old button bar and adds it to the new one, then toggles it on.
		/// </summary>
		public void MoveAnchorableToSection(LayoutAnchorable anchorable, AnchorSide targetSection)
		{
			if (anchorable == null) return;

			// Ensure it's auto-hidden first
			if (!anchorable.IsAutoHidden)
				anchorable.ToggleSingleAutoHide();

			// Remove from old button bar
			RemoveFromButtonBar(_leftTopButtonBar, anchorable);
			RemoveFromButtonBar(_leftBottomButtonBar, anchorable);
			RemoveFromButtonBar(_rightTopButtonBar, anchorable);

			// Move the anchorable in the layout model to the target side
			MoveAnchorableToSide(anchorable, targetSection);

			// Add to the target button bar
			var targetBar = GetButtonBarForSection(targetSection);
			if (targetBar != null)
			{
				var btn = new ToggleDockButton { Anchorable = anchorable, Section = targetSection };
				targetBar.Items.Add(btn);
			}

			// Toggle it on
			ToggleAnchorable(anchorable, targetSection);
		}

		#endregion Public Methods — Drag Support

		#region Private Methods

		private void ToggleDockingManager_Loaded(object sender, RoutedEventArgs e)
		{
			SetupToggleDockButtonBars();

			// Replace pin icons with minimize icons after layout settles
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

					// Replace the pin image with a minimize icon
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
					// Hide the close/hide button — toggle mode uses Minimize instead
					btn.Visibility = Visibility.Collapsed;
				}
			}
		}

		/// <summary>Creates a vector minimize icon (underscore line at bottom).</summary>
		private static UIElement CreateMinimizeIcon()
		{
			var path = new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M2,11 L11,11"),
				Stroke = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
				StrokeThickness = 1.5,
				Width = 13,
				Height = 13,
				Stretch = Stretch.None,
				SnapsToDevicePixels = true
			};
			return path;
		}

		private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
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

		private void SetupToggleDockButtonBars()
		{
			RemoveToggleDockButtonBars();

			// Hide the standard auto-hide side panels — we use button bars instead
			if (LeftSidePanel != null) LeftSidePanel.Visibility = Visibility.Collapsed;
			if (RightSidePanel != null) RightSidePanel.Visibility = Visibility.Collapsed;
			if (TopSidePanel != null) TopSidePanel.Visibility = Visibility.Collapsed;
			if (BottomSidePanel != null) BottomSidePanel.Visibility = Visibility.Collapsed;

			// Move all docked anchorables to auto-hide so they appear in button bars
			AutoHideAllDockedAnchorables();

			// Collect anchorables from each side
			var leftAnchorables = CollectAnchorables(Layout.LeftSide);
			var rightAnchorables = CollectAnchorables(Layout.RightSide);
			var bottomAnchorables = CollectAnchorables(Layout.BottomSide);

			// Create button bars
			_leftTopButtonBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Section = AnchorSide.Left };
			_leftTopButtonBar.SetAnchorables(leftAnchorables);

			_leftBottomButtonBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Section = AnchorSide.Bottom };
			_leftBottomButtonBar.SetAnchorables(bottomAnchorables);

			_rightTopButtonBar = new ToggleDockButtonBar { Orientation = Orientation.Vertical, Section = AnchorSide.Right };
			_rightTopButtonBar.SetAnchorables(rightAnchorables);

			// Inject into the template grid
			var rootGrid = FindTemplateRootGrid();
			if (rootGrid == null) return;

			// Wrap left bars in a DockPanel (top + bottom)
			var leftDockPanel = new DockPanel();
			DockPanel.SetDock(_leftTopButtonBar, Dock.Top);
			DockPanel.SetDock(_leftBottomButtonBar, Dock.Bottom);
			leftDockPanel.Children.Add(_leftTopButtonBar);
			leftDockPanel.Children.Add(_leftBottomButtonBar);
			leftDockPanel.Children.Add(new Border()); // fill remaining space

			Grid.SetRow(leftDockPanel, 0);
			Grid.SetRowSpan(leftDockPanel, 3);
			Grid.SetColumn(leftDockPanel, 0);

			Grid.SetRow(_rightTopButtonBar, 0);
			Grid.SetRowSpan(_rightTopButtonBar, 3);
			Grid.SetColumn(_rightTopButtonBar, 2);
			_rightTopButtonBar.VerticalAlignment = VerticalAlignment.Top;

			rootGrid.Children.Add(leftDockPanel);
			rootGrid.Children.Add(_rightTopButtonBar);

			_injectedLeftDockPanel = leftDockPanel;
		}

		private void RemoveToggleDockButtonBars()
		{
			if (_injectedLeftDockPanel != null)
			{
				(VisualTreeHelper.GetParent(_injectedLeftDockPanel) as Grid)?.Children.Remove(_injectedLeftDockPanel);
				_injectedLeftDockPanel = null;
			}
			if (_rightTopButtonBar != null)
			{
				(VisualTreeHelper.GetParent(_rightTopButtonBar) as Grid)?.Children.Remove(_rightTopButtonBar);
			}
			_leftTopButtonBar = null;
			_leftBottomButtonBar = null;
			_rightTopButtonBar = null;
		}

		/// <summary>Walks the visual tree to find the 3×3 Grid inside the DockingManager template.</summary>
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

		/// <summary>Moves all currently docked anchorables to auto-hide state.</summary>
		private void AutoHideAllDockedAnchorables()
		{
			var dockedAnchorables = Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.Where(a => a.Parent is LayoutAnchorablePane && !a.IsFloating)
				.ToList();

			foreach (var anchorable in dockedAnchorables)
				anchorable.ToggleSingleAutoHide();
		}

		/// <summary>Hides any currently docked anchorable in the given section by sending it back to auto-hide.</summary>
		private void HideDockedInSection(AnchorSide section)
		{
			var dockedInSection = Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.Where(a => a.Parent is LayoutAnchorablePane pane && !a.IsFloating && pane.GetSide() == section)
				.ToList();

			foreach (var anc in dockedInSection)
				anc.ToggleSingleAutoHide();
		}

		private void RefreshButtonStates()
		{
			RefreshBarStates(_leftTopButtonBar);
			RefreshBarStates(_leftBottomButtonBar);
			RefreshBarStates(_rightTopButtonBar);
		}

		private static void RefreshBarStates(ToggleDockButtonBar bar)
		{
			if (bar == null) return;
			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable != null)
					btn.IsChecked = !btn.Anchorable.IsAutoHidden;
				else if (item is ToggleDockButtonGroup grp)
					grp.RefreshState();
			}
		}

		/// <summary>Determines which sidebar section an anchorable belongs to by checking button bars.</summary>
		private AnchorSide GetAnchorableSection(LayoutAnchorable anchorable)
		{
			if (_leftTopButtonBar?.ContainsAnchorable(anchorable) == true) return AnchorSide.Left;
			if (_rightTopButtonBar?.ContainsAnchorable(anchorable) == true) return AnchorSide.Right;
			if (_leftBottomButtonBar?.ContainsAnchorable(anchorable) == true) return AnchorSide.Bottom;

			// Fallback: infer from current layout position
			if (anchorable.Parent is LayoutAnchorablePane pane)
				return pane.GetSide();
			if (anchorable.Parent is LayoutAnchorGroup group && group.Parent is LayoutAnchorSide side)
				return side.Side;

			return AnchorSide.Left;
		}

		private static bool ContainsAnchorable(ToggleDockButtonBar bar, LayoutAnchorable anchorable)
		{
			return bar?.ContainsAnchorable(anchorable) == true;
		}

		private void RemoveFromButtonBar(ToggleDockButtonBar bar, LayoutAnchorable anchorable)
		{
			if (bar == null) return;
			for (int i = bar.Items.Count - 1; i >= 0; i--)
			{
				if (bar.Items[i] is ToggleDockButton btn && btn.Anchorable == anchorable)
				{
					bar.Items.RemoveAt(i);
					return;
				}
				if (bar.Items[i] is ToggleDockButtonGroup grp && grp.RemoveAnchorable(anchorable))
				{
					if (grp.Anchorables.Count == 0)
						bar.Items.RemoveAt(i);
					else if (grp.Anchorables.Count == 1)
					{
						// Ungroup single remaining
						var remaining = grp.Anchorables.First();
						bar.Items.RemoveAt(i);
						bar.Items.Insert(i, new ToggleDockButton { Anchorable = remaining, Section = bar.Section });
					}
					return;
				}
			}
		}

		private ToggleDockButtonBar GetButtonBarForSection(AnchorSide section)
		{
			switch (section)
			{
				case AnchorSide.Left: return _leftTopButtonBar;
				case AnchorSide.Right: return _rightTopButtonBar;
				case AnchorSide.Bottom: return _leftBottomButtonBar;
				default: return _leftTopButtonBar;
			}
		}

		private void MoveAnchorableToSide(LayoutAnchorable anchorable, AnchorSide targetSide)
		{
			// Remove from current parent
			if (anchorable.Parent is LayoutAnchorGroup oldGroup)
				oldGroup.RemoveChild(anchorable);
			else if (anchorable.Parent is LayoutAnchorablePane oldPane)
				oldPane.RemoveChild(anchorable);

			// Find or create the target LayoutAnchorSide and add to it
			LayoutAnchorSide layoutSide;
			switch (targetSide)
			{
				case AnchorSide.Left: layoutSide = Layout.LeftSide; break;
				case AnchorSide.Right: layoutSide = Layout.RightSide; break;
				case AnchorSide.Bottom: layoutSide = Layout.BottomSide; break;
				case AnchorSide.Top: layoutSide = Layout.TopSide; break;
				default: layoutSide = Layout.LeftSide; break;
			}

			LayoutAnchorGroup targetGroup;
			if (layoutSide.Children.Count > 0)
			{
				targetGroup = layoutSide.Children.First();
			}
			else
			{
				targetGroup = new LayoutAnchorGroup();
				layoutSide.Children.Add(targetGroup);
			}

			targetGroup.Children.Add(anchorable);
		}

		#endregion Private Methods
	}
}
