using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock.Controls;
using AvalonDock.Core;
using AvalonDock.Layout;

namespace AvalonDock;

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
[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Microsoft.Maintainability",
	"CA1506:AvoidExcessiveClassCoupling",
	Justification = "DockingManager subclass is inherently highly coupled to WPF and layout types.")]
public class ToggleDockingManager : DockingManager
{
	/// <summary>
	/// The layout engine used for all layout tree operations.
	/// </summary>
	private readonly ToggleLayoutEngine _layoutEngine = new ToggleLayoutEngine();

	/// <summary>
	/// Gets the <see cref="ILayoutEngine"/> used by this manager for layout tree operations.
	/// </summary>
	public override ILayoutEngine LayoutEngine => _layoutEngine;

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

	private readonly Dictionary<IToolbox, LayoutAnchorable> _toolboxToAnchorable =
		new Dictionary<IToolbox, LayoutAnchorable>();

	/// <summary>
	/// Key bindings registered on the host window for toolbox shortcuts.
	/// Tracked so they can be removed and rebuilt when toolboxes change.
	/// </summary>
	private readonly List<KeyBinding> _shortcutBindings = new List<KeyBinding>();

	private static readonly KeyGestureConverter _gestureConverter = new KeyGestureConverter();

	private int _syncDepth;

	/// <summary>
	/// The left Separator field.
	/// </summary>
	internal FrameworkElement _leftSeparator;

	/// <summary>
	/// The right Separator field.
	/// </summary>
	internal FrameworkElement _rightSeparator;

	/// <summary>
	/// The button that opens the hidden anchorables menu.
	/// </summary>
	private Button _showHiddenButton;

	private static Style _staticToggleStyle;

	private static Style LoadToggleStyle()
	{
		if (_staticToggleStyle == null)
		{
			var dict = new ResourceDictionary
			{
				Source = new Uri("/AvalonDock;component/Themes/generic.xaml", UriKind.Relative)
			};

			_staticToggleStyle = dict["ToggleAnchorablePaneControlStyle"] as Style;
		}

		return _staticToggleStyle;
	}

	/// <summary>
	/// <see cref="LayoutPriority"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty LayoutPriorityProperty =
		DependencyProperty.Register(
			nameof(LayoutPriority),
			typeof(DockLayoutPriority),
			typeof(ToggleDockingManager),
			new PropertyMetadata(DockLayoutPriority.BottomFullWidth));

	/// <summary>
	/// <see cref="ButtonSize"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty ButtonSizeProperty =
		DependencyProperty.Register(
			nameof(ButtonSize),
			typeof(double),
			typeof(ToggleDockingManager),
			new PropertyMetadata(25.0));

	/// <summary>
	/// <see cref="ShowHeaderMinimizeButton"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty ShowHeaderMinimizeButtonProperty =
		DependencyProperty.Register(
			nameof(ShowHeaderMinimizeButton),
			typeof(bool),
			typeof(ToggleDockingManager),
			new PropertyMetadata(true));

	/// <summary>
	/// <see cref="ShowHeaderOptionsButton"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty ShowHeaderOptionsButtonProperty =
		DependencyProperty.Register(
			nameof(ShowHeaderOptionsButton),
			typeof(bool),
			typeof(ToggleDockingManager),
			new PropertyMetadata(true));

	/// <summary>
	/// <see cref="DefaultDockWidth"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty DefaultDockWidthProperty =
		DependencyProperty.Register(
			nameof(DefaultDockWidth),
			typeof(double),
			typeof(ToggleDockingManager),
			new PropertyMetadata(250.0));

	/// <summary>
	/// <see cref="DefaultDockHeight"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty DefaultDockHeightProperty =
		DependencyProperty.Register(
			nameof(DefaultDockHeight),
			typeof(double),
			typeof(ToggleDockingManager),
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
		DefaultStyleKeyProperty.OverrideMetadata(
			typeof(ToggleDockingManager),
			new FrameworkPropertyMetadata(typeof(ToggleDockingManager)));
		BorderThicknessProperty.OverrideMetadata(
			typeof(ToggleDockingManager),
			new FrameworkPropertyMetadata(new Thickness(0)));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ToggleDockingManager"/> class.
	/// </summary>
	public ToggleDockingManager()
	{
		LayoutUpdateStrategy = new ToggleLayoutStrategy();
		AnchorablePaneControlStyle = LoadToggleStyle();
		Loaded += ToggleDockingManager_Loaded;
		ActiveContentChanged += OnActiveContentChangedForAnchorableState;
	}

	/// <inheritdoc/>
	protected override void OnDockLayoutChanged(Core.IRootDock oldValue, Core.IRootDock newValue)
	{
		base.OnDockLayoutChanged(oldValue, newValue);
		if (IsLoaded)
		{
			SetupToggleDockButtonBars();
		}
	}

	/// <inheritdoc/>
	protected override void OnThemeChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnThemeChanged(e);

		if (IsLoaded)
		{
			// Schedule re-setup after the new theme resources are fully available.
			// OnApplyTemplate may not fire when switching between themes that share
			// the same template structure (e.g. Arc Dark ↔ Arc Light).
			Dispatcher.BeginInvoke(
				System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(ReapplyThemeStyles));
		}
	}

	/// <inheritdoc/>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		// After theme change, re-inject the sidebar buttons into the new template
		if (IsLoaded)
		{
			ReapplyThemeStyles();
		}
	}

	private void ReapplyThemeStyles()
	{
		ApplyToggleAnchorableStyle();
		SetupToggleDockButtonBars();
		RefreshButtonStates();
		Dispatcher.BeginInvoke(
			System.Windows.Threading.DispatcherPriority.Loaded,
			new System.Action(UpdatePinButtonsToMinimize));
	}

	private void ApplyToggleAnchorableStyle()
	{
		var style = TryFindResource("ToggleAnchorablePaneControlStyle") as Style;
		AnchorablePaneControlStyle = style ?? LoadToggleStyle();
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

			// After grouping, ensure bottom zone order is correct
			if (ToggleLayoutEngine.IsBottomZone(zone))
			{
				EnsureBottomZoneOrder();
			}

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
		Dispatcher.BeginInvoke(
			System.Windows.Threading.DispatcherPriority.Loaded,
			new System.Action(UpdatePinButtonsToMinimize));
		SetToolboxIsOpen(anchorable);
	}

	/// <summary>
	/// Executes the move Anchorable To Zone operation.
	/// </summary>
	/// <param name="anchorable">The anchorable.</param>
	/// <param name="targetZone">The target Zone.</param>
	public void MoveAnchorableToZone(LayoutAnchorable anchorable, DockZone targetZone)
	{
		if (anchorable == null)
		{
			return;
		}

		var currentZone = GetAnchorableZone(anchorable);

		// If already in the target zone, just toggle on
		if (currentZone == targetZone)
		{
			if (anchorable.IsAutoHidden)
			{
				ToggleAnchorable(anchorable, targetZone);
			}

			return;
		}

		// Ensure auto-hidden first
		if (!anchorable.IsAutoHidden)
		{
			AutoHideFromDock(anchorable, currentZone);
		}

		// Remove from current layout parent
		if (anchorable.Parent is LayoutAnchorGroup oldGroup)
		{
			oldGroup.RemoveChild(anchorable);
		}

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

			if (anchorable.Content is IToolbox movedToolbox)
			{
				RegisterToolbox(movedToolbox, anchorable);
			}
		}

		// Toggle it on
		ToggleAnchorable(anchorable, targetZone);
	}

	/// <inheritdoc/>
	internal override void ExecuteAutoHideCommand(LayoutAnchorable anchorable)
	{
		if (anchorable == null)
		{
			return;
		}

		var zone = GetAnchorableZone(anchorable);
		ToggleAnchorable(anchorable, zone);
	}

	/// <inheritdoc/>
	internal override void StartDraggingFloatingWindowForPane(LayoutAnchorablePane paneModel)
	{
		if (paneModel == null)
		{
			return;
		}

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
		if (startDrag && contentModel is LayoutAnchorable anchorable)
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
		RefreshShortcuts();
		Dispatcher.BeginInvoke(
			System.Windows.Threading.DispatcherPriority.Loaded,
			new System.Action(UpdatePinButtonsToMinimize));
	}

	private void OpenDefaultToolboxes()
	{
		if (Layout == null)
		{
			return;
		}

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
			// ToggleAnchorablePaneTitle has self-contained buttons wired in its OnApplyTemplate
			if (title is ToggleAnchorablePaneTitle)
			{
				continue;
			}

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
			{
				dd.Visibility = Visibility.Collapsed;
			}

			// Inject three-dot menu button if not already present
			var grid = FindVisualChildren<Grid>(title).FirstOrDefault();
			if (grid != null && !grid.Children.OfType<System.Windows.Controls.Button>()
					.Any(b => b.Name == "PART_ToggleMenu"))
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
				int newCol = grid.ColumnDefinitions.Count - 1;

				// Move minimize button (PART_AutoHidePin) to the last column
				var autoHideBtn = grid.Children.OfType<System.Windows.Controls.Button>()
					.FirstOrDefault(b => b.Name == "PART_AutoHidePin");
				if (autoHideBtn != null)
				{
					Grid.SetColumn(autoHideBtn, newCol);
				}

				// Put three-dot menu in the old auto-hide column (2)
				var menuBtn = CreateThreeDotMenuButton(title);
				Grid.SetColumn(menuBtn, 2);
				grid.Children.Add(menuBtn);
			}
		}
	}

	private System.Windows.Controls.Button CreateThreeDotMenuButton(AnchorablePaneTitle title)
	{
		var ellipsisPath = new System.Windows.Shapes.Path
		{
			Data = Geometry.Parse(
				"M64 360a56 56 0 1 0 0 112 56 56 0 1 0 0-112zm0-160a56 56 0 1 0 0 112 56 56 0 1 0 0-112zM120 96A56 56 0 1 0 8 96a56 56 0 1 0 112 0z"),
			Stretch = Stretch.Uniform,
			Width = 4,
			Height = 14,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		// Bind Fill to the button's inherited Foreground via TextElement property inheritance
		ellipsisPath.SetBinding(
			System.Windows.Shapes.Shape.FillProperty,
			new System.Windows.Data.Binding
			{
				Path = new PropertyPath(System.Windows.Documents.TextElement.ForegroundProperty),
				RelativeSource = System.Windows.Data.RelativeSource.Self,
			});

		var btn = new System.Windows.Controls.Button
		{
			Name = "PART_ToggleMenu",
			Content = ellipsisPath,
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
			if (anchorable == null)
			{
				return;
			}

			var menu = BuildToggleContextMenu(anchorable);
			menu.PlacementTarget = btn;
			menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			menu.IsOpen = true;
		};

		return btn;
	}

	/// <summary>
	/// Builds the toggle context menu for an anchorable.
	/// </summary>
	/// <param name="anchorable">The anchorable to build the menu for.</param>
	/// <returns>The context menu.</returns>
	internal ContextMenu BuildToggleContextMenu(LayoutAnchorable anchorable)
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
			{
				anchorable.ToggleSingleAutoHide();
			}

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
		if (parent == null)
		{
			yield break;
		}

		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i);
			if (child is T t)
			{
				yield return t;
			}

			foreach (var c in FindVisualChildren<T>(child))
			{
				yield return c;
			}
		}
	}

	/// <summary>
	/// Executes the fix Split Orientation operation.
	/// </summary>
	/// <param name="anchorable">The anchorable.</param>
	/// <param name="zone">The zone.</param>
	private void FixSplitOrientation(LayoutAnchorable anchorable, DockZone zone)
	{
		_layoutEngine.FixSplitOrientationForZone(anchorable, zone);
	}

	/// <summary>
	/// Ensures that panes within a bottom horizontal group are ordered
	/// correctly: BottomLeft panes before BottomRight panes.
	/// Uses the button bars to determine each anchorable's zone.
	/// </summary>
	private void EnsureBottomZoneOrder()
	{
		var root = Layout as LayoutRoot;
		if (root?.RootPanel == null)
		{
			return;
		}

		// Find the horizontal group of bottom anchorable panes
		LayoutAnchorablePaneGroup group = null;
		foreach (var child in root.RootPanel.Children)
		{
			if (child is LayoutAnchorablePaneGroup g && g.Orientation == Orientation.Horizontal)
			{
				group = g;
				break;
			}
		}

		if (group == null || group.Children.Count < 2)
		{
			return;
		}

		// Check if any BottomRight pane appears before a BottomLeft pane
		bool needsReorder = false;
		bool seenRight = false;
		foreach (var child in group.Children.OfType<LayoutAnchorablePane>())
		{
			var anc = child.Children.OfType<LayoutAnchorable>().FirstOrDefault();
			if (anc == null)
			{
				continue;
			}

			var zone = GetAnchorableZone(anc);
			if (zone == DockZone.BottomRight)
			{
				seenRight = true;
			}
			else if (zone == DockZone.BottomLeft && seenRight)
			{
				needsReorder = true;
				break;
			}
		}

		if (!needsReorder)
		{
			return;
		}

		// Partition into left and right panes, preserving relative order within each group
		var leftPanes = new List<ILayoutAnchorablePane>();
		var rightPanes = new List<ILayoutAnchorablePane>();
		foreach (var child in group.Children.ToList())
		{
			if (child is LayoutAnchorablePane pane)
			{
				var anc = pane.Children.OfType<LayoutAnchorable>().FirstOrDefault();
				if (anc != null && GetAnchorableZone(anc) == DockZone.BottomRight)
				{
					rightPanes.Add(pane);
				}
				else
				{
					leftPanes.Add(pane);
				}
			}
			else
			{
				leftPanes.Add(child);
			}
		}

		// Rebuild group: all left panes first, then right panes
		while (group.Children.Count > 0)
		{
			group.Children.RemoveAt(group.Children.Count - 1);
		}

		foreach (var p in leftPanes)
		{
			group.Children.Add(p);
		}

		foreach (var p in rightPanes)
		{
			group.Children.Add(p);
		}
	}

	/// <summary>
	/// Executes the ensure Bottom Full Width operation.
	/// </summary>
	private void EnsureBottomFullWidth()
	{
		_layoutEngine.EnsureBottomFullWidth(Layout as LayoutRoot);
	}

	/// <summary>
	/// Executes the ensure Sides Full Height operation.
	/// </summary>
	private void EnsureSidesFullHeight()
	{
		_layoutEngine.EnsureSidesFullHeight(Layout as LayoutRoot);
	}

	/// <summary>
	/// Executes the dock From Auto Hide operation.
	/// </summary>
	/// <param name="anchorable">The anchorable.</param>
	/// <param name="zone">The zone.</param>
	private void DockFromAutoHide(LayoutAnchorable anchorable, DockZone zone)
	{
		var parentGroup = anchorable.Parent as LayoutAnchorGroup;
		if (parentGroup == null)
		{
			return;
		}

		var parentSide = parentGroup.Parent as LayoutAnchorSide;
		if (parentSide == null)
		{
			return;
		}

		var previousContainer = ((ILayoutPreviousContainer)parentGroup).PreviousContainer as LayoutAnchorablePane;
		if (previousContainer != null && previousContainer.Root == null)
		{
			previousContainer = null;
		}

		var root = parentGroup.Root as LayoutRoot;

		if (previousContainer == null)
		{
			var side = ToggleLayoutEngine.ZoneToAnchorSide(zone);
			previousContainer = new LayoutAnchorablePane
			{
				DockMinWidth = anchorable.AutoHideMinWidth,
				DockMinHeight = anchorable.AutoHideMinHeight
			};

			// Apply default dock dimensions from the manager
			if (side == AnchorSide.Left || side == AnchorSide.Right)
			{
				previousContainer.DockWidth = new GridLength(DefaultDockWidth);
			}
			else
			{
				previousContainer.DockHeight = new GridLength(DefaultDockHeight);
			}

			_layoutEngine.InsertPaneForZone(root, previousContainer, zone);
		}
		else
		{
			// Re-insert at the correct zone position to maintain zone ordering
			((ILayoutContainer)previousContainer.Parent)?.RemoveChild(previousContainer);
			_layoutEngine.InsertPaneForZone(root, previousContainer, zone);
		}

		parentGroup.Children.Remove(anchorable);
		previousContainer.Children.Add(anchorable);

		if (parentGroup.Children.Count == 0)
		{
			parentSide.Children.Remove(parentGroup);
		}
	}

	/// <summary>
	/// Executes the auto Hide From Dock operation.
	/// </summary>
	/// <param name="anchorable">The anchorable.</param>
	/// <param name="zone">The zone.</param>
	private void AutoHideFromDock(LayoutAnchorable anchorable, DockZone zone)
	{
		var parentPane = anchorable.Parent as LayoutAnchorablePane;
		if (parentPane == null)
		{
			return;
		}

		var root = anchorable.Root;
		if (root == null)
		{
			return;
		}

		var side = ToggleLayoutEngine.ZoneToAnchorSide(zone);
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
		if (LeftSidePanel != null)
		{
			LeftSidePanel.Visibility = Visibility.Collapsed;
		}

		if (RightSidePanel != null)
		{
			RightSidePanel.Visibility = Visibility.Collapsed;
		}

		if (TopSidePanel != null)
		{
			TopSidePanel.Visibility = Visibility.Collapsed;
		}

		if (BottomSidePanel != null)
		{
			BottomSidePanel.Visibility = Visibility.Collapsed;
		}

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
			{
				leftBottom.Add(anc);
			}
			else
			{
				leftTop.Add(anc);
			}
		}

		var rightTop = new List<LayoutAnchorable>();
		var rightBottom = new List<LayoutAnchorable>();
		foreach (var anc in rightAnchorables)
		{
			if (anc.Content is IToolbox t && t.Zone == DockZone.RightBottom)
			{
				rightBottom.Add(anc);
			}
			else
			{
				rightTop.Add(anc);
			}
		}

		var bottomLeft = new List<LayoutAnchorable>();
		var bottomRight = new List<LayoutAnchorable>();
		foreach (var anc in bottomAnchorables)
		{
			if (anc.Content is IToolbox t && t.Zone == DockZone.BottomRight)
			{
				bottomRight.Add(anc);
			}
			else
			{
				bottomLeft.Add(anc);
			}
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

		RegisterToolboxesFromBars();

		// Inject into the template grid
		var rootGrid = FindTemplateRootGrid();
		if (rootGrid == null)
		{
			return;
		}

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
		leftPanel.Children.Add(CreateShowHiddenButton());
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
		// Unsubscribe from all toolbox PropertyChanged events
		foreach (var toolbox in _toolboxToAnchorable.Keys.ToList())
		{
			UnregisterToolbox(toolbox);
		}

		// Clear icon content on all buttons before removing them from the visual tree.
		// Icon Viewbox instances use RelativeSource.FindAncestor bindings to the button's
		// Foreground. If the Viewbox is re-parented directly to a new button, WPF may not
		// re-resolve those bindings, causing stale colors (e.g. white icon stuck after
		// the focused state is cleared). Clearing IconContent first ensures the Viewbox is
		// properly disconnected so bindings refresh when re-attached.
		ClearBarIconContent(_leftTopBar);
		ClearBarIconContent(_leftBottomBar);
		ClearBarIconContent(_rightTopBar);
		ClearBarIconContent(_rightBottomBar);
		ClearBarIconContent(_bottomLeftBar);
		ClearBarIconContent(_bottomRightBar);

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

	private static void ClearBarIconContent(ToggleDockButtonBar bar)
	{
		if (bar == null)
		{
			return;
		}

		foreach (var item in bar.Items)
		{
			if (item is ToggleDockButton btn)
			{
				btn.IconContent = null;
				btn.IconSource = null;
			}
		}
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
					{
						return grid;
					}
				}
			}

			if (child is Grid g)
			{
				return g;
			}
		}

		return null;
	}

	private static List<LayoutAnchorable> CollectAnchorables(LayoutAnchorSide side)
	{
		var result = new List<LayoutAnchorable>();
		if (side == null)
		{
			return result;
		}

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
		{
			a.ToggleSingleAutoHide();
		}
	}

	/// <summary>
	/// Executes the hide Docked In Bar operation.
	/// </summary>
	/// <param name="bar">The bar.</param>
	private void HideDockedInBar(ToggleDockButtonBar bar)
	{
		if (bar == null)
		{
			return;
		}

		foreach (var item in bar.Items)
		{
			if (item is ToggleDockButton btn && btn.Anchorable != null && !btn.Anchorable.IsAutoHidden)
			{
				AutoHideFromDock(btn.Anchorable, bar.Zone);
			}
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
		if (bar == null)
		{
			return;
		}

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
		if (_leftTopBar?.ContainsAnchorable(anchorable) == true)
		{
			return DockZone.LeftTop;
		}

		if (_leftBottomBar?.ContainsAnchorable(anchorable) == true)
		{
			return DockZone.LeftBottom;
		}

		if (_rightTopBar?.ContainsAnchorable(anchorable) == true)
		{
			return DockZone.RightTop;
		}

		if (_rightBottomBar?.ContainsAnchorable(anchorable) == true)
		{
			return DockZone.RightBottom;
		}

		if (_bottomLeftBar?.ContainsAnchorable(anchorable) == true)
		{
			return DockZone.BottomLeft;
		}

		if (_bottomRightBar?.ContainsAnchorable(anchorable) == true)
		{
			return DockZone.BottomRight;
		}

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

	/// <summary>
	/// Creates the button that opens the hidden anchorables menu.
	/// </summary>
	/// <returns>The show-hidden button, docked to Top.</returns>
	private Button CreateShowHiddenButton()
	{
		_showHiddenButton = new Button
		{
			Width = ButtonSize,
			Height = ButtonSize,
			Padding = new Thickness(0),
			Margin = new Thickness(2),
			Focusable = false,
			ToolTip = "Hidden Panels",
			Template = CreateShowHiddenButtonTemplate(),
		};

		_showHiddenButton.SetResourceReference(Control.ForegroundProperty, ToggleDockButton.ForegroundBrushKey);

		_showHiddenButton.Click += (s, e) =>
		{
			var menu = BuildShowHiddenContextMenu();
			if (menu == null)
			{
				return;
			}

			menu.PlacementTarget = _showHiddenButton;
			menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
			menu.IsOpen = true;
		};

		DockPanel.SetDock(_showHiddenButton, Dock.Top);
		return _showHiddenButton;
	}

	private static ControlTemplate CreateShowHiddenButtonTemplate()
	{
		var template = new ControlTemplate(typeof(Button));

		var borderFactory = new FrameworkElementFactory(typeof(Border), "Bd");
		borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
		borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
		borderFactory.SetValue(UIElement.SnapsToDevicePixelsProperty, true);

		var pathFactory = new FrameworkElementFactory(typeof(System.Windows.Shapes.Path));
		pathFactory.SetValue(
			System.Windows.Shapes.Path.DataProperty,
			Geometry.Parse(
				"M8 256a56 56 0 1 1 112 0A56 56 0 1 1 8 256zm160 0a56 56 0 1 1 112 0 56 56 0 1 1 -112 0zm216-56a56 56 0 1 1 0 112 56 56 0 1 1 0-112z"));
		pathFactory.SetValue(System.Windows.Shapes.Path.StretchProperty, Stretch.Uniform);
		pathFactory.SetValue(FrameworkElement.WidthProperty, 12.0);
		pathFactory.SetValue(FrameworkElement.HeightProperty, 12.0);
		pathFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
		pathFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
		pathFactory.SetBinding(
			System.Windows.Shapes.Path.FillProperty,
			new System.Windows.Data.Binding("Foreground")
			{
				RelativeSource =
					new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
			});

		borderFactory.AppendChild(pathFactory);
		template.VisualTree = borderFactory;

		var hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
		hoverTrigger.Setters.Add(
			new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80)), "Bd"));
		template.Triggers.Add(hoverTrigger);

		return template;
	}

	/// <summary>
	/// Builds a context menu listing all hidden anchorables.
	/// </summary>
	/// <returns>The context menu, or null if no anchorables are hidden.</returns>
	private ContextMenu BuildShowHiddenContextMenu()
	{
		if (Layout?.Hidden == null || Layout.Hidden.Count == 0)
		{
			return null;
		}

		var menu = new ContextMenu();
		foreach (var anchorable in Layout.Hidden.ToList())
		{
			var mi = new MenuItem { Header = anchorable.Title };

			// Try to show the icon
			var attachedIcon = ToggleDock.GetIcon(anchorable);
			if (attachedIcon is FrameworkElement iconElement)
			{
				var rect = new System.Windows.Shapes.Rectangle
				{
					Width = 16,
					Height = 16,
					Fill = new VisualBrush(iconElement) { Stretch = Stretch.Uniform },
				};
				mi.Icon = rect;
			}
			else if (anchorable.IconSource != null)
			{
				mi.Icon = new System.Windows.Controls.Image
				{
					Source = anchorable.IconSource,
					Width = 16,
					Height = 16,
				};
			}

			var anc = anchorable;
			mi.Click += (s, ev) => RestoreHiddenAnchorable(anc);
			menu.Items.Add(mi);
		}

		return menu;
	}

	/// <summary>
	/// Restores a hidden anchorable back to the toggle button bars.
	/// </summary>
	/// <param name="anchorable">The hidden anchorable to restore.</param>
	internal void RestoreHiddenAnchorable(LayoutAnchorable anchorable)
	{
		if (anchorable == null)
		{
			return;
		}

		var zone = DockZone.LeftTop;
		if (anchorable.Content is IToolbox toolbox)
		{
			zone = toolbox.Zone;
		}

		// Remove from Hidden collection
		if (Layout.Hidden.Contains(anchorable))
		{
			Layout.Hidden.Remove(anchorable);
		}

		// Add to the correct layout side as auto-hidden
		var targetSide = GetLayoutSideForZone(zone);
		var group = new LayoutAnchorGroup();
		targetSide.Children.Add(group);
		group.Children.Add(anchorable);

		// Add button to the appropriate bar
		var bar = GetBarForZone(zone);
		if (bar != null)
		{
			var btn = new ToggleDockButton { Anchorable = anchorable, Zone = zone };
			bar.Items.Add(btn);

			if (anchorable.Content is IToolbox restoredToolbox)
			{
				RegisterToolbox(restoredToolbox, anchorable);
			}
		}

		RefreshButtonStates();

		// Immediately open the restored anchorable
		ToggleAnchorable(anchorable, zone);
	}

	/// <summary>
	/// Removes the button for the specified anchorable from all button bars.
	/// </summary>
	/// <param name="anchorable">The anchorable whose button to remove.</param>
	internal void RemoveButtonFromAllBars(LayoutAnchorable anchorable)
	{
		RemoveFromAllBars(anchorable);
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
		if (bar == null)
		{
			return;
		}

		for (int i = bar.Items.Count - 1; i >= 0; i--)
		{
			if (bar.Items[i] is ToggleDockButton btn && btn.Anchorable == anchorable)
			{
				bar.Items.RemoveAt(i);
				return;
			}
		}
	}

	private void OnActiveContentChangedForAnchorableState(object sender, EventArgs e)
	{
		RefreshButtonStates();
	}

	private void RegisterToolboxesFromBars()
	{
		ToggleDockButtonBar[] allBars = { _leftTopBar, _leftBottomBar, _rightTopBar, _rightBottomBar, _bottomLeftBar, _bottomRightBar };
		foreach (var bar in allBars)
		{
			if (bar == null)
			{
				continue;
			}

			foreach (var item in bar.Items)
			{
				if (item is ToggleDockButton btn && btn.Anchorable?.Content is IToolbox toolbox)
				{
					RegisterToolbox(toolbox, btn.Anchorable);
				}
			}
		}
	}

	/// <summary>
	/// Registers a toolbox↔anchorable link: subscribes to the toolbox's PropertyChanged
	/// so the TDM can react when <see cref="IToolbox.IsOpen"/> changes from the MVVM layer.
	/// </summary>
	/// <param name="toolbox">The toolbox to register.</param>
	/// <param name="anchorable">The layout anchorable linked to the toolbox.</param>
	internal void RegisterToolbox(IToolbox toolbox, LayoutAnchorable anchorable)
	{
		if (_toolboxToAnchorable.ContainsKey(toolbox))
		{
			_toolboxToAnchorable[toolbox] = anchorable;
			return;
		}

		_toolboxToAnchorable[toolbox] = anchorable;

		if (toolbox is INotifyPropertyChanged npc)
		{
			npc.PropertyChanged += OnToolboxPropertyChanged;
		}

		RefreshShortcuts();
	}

	/// <summary>
	/// Unregisters a toolbox↔anchorable link and unsubscribes from PropertyChanged.
	/// </summary>
	/// <param name="toolbox">The toolbox to unregister.</param>
	internal void UnregisterToolbox(IToolbox toolbox)
	{
		_toolboxToAnchorable.Remove(toolbox);

		if (toolbox is INotifyPropertyChanged npc)
		{
			npc.PropertyChanged -= OnToolboxPropertyChanged;
		}

		RefreshShortcuts();
	}

	private void OnToolboxPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(IToolbox.IsOpen) || _syncDepth > 0)
		{
			return;
		}

		if (!(sender is IToolbox toolbox) || !_toolboxToAnchorable.TryGetValue(toolbox, out var anchorable))
		{
			return;
		}

		bool wantOpen = toolbox.IsOpen;
		bool isOpen = !anchorable.IsAutoHidden;

		if (wantOpen == isOpen)
		{
			return;
		}

		_syncDepth++;
		try
		{
			if (wantOpen)
			{
				var zone = GetAnchorableZone(anchorable);
				HideDockedInBar(GetBarForZone(zone));
				DockFromAutoHide(anchorable, zone);
				FixSplitOrientation(anchorable, zone);

				if (ToggleLayoutEngine.IsBottomZone(zone))
				{
					EnsureBottomZoneOrder();
				}

				switch (LayoutPriority)
				{
					case DockLayoutPriority.BottomFullWidth:
						EnsureBottomFullWidth();
						break;
					case DockLayoutPriority.SidesFullHeight:
						EnsureSidesFullHeight();
						break;
				}

				ActiveContent = anchorable.Content;
			}
			else
			{
				var zone = GetAnchorableZone(anchorable);
				AutoHideFromDock(anchorable, zone);
			}

			RefreshButtonStates();
			Dispatcher.BeginInvoke(
				System.Windows.Threading.DispatcherPriority.Loaded,
				new System.Action(UpdatePinButtonsToMinimize));
		}
		finally
		{
			_syncDepth--;
		}
	}

	/// <summary>
	/// Rebuilds the keyboard shortcut <see cref="KeyBinding"/>s on the host window from the
	/// <see cref="IToolbox.Shortcut"/> of every registered toolbox. Existing bindings created by
	/// this manager are removed first so the method is safe to call repeatedly. A no-op until the
	/// manager is hosted in a <see cref="Window"/>.
	/// </summary>
	private void RefreshShortcuts()
	{
		var window = Window.GetWindow(this);
		if (window == null)
		{
			return;
		}

		foreach (var binding in _shortcutBindings)
		{
			window.InputBindings.Remove(binding);
		}

		_shortcutBindings.Clear();

		foreach (var pair in _toolboxToAnchorable)
		{
			var gesture = TryParseGesture(pair.Key.Shortcut);
			if (gesture == null)
			{
				continue;
			}

			var binding = new KeyBinding(new ToggleToolboxCommand(this, pair.Key), gesture);
			window.InputBindings.Add(binding);
			_shortcutBindings.Add(binding);
		}
	}

	/// <summary>
	/// Parses a WPF gesture string (e.g. <c>"Ctrl+Shift+E"</c>) into a <see cref="KeyGesture"/>.
	/// Returns <c>null</c> for null/empty/invalid input rather than throwing.
	/// </summary>
	/// <param name="text">The gesture string.</param>
	/// <returns>The parsed gesture, or <c>null</c>.</returns>
	private static KeyGesture TryParseGesture(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}

		try
		{
			return _gestureConverter.ConvertFromInvariantString(text) as KeyGesture;
		}
		catch (NotSupportedException)
		{
			return null;
		}
		catch (FormatException)
		{
			return null;
		}
	}

	/// <summary>
	/// Toggles the docked/auto-hidden state of a toolbox's anchorable, mirroring a click on its
	/// sidebar button. Used as the command target for keyboard shortcut <see cref="KeyBinding"/>s.
	/// </summary>
	private sealed class ToggleToolboxCommand : ICommand
	{
		private readonly ToggleDockingManager _manager;
		private readonly IToolbox _toolbox;

		public ToggleToolboxCommand(ToggleDockingManager manager, IToolbox toolbox)
		{
			_manager = manager;
			_toolbox = toolbox;
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add { }
			remove { }
		}

		public bool CanExecute(object parameter) => true;

		public void Execute(object parameter)
		{
			if (!_manager._toolboxToAnchorable.TryGetValue(_toolbox, out var anchorable))
			{
				return;
			}

			var zone = _manager.GetAnchorableZone(anchorable);
			_manager.ToggleAnchorable(anchorable, zone);
		}
	}

	/// <summary>
	/// Sets <see cref="IToolbox.IsOpen"/> on the single toolbox that was just toggled
	/// in the view layer. Uses <see cref="_syncDepth"/> to prevent re-entrancy.
	/// </summary>
	private void SetToolboxIsOpen(LayoutAnchorable anchorable)
	{
		if (!(anchorable.Content is IToolbox toolbox))
		{
			return;
		}

		_syncDepth++;
		try
		{
			toolbox.IsOpen = !anchorable.IsAutoHidden;
		}
		finally
		{
			_syncDepth--;
		}
	}
}