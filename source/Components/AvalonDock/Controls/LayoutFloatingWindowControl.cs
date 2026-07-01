using System;
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

using AvalonDock.Layout;
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
		private bool _internalCloseFlag = false;
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
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			Closing += OnClosing;
			SizeChanged += OnSizeChanged;
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
						var mousePosition = Win32Helper.GetMousePosition();
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
					if (_dragService != null && Mouse.LeftButton == MouseButtonState.Released)
					{
						_dragService.Abort();
						_dragService = null;
						SetIsDragging(false);
					}

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
			SizeChanged -= OnSizeChanged;
			if (Content != null)
			{
				(Content as FloatingWindowContentHost)?.Dispose();
				if (_hwndSrc != null)
				{
					_hwndSrc.RemoveHook(_hwndSrcHook);
					_hwndSrc.Dispose();
					_hwndSrc = null;
				}
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
			// Debug.Assert(this.Owner != null);
			base.OnInitialized(e);
		}

		/// <inheritdoc/>
		protected override void OnClosing(CancelEventArgs e)
		{
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
			_hwndSrcHook = FilterMessage;
			_hwndSrc.AddHook(_hwndSrcHook);
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
				this.SetParentToMainWindowOf(manager);
			}
			else
			{
				this.SetParentWindowToNull();
			}
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

			var mousePosition = Win32Helper.GetMousePosition();
			_dragService.UpdateMouseLocation(mousePosition);
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
			protected override void DestroyWindowCore(HandleRef hwnd)
			{
				_manager.InternalRemoveLogicalChild(_rootPresenter);
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