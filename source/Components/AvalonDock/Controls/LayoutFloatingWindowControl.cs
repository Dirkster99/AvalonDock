using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using AvalonDock.Layout;
using AvalonDock.Platform;
using AvalonDock.Themes;
using Microsoft.Windows.Shell;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout floating window control.
	/// </summary>
	public abstract class LayoutFloatingWindowControl : Window, ILayoutControl
	{
		private ResourceDictionary currentThemeResourceDictionary; // = null
		private bool _isInternalChange; // false
		private readonly ILayoutElement _model;
		private bool _attachDrag = false;
		private HwndSource _hwndSrc;
		private HwndSourceHook _hwndSrcHook;
		private DragService _dragService = null;

		/// <summary>
		/// The <see cref="DragService"/> driving the current drag of this floating window, or null when
		/// no drag is in progress. Test-only surface (see TestInternalsVisibleTo.cs): lets a test query
		/// the live compass drop-target geometry (<see cref="DragService.CurrentOverlayWindow"/>) by
		/// <see cref="DropTargetType"/> during a real drag, instead of guessing screen offsets.
		/// </summary>
		internal DragService CurrentDragService => _dragService;

		/// <summary>The live OS cursor position, in screen coordinates. Test-only surface used to verify
		/// that this window's position tracks the pointer during a portable caption drag.</summary>
		internal Point CurrentPointerScreenPosition => PlatformHelper.GetCursorPosition();
		internal Point PortableDragOffsetForDiagnostics => _portableDragOffset;
		private bool _internalCloseFlag = false;

		/// <summary>
		/// The window hosting the <see cref="DockingManager"/> that has not been shown yet and whose
		/// <see cref="Window.SourceInitialized"/> event is awaited to establish the ownership (issue #618).
		/// </summary>
		private Window _deferredOwnerWindow;

		/// <summary>
		/// The <see cref="DockingManager"/> whose <see cref="FrameworkElement.Loaded"/> event is awaited
		/// before this floating window is shown (issue #618).
		/// </summary>
		private DockingManager _deferredShowManager;

		/// <summary>
		/// Caches the inheritable dependency properties that are mirrored from the <see cref="DockingManager"/>
		/// onto every floating window.
		/// </summary>
		private static readonly Lazy<DependencyProperty[]> InheritableProperties = new Lazy<DependencyProperty[]>(GetInheritableProperties);

		/// <summary>
		/// Stores the inheritable dependency properties whose value is currently mirrored from the
		/// <see cref="DockingManager"/> onto this floating window.
		/// </summary>
		private readonly HashSet<DependencyProperty> _mirroredInheritedProperties = new HashSet<DependencyProperty>();
		private bool _isClosing = false;

		/// <summary>
		/// Is false until the margins have been found once.
		/// </summary>
		/// <see cref="TotalMargin"/>
		private bool _isTotalMarginSet = false;

		static LayoutFloatingWindowControl()
		{
			AllowsTransparencyProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(false));
			ContentProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(null, null, CoerceContentValue));
			ShowInTaskbarProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutFloatingWindowControl"/> class.
		/// </summary>
		/// <param name="model">The layout model.</param>
		protected LayoutFloatingWindowControl(ILayoutElement model)
		{
			if (UsePortableCaptionDrag)
				WindowChrome.SetIsHitTestVisibleInChrome(this, true);
			Loaded += OnLoaded;
			SourceInitialized += (_, __) => PlatformHelper.CacheNativeWindowHandle(this);
			Unloaded += OnUnloaded;
			Closing += OnClosing;
			SizeChanged += OnSizeChanged;
			if (UsePortableCaptionDrag)
				InputManager.Current.PostProcessInput += OnPortablePostProcessInput;
			_model = model;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutFloatingWindowControl"/> class.
		/// </summary>
		/// <param name="model">The layout model.</param>
		/// <param name="isContentImmutable">The is content immutable.</param>
		protected LayoutFloatingWindowControl(ILayoutElement model, bool isContentImmutable)
		  : this(model)
		{
			IsContentImmutable = isContentImmutable;
		}

		/// <summary>
		/// Gets or sets the drag delta.
		/// </summary>
		internal Point DragDelta { get; set; }

		/// <summary>
		/// Gets the model.
		/// </summary>
		public abstract ILayoutElement Model { get; }

		/// <summary>
		/// <see cref="IsContentImmutable"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsContentImmutableProperty = DependencyProperty.Register(nameof(IsContentImmutable), typeof(bool), typeof(LayoutFloatingWindowControl),
				  new FrameworkPropertyMetadata(false));

		/// <summary>
		/// Gets a value indicating whether this instance is content immutable.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets wether the content can be modified.")]
		[Category("Other")]
		public bool IsContentImmutable
		{
			get => (bool)GetValue(IsContentImmutableProperty);
			private set => SetValue(IsContentImmutableProperty, value);
		}

		/// <summary><see cref="IsDragging"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey IsDraggingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsDragging), typeof(bool), typeof(LayoutFloatingWindowControl),
				new FrameworkPropertyMetadata(false, OnIsDraggingChanged));

		/// <summary>
		/// <see cref="IsDragging"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a value indicating whether this instance is dragging.
		/// </summary>
		[Bindable(true)]
		[Description("Gets wether this floating window is being dragged.")]
		[Category("FloatingWindow")]
		public bool IsDragging => (bool)GetValue(IsDraggingProperty);

		/// <summary>
		/// Sets the is dragging.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetIsDragging(bool value) => SetValue(IsDraggingPropertyKey, value);

		/// <summary>Handles changes to the <see cref="IsDragging"/> property.</summary>
		private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutFloatingWindowControl)d).OnIsDraggingChanged(e);

		/// <summary>
		/// Raises the is dragging changed event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnIsDraggingChanged(DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
				CaptureMouse();
			else
				ReleaseMouseCapture();
		}

		/// <summary>
		/// Gets a value indicating whether the close initiated by user flag is set.
		/// </summary>
		protected bool CloseInitiatedByUser => !_internalCloseFlag;

		/// <summary>
		/// Gets or sets a value indicating whether the keep content visible on close flag is set.
		/// </summary>
		internal bool KeepContentVisibleOnClose { get; set; }

		/// <summary>
		/// <see cref="OwnedByDockingManagerWindow"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty OwnedByDockingManagerWindowProperty =
			DependencyProperty.Register("OwnedByDockingManagerWindow", typeof(bool), typeof(LayoutFloatingWindowControl), new PropertyMetadata(true, OwnedByDockingManagerWindowPropertyChanged));

		/// <summary>
		/// Gets or sets a value indicating whether the owned by docking manager window flag is set.
		/// </summary>
		public bool OwnedByDockingManagerWindow
		{
			get { return (bool)GetValue(OwnedByDockingManagerWindowProperty); }
			set { SetValue(OwnedByDockingManagerWindowProperty, value); }
		}

		private static void OwnedByDockingManagerWindowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is LayoutFloatingWindowControl w && w.IsLoaded)
			{
				w.UpdateOwnership();
			}
		}

		/// <summary>
		/// <see cref="AllowMinimize"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AllowMinimizeProperty =
			DependencyProperty.Register("AllowMinimize", typeof(bool), typeof(LayoutFloatingWindowControl), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets a value indicating whether the allow minimize flag is set.
		/// </summary>
		public bool AllowMinimize
		{
			get { return (bool)GetValue(AllowMinimizeProperty); }
			set { SetValue(AllowMinimizeProperty, value); }
		}

		/// <summary>
		/// <see cref="ResizeBorderThickness"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResizeBorderThicknessProperty =
			DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(LayoutFloatingWindowControl),
				new PropertyMetadata(default(Thickness), OnResizeBorderThicknessChanged));

		/// <summary>
		/// Gets or sets the resize border thickness.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the resize border thickness for this floating window.")]
		[Category("FloatingWindow")]
		public Thickness ResizeBorderThickness
		{
			get { return (Thickness)GetValue(ResizeBorderThicknessProperty); }
			set { SetValue(ResizeBorderThicknessProperty, value); }
		}

		private static void OnResizeBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is LayoutFloatingWindowControl w && w.IsLoaded)
			{
				w.ApplyResizeBorderThickness();
			}
		}

		private void ApplyResizeBorderThickness()
		{
			var thickness = ResizeBorderThickness;
			if (thickness == default(Thickness))
			{
				return;
			}

			var chrome = WindowChrome.GetWindowChrome(this);
			if (chrome != null)
			{
				chrome.ResizeBorderThickness = thickness;
			}
		}

		/// <summary>
		/// <see cref="IsMaximized"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(nameof(IsMaximized), typeof(bool), typeof(LayoutFloatingWindowControl),
						  new FrameworkPropertyMetadata(false));

		/// <summary>
		/// Gets a value indicating whether this instance is maximized.
		/// </summary>
		public bool IsMaximized
		{
			get => (bool)GetValue(IsMaximizedProperty);
			private set
			{
				SetValue(IsMaximizedProperty, value);
				UpdatePositionAndSizeOfPanes();
			}
		}

		/// <inheritdoc/>
		protected override void OnStateChanged(EventArgs e)
		{
			if (!_isInternalChange)
			{
				if (WindowState == WindowState.Maximized)
				{
					// Forward external changes to WindowState from any state to a new Maximized state
					// to the LayoutFloatingWindowControl internal representation.
					UpdateMaximizedState(true);
				}
				else if (IsMaximized && OwnedByDockingManagerWindow)
				{
					// Override any external changes to WindowState when owned and in Maximized state.
					// This override fixes the issue of an owned LayoutFloatingWindowControl loosing
					// its Maximized state when the owner window is restored from a Minimized state.
					WindowState = WindowState.Maximized;
				}
			}

			base.OnStateChanged(e);
		}

		private static readonly DependencyPropertyKey TotalMarginPropertyKey =
			DependencyProperty.RegisterReadOnly(
				nameof(TotalMargin),
				typeof(Thickness),
				typeof(LayoutFloatingWindowControl),
				new FrameworkPropertyMetadata(default(Thickness)));

		/// <summary>
		/// <see cref="TotalMargin"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TotalMarginProperty = TotalMarginPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets or sets the total margin.
		/// </summary>
		public Thickness TotalMargin
		{
			get { return (Thickness)GetValue(TotalMarginProperty); }
			protected set { SetValue(TotalMarginPropertyKey, value); }
		}

		/// <summary>
		/// The content min height property key.
		/// </summary>
		public static readonly DependencyPropertyKey ContentMinHeightPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(ContentMinHeight), typeof(double), typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(0.0));

		/// <summary>
		/// <see cref="ContentMinHeight"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ContentMinHeightProperty =
			ContentMinHeightPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets or sets the content min height.
		/// </summary>
		public double ContentMinHeight
		{
			get { return (double)GetValue(ContentMinHeightProperty); }
			set { SetValue(ContentMinHeightPropertyKey, value); }
		}

		/// <summary>
		/// The content min width property key.
		/// </summary>
		public static readonly DependencyPropertyKey ContentMinWidthPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(ContentMinWidth), typeof(double), typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(0.0));

		/// <summary>
		/// <see cref="ContentMinWidth"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ContentMinWidthProperty =
			ContentMinWidthPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets or sets the content min width.
		/// </summary>
		public double ContentMinWidth
		{
			get { return (double)GetValue(ContentMinWidthProperty); }
			set { SetValue(ContentMinWidthPropertyKey, value); }
		}

		/// <summary>
		/// Updates the theme resources.
		/// </summary>
		/// <param name="oldTheme">The old theme.</param>
		internal virtual void UpdateThemeResources(Theme oldTheme = null)
		{
			if (oldTheme != null) // Remove the old theme if present
			{
				if (oldTheme is DictionaryTheme)
				{
					if (currentThemeResourceDictionary != null)
					{
						Resources.MergedDictionaries.Remove(currentThemeResourceDictionary);
						currentThemeResourceDictionary = null;
					}
				}
				else
				{
					var resourceDictionaryToRemove =
						Resources.MergedDictionaries.FirstOrDefault(r => r.Source == oldTheme.GetResourceUri());
					if (resourceDictionaryToRemove != null)
					{
						Resources.MergedDictionaries.Remove(
							resourceDictionaryToRemove);
					}
				}
			}

			// Implicit parameter to this method is the new theme already set here
			var manager = _model.Root?.Manager;
			if (manager?.Theme == null) return;
			if (manager.Theme is DictionaryTheme dictionaryTheme)
			{
				currentThemeResourceDictionary = dictionaryTheme.ThemeResourceDictionary;
				Resources.MergedDictionaries.Add(currentThemeResourceDictionary);
			}
			else
			{
				Resources.MergedDictionaries.Add(new ResourceDictionary { Source = manager.Theme.GetResourceUri() });
			}
		}

		/// <summary>
		/// Attach drag.
		/// </summary>
		/// <param name="onActivated">The on activated.</param>
		internal void AttachDrag(bool onActivated = true)
		{
			if (onActivated)
			{
				_attachDrag = true;
				Activated += OnActivated;
			}
			else
			{
				var windowHandle = new WindowInteropHelper(this).Handle;
				var lParam = new IntPtr(((int)Left & 0xFFFF) | ((int)Top << 16));
				Win32Helper.SendMessage(windowHandle, Win32Helper.WM_NCLBUTTONDOWN, new IntPtr(Win32Helper.HT_CAPTION), lParam);
			}
		}

		/// <summary>
		/// Filter message.
		/// </summary>
		/// <param name="hwnd">The hwnd.</param>
		/// <param name="msg">The msg.</param>
		/// <param name="wParam">The w param.</param>
		/// <param name="lParam">The l param.</param>
		/// <param name="handled">The handled.</param>
		/// <returns>The filter message.</returns>
		protected virtual IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			handled = false;

			switch (msg)
			{
				case Win32Helper.WM_ACTIVATE:
					UpdateWindowsSizeBasedOnMinSize();
					break;

				case Win32Helper.WM_EXITSIZEMOVE:
					UpdatePositionAndSizeOfPanes();

					if (_dragService != null)
					{
					var mousePosition = PlatformHelper.GetCursorPosition();
					_dragService.Drop(mousePosition, out var dropFlag);
						_dragService = null;
						SetIsDragging(false);
						if (dropFlag) InternalClose();
					}

					break;

				case Win32Helper.WM_MOVING:
					{
						UpdateDragPosition();
						if (IsMaximized) UpdateMaximizedState(false);
					}

					break;

				case Win32Helper.WM_LBUTTONUP: // set as handled right button click on title area (after showing context menu)
					if (_dragService != null && Mouse.LeftButton == MouseButtonState.Released) AbortDrag();

					break;

				case Win32Helper.WM_SYSCOMMAND:
					var command = (int)wParam & 0xFFF0;
					if (command == Win32Helper.SC_MAXIMIZE || command == Win32Helper.SC_RESTORE) UpdateMaximizedState(command == Win32Helper.SC_MAXIMIZE);
					break;
			}

			return IntPtr.Zero;
		}

		/// <summary>
		/// Set the margins of the window control (including the borders of the floating window and the title bar).
		/// The result will be stored in <code>_totalMargin</code>.
		/// </summary>
		/// <remarks>If the control is not loaded <code>_totalMargin</code> will not be set.</remarks>
		private void UpdateMargins()
		{
			// The grid with window bar and content
			var grid = this.GetChildrenRecursive()
				.OfType<Grid>()
				.FirstOrDefault(g => g.RowDefinitions.Count > 0);
			ContentPresenter contentControl = this.GetChildrenRecursive()
				.OfType<ContentPresenter>()
				.FirstOrDefault(c => c.Content is LayoutContent);
			if (contentControl == null)
				return;
			// The content control in the grid, this has a different tree to walk up
			var layoutContent = (LayoutContent)contentControl.Content;
			if (grid != null)
			{
				FrameworkElement content = null;
				var contentObj = layoutContent.Content;

				if (contentObj is ILayoutContentElement layoutContentElement)
				{
					content = layoutContentElement.Content;
				}
				else if (contentObj is FrameworkElement frameworkElement)
				{
					content = frameworkElement;
				}

				if (content == null) return;
				var parents = content.GetParents().ToArray();
				var children = this.GetChildrenRecursive()
					.TakeWhile(c => c != grid)
					.ToArray();
				var borders = children
					.OfType<Border>()
					.Concat(parents
						.OfType<Border>())
					.ToArray();
				var controls = children
					.OfType<Control>()
					.Concat(parents
						.OfType<Control>())
					.ToArray();
				var frameworkElements = children
					.OfType<FrameworkElement>()
					.Concat(parents
						.OfType<FrameworkElement>())
					.ToArray();
				var padding = controls.Sum(b => b.Padding);
				var border = borders.Sum(b => b.BorderThickness);
				var margin = frameworkElements.Sum(f => f.Margin);
				margin = margin.Add(padding).Add(border).Add(grid.Margin);
				margin.Top = grid.RowDefinitions[0].MinHeight;
				TotalMargin = margin;
				_isTotalMarginSet = true;
			}
		}

		/// <summary>
		/// Update the floating window size based on the <code>MinHeight</code> and <code>MinWidth</code> of the content of the control.
		/// </summary>
		/// <remarks>This will only be run once, when the window is rendered the first time and <code>_totalMargin</code> is identified.</remarks>
		private void UpdateWindowsSizeBasedOnMinSize()
		{
			if (!_isTotalMarginSet)
			{
				UpdateMargins();
				if (_isTotalMarginSet)
				{
					// The LayoutAnchorableControl is bound via the ContentPresenter, hence it is best to do below in code and not in a style
					// See https://github.com/Dirkster99/AvalonDock/pull/146#issuecomment-609974424
					var layoutContents = this.GetChildrenRecursive()
						.OfType<ContentPresenter>()
						.Select(c => c.Content)
						.OfType<LayoutContent>()
						.Select(lc => lc.Content);
					var contents = layoutContents.Select(obj => obj is ILayoutContentElement elem
																? elem.Content
																: obj as FrameworkElement)
												 .Where(fe => fe != null);
					foreach (var content in contents)
					{
						ContentMinHeight = Math.Max(content.MinHeight, ContentMinHeight);
						ContentMinWidth = Math.Max(content.MinWidth, ContentMinWidth);
						if ((this.Model?.Root?.Manager?.AutoWindowSizeWhenOpened).GetValueOrDefault())
						{
							var parent = content.GetParents()
								.OfType<FrameworkElement>()
								.FirstOrDefault();
							// StackPanels among others have an ActualHeight larger than visible, hence we check the parent control as well
							if (content.ActualHeight < content.MinHeight ||
								parent != null && parent.ActualHeight < content.MinHeight)
							{
								Height = content.MinHeight + TotalMargin.Top + TotalMargin.Bottom;
							}

							if (content.ActualWidth < content.MinWidth ||
								parent != null && parent.ActualWidth < content.MinWidth)
							{
								Width = content.MinWidth + TotalMargin.Left + TotalMargin.Right;
							}

							if (Height > content.ActualHeight)
							{
								Height = content.ActualHeight + TotalMargin.Top + TotalMargin.Bottom;
							}

							if (Width > content.ActualWidth)
							{
								Width = content.ActualWidth + TotalMargin.Left + TotalMargin.Right;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Internal close.
		/// </summary>
		/// <param name="closeInitiatedByUser">The close initiated by user.</param>
		internal void InternalClose(bool closeInitiatedByUser = false)
		{
			_internalCloseFlag = !closeInitiatedByUser;
			if (_isClosing) return;
			_isClosing = true;
			Close();
		}

		/// <inheritdoc/>
		protected override void OnClosed(EventArgs e)
		{
			CleanupPortableCaptionDrag();
			SizeChanged -= OnSizeChanged;
			if (UsePortableCaptionDrag)
				InputManager.Current.PostProcessInput -= OnPortablePostProcessInput;
			DetachDeferredOwnershipUpdate();
			CancelDeferredShow();

			// A drag that is still running would keep the overlay window of its current host on screen
			// for the rest of the session, because the window that drives the drag is gone (issue #587).
			AbortDrag();

			if (Content is FloatingWindowContentHost contentHost)
			{
				contentHost.Dispose();

				// Closing this window has already destroyed the native window hosting the content, so
				// HwndHost skips DestroyWindowCore and neither the HwndSource nor the logical child that
				// the DockingManager holds would ever be released (issue #587).
				contentHost.ReleaseHostedContent();
			}

			if (_hwndSrc != null)
			{
				_hwndSrc.RemoveHook(_hwndSrcHook);
				_hwndSrc.Dispose();
				_hwndSrc = null;
			}

			base.OnClosed(e);
		}

		/// <inheritdoc/>
		protected override void OnInitialized(EventArgs e)
		{
			CommandBindings.Add(new CommandBinding(
				Microsoft.Windows.Shell.SystemCommands.CloseWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.CloseWindow((Window)args.Parameter)));
			CommandBindings.Add(new CommandBinding(
				Microsoft.Windows.Shell.SystemCommands.MaximizeWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow((Window)args.Parameter)));
			CommandBindings.Add(new CommandBinding(
				Microsoft.Windows.Shell.SystemCommands.MinimizeWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow((Window)args.Parameter)));
			CommandBindings.Add(new CommandBinding(
				Microsoft.Windows.Shell.SystemCommands.RestoreWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow((Window)args.Parameter)));

			// On portable backends WindowChromeWorker is inert, so the borderless custom caption the
			// template draws is never applied and the backend additionally draws its own native title
			// bar - the window ends up with two captions. Worse, dragging the native one runs a native
			// window move that never reaches DragService, so no drop-target compass appears and
			// re-docking is impossible. WindowStyle.None removes it; LibreWPF maps None + CanResize to
			// a hidden-but-resizable border, so the window stays resizable.
			if (UsePortableCaptionDrag &&
				Environment.GetEnvironmentVariable("AVALONDOCK_KEEP_NATIVE_CAPTION") != "1")
			{
				// Remember the caption height the theme declared before detaching the chrome: it
				// defines how tall the draggable caption strip is (see OnPortableCaptionMouseDown).
				var themeChrome = WindowChrome.GetWindowChrome(this);
				if (themeChrome != null && themeChrome.CaptionHeight > 0)
					_portableCaptionHeight = themeChrome.CaptionHeight;

				WindowStyle = WindowStyle.None;

				// WindowChrome spins up a WindowChromeWorker that hooks the HwndSource and issues Win32
				// window operations (regions, frame metrics, NC hit-testing). None of that is meaningful
				// off Windows - the caption is already gone - and leaving it hooked lets it interfere
				// with the portable backend.
				WindowChrome.SetWindowChrome(this, null);
			}

			// Debug.Assert(this.Owner != null);
			base.OnInitialized(e);
		}

		internal void SyncInheritedProperties()
		{
			foreach (var property in InheritableProperties.Value)
				SyncInheritedProperty(property);
		}

		/// <summary>
		/// Mirrors the current value of a single inheritable dependency property of the owning
		/// <see cref="DockingManager"/> onto this floating window.
		/// </summary>
		/// <param name="property">The inheritable dependency property to mirror.</param>
		/// <remarks>
		/// A value that has been assigned to the floating window itself - by a style, a trigger, a binding or by
		/// application code - always wins over the mirrored value and is never overwritten.
		/// </remarks>
		internal void SyncInheritedProperty(DependencyProperty property)
		{
			if (property == null || _isClosing)
				return;

			var manager = Model?.Root?.Manager;
			if (manager == null)
				return;

			if (DependencyPropertyHelper.GetValueSource(manager, property).BaseValueSource == BaseValueSource.Default)
			{
				// The docking manager fell back to the default value, so there is nothing left to mirror.
				if (_mirroredInheritedProperties.Remove(property))
					ClearValue(property);
				return;
			}

			if (!_mirroredInheritedProperties.Contains(property) &&
				DependencyPropertyHelper.GetValueSource(this, property).BaseValueSource > BaseValueSource.Inherited)
			{
				return;
			}

			_mirroredInheritedProperties.Add(property);
			SetValue(property, manager.GetValue(property));
		}

		/// <summary>
		/// Determines the inheritable dependency properties that are mirrored from the
		/// <see cref="DockingManager"/> onto a floating window.
		/// </summary>
		/// <returns>The inheritable dependency properties of a <see cref="LayoutFloatingWindowControl"/>.</returns>
		private static DependencyProperty[] GetInheritableProperties()
		{
			// Attached inheritable properties are not exposed as CLR properties and have to be listed explicitly.
			var properties = new List<DependencyProperty>
			{
				TextOptions.TextFormattingModeProperty,
				TextOptions.TextRenderingModeProperty,
				TextOptions.TextHintingModeProperty,
			};

			foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(typeof(LayoutFloatingWindowControl)))
			{
				if (descriptor.IsReadOnly)
					continue;

				var dependencyProperty = DependencyPropertyDescriptor.FromProperty(descriptor)?.DependencyProperty;
				if (dependencyProperty == null || properties.Contains(dependencyProperty))
					continue;

				if (dependencyProperty.GetMetadata(typeof(LayoutFloatingWindowControl)) is FrameworkPropertyMetadata metadata && metadata.Inherits)
					properties.Add(dependencyProperty);
			}

			return properties.ToArray();
		}

		/// <inheritdoc/>
		protected override void OnClosing(CancelEventArgs e)
		{
			// Stop every callback that can touch the native window before base.OnClosing
			// allows the backend to destroy it.  On macOS an objc_msgSend to a stale
			// NSWindow is a process-fatal access violation and cannot be caught here.
			CleanupPortableCaptionDrag();
			base.OnClosing(e);
			AssureOwnerIsNotMinimized();
		}

		/// <summary>
		/// Prevents a known bug in WPF, which wronlgy minimizes the parent window, when closing this control
		/// </summary>
		private void AssureOwnerIsNotMinimized()
		{
			try
			{
				Owner?.Activate();
			}
			catch (Exception)
			{
			}
		}

		private static object CoerceContentValue(DependencyObject sender, object content)
		{
			if (!(sender is LayoutFloatingWindowControl lfwc)) return null;
			if (lfwc.IsLoaded && lfwc.IsContentImmutable) return lfwc.Content;
			return new FloatingWindowContentHost((LayoutFloatingWindowControl)sender) { Content = content as UIElement };
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnLoaded;

			this.UpdateOwnership();
			ApplyResizeBorderThickness();

			_hwndSrc = PresentationSource.FromDependencyObject(this) as HwndSource;
			if (UsePortableCaptionDrag)
			{
				// Portable backend (LibreWPF on macOS/Linux): the HwndSource is a shim that does not
				// pump the WM_NCLBUTTONDOWN/WM_MOVING/WM_EXITSIZEMOVE modal-move messages the Win32
				// caption drag relies on, and WindowChrome caption dragging is equally inert. Drive the
				// drag engine from a managed caption drag instead (OnPortableCaptionMouseDown).
				AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(OnPortableCaptionMouseDown), handledEventsToo: true);
				AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPortableCaptionMouseDown), handledEventsToo: true);
				AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnPortableCaptionMouseDown), handledEventsToo: true);
				AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPortableCaptionMouseDown), handledEventsToo: true);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					LocationChanged += OnPortableNativeLocationChanged;
			}
			else if (_hwndSrc != null)
			{
				// Win32 path: the modal move loop + WM_MOVING/WM_EXITSIZEMOVE drive the drag engine.
				_hwndSrcHook = FilterMessage;
				_hwndSrc.AddHook(_hwndSrcHook);
			}
			// Restore maximize state
			var maximized = Model.Descendents().OfType<ILayoutElementForFloatingWindow>().Any(l => l.IsMaximized);
			UpdateMaximizedState(maximized);
		}

		/// <summary>
		/// Updates the ownership.
		/// </summary>
		internal void UpdateOwnership()
		{
			// Determine whether the child window should be owned by the parent or act independently
			// according to OwnedByDockingManagerWindow property.
			var manager = Model?.Root?.Manager;
			if (OwnedByDockingManagerWindow && manager != null)
			{
				if (this.SetParentToMainWindowOf(manager))
				{
					DetachDeferredOwnershipUpdate();
				}
				else
				{
					// The window hosting the DockingManager has not been shown yet, so it cannot own this
					// floating window before it has created its native window handle (issue #618).
					DeferOwnershipUpdate(Window.GetWindow(manager));
				}
			}
			else
			{
				DetachDeferredOwnershipUpdate();
				this.SetParentWindowToNull();
			}
		}

		/// <summary>
		/// Retries <see cref="UpdateOwnership"/> as soon as <paramref name="ownerWindow"/> has created its
		/// native window handle, because WPF cannot own a window by a window that has never been shown.
		/// </summary>
		/// <param name="ownerWindow">The window hosting the <see cref="DockingManager"/> of this floating window.</param>
		private void DeferOwnershipUpdate(Window ownerWindow)
		{
			if (ownerWindow == null || ReferenceEquals(ownerWindow, _deferredOwnerWindow))
				return;

			DetachDeferredOwnershipUpdate();
			_deferredOwnerWindow = ownerWindow;
			ownerWindow.SourceInitialized += OnDeferredOwnerWindowSourceInitialized;
		}

		/// <summary>
		/// Stops waiting for the window hosting the <see cref="DockingManager"/> to be shown.
		/// </summary>
		private void DetachDeferredOwnershipUpdate()
		{
			if (_deferredOwnerWindow == null)
				return;

			_deferredOwnerWindow.SourceInitialized -= OnDeferredOwnerWindowSourceInitialized;
			_deferredOwnerWindow = null;
		}

		private void OnDeferredOwnerWindowSourceInitialized(object sender, EventArgs e)
		{
			DetachDeferredOwnershipUpdate();
			if (_isClosing)
				return;

			UpdateOwnership();
		}

		/// <summary>
		/// Shows this floating window, or postpones the operation until the <see cref="DockingManager"/> is
		/// loaded when the window hosting it has not been shown yet.
		/// </summary>
		/// <remarks>
		/// Showing a floating window while the window hosting the <see cref="DockingManager"/> is still
		/// invisible puts a window on screen that has no visible owner, so the operation is postponed until
		/// the <see cref="DockingManager"/> is loaded - the same point in time at which
		/// <see cref="DockingManager"/> creates the floating windows of a layout that was assigned before the
		/// hosting window was shown (issue #618).
		/// </remarks>
		internal void ShowWhenHostWindowIsShown()
		{
			var manager = Model?.Root?.Manager;
			if (manager == null)
			{
				Show();
				return;
			}

			// A DockingManager that is not hosted in a WPF Window - inside a WindowsFormsHost, for example -
			// never gets a hosting window to wait for, so the floating window is shown right away.
			var hostWindow = Window.GetWindow(manager);
			if (hostWindow == null || hostWindow.IsWindowHandleCreated())
			{
				Show();
				return;
			}

			// Establishes the ownership as soon as the hosting window has created its window handle, which
			// happens before the DockingManager is loaded.
			UpdateOwnership();
			DeferShow(manager);
		}

		/// <summary>
		/// Shows this floating window as soon as <paramref name="manager"/> is loaded.
		/// </summary>
		/// <param name="manager">The docking manager owning this floating window.</param>
		private void DeferShow(DockingManager manager)
		{
			if (ReferenceEquals(manager, _deferredShowManager))
				return;

			CancelDeferredShow();
			_deferredShowManager = manager;
			manager.Loaded += OnDeferredShowManagerLoaded;
		}

		/// <summary>
		/// Stops waiting for the <see cref="DockingManager"/> to be loaded.
		/// </summary>
		private void CancelDeferredShow()
		{
			if (_deferredShowManager == null)
				return;

			_deferredShowManager.Loaded -= OnDeferredShowManagerLoaded;
			_deferredShowManager = null;
		}

		private void OnDeferredShowManagerLoaded(object sender, RoutedEventArgs e)
		{
			CancelDeferredShow();
			if (_isClosing)
				return;

			Show();
		}

		private const double KeyboardMoveStep = 10.0;

		/// <inheritdoc/>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);
			var manager = Model?.Root?.Manager;
			if (manager == null || !manager.AllowMovingFloatingWindowWithKeyboard)
			{
				return;
			}

			switch (e.Key)
			{
				case Key.Left:
					Left -= KeyboardMoveStep;
					e.Handled = true;
					break;
				case Key.Right:
					Left += KeyboardMoveStep;
					e.Handled = true;
					break;
				case Key.Up:
					Top -= KeyboardMoveStep;
					e.Handled = true;
					break;
				case Key.Down:
					Top += KeyboardMoveStep;
					e.Handled = true;
					break;
			}
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			Unloaded -= OnUnloaded;
			if (_hwndSrc == null) return;
			_hwndSrc.RemoveHook(_hwndSrcHook);
			InternalClose();
		}

		private void OnClosing(object sender, CancelEventArgs e)
		{
			Closing -= OnClosing;
			// If this window was Closed not from InternalClose method,
			// mark it as closing to avoid "InvalidOperationException: : Cannot set Visibility to Visible or call Show, ShowDialog,
			// Close, or WindowInteropHelper.EnsureHandle while a Window is closing".
			if (!_isClosing) _isClosing = true;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			foreach (var posElement in Model.Descendents().OfType<ILayoutElementForFloatingWindow>())
			{
				posElement.FloatingWidth = ActualWidth;
				posElement.FloatingHeight = ActualHeight;
				posElement.RaiseFloatingPropertiesUpdated();
			}
		}

		private void OnActivated(object sender, EventArgs e)
		{
			InternalOnActivated(sender, e);
		}

		private void InternalOnActivated(object sender, EventArgs e, int retryCount = 0)
		{
			Activated -= OnActivated;

			if (!_attachDrag || Mouse.LeftButton != MouseButtonState.Pressed)
			{
				return;
			}

			var windowHandle = new WindowInteropHelper(this).Handle;

			// Check if the visual is connected to a PresentationSource to avoid InvalidOperationException
			// in multi-DPI scenarios where the window might not be fully initialized yet
			if (PresentationSource.FromVisual(this) == null)
			{
				if (retryCount >= 5)
				{
					// Give up after several retries to avoid infinite loops
					_attachDrag = false;
					return;
				}

				// If not connected, defer the operation until the visual is properly initialized
				Dispatcher.Invoke(
					async () =>
					{
						if (_attachDrag && Mouse.LeftButton == MouseButtonState.Pressed)
						{
							await Task.Delay(10);
							retryCount++;
							InternalOnActivated(sender, e, retryCount);
						}
					}, System.Windows.Threading.DispatcherPriority.Loaded);
				return;
			}

			var mousePosition = this.PointToScreenDPI(Mouse.GetPosition(this));

			var area = this.GetScreenArea();

			// BugFix Issue #6
			// This code is initializes the drag when content (document or toolwindow) is dragged
			// A second chance back up plan if DragDelta is not set
			if (DragDelta == default) DragDelta = new Point(3, 3);
			Left = mousePosition.X - DragDelta.X;                 // BugFix Issue #6
			Top = mousePosition.Y - DragDelta.Y;

			if (this.GetScreenArea().Size != area.Size) // setting the top/left co-ordinates has changed the size - this means moving to a screen with a different DPI. Recalculate mouse position based on new DPI to avoid wrong drag location
			{
				// Ensure the visual is still connected before recalculating mouse position
				if (PresentationSource.FromVisual(this) != null)
				{
					mousePosition = this.PointToScreenDPI(Mouse.GetPosition(this));
					Left = mousePosition.X - DragDelta.X;
					Top = mousePosition.Y - DragDelta.Y;
				}
			}

			_attachDrag = false;
			Show();
			var lParam = new IntPtr(((int)mousePosition.X & 0xFFFF) | ((int)mousePosition.Y << 16));
			Win32Helper.SendMessage(windowHandle, Win32Helper.WM_NCLBUTTONDOWN, new IntPtr(Win32Helper.HT_CAPTION), lParam);
		}

		private void UpdatePositionAndSizeOfPanes()
		{
			foreach (var posElement in Model.Descendents().OfType<ILayoutElementForFloatingWindow>())
			{
				posElement.FloatingLeft = Left;
				posElement.FloatingTop = Top;
				posElement.FloatingWidth = Width;
				posElement.FloatingHeight = Height;
				posElement.RaiseFloatingPropertiesUpdated();
			}
		}

		private void UpdateMaximizedState(bool isMaximized)
		{
			foreach (var posElement in Model.Descendents().OfType<ILayoutElementForFloatingWindow>())
				posElement.IsMaximized = isMaximized;
			IsMaximized = isMaximized;
			_isInternalChange = true;

			if (isMaximized)
			{
				WindowState = WindowState.Maximized;
			}
			else if (!this.AllowMinimize || this.WindowState != WindowState.Minimized)
			{
				// If minimize is not supported, this prevents the window from being minimized.
				// by resetting it to the normal state.
				WindowState = WindowState.Normal;
			}

			_isInternalChange = false;
		}

		private void UpdateDragPosition()
		{
			if (_dragService == null)
			{
				if (Model?.Root?.Manager == null)
					return;
				_dragService = new DragService(this);
				SetIsDragging(true);
			}

			var mousePosition = PlatformHelper.GetCursorPosition();
			_dragService.UpdateMouseLocation(mousePosition);
		}

		#region Portable (non-HWND) caption drag

		// Managed replacement for the Win32 caption drag on backends without an HwndSource (LibreWPF).
		// A press on the caption starts a mouse-captured move: each move repositions the window to
		// follow the pointer and feeds the DragService (which shows the OverlayWindow drop targets),
		// and the release drops onto the current target - the same DragService the WM_MOVING path uses.
		private bool _portableDragging;
		private bool _portableNativeDragging;
		private DispatcherTimer _portableNativeDragTimer;
		private Point _portableDragOffset;   // pointer-to-window-origin offset, in screen coords
		private Point _portableLastPointer;  // last pointer screen position seen during the drag
		private DateTime _portableDragStartUtc;

		// Set when a drag is forced to end (by either watchdog in OnPortableNativeDragTick) while the
		// real mouse button is still physically down. Without this, OnPortableNativeLocationChanged's
		// "button still held" check would immediately treat the still-down button as the start of a
		// brand new native drag the instant this one ends, causing a rapid end/restart storm instead of
		// actually stopping. Cleared only once the button is observed to have actually gone up.
		private bool _suppressNativeDragUntilButtonReleased;

		// No legitimate drag in this app runs anywhere near this long - it exists purely as a backstop
		// against a drag whose end was missed (see OnPortableNativeDragTick), so it can be generous.
		private static readonly TimeSpan MaxPortableDragDuration = TimeSpan.FromSeconds(15);

		// See OnPortableNativeDragTick: synthetic (injected) mouse-downs are not reflected in the
		// physical HID button state immediately, so the button-state watchdog must not run before
		// the injected down has had a chance to reach the WPF input pipeline.
		private static readonly TimeSpan PortableNativeDragGracePeriod = TimeSpan.FromMilliseconds(500);

		// The Win32 caption-drag path (WM_NCLBUTTONDOWN + WM_MOVING/WM_EXITSIZEMOVE) only works on real
		// Windows HWNDs. Everywhere else (LibreWPF on macOS/Linux) use the managed caption drag.
		internal static bool UsePortableCaptionDrag { get; } = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		private void OnPortableNativeLocationChanged(object sender, EventArgs e)
		{
			if (_portableNativeDragging)
			{
				_portableLastPointer = PlatformHelper.GetCursorPosition();
				_dragService?.UpdateMouseLocation(_portableLastPointer);
				return;
			}

			if (_portableDragging || Model?.Root?.Manager == null)
				return;

			// A native title-bar drag can begin before the managed caption receives MouseDown.
			// Only promote native movement while the real left button is still held.
			var buttonDown = Mouse.LeftButton == MouseButtonState.Pressed || PlatformHelper.IsLeftButtonDown();
			if (_suppressNativeDragUntilButtonReleased)
			{
				if (buttonDown)
					return;
				_suppressNativeDragUntilButtonReleased = false;
			}

			if (!buttonDown)
				return;

			_portableDragging = true;
			_portableNativeDragging = true;
			_portableDragStartUtc = DateTime.UtcNow;
			_portableLastPointer = PlatformHelper.GetCursorPosition();
			var nativeOrigin = PlatformHelper.GetWindowContentOrigin(this);
			_portableDragOffset = new Point(_portableLastPointer.X - nativeOrigin.X, _portableLastPointer.Y - nativeOrigin.Y);
			var dragService = new DragService(this);
			_dragService = dragService;
			dragService.UpdateMouseLocation(_portableLastPointer);

			_portableNativeDragTimer ??= new DispatcherTimer(
				TimeSpan.FromMilliseconds(16),
				DispatcherPriority.Input,
				OnPortableNativeDragTick,
				Dispatcher);
			_portableNativeDragTimer.Start();
		}

		private void OnPortableNativeDragTick(object sender, EventArgs e)
		{
			// Watchdog: a native title-bar drag doesn't reliably route its eventual mouse-up back
			// through OnMouseLeftButtonUp/OnPortablePostProcessInput (the OS drove the window move
			// directly, not WPF's input pipeline), so this timer can otherwise keep polling forever
			// after the real drag has ended, using an increasingly stale _floatingWindow/_dragService
			// until something else invalidates them (e.g. a later, unrelated layout change detaches
			// this window's model) and the next tick crashes. Poll the real button state directly and
			// tear the drag down as soon as it's no longer held, regardless of what WPF's routed events
			// did or didn't deliver.
			//
			// Synthetic input (OS-level automation, e.g. cliclick CGEventPost) posts a mouse-down that
			// does NOT update the physical HID button state, so PlatformHelper.IsLeftButtonDown() stays
			// false for the whole drag while WPF's Mouse.LeftButton only becomes Pressed once the
			// injected event has actually been routed. A tick that runs before that routing completes
			// would read "button not down" and kill a perfectly good drag - hence the grace period:
			// give the injected down a moment to reach the WPF input pipeline before trusting the
			// button-state check.
			var dragAge = DateTime.UtcNow - _portableDragStartUtc;
			if (dragAge > PortableNativeDragGracePeriod
				&& Mouse.LeftButton != MouseButtonState.Pressed && !PlatformHelper.IsLeftButtonDown())
			{
				EndPortableDrag(drop: true);
				return;
			}

			// Second, independent backstop: live button-state polling can be fooled if a *different*,
			// later drag's real mouse-down happens to be down at the exact tick this checks (observed in
			// practice with back-to-back synthetic drags in tests). No legitimate drag runs this long, so
			// once exceeded, abort unconditionally rather than trusting button state at all.
			if (dragAge > MaxPortableDragDuration)
			{
				// The button-state check above just confirmed the button still reads as down - ending
				// the drag here must not let OnPortableNativeLocationChanged treat that same still-down
				// button as a new drag starting on the very next call.
				_suppressNativeDragUntilButtonReleased = true;
				EndPortableDrag(drop: false);
				return;
			}

			_portableLastPointer = PlatformHelper.GetCursorPosition();
			PlatformHelper.SetWindowPosition(
				this,
				_portableLastPointer.X - _portableDragOffset.X,
				_portableLastPointer.Y - _portableDragOffset.Y);
			_dragService?.UpdateMouseLocation(_portableLastPointer);
		}

		/// <summary>
		/// Height of the draggable caption strip, taken from the theme's WindowChrome before that
		/// chrome is detached. Presses below it belong to the content, not to the window drag.
		/// </summary>
		private double _portableCaptionHeight = 20;

		/// <summary>Activates the window, ignoring teardown races.</summary>
		private void ActivateWindowForCaptionPress()
		{
			if (IsActive) return;
			try
			{
				Activate();
			}
			catch (InvalidOperationException)
			{
				// Activation can throw while the window is being torn down; never break the drag for it.
			}
		}

		/// <summary>
		/// Marks the floating window's visible content active, which is what drives the pane header's
		/// active styling. Mirrors what AnchorablePaneTitle does on mouse-up, which cannot run while
		/// the caption drag holds mouse capture.
		/// </summary>
		private void ActivateContentForCaptionPress()
		{
			var content = Model?.Descendents().OfType<LayoutContent>().FirstOrDefault(c => c.IsSelected)
				?? Model?.Descendents().OfType<LayoutContent>().FirstOrDefault();

			if (content != null && !content.IsActive)
				content.IsActive = true;
		}

		private void OnPortableCaptionMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_portableDragging)
			{
				e.Handled = true;
				return;
			}
			if (e.ChangedButton != MouseButton.Left) return;
			if (Model?.Root?.Manager == null) return;

			// Only the caption drags the window. Buttons/menus in the title bar opt out of caption
			// treatment via WindowChrome.IsHitTestVisibleInChrome (the same flag the Win32 chrome uses),
			// so walk up from the hit element and bail if any ancestor is marked interactive.
			for (var d = e.OriginalSource as DependencyObject; d != null; d = VisualTreeHelperGetParent(d))
			{
				if (!ReferenceEquals(d, this) && d is UIElement ui &&
					ui.ReadLocalValue(WindowChrome.IsHitTestVisibleInChromeProperty) is true &&
					d is not DropDownControlArea)
					return;
				if (d is System.Windows.Controls.Primitives.ButtonBase)
					return;
			}

			// Only the caption STRIP drags the window. Without this bound every press inside the
			// floating window starts a window drag, so clicking a tab moved the window instead of
			// selecting it, and grabbing mouse capture also stopped the window activating. Win32 gets
			// this bound for free from WindowChrome.CaptionHeight; the managed drag applies it here.
			if (UsePortableCaptionDrag && e.GetPosition(this).Y > _portableCaptionHeight)
				return;

			// Taking mouse capture below suppresses what a press would normally do, in two ways: the
			// window never activates, and - more visibly - the pane header stays in its inactive (grey)
			// state, since that styling is driven by the layout content's IsActive, which
			// AnchorablePaneTitle sets on mouse-up and never sees while this window holds capture.
			ActivateWindowForCaptionPress();
			ActivateContentForCaptionPress();

			var pointer = PointToScreen(e.GetPosition(this));
			var nativeOrigin = PlatformHelper.GetWindowContentOrigin(this);
			_portableDragOffset = new Point(pointer.X - nativeOrigin.X, pointer.Y - nativeOrigin.Y);
			_portableLastPointer = pointer;
			_portableDragging = true;
			_portableDragStartUtc = DateTime.UtcNow;
			if (_dragService == null)
				_dragService = new DragService(this);
			SetIsDragging(true);
			_portableNativeDragTimer ??= new DispatcherTimer(
				TimeSpan.FromMilliseconds(16),
				DispatcherPriority.Input,
				OnPortableNativeDragTick,
				Dispatcher);
			_portableNativeDragTimer.Start();
			CaptureMouse();
			e.Handled = true;
		}

		private static DependencyObject VisualTreeHelperGetParent(DependencyObject d)
		{
			// LogicalTreeHelper for content, VisualTreeHelper for template parts - use visual first.
			return (d is Visual || d is System.Windows.Media.Media3D.Visual3D)
				? VisualTreeHelper.GetParent(d)
				: LogicalTreeHelper.GetParent(d);
		}

		/// <inheritdoc/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!_portableDragging || _portableNativeDragging) return;

			var pointer = PointToScreen(e.GetPosition(this));
			_portableLastPointer = pointer;
			_dragService?.UpdateMouseLocation(pointer);
		}

		/// <summary>
		/// True when a WPF mouse-up must not be treated as the end of a drag because the physical
		/// button is still held down.
		/// <para>
		/// Portable backends deliver spurious mouse-up events mid-press - most visibly when a native
		/// window or popup is shown while the button is down, which is precisely what a drag does when
		/// the drop-target overlay appears. Acting on one ends a drag the user is still performing: the
		/// window stops following the pointer and no drop ever happens. The watchdog in
		/// <see cref="OnPortableNativeDragTick"/> already refuses to trust WPF's button state for this
		/// reason, but it only runs after a grace period, and a phantom up routed through these two
		/// paths kills the drag well before that.
		/// </para>
		/// <para>
		/// The X server's physical button state cannot be fooled by a synthesized WPF event, so it is
		/// the arbiter. A genuine release updates it before the resulting WPF event is routed, so real
		/// mouse-ups still end the drag here; and if this ever misreads, the watchdog remains as a
		/// backstop, so a drag can never be left running indefinitely.
		/// </para>
		/// </summary>
		private static bool IsPhantomMouseUp() => UsePortableCaptionDrag && PlatformHelper.IsLeftButtonDown();

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);
			if (_portableDragging && !IsPhantomMouseUp()) EndPortableDrag(drop: true);
		}

		private void OnPortablePostProcessInput(object sender, ProcessInputEventArgs e)
		{
			if (_portableDragging &&
				e.StagingItem.Input is MouseButtonEventArgs mouseButtonEvent &&
				mouseButtonEvent.RoutedEvent == Mouse.MouseUpEvent &&
				mouseButtonEvent.ChangedButton == MouseButton.Left &&
				!IsPhantomMouseUp())
			{
				_portableLastPointer = PlatformHelper.GetCursorPosition();
				EndPortableDrag(drop: true);
			}
		}

		/// <inheritdoc/>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);
			// Portable backends can lose capture as the pointer crosses native windows. The
			// real button-up still completes the drag, while the timer keeps its position live.
		}

		private void EndPortableDrag(bool drop)
		{
			if (!_portableDragging) return;
			_portableDragging = false;
			_portableNativeDragging = false;
			_portableNativeDragTimer?.Stop();
			if (IsMouseCaptured) ReleaseMouseCapture();

			var dropHandled = false;
			if (_dragService != null)
			{
				if (drop)
					_dragService.Drop(_portableLastPointer, out dropHandled);
				else
					_dragService.Abort();
				_dragService = null;
			}

			SetIsDragging(false);
			if (dropHandled)
				Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => InternalClose()));
		}

		private void CleanupPortableCaptionDrag()
		{
			if (!UsePortableCaptionDrag)
				return;

			_portableNativeDragTimer?.Stop();
			LocationChanged -= OnPortableNativeLocationChanged;
			InputManager.Current.PostProcessInput -= OnPortablePostProcessInput;

			_portableDragging = false;
			_portableNativeDragging = false;
			if (IsMouseCaptured)
				ReleaseMouseCapture();

			_dragService?.Abort();
			_dragService = null;
			SetIsDragging(false);
		}

		internal void CompletePortableDragForDiagnostics()
		{
			EndPortableDrag(drop: true);
		}

		#endregion Portable (non-HWND) caption drag

		/// <summary>
		/// Ends a drag operation that is still in progress without dropping anything.
		/// </summary>
		/// <remarks>
		/// The overlay windows that the drag has put on screen belong to the drop target hosts, not to
		/// this window, so they have to be taken down explicitly whenever a drag ends by any other means
		/// than a regular drop (issue #587).
		/// </remarks>
		private void AbortDrag()
		{
			if (_dragService == null) return;

			var dragService = _dragService;
			_dragService = null;
			dragService.Abort();
			SetIsDragging(false);
		}

		/// <summary>
		/// Enable bindings.
		/// </summary>
		public virtual void EnableBindings()
		{
		}

		/// <summary>
		/// Disable bindings.
		/// </summary>
		public virtual void DisableBindings()
		{
		}

		/// <summary>
		/// Represents the floating window content host.
		/// </summary>
		protected internal class FloatingWindowContentHost : HwndHost
		{
			private readonly LayoutFloatingWindowControl _owner;
			private HwndSource _wpfContentHost = null;
			private Border _rootPresenter = null;
			private DockingManager _manager = null;

			/// <summary>
			/// Initializes a new instance of the <see cref="FloatingWindowContentHost"/> class.
			/// </summary>
			/// <param name="owner">The owner.</param>
			public FloatingWindowContentHost(LayoutFloatingWindowControl owner)
			{
				_owner = owner;
				var binding = new Binding(nameof(SizeToContent)) { Source = _owner };
				BindingOperations.SetBinding(this, SizeToContentProperty, binding);
			}

			/// <summary>
			/// Gets the root visual.
			/// </summary>
			public Visual RootVisual => _rootPresenter;

			/// <summary>
			/// <see cref="Content"/> dependency property.
			/// </summary>
			public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(FloatingWindowContentHost),
					new FrameworkPropertyMetadata(null, OnContentChanged));

			/// <summary>
			/// Gets or sets the content.
			/// </summary>
			public UIElement Content
			{
				get => (UIElement)GetValue(ContentProperty);
				set => SetValue(ContentProperty, value);
			}

			/// <summary>Handles changes to the <see cref="Content"/> property.</summary>
			private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((FloatingWindowContentHost)d).OnContentChanged((UIElement)e.OldValue, (UIElement)e.NewValue);

			/// <summary>
			/// Raises the content changed event.
			/// </summary>
			/// <param name="oldValue">The old value.</param>
			/// <param name="newValue">The new value.</param>
			protected virtual void OnContentChanged(UIElement oldValue, UIElement newValue)
			{
				if (_rootPresenter != null) _rootPresenter.Child = Content;
				if (oldValue is FrameworkElement oldContent) oldContent.SizeChanged -= Content_SizeChanged;
				if (newValue is FrameworkElement newContent) newContent.SizeChanged += Content_SizeChanged;
			}

			/// <summary>
			/// <see cref="SizeToContent"/> dependency property.
			/// </summary>
			public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register(nameof(SizeToContent), typeof(SizeToContent), typeof(FloatingWindowContentHost),
					new FrameworkPropertyMetadata(SizeToContent.Manual, OnSizeToContentChanged));

			/// <summary>
			/// Gets or sets the size to content.
			/// </summary>
			public SizeToContent SizeToContent
			{
				get => (SizeToContent)GetValue(SizeToContentProperty);
				set => SetValue(SizeToContentProperty, value);
			}

			/// <summary>Handles changes to the <see cref="SizeToContent"/> property.</summary>
			private static void OnSizeToContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((FloatingWindowContentHost)d).OnSizeToContentChanged((SizeToContent)e.OldValue, (SizeToContent)e.NewValue);

			/// <summary>
			/// Raises the size to content changed event.
			/// </summary>
			/// <param name="oldValue">The old value.</param>
			/// <param name="newValue">The new value.</param>
			protected virtual void OnSizeToContentChanged(SizeToContent oldValue, SizeToContent newValue)
			{
				if (_wpfContentHost != null) _wpfContentHost.SizeToContent = newValue;
			}

			/// <inheritdoc/>
			protected override HandleRef BuildWindowCore(HandleRef hwndParent)
			{
				// A rebuild must never orphan the native window of a previous build - its HWND would
				// stay alive until the process ends (issue #587).
				ReleaseHostedContent();

				_wpfContentHost = new HwndSource(new HwndSourceParameters
				{
					ParentWindow = hwndParent.Handle,
					WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN,
					Width = 1,
					Height = 1,
					UsesPerPixelOpacity = true,
				});

				_rootPresenter = new Border { Child = new AdornerDecorator { Child = Content }, Focusable = true };
				AutomationProperties.SetName(_rootPresenter, "FloatingWindowHost");
				_rootPresenter.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background)) { Source = _owner });
				_wpfContentHost.RootVisual = _rootPresenter;
				_manager = _owner.Model.Root.Manager;
				_manager.InternalAddLogicalChild(_rootPresenter);
				return new HandleRef(this, _wpfContentHost.Handle);
			}

			/// <inheritdoc/>
			protected override void DestroyWindowCore(HandleRef hwnd) => ReleaseHostedContent();

			/// <summary>
			/// Releases the native window hosting the content of the floating window together with the
			/// logical child that the <see cref="DockingManager"/> keeps on its behalf.
			/// </summary>
			/// <remarks>
			/// <see cref="HwndHost"/> only calls <see cref="DestroyWindowCore"/> while the hosted window is
			/// still alive. Closing a <see cref="Window"/> destroys its native window - and with it every
			/// child window - before <see cref="Window.Closed"/> is raised, so that call is skipped and both
			/// the <see cref="HwndSource"/> and the logical child would survive every floating window for
			/// the rest of the session (issue #587). The clean up is therefore also driven explicitly by
			/// the floating window when it has been closed, and is idempotent.
			/// </remarks>
			internal void ReleaseHostedContent()
			{
				if (_rootPresenter != null)
				{
					_manager?.InternalRemoveLogicalChild(_rootPresenter);
					_rootPresenter = null;
				}

				_manager = null;

				if (_wpfContentHost == null) return;
				_wpfContentHost.Dispose();
				_wpfContentHost = null;
			}

			/// <inheritdoc/>
			protected override Size MeasureOverride(Size constraint)
			{
				if (Content == null) return base.MeasureOverride(constraint);
				Content.Measure(constraint);
				return Content.DesiredSize;
			}

			/// <summary>
			/// Content_SizeChanged event handler.
			/// </summary>
			private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
			{
				InvalidateMeasure();
				InvalidateArrange();
			}
		}
	}
}
