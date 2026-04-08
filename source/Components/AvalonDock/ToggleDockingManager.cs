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
			var pinButtons = FindVisualChildren<System.Windows.Controls.Button>(this)
				.Where(b => b.Name == "PART_AutoHidePin");

			foreach (var pin in pinButtons)
			{
				pin.ToolTip = "Minimize";

				// Replace the pin image with a minimize icon (underscore / dash at bottom)
				var border = pin.Content as Border;
				if (border == null)
				{
					// Wrap in a transparent border for hit testing
					border = new Border { Background = Brushes.Transparent };
					pin.Content = border;
				}

				border.Child = CreateMinimizeIcon();
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
			}
		}

		/// <summary>Determines which sidebar section an anchorable belongs to by checking button bars.</summary>
		private AnchorSide GetAnchorableSection(LayoutAnchorable anchorable)
		{
			if (ContainsAnchorable(_leftTopButtonBar, anchorable)) return AnchorSide.Left;
			if (ContainsAnchorable(_rightTopButtonBar, anchorable)) return AnchorSide.Right;
			if (ContainsAnchorable(_leftBottomButtonBar, anchorable)) return AnchorSide.Bottom;

			// Fallback: infer from current layout position
			if (anchorable.Parent is LayoutAnchorablePane pane)
				return pane.GetSide();
			if (anchorable.Parent is LayoutAnchorGroup group && group.Parent is LayoutAnchorSide side)
				return side.Side;

			return AnchorSide.Left;
		}

		private static bool ContainsAnchorable(ToggleDockButtonBar bar, LayoutAnchorable anchorable)
		{
			if (bar == null) return false;
			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable == anchorable)
					return true;
			}
			return false;
		}

		#endregion Private Methods
	}
}
