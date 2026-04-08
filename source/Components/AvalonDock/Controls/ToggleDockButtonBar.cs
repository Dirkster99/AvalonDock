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
		/// <summary>Gets/sets which anchor side this button bar represents.</summary>
		public AnchorSide Section
		{
			get => (AnchorSide)GetValue(SectionProperty);
			set => SetValue(SectionProperty, value);
		}

		public static readonly DependencyProperty SectionProperty =
			DependencyProperty.Register(nameof(Section), typeof(AnchorSide), typeof(ToggleDockButtonBar), new PropertyMetadata(AnchorSide.Left));

		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ToggleDockButtonBar), new PropertyMetadata(Orientation.Vertical));

		/// <summary>Populates this bar with buttons for the given anchorables.</summary>
		public void SetAnchorables(IEnumerable<LayoutAnchorable> anchorables)
		{
			Items.Clear();
			foreach (var anc in anchorables)
			{
				var btn = new ToggleDockButton { Anchorable = anc, Section = Section };
				Items.Add(btn);
			}
		}

		/// <summary>
		/// Merges <paramref name="source"/> into <paramref name="target"/> to form a group.
		/// If target is already a group, the source's anchorable is added.
		/// Otherwise, both are replaced with a new <see cref="ToggleDockButtonGroup"/>.
		/// </summary>
		internal void MergeButtons(ToggleButton target, ToggleDockButton source)
		{
			if (target == source) return;

			var sourceAnc = source.Anchorable;
			if (sourceAnc == null) return;

			int targetIdx = Items.IndexOf(target);
			int sourceIdx = Items.IndexOf(source);
			if (targetIdx < 0 || sourceIdx < 0) return;

			if (target is ToggleDockButtonGroup group)
			{
				// Add to existing group
				group.AddAnchorable(sourceAnc);
				Items.RemoveAt(sourceIdx);
			}
			else if (target is ToggleDockButton targetBtn)
			{
				var targetAnc = targetBtn.Anchorable;
				if (targetAnc == null) return;

				// Create new group replacing both
				var newGroup = new ToggleDockButtonGroup { Section = Section };
				newGroup.AddAnchorable(targetAnc);
				newGroup.AddAnchorable(sourceAnc);

				Items.RemoveAt(sourceIdx > targetIdx ? sourceIdx : targetIdx);
				Items.RemoveAt(sourceIdx > targetIdx ? targetIdx : sourceIdx);
				Items.Insert(Math.Min(sourceIdx, targetIdx), newGroup);
			}
		}

		/// <summary>Splits a group back into individual buttons at the group's position.</summary>
		internal void UngroupButton(ToggleDockButtonGroup group)
		{
			int idx = Items.IndexOf(group);
			if (idx < 0) return;

			var anchorables = group.Anchorables.ToList();
			Items.RemoveAt(idx);
			foreach (var anc in anchorables)
			{
				var btn = new ToggleDockButton { Anchorable = anc, Section = Section };
				Items.Insert(idx++, btn);
			}
		}

		/// <summary>Checks whether any button or group in this bar references the given anchorable.</summary>
		internal bool ContainsAnchorable(LayoutAnchorable anchorable)
		{
			foreach (var item in Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable == anchorable)
					return true;
				if (item is ToggleDockButtonGroup grp && grp.Anchorables.Contains(anchorable))
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

		/// <summary>Which anchor side section this button belongs to.</summary>
		public AnchorSide Section
		{
			get => (AnchorSide)GetValue(SectionProperty);
			set => SetValue(SectionProperty, value);
		}

		public static readonly DependencyProperty SectionProperty =
			DependencyProperty.Register(nameof(Section), typeof(AnchorSide), typeof(ToggleDockButton), new PropertyMetadata(AnchorSide.Left));

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
			manager?.ToggleAnchorable(Anchorable, Section);
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
	/// A toggle button that represents a group of anchorable tool windows stacked together.
	/// Clicking it toggles all anchorables in the group as tabs in a single docked pane.
	/// Displays combined icons separated by "/" visuals.
	/// </summary>
	public class ToggleDockButtonGroup : ToggleButton
	{
		private readonly ObservableCollection<LayoutAnchorable> _anchorables = new ObservableCollection<LayoutAnchorable>();

		static ToggleDockButtonGroup()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleDockButtonGroup), new FrameworkPropertyMetadata(typeof(ToggleDockButtonGroup)));
		}

		/// <summary>The anchorables in this group.</summary>
		public ReadOnlyObservableCollection<LayoutAnchorable> Anchorables { get; }

		/// <summary>Which anchor side section this group belongs to.</summary>
		public AnchorSide Section
		{
			get => (AnchorSide)GetValue(SectionProperty);
			set => SetValue(SectionProperty, value);
		}

		public static readonly DependencyProperty SectionProperty =
			DependencyProperty.Register(nameof(Section), typeof(AnchorSide), typeof(ToggleDockButtonGroup), new PropertyMetadata(AnchorSide.Left));

		public ToggleDockButtonGroup()
		{
			Anchorables = new ReadOnlyObservableCollection<LayoutAnchorable>(_anchorables);
			AllowDrop = true;
			ContextMenu = CreateContextMenu();
		}

		/// <summary>Adds an anchorable to this group and refreshes the visual.</summary>
		public void AddAnchorable(LayoutAnchorable anchorable)
		{
			if (anchorable == null || _anchorables.Contains(anchorable)) return;
			_anchorables.Add(anchorable);
			UpdateVisual();
		}

		/// <summary>Removes an anchorable from this group.</summary>
		public bool RemoveAnchorable(LayoutAnchorable anchorable)
		{
			bool removed = _anchorables.Remove(anchorable);
			if (removed) UpdateVisual();
			return removed;
		}

		/// <summary>Refreshes IsChecked based on whether any anchorable in the group is docked.</summary>
		public void RefreshState()
		{
			IsChecked = _anchorables.Any(a => !a.IsAutoHidden);
		}

		protected override void OnClick()
		{
			base.OnClick();
			if (_anchorables.Count == 0) return;

			var firstAnc = _anchorables.FirstOrDefault();
			var manager = firstAnc?.Root?.Manager as ToggleDockingManager;
			if (manager == null) return;

			manager.ToggleGroup(this);
		}

		#region Private Methods

		private void UpdateVisual()
		{
			// Build combined icon panel: icon1 / icon2 / icon3
			var panel = new StackPanel { Orientation = Orientation.Horizontal };
			for (int i = 0; i < _anchorables.Count; i++)
			{
				if (i > 0)
				{
					panel.Children.Add(new TextBlock
					{
						Text = "/",
						Margin = new Thickness(2, 0, 2, 0),
						VerticalAlignment = VerticalAlignment.Center,
						Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
						FontSize = 10
					});
				}

				var anc = _anchorables[i];
				var icon = ToggleDock.GetIcon(anc);
				if (icon is UIElement uiIcon)
				{
					// Clone the icon via XamlWriter/XamlReader for reuse
					var iconCopy = CloneUIElement(uiIcon);
					if (iconCopy != null)
						panel.Children.Add(iconCopy);
					else
						panel.Children.Add(CreateFallbackIcon(anc.Title));
				}
				else if (anc.IconSource != null)
				{
					panel.Children.Add(new Image
					{
						Source = anc.IconSource,
						Width = 14, Height = 14,
						Stretch = Stretch.Uniform,
						VerticalAlignment = VerticalAlignment.Center
					});
				}
				else
				{
					panel.Children.Add(CreateFallbackIcon(anc.Title));
				}
			}

			Content = panel;
			ToolTip = string.Join(" / ", _anchorables.Select(a => a.Title));
			RefreshState();
		}

		private static UIElement CloneUIElement(UIElement element)
		{
			try
			{
				var xaml = System.Windows.Markup.XamlWriter.Save(element);
				return System.Windows.Markup.XamlReader.Parse(xaml) as UIElement;
			}
			catch
			{
				return null;
			}
		}

		private static UIElement CreateFallbackIcon(string title)
		{
			return new TextBlock
			{
				Text = string.IsNullOrEmpty(title) ? "?" : title.Substring(0, 1),
				FontSize = 11,
				FontWeight = FontWeights.Bold,
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55))
			};
		}

		private ContextMenu CreateContextMenu()
		{
			var menu = new ContextMenu();
			var ungroupItem = new MenuItem { Header = "Ungroup" };
			ungroupItem.Click += (s, e) =>
			{
				var bar = ItemsControl.ItemsControlFromItemContainer(this) as ToggleDockButtonBar
					?? FindParent<ToggleDockButtonBar>(this);
				bar?.UngroupButton(this);
			};
			menu.Items.Add(ungroupItem);
			return menu;
		}

		private static T FindParent<T>(DependencyObject child) where T : DependencyObject
		{
			var parent = VisualTreeHelper.GetParent(child);
			while (parent != null)
			{
				if (parent is T t) return t;
				parent = VisualTreeHelper.GetParent(parent);
			}
			return null;
		}

		#endregion Private Methods
	}

	/// <summary>
	/// A transparent overlay window used for drag-and-drop of toggle dock buttons.
	/// Shows drop target indicators over valid buttons and handles mouse tracking.
	/// This avoids the WPF DragDrop system which conflicts with ToggleButton mouse capture.
	/// </summary>
	internal class ToggleDockDragOverlay : Window
	{
		private readonly ToggleDockButton _sourceButton;
		private readonly ToggleDockButtonBar _sourceBar;
		private readonly Window _ownerWindow;
		private readonly List<DropTarget> _dropTargets = new List<DropTarget>();
		private int _hoveredTargetIndex = -1;

		// Visual for the dragged button ghost
		private readonly VisualBrush _ghostBrush;
		private readonly Size _ghostSize;

		private struct DropTarget
		{
			public Rect ScreenRect;
			public ToggleButton Button;
		}

		private ToggleDockDragOverlay(ToggleDockButton source, ToggleDockButtonBar sourceBar, Window owner)
		{
			_sourceButton = source;
			_sourceBar = sourceBar;
			_ownerWindow = owner;

			_ghostBrush = new VisualBrush(source) { Opacity = 0.6 };
			_ghostSize = source.RenderSize;

			WindowStyle = WindowStyle.None;
			AllowsTransparency = true;
			Background = System.Windows.Media.Brushes.Transparent;
			ShowInTaskbar = false;
			Topmost = true;
			ResizeMode = ResizeMode.NoResize;

			// Cover the entire owner window
			Left = owner.Left;
			Top = owner.Top;
			Width = owner.ActualWidth;
			Height = owner.ActualHeight;

			Cursor = Cursors.Hand;
		}

		internal static void StartDrag(ToggleDockButton source, ToggleDockButtonBar sourceBar)
		{
			var owner = Window.GetWindow(source);
			if (owner == null) return;

			var overlay = new ToggleDockDragOverlay(source, sourceBar, owner);
			overlay.CollectDropTargets(owner);
			overlay.Show();
			overlay.CaptureMouse();
		}

		private void CollectDropTargets(Window owner)
		{
			// Find all ToggleDockButtons and ToggleDockButtonGroups in the window
			CollectTargetsRecursive(owner);
		}

		private void CollectTargetsRecursive(DependencyObject parent)
		{
			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);

				if (child is ToggleDockButton btn && btn != _sourceButton && btn.IsVisible)
				{
					var screenPos = btn.PointToScreen(new Point(0, 0));
					var rect = new Rect(
						screenPos.X - _ownerWindow.Left,
						screenPos.Y - _ownerWindow.Top,
						btn.ActualWidth,
						btn.ActualHeight);
					_dropTargets.Add(new DropTarget { ScreenRect = rect, Button = btn });
				}
				else if (child is ToggleDockButtonGroup grp && grp.IsVisible)
				{
					var screenPos = grp.PointToScreen(new Point(0, 0));
					var rect = new Rect(
						screenPos.X - _ownerWindow.Left,
						screenPos.Y - _ownerWindow.Top,
						grp.ActualWidth,
						grp.ActualHeight);
					_dropTargets.Add(new DropTarget { ScreenRect = rect, Button = grp });
				}

				CollectTargetsRecursive(child);
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
			ReleaseMouseCapture();

			var pos = e.GetPosition(this);
			ToggleButton hitButton = null;
			foreach (var target in _dropTargets)
			{
				if (target.ScreenRect.Contains(pos))
				{
					hitButton = target.Button;
					break;
				}
			}

			Close();

			if (hitButton != null)
			{
				// Merge source into target
				_sourceBar.MergeButtons(hitButton, _sourceButton);
			}
		}

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
			// If we lost capture unexpectedly, close
			if (IsVisible)
				Close();
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var mousePos = Mouse.GetPosition(this);

			// Draw drop target indicators
			var normalBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x00, 0x7A, 0xCC));
			var hoverBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x7A, 0xCC));
			var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)), 2);

			_hoveredTargetIndex = -1;
			for (int i = 0; i < _dropTargets.Count; i++)
			{
				var target = _dropTargets[i];
				bool isHovered = target.ScreenRect.Contains(mousePos);
				if (isHovered) _hoveredTargetIndex = i;

				var inflated = target.ScreenRect;
				inflated.Inflate(3, 3);
				dc.DrawRoundedRectangle(
					isHovered ? hoverBrush : normalBrush,
					borderPen,
					inflated,
					3, 3);
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
