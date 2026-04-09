/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
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

namespace AvalonDock.Controls
{
	/// <summary>
	/// Returns true when the value is not null. Used in XAML triggers to show/hide icon elements.
	/// </summary>
	public sealed class NullToFalseConverter : IValueConverter
	{
		public static readonly NullToFalseConverter Instance = new NullToFalseConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value != null;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}
	/// <summary>
	/// A vertical/horizontal bar of toggle buttons representing anchorable tool windows.
	/// Used by <see cref="ToggleDockingManager"/> to provide VSCode/Rider-style sidebar buttons.
	/// </summary>
	public class ToggleDockButtonBar : ItemsControl
	{
		/// <summary>Gets/sets which dock zone this button bar represents.</summary>
		public DockZone Zone
		{
			get => (DockZone)GetValue(ZoneProperty);
			set => SetValue(ZoneProperty, value);
		}

		public static readonly DependencyProperty ZoneProperty =
			DependencyProperty.Register(nameof(Zone), typeof(DockZone), typeof(ToggleDockButtonBar), new PropertyMetadata(DockZone.LeftTop));

		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ToggleDockButtonBar), new PropertyMetadata(Orientation.Vertical));

		/// <summary>Populates this bar with buttons for the given anchorables.</summary>
		public void SetAnchorables(IEnumerable<LayoutAnchorable> anchorables, DockZone zone)
		{
			Items.Clear();
			foreach (var anc in anchorables)
			{
				var btn = new ToggleDockButton { Anchorable = anc, Zone = zone };
				Items.Add(btn);
			}
		}

		/// <summary>Checks whether any button in this bar references the given anchorable.</summary>
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
	/// A toggle button that represents a single anchorable tool window in the sidebar.
	/// Clicking it toggles the anchorable between auto-hidden and docked states.
	/// Supports drag to reorder or group with other buttons.
	/// </summary>
	public class ToggleDockButton : ToggleButton
	{
		private Point _dragStartPoint;
		private bool _isMouseDown;
		private bool _isDragging;

		static ToggleDockButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleDockButton), new FrameworkPropertyMetadata(typeof(ToggleDockButton)));
		}

		/// <summary>The anchorable model this button controls.</summary>
		public LayoutAnchorable Anchorable
		{
			get => (LayoutAnchorable)GetValue(AnchorableProperty);
			set => SetValue(AnchorableProperty, value);
		}

		public static readonly DependencyProperty AnchorableProperty =
			DependencyProperty.Register(nameof(Anchorable), typeof(LayoutAnchorable), typeof(ToggleDockButton),
				new PropertyMetadata(null, OnAnchorableChanged));

		/// <summary>Which dock zone this button belongs to.</summary>
		public DockZone Zone
		{
			get => (DockZone)GetValue(ZoneProperty);
			set => SetValue(ZoneProperty, value);
		}

		public static readonly DependencyProperty ZoneProperty =
			DependencyProperty.Register(nameof(Zone), typeof(DockZone), typeof(ToggleDockButton), new PropertyMetadata(DockZone.LeftTop));

		/// <summary>Icon source from the associated anchorable.</summary>
		public ImageSource IconSource
		{
			get => (ImageSource)GetValue(IconSourceProperty);
			set => SetValue(IconSourceProperty, value);
		}

		public static readonly DependencyProperty IconSourceProperty =
			DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(ToggleDockButton), new PropertyMetadata(null));

		/// <summary>Icon content (e.g. a Path or any UIElement) displayed before the title.</summary>
		public object IconContent
		{
			get => GetValue(IconContentProperty);
			set => SetValue(IconContentProperty, value);
		}

		public static readonly DependencyProperty IconContentProperty =
			DependencyProperty.Register(nameof(IconContent), typeof(object), typeof(ToggleDockButton), new PropertyMetadata(null));

		/// <summary>DataTemplate for rendering the icon. When set, <see cref="IconContent"/> is used as the DataContext.</summary>
		public DataTemplate IconTemplate
		{
			get => (DataTemplate)GetValue(IconTemplateProperty);
			set => SetValue(IconTemplateProperty, value);
		}

		public static readonly DependencyProperty IconTemplateProperty =
			DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(ToggleDockButton), new PropertyMetadata(null));

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

		protected override void OnClick()
		{
			base.OnClick();
			if (Anchorable == null) return;

			var manager = Anchorable.Root?.Manager as ToggleDockingManager;
			manager?.ToggleAnchorable(Anchorable, Zone);
		}

		#region Drag Support

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			_dragStartPoint = e.GetPosition(this);
			_isMouseDown = true;
			_isDragging = false;
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			if (!_isDragging)
				base.OnMouseLeftButtonUp(e);
			_isDragging = false;
		}

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

		internal static T FindParent<T>(DependencyObject child) where T : DependencyObject
		{
			var parent = VisualTreeHelper.GetParent(child);
			while (parent != null)
			{
				if (parent is T t) return t;
				parent = VisualTreeHelper.GetParent(parent);
			}
			return null;
		}

		#endregion Drag Support
	}

	/// <summary>
	/// A transparent overlay window that shows 6 edge drop zones when dragging a toggle button.
	/// Zones: LeftTop, LeftBottom, RightTop, RightBottom, BottomLeft, BottomRight.
	/// </summary>
	internal class ToggleDockDragOverlay : Window
	{
		private readonly ToggleDockButton _sourceButton;
		private readonly ToggleDockButtonBar _sourceBar;
		private readonly ToggleDockingManager _manager;
		private readonly Window _ownerWindow;
		private readonly List<DropZone> _dropZones = new List<DropZone>();

		// Visual for the dragged button ghost
		private readonly VisualBrush _ghostBrush;
		private readonly Size _ghostSize;

		internal struct DropZone
		{
			public Rect Rect;
			public DockZone Zone;
			public string Label;
		}

		private ToggleDockDragOverlay(ToggleDockButton source, ToggleDockButtonBar sourceBar, ToggleDockingManager manager, Window owner)
		{
			_sourceButton = source;
			_sourceBar = sourceBar;
			_manager = manager;
			_ownerWindow = owner;

			_ghostBrush = new VisualBrush(source) { Opacity = 0.6 };
			_ghostSize = source.RenderSize;

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

		internal static void StartDrag(ToggleDockButton source, ToggleDockButtonBar sourceBar)
		{
			var owner = Window.GetWindow(source);
			if (owner == null) return;

			var manager = source.Anchorable?.Root?.Manager as ToggleDockingManager;
			if (manager == null) return;

			var overlay = new ToggleDockDragOverlay(source, sourceBar, manager, owner);
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

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			InvalidateVisual();
		}

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

			if (hitZone.HasValue && _sourceButton.Anchorable != null)
			{
				_manager.MoveAnchorableToZone(_sourceButton.Anchorable, hitZone.Value.Zone);
			}
		}

		private bool _isClosing;

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

		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);
			if (IsVisible && !_isClosing)
				Close();
		}

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

				dc.DrawRoundedRectangle(
					isHovered ? hoverBrush : normalBrush,
					borderPen,
					zone.Rect,
					4, 4);

				// Draw zone label
				var formattedText = new FormattedText(
					zone.Label,
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					labelTypeface,
					14,
					new SolidColorBrush(Color.FromArgb(isHovered ? (byte)0xCC : (byte)0x66, 0x00, 0x7A, 0xCC))
#if !NET40
					, 1.0
#endif
					);

				var textX = zone.Rect.X + (zone.Rect.Width - formattedText.Width) / 2;
				var textY = zone.Rect.Y + (zone.Rect.Height - formattedText.Height) / 2;
				dc.DrawText(formattedText, new Point(textX, textY));
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
