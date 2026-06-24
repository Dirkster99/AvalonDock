using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock.Core;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the null to false converter.
	/// </summary>
	public sealed class NullToFalseConverter : IValueConverter
	{
		/// <summary>
		/// The shared instance.
		/// </summary>
		public static readonly NullToFalseConverter Instance = new NullToFalseConverter();

		/// <summary>
		/// Convert.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>The convert.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value != null;

		/// <summary>
		/// Convert back.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>The convert back.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}

	/// <summary>
	/// Represents the toggle dock button bar.
	/// </summary>
	public class ToggleDockButtonBar : ItemsControl
	{
		/// <summary>
		/// Gets or sets the zone.
		/// </summary>
		public DockZone Zone
		{
			get => (DockZone)GetValue(ZoneProperty);
			set => SetValue(ZoneProperty, value);
		}

		/// <summary>
		/// <see cref="Zone"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ZoneProperty =
			DependencyProperty.Register(nameof(Zone), typeof(DockZone), typeof(ToggleDockButtonBar), new PropertyMetadata(DockZone.LeftTop));

		/// <summary>
		/// Gets or sets the orientation.
		/// </summary>
		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		/// <summary>
		/// <see cref="Orientation"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ToggleDockButtonBar), new PropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Sets the anchorables.
		/// </summary>
		/// <param name="anchorables">The anchorables.</param>
		/// <param name="zone">The zone.</param>
		public void SetAnchorables(IEnumerable<LayoutAnchorable> anchorables, DockZone zone)
		{
			Items.Clear();
			foreach (var anc in anchorables)
			{
				var btn = new ToggleDockButton { Anchorable = anc, Zone = zone };
				Items.Add(btn);
			}
		}

		/// <summary>
		/// Contains anchorable.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <returns>true if the collection contains the specified item; otherwise, false.</returns>
		internal bool ContainsAnchorable(LayoutAnchorable anchorable)
		{
			foreach (var item in Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable == anchorable)
					return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Represents the toggle dock button.
	/// </summary>
	public class ToggleDockButton : ToggleButton
	{
		/// <summary>
		/// Resource key for the sidebar button foreground brush, shared by all sidebar buttons.
		/// </summary>
		public static readonly ComponentResourceKey ForegroundBrushKey =
			new ComponentResourceKey(typeof(ToggleDockButton), "ForegroundBrush");

		private Point _dragStartPoint;
		private bool _isMouseDown;
		private bool _isDragging;

		static ToggleDockButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleDockButton), new FrameworkPropertyMetadata(typeof(ToggleDockButton)));
		}

		/// <inheritdoc/>
		protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonUp(e);
			if (Anchorable == null) return;

			var manager = Anchorable.Root?.Manager as ToggleDockingManager;
			if (manager == null) return;

			var menu = manager.BuildToggleContextMenu(Anchorable);

			// Insert "Hide" at the top, followed by a separator
			var hideItem = new MenuItem { Header = "Hide" };
			hideItem.Click += (s, ev) =>
			{
				var layoutItem = manager.GetLayoutItemFromModel(Anchorable) as LayoutAnchorableItem;
				layoutItem?.HideCommand?.Execute(null);
				manager.RemoveButtonFromAllBars(Anchorable);
			};

			menu.Items.Insert(0, hideItem);
			menu.Items.Insert(1, new Separator());

			menu.PlacementTarget = this;
			menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			menu.IsOpen = true;
			e.Handled = true;
		}

		/// <summary>
		/// Gets or sets the anchorable.
		/// </summary>
		public LayoutAnchorable Anchorable
		{
			get => (LayoutAnchorable)GetValue(AnchorableProperty);
			set => SetValue(AnchorableProperty, value);
		}

		/// <summary>
		/// <see cref="Anchorable"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AnchorableProperty =
			DependencyProperty.Register(nameof(Anchorable), typeof(LayoutAnchorable), typeof(ToggleDockButton),
				new PropertyMetadata(null, OnAnchorableChanged));

		/// <summary>
		/// Gets or sets the zone.
		/// </summary>
		public DockZone Zone
		{
			get => (DockZone)GetValue(ZoneProperty);
			set => SetValue(ZoneProperty, value);
		}

		/// <summary>
		/// <see cref="Zone"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ZoneProperty =
			DependencyProperty.Register(nameof(Zone), typeof(DockZone), typeof(ToggleDockButton), new PropertyMetadata(DockZone.LeftTop));

		/// <summary>
		/// Gets or sets the icon source.
		/// </summary>
		public ImageSource IconSource
		{
			get => (ImageSource)GetValue(IconSourceProperty);
			set => SetValue(IconSourceProperty, value);
		}

		/// <summary>
		/// <see cref="IconSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IconSourceProperty =
			DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(ToggleDockButton), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the icon content.
		/// </summary>
		public object IconContent
		{
			get => GetValue(IconContentProperty);
			set => SetValue(IconContentProperty, value);
		}

		/// <summary>
		/// <see cref="IconContent"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IconContentProperty =
			DependencyProperty.Register(nameof(IconContent), typeof(object), typeof(ToggleDockButton), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the icon template.
		/// </summary>
		public DataTemplate IconTemplate
		{
			get => (DataTemplate)GetValue(IconTemplateProperty);
			set => SetValue(IconTemplateProperty, value);
		}

		/// <summary>
		/// <see cref="IconTemplate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IconTemplateProperty =
			DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(ToggleDockButton), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets a value indicating whether this instance is anchorable focused.
		/// </summary>
		public bool IsAnchorableFocused
		{
			get => (bool)GetValue(IsAnchorableFocusedProperty);
			set => SetValue(IsAnchorableFocusedProperty, value);
		}

		/// <summary>
		/// <see cref="IsAnchorableFocused"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsAnchorableFocusedProperty =
			DependencyProperty.Register(nameof(IsAnchorableFocused), typeof(bool), typeof(ToggleDockButton), new PropertyMetadata(false));

		private static void OnAnchorableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var btn = (ToggleDockButton)d;
			if (e.NewValue is LayoutAnchorable anc)
			{
				btn.Content = anc.Title;
				btn.IsChecked = !anc.IsAutoHidden;

				// Read attached properties from the LayoutAnchorable (set by developer in XAML)
				var attachedIcon = ToggleDock.GetIcon(anc);
				var attachedToolTip = ToggleDock.GetToolTip(anc);
				var attachedIconTemplate = ToggleDock.GetIconTemplate(anc);

				// Icon: prefer attached ToggleDock.Icon, fall back to LayoutAnchorable.IconSource
				if (attachedIcon != null)
					btn.IconContent = attachedIcon;
				else if (anc.IconSource != null)
					btn.IconSource = anc.IconSource;

				// IconTemplate: if set, apply as the button's ContentTemplate for the icon presenter
				if (attachedIconTemplate != null)
					btn.IconTemplate = attachedIconTemplate;

				// ToolTip: prefer attached ToggleDock.ToolTip, fall back to Title
				btn.ToolTip = attachedToolTip ?? anc.Title;
			}
		}

		/// <inheritdoc/>
		protected override void OnClick()
		{
			// Don't fire toggle when a drag is in progress
			if (_isDragging)
			{
				_isDragging = false;
				return;
			}

			base.OnClick();
			if (Anchorable == null) return;

			var manager = Anchorable.Root?.Manager as ToggleDockingManager;
			manager?.ToggleAnchorable(Anchorable, Zone);
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			_dragStartPoint = e.GetPosition(this);
			_isMouseDown = true;
			_isDragging = false;
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			if (!_isDragging)
				base.OnMouseLeftButtonUp(e);
			_isDragging = false;
		}

		/// <inheritdoc/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!_isMouseDown || e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

			var pos = e.GetPosition(this);
			var diff = pos - _dragStartPoint;
			if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				_isMouseDown = false;
				_isDragging = true;
				ReleaseMouseCapture();

				// Find the parent bar
				var bar = ItemsControl.ItemsControlFromItemContainer(this) as ToggleDockButtonBar
					?? FindParent<ToggleDockButtonBar>(this);
				if (bar == null) return;

				// Start overlay-based drag
				ToggleDockDragOverlay.StartDrag(this, bar);
			}
		}

		/// <summary>
		/// Finds the parent.
		/// </summary>
		/// <typeparam name="T">The type of t.</typeparam>
		/// <param name="child">The child.</param>
		/// <returns>The find parent.</returns>
		internal static T FindParent<T>(DependencyObject child)
			where T : DependencyObject
		{
			var parent = VisualTreeHelper.GetParent(child);
			while (parent != null)
			{
				if (parent is T t) return t;
				parent = VisualTreeHelper.GetParent(parent);
			}

			return null;
		}
	}

	/// <summary>
	/// Represents the toggle dock drag overlay.
	/// </summary>
	internal class ToggleDockDragOverlay : Window
	{
		private readonly LayoutAnchorable _sourceAnchorable;
		private readonly ToggleDockingManager _manager;
		private readonly Window _ownerWindow;
		private readonly List<DropZone> _dropZones = new List<DropZone>();

		// Visual for the dragged button ghost
		private readonly VisualBrush _ghostBrush;
		private readonly Size _ghostSize;

		/// <summary>
		/// Represents the drop zone.
		/// </summary>
		internal struct DropZone
		{
			/// <summary>
			/// The rect.
			/// </summary>
			public Rect Rect;

			/// <summary>
			/// The zone.
			/// </summary>
			public DockZone Zone;

			/// <summary>
			/// The label.
			/// </summary>
			public string Label;

			/// <summary>
			/// The insertion line start.
			/// </summary>
			public Point? InsertionLineStart;

			/// <summary>
			/// The insertion line end.
			/// </summary>
			public Point? InsertionLineEnd;
		}

		private ToggleDockDragOverlay(LayoutAnchorable anchorable, Visual ghostVisual, Size ghostSize, ToggleDockingManager manager, Window owner)
		{
			_sourceAnchorable = anchorable;
			_manager = manager;
			_ownerWindow = owner;

			_ghostBrush = new VisualBrush(ghostVisual) { Opacity = 0.6 };
			_ghostSize = ghostSize;

			WindowStyle = WindowStyle.None;
			AllowsTransparency = true;
			Background = System.Windows.Media.Brushes.Transparent;
			ShowInTaskbar = false;
			Topmost = true;
			ResizeMode = ResizeMode.NoResize;

			// Position using the same DPI-aware approach as AvalonDock's OverlayWindow
			var rectWindow = new Rect(
				_manager.PointToScreenDPIWithoutFlowDirection(new Point()),
				_manager.TransformActualSizeToAncestor());
			Left = rectWindow.Left;
			Top = rectWindow.Top;
			Width = rectWindow.Width;
			Height = rectWindow.Height;

			Cursor = Cursors.Hand;
		}

		/// <summary>
		/// Start drag.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="sourceBar">The source bar.</param>
		internal static void StartDrag(ToggleDockButton source, ToggleDockButtonBar sourceBar)
		{
			var owner = Window.GetWindow(source);
			if (owner == null) return;

			var manager = source.Anchorable?.Root?.Manager as ToggleDockingManager;
			if (manager == null) return;

			var overlay = new ToggleDockDragOverlay(source.Anchorable, source, source.RenderSize, manager, owner);
			overlay.BuildDropZones();
			overlay.Show();
			overlay.CaptureMouse();
		}

		/// <summary>
		/// Start drag from pane.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		/// <param name="manager">The manager.</param>
		internal static void StartDragFromPane(LayoutAnchorable anchorable, ToggleDockingManager manager)
		{
			var owner = Window.GetWindow(manager);
			if (owner == null || anchorable == null) return;

			var overlay = new ToggleDockDragOverlay(anchorable, manager, new Size(24, 24), manager, owner);
			overlay.BuildDropZones();
			overlay.Show();
			overlay.CaptureMouse();
		}

		private void BuildDropZones()
		{
			double totalW = Width;
			double totalH = Height;

			// Get sidebar bar widths using their actual rendered sizes
			double leftBarWidth = 0;
			double rightBarWidth = 0;

			if (_manager._injectedLeftDockPanel != null && _manager._injectedLeftDockPanel.IsVisible)
			{
				var leftBarRect = _manager._injectedLeftDockPanel.GetScreenArea();
				leftBarWidth = leftBarRect.Width;
			}

			if (_manager._rightTopBar != null && _manager._rightTopBar.IsVisible)
			{
				var rightBarRect = _manager._rightTopBar.GetScreenArea();
				rightBarWidth = rightBarRect.Width;
			}

			double contentX = leftBarWidth;
			double contentW = totalW - leftBarWidth - rightBarWidth;
			double contentH = totalH;

			if (contentW < 50 || contentH < 50) return;

			double leftW = GetOpenDockWidth(AnchorSide.Left, contentW * 0.25);
			double rightW = GetOpenDockWidth(AnchorSide.Right, contentW * 0.25);
			double bottomH = GetOpenDockHeight(AnchorSide.Bottom, contentH * 0.25);

			double sideH = contentH - bottomH;
			double halfSideH = sideH / 2.0;

			// Content area drop zones
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(contentX, 0, leftW, halfSideH),
				Zone = DockZone.LeftTop,
				Label = "Left Top"
			});
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(contentX, halfSideH, leftW, halfSideH),
				Zone = DockZone.LeftBottom,
				Label = "Left Bottom"
			});
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(contentX + contentW - rightW, 0, rightW, halfSideH),
				Zone = DockZone.RightTop,
				Label = "Right Top"
			});
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(contentX + contentW - rightW, halfSideH, rightW, halfSideH),
				Zone = DockZone.RightBottom,
				Label = "Right Bottom"
			});
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(contentX, sideH, contentW / 2, bottomH),
				Zone = DockZone.BottomLeft,
				Label = "Bottom Left"
			});
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(contentX + contentW / 2, sideH, contentW / 2, bottomH),
				Zone = DockZone.BottomRight,
				Label = "Bottom Right"
			});

			// Sidebar panel drop zones — cover entire panel area, split at separator
			AddSidebarPanelDropZones(_manager._injectedLeftDockPanel,
				_manager._leftTopBar, DockZone.LeftTop,
				_manager._leftBottomBar, DockZone.LeftBottom,
				isVertical: true);
			AddSidebarPanelDropZones(_manager._injectedRightDockPanel,
				_manager._rightTopBar, DockZone.RightTop,
				_manager._rightBottomBar, DockZone.RightBottom,
				isVertical: true);
			// Bottom bars sit at the bottom of left/right panels — add them as simple bar zones
			AddBarDropZone(_manager._bottomLeftBar, DockZone.BottomLeft);
			AddBarDropZone(_manager._bottomRightBar, DockZone.BottomRight);
		}

		private void AddSidebarPanelDropZones(DockPanel panel,
			ToggleDockButtonBar topBar, DockZone topZone,
			ToggleDockButtonBar bottomBar, DockZone bottomZone,
			bool isVertical)
		{
			if (panel == null || !panel.IsVisible) return;

			var managerScreenPos = new Point(Left, Top);
			var panelScreen = panel.GetScreenArea();
			var panelRect = new Rect(
				panelScreen.Left - managerScreenPos.X,
				panelScreen.Top - managerScreenPos.Y,
				Math.Max(panelScreen.Width, 20),
				Math.Max(panelScreen.Height, 20));

			// Exclude the bottom-dock bar area from the panel zone
			double bottomBarHeight = 0;
			var bottomDockBar = topZone.ToString().StartsWith("Left") ? _manager._bottomLeftBar : _manager._bottomRightBar;
			if (bottomDockBar != null && bottomDockBar.IsVisible)
			{
				var bbScreen = bottomDockBar.GetScreenArea();
				bottomBarHeight = bbScreen.Height;
			}

			double usableHeight = panelRect.Height - bottomBarHeight;
			if (usableHeight < 10) return;

			// Find actual separator position
			var separator = topZone.ToString().StartsWith("Left") ? _manager._leftSeparator : _manager._rightSeparator;
			double splitY;
			if (separator != null && separator.IsVisible)
			{
				var sepScreen = separator.GetScreenArea();
				splitY = sepScreen.Top + sepScreen.Height / 2.0 - managerScreenPos.Y;
			}
			else
			{
				splitY = panelRect.Y + usableHeight / 2.0;
			}

			double topHeight = splitY - panelRect.Y;
			double bottomHeight = panelRect.Y + usableHeight - splitY;

			// Top half → topZone
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(panelRect.X, panelRect.Y, panelRect.Width, topHeight),
				Zone = topZone,
				Label = null,
				InsertionLineStart = new Point(panelRect.X + 2, splitY),
				InsertionLineEnd = new Point(panelRect.X + panelRect.Width - 2, splitY)
			});

			// Bottom half → bottomZone
			_dropZones.Add(new DropZone
			{
				Rect = new Rect(panelRect.X, splitY, panelRect.Width, bottomHeight),
				Zone = bottomZone,
				Label = null,
				InsertionLineStart = new Point(panelRect.X + 2, splitY),
				InsertionLineEnd = new Point(panelRect.X + panelRect.Width - 2, splitY)
			});
		}

		private void AddBarDropZone(ToggleDockButtonBar bar, DockZone zone)
		{
			if (bar == null || !bar.IsVisible) return;

			// Convert bar's screen position to overlay-relative coordinates
			var managerScreenPos = new Point(Left, Top);
			var barScreenArea = bar.GetScreenArea();
			var barRect = new Rect(
				barScreenArea.Left - managerScreenPos.X,
				barScreenArea.Top - managerScreenPos.Y,
				Math.Max(barScreenArea.Width, 20),
				Math.Max(barScreenArea.Height, 20));

			_dropZones.Add(new DropZone { Rect = barRect, Zone = zone, Label = null });
		}

		private double GetOpenDockWidth(AnchorSide side, double fallback)
		{
			// Find any visible LayoutAnchorablePaneControl docked on this side
			foreach (var paneCtrl in FindVisualChildren<LayoutAnchorablePaneControl>(_manager))
			{
				if (paneCtrl.Model is LayoutAnchorablePane pane
					&& pane.GetSide() == side
					&& pane.Children.Any(a => !a.IsAutoHidden)
					&& paneCtrl.ActualWidth > 10)
				{
					return paneCtrl.ActualWidth;
				}
			}

			return fallback;
		}

		private double GetOpenDockHeight(AnchorSide side, double fallback)
		{
			foreach (var paneCtrl in FindVisualChildren<LayoutAnchorablePaneControl>(_manager))
			{
				if (paneCtrl.Model is LayoutAnchorablePane pane
					&& pane.GetSide() == side
					&& pane.Children.Any(a => !a.IsAutoHidden)
					&& paneCtrl.ActualHeight > 10)
				{
					return paneCtrl.ActualHeight;
				}
			}

			return fallback;
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

		/// <inheritdoc/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			InvalidateVisual();
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			var pos = e.GetPosition(this);
			DropZone? hitZone = null;
			for (int i = _dropZones.Count - 1; i >= 0; i--)
			{
				if (_dropZones[i].Rect.Contains(pos))
				{
					hitZone = _dropZones[i];
					break;
				}
			}

			// Prevent OnLostMouseCapture from closing before we act
			_isClosing = true;
			ReleaseMouseCapture();
			Close();

			if (hitZone.HasValue && _sourceAnchorable != null)
			{
				_manager.MoveAnchorableToZone(_sourceAnchorable, hitZone.Value.Zone);
			}
		}

		private bool _isClosing;

		/// <inheritdoc/>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				ReleaseMouseCapture();
				Close();
				e.Handled = true;
			}

			base.OnKeyDown(e);
		}

		/// <inheritdoc/>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);
			if (IsVisible && !_isClosing)
				Close();
		}

		/// <inheritdoc/>
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var mousePos = Mouse.GetPosition(this);

			var normalBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x7A, 0xCC));
			var hoverBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x00, 0x7A, 0xCC));
			var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x7A, 0xCC)), 1.5);
			var labelTypeface = new Typeface("Segoe UI");

			// Check bottom zones first for hover priority (same as drop)
			int hoveredIndex = -1;
			for (int i = _dropZones.Count - 1; i >= 0; i--)
			{
				if (_dropZones[i].Rect.Contains(mousePos))
				{
					hoveredIndex = i;
					break;
				}
			}

			for (int i = 0; i < _dropZones.Count; i++)
			{
				var zone = _dropZones[i];
				bool isHovered = i == hoveredIndex;
				bool isBarZone = zone.Label == null;

				if (isBarZone)
				{
					// Bar zones: only show insertion indicator when hovered
					if (isHovered)
					{
						// Light highlight on the zone
						dc.DrawRoundedRectangle(
							new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x7A, 0xCC)),
							null, zone.Rect, 2, 2);

						// Draw insertion line if available
						if (zone.InsertionLineStart.HasValue && zone.InsertionLineEnd.HasValue)
						{
							var linePen = new Pen(new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x7A, 0xCC)), 3)
							{
								DashCap = PenLineCap.Round,
								StartLineCap = PenLineCap.Round,
								EndLineCap = PenLineCap.Round
							};
							dc.DrawLine(linePen, zone.InsertionLineStart.Value, zone.InsertionLineEnd.Value);

							// Draw small circles at each end
							var circleBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x7A, 0xCC));
							dc.DrawEllipse(circleBrush, null, zone.InsertionLineStart.Value, 3, 3);
							dc.DrawEllipse(circleBrush, null, zone.InsertionLineEnd.Value, 3, 3);
						}
						else
						{
							dc.DrawRoundedRectangle(hoverBrush, borderPen, zone.Rect, 2, 2);
						}
					}
				}
				else
				{
					// Content area zones: always visible with label
					dc.DrawRoundedRectangle(
						isHovered ? hoverBrush : normalBrush,
						borderPen,
						zone.Rect,
						4, 4);

					var formattedText = new FormattedText(
						zone.Label,
						System.Globalization.CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						labelTypeface,
						14,
						new SolidColorBrush(Color.FromArgb(isHovered ? (byte)0xCC : (byte)0x66, 0x00, 0x7A, 0xCC)),
						1.0);

					var textX = zone.Rect.X + (zone.Rect.Width - formattedText.Width) / 2;
					var textY = zone.Rect.Y + (zone.Rect.Height - formattedText.Height) / 2;
					dc.DrawText(formattedText, new Point(textX, textY));
				}
			}

			// Draw ghost of dragged button near cursor
			var ghostRect = new Rect(
				mousePos.X + 12,
				mousePos.Y - _ghostSize.Height / 2,
				_ghostSize.Width,
				_ghostSize.Height);
			dc.DrawRectangle(_ghostBrush, new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)), 1), ghostRect);
		}
	}
}