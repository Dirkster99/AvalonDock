/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AvalonDock.Layout;
using System.Windows.Documents;
using AvalonDock.Themes;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="Window"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Implements an abstraction layer for floating windows that can host other controls
	/// (eg. documents and/or <see cref="LayoutAnchorable"/>).
	/// 
	/// A floating window can be dragged around independently of the <see cref="DockingManager"/>.
	/// </summary>
	/// <seealso cref="Window"/>
	/// <seealso cref="ILayoutControl"/>
	public abstract class LayoutFloatingWindowControl : Window, ILayoutControl
	{
		#region fields
		private ResourceDictionary currentThemeResourceDictionary; // = null
		private bool _isInternalChange; //false
		private ILayoutElement _model;
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
        #endregion fields

		#region Constructors

		static LayoutFloatingWindowControl()
		{
			ContentProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(null, null, CoerceContentValue));
			AllowsTransparencyProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(false));
			ShowInTaskbarProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(false));
		}

		protected LayoutFloatingWindowControl(ILayoutElement model)
		{
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			Closing += OnClosing;
			SizeChanged += OnSizeChanged;
			_model = model;
		}

		protected LayoutFloatingWindowControl(ILayoutElement model, bool isContentImmutable)
		  : this(model)
		{
			IsContentImmutable = isContentImmutable;
		}

		#endregion Constructors

		#region Properties
		/// <summary>
		/// Gets/Sets the X,Y delta between the element being dragged and the
		/// mouse position. The value of this property is used during the drag
		/// cycle to position the dragged item under the mouse pointer.
		/// 
		/// Set this property on initialization to ensure that
		/// the delta between mouse and control being dragged
		/// remains constant.
		/// </summary>
		internal Point DragDelta { get; set; }

		public abstract ILayoutElement Model { get; }

		#region IsContentImmutable

		/// <summary> <see cref="IsContentImmutable"/> dependency property.</summary>
		public static readonly DependencyProperty IsContentImmutableProperty = DependencyProperty.Register(nameof(IsContentImmutable), typeof(bool), typeof(LayoutFloatingWindowControl),
				  new FrameworkPropertyMetadata(false));

		/// <summary>
		/// Gets/sets the <see cref="IsContentImmutable"/> property.  This dependency property 
		/// indicates if the content can be modified.
		/// </summary>
		public bool IsContentImmutable
		{
			get => (bool)GetValue(IsContentImmutableProperty);
			private set => SetValue(IsContentImmutableProperty, value);
		}

		#endregion IsContentImmutable

		#region IsDragging

		/// <summary><see cref="IsDragging"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey IsDraggingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsDragging), typeof(bool), typeof(LayoutFloatingWindowControl),
				new FrameworkPropertyMetadata(false, OnIsDraggingChanged));

		public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the <see cref="IsDragging"/> property. This dependency property 
		/// indicates that this floating window is being dragged.
		/// </summary>
		public bool IsDragging => (bool)GetValue(IsDraggingProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="IsDragging"/> property.  
		/// This dependency property indicates that this floating window is being dragged.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetIsDragging(bool value) => SetValue(IsDraggingPropertyKey, value);

		/// <summary>Handles changes to the <see cref="IsDragging"/> property.</summary>
		private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutFloatingWindowControl)d).OnIsDraggingChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="IsDragging"/> property.</summary>
		protected virtual void OnIsDraggingChanged(DependencyPropertyChangedEventArgs e)
		{
			if ((bool) e.NewValue)
				CaptureMouse();
			else
				ReleaseMouseCapture();
		}

		#endregion IsDragging

		#region CloseInitiatedByUser

		protected bool CloseInitiatedByUser => !_internalCloseFlag;

		#endregion CloseInitiatedByUser

		internal bool KeepContentVisibleOnClose { get; set; }

		#region IsMaximized

		/// <summary><see cref="IsMaximized"/> dependency property.</summary>
		public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(nameof(IsMaximized), typeof(bool), typeof(LayoutFloatingWindowControl),
				  new FrameworkPropertyMetadata(false));

		/// <summary>Gets/sets the <see cref="IsMaximized"/> property. This dependency property indicates if the window is maximized.</summary>
		/// <remarks>Provides a secure method for setting the <see cref="IsMaximized"/> property.</remarks>
		public bool IsMaximized
		{
			get => (bool)GetValue(IsMaximizedProperty);
			private set
			{
				SetValue(IsMaximizedProperty, value);
				UpdatePositionAndSizeOfPanes();
			}
		}

		/// <inheritdoc />
		protected override void OnStateChanged(EventArgs e)
		{
			if (!_isInternalChange)
				UpdateMaximizedState(WindowState == WindowState.Maximized);

			base.OnStateChanged(e);
		}

		#endregion IsMaximized

        #region TotalMargin

        private static readonly DependencyPropertyKey TotalMarginPropertyKey = 
            DependencyProperty.RegisterReadOnly(nameof(TotalMargin),
                typeof(Thickness), 
                typeof(LayoutFloatingWindowControl),
                new FrameworkPropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty TotalMarginProperty = TotalMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// The total margin (including window chrome and title bar).
        /// 
        /// The margin is queried from the visual tree the first time it is rendered, zero until the first call of FilterMessage(WM_ACTIVATE)
        /// </summary>
		public Thickness TotalMargin
		{
            get { return (Thickness) GetValue(TotalMarginProperty); }
            protected set { SetValue(TotalMarginPropertyKey, value); }
        }

		#endregion

		#region ContentMinHeight
		public static readonly DependencyPropertyKey ContentMinHeightPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ContentMinHeight), typeof(double), typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ContentMinHeightProperty =
            ContentMinHeightPropertyKey.DependencyProperty;

		/// <summary>
		/// The MinHeight of the content of the window, will be 0 until the window has been rendered, or if the MinHeight is unset for the content
		/// </summary>
		public double ContentMinHeight
        {
            get { return (double) GetValue(ContentMinHeightProperty); }
            set { SetValue(ContentMinHeightPropertyKey, value); }
        }
		#endregion

		#region ContentMinWidth
		public static readonly DependencyPropertyKey ContentMinWidthPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ContentMinWidth), typeof(double), typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ContentMinWidthProperty =
            ContentMinWidthPropertyKey.DependencyProperty;

		/// <summary>
		/// The MinWidth ocf the content of the window, will be 0 until the window has been rendered, or if the MinWidth is unset for the content
		/// </summary>
		public double ContentMinWidth
        {
            get { return (double) GetValue(ContentMinWidthProperty); }
            set { SetValue(ContentMinWidthPropertyKey, value); }
        }
		#endregion

		#region SetWindowSizeWhenOpened

        public static readonly DependencyProperty SetWindowSizeWhenOpenedProperty = DependencyProperty.Register(
            nameof(SetWindowSizeWhenOpened), typeof(bool), typeof(LayoutFloatingWindowControl), new PropertyMetadata(false,
                (sender, args) =>
                {
                    if (args.OldValue != args.NewValue &&
                        sender is LayoutFloatingWindowControl control)
                    {
						// This will resize the window when this property is set to true and the window is open
                        control._isTotalMarginSet = false;
                    }
                }));

		/// <summary>
		/// If true the MinHeight and MinWidth of the content will be used together with the margins to determine the initial size of the floating window
		/// </summary>
		/// <seealso cref="TotalMargin"/>
		/// <seealso cref="ContentMinWidth"/>
		/// <seealso cref="ContentMinHeight"/>
		public bool SetWindowSizeWhenOpened
        {
            get { return (bool) GetValue(SetWindowSizeWhenOpenedProperty); }
            set { SetValue(SetWindowSizeWhenOpenedProperty, value); }
        }

        #endregion
		#endregion Properties

		#region Internal Methods

		internal virtual void UpdateThemeResources(Theme oldTheme = null)
		{
			if (oldTheme != null)
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
						Resources.MergedDictionaries.Remove(
							resourceDictionaryToRemove);
				}
			}

			var manager = _model.Root?.Manager;
			if (manager?.Theme == null) return;
			if (manager.Theme is DictionaryTheme dictionaryTheme)
			{
				currentThemeResourceDictionary = dictionaryTheme.ThemeResourceDictionary;
				Resources.MergedDictionaries.Add(currentThemeResourceDictionary);
			}
			else
				Resources.MergedDictionaries.Add(new ResourceDictionary {Source = manager.Theme.GetResourceUri()});
		}

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

		protected virtual IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			handled = false;

			switch (msg)
			{
				case Win32Helper.WM_ACTIVATE:
					if (((int)wParam & 0xFFFF) == Win32Helper.WA_INACTIVE)
					{
						if (lParam == this.GetParentWindowHandle())
						{
							Win32Helper.SetActiveWindow(_hwndSrc.Handle);
							handled = true;
						}
					}
                    UpdateWindowsSizeBasedOnMinSize();

                    break;
				case Win32Helper.WM_EXITSIZEMOVE:
					UpdatePositionAndSizeOfPanes();

					if (_dragService != null)
					{
						var mousePosition = this.TransformToDeviceDPI(Win32Helper.GetMousePosition());
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
				case Win32Helper.WM_LBUTTONUP: //set as handled right button click on title area (after showing context menu)
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
    		if (grid != null && layoutContent.Content is FrameworkElement content)
            {
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
                if(_isTotalMarginSet)
                {
					// The LayoutAnchorableControl is bound via the ContentPresenter, hence it is best to do below in code and not in a style
					// See https://github.com/Dirkster99/AvalonDock/pull/146#issuecomment-609974424
					var layoutContents = this.GetChildrenRecursive()
                        .OfType<ContentPresenter>()
                        .Select(c => c.Content)
                        .OfType<LayoutContent>()
                        .Select(lc => lc.Content);
					var contents = layoutContents.OfType<FrameworkElement>();
                    foreach (var content in contents)
                    {
                        ContentMinHeight = Math.Max(content.MinHeight, ContentMinHeight);
                        ContentMinWidth = Math.Max(content.MinWidth, ContentMinWidth);
                        if (SetWindowSizeWhenOpened)
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
                        }
                    }
                }
            }
        }

        internal void InternalClose(bool closeInitiatedByUser = false)
		{
			_internalCloseFlag = !closeInitiatedByUser;
			if (_isClosing) return;
			_isClosing = true;
			Close();
		}

		#endregion Internal Methods

		#region Overrides

		/// <inheritdoc />
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

		/// <inheritdoc />
		protected override void OnInitialized(EventArgs e)
		{
			CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.CloseWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.CloseWindow((Window)args.Parameter)));
			CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.MaximizeWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow((Window)args.Parameter)));
			CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.MinimizeWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow((Window)args.Parameter)));
			CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.RestoreWindowCommand,
				(s, args) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow((Window)args.Parameter)));
			//Debug.Assert(this.Owner != null);
			base.OnInitialized(e);
		}
		
		#endregion Overrides

		#region Private Methods

		private static object CoerceContentValue(DependencyObject sender, object content)
		{
			if (!(sender is LayoutFloatingWindowControl lfwc)) return null;
			if (lfwc.IsLoaded && lfwc.IsContentImmutable) return lfwc.Content;
			return new FloatingWindowContentHost((LayoutFloatingWindowControl) sender) { Content = content as UIElement };
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnLoaded;
			this.SetParentToMainWindowOf(Model.Root.Manager);
			_hwndSrc = PresentationSource.FromDependencyObject(this) as HwndSource;
			_hwndSrcHook = FilterMessage;
			_hwndSrc.AddHook(_hwndSrcHook);
			// Restore maximize state
			var maximized = Model.Descendents().OfType<ILayoutElementForFloatingWindow>().Any(l => l.IsMaximized);
			UpdateMaximizedState(maximized);
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
			Activated -= OnActivated;

			if (!_attachDrag || Mouse.LeftButton != MouseButtonState.Pressed) return;
			var windowHandle = new WindowInteropHelper(this).Handle;
			var mousePosition = this.PointToScreenDPI(Mouse.GetPosition(this));

			// BugFix Issue #6
			// This code is initializes the drag when content (document or toolwindow) is dragged
			// A second chance back up plan if DragDelta is not set
			if (DragDelta == default) DragDelta = new Point(3, 3);
			Left = mousePosition.X - DragDelta.X;                 // BugFix Issue #6
			Top = mousePosition.Y - DragDelta.Y;
			_attachDrag = false;
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
			WindowState = isMaximized ? WindowState.Maximized : WindowState.Normal;
			_isInternalChange = false;
		}

		private void UpdateDragPosition()
		{
			if (_dragService == null)
			{
				_dragService = new DragService(this);
				SetIsDragging(true);
			}
			var mousePosition = this.TransformToDeviceDPI(Win32Helper.GetMousePosition());
			_dragService.UpdateMouseLocation(mousePosition);
		}

		#endregion Private Methods

		public virtual void EnableBindings()
		{
		}

		public virtual void DisableBindings()
		{
		}

		#region Internal Classes

		protected internal class FloatingWindowContentHost : HwndHost
		{
			#region fields
			private LayoutFloatingWindowControl _owner;
			private HwndSource _wpfContentHost = null;
			private Border _rootPresenter = null;
			private DockingManager _manager = null;
			#endregion fields

			#region Constructors

			public FloatingWindowContentHost(LayoutFloatingWindowControl owner)
			{
				_owner = owner;
				var binding = new Binding(nameof(SizeToContent)) { Source = _owner };
				BindingOperations.SetBinding(this, SizeToContentProperty, binding);
			}

			#endregion Constructors

			#region Properties

			public Visual RootVisual => _rootPresenter;

			#region Content

			/// <summary><see cref="Content"/> dependency property. </summary>
			public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(FloatingWindowContentHost),
					new FrameworkPropertyMetadata(null, OnContentChanged));

			/// <summary>
			/// Gets or sets the <see cref="Content"/> property.  This dependency property 
			/// indicates ....
			/// </summary>
			public UIElement Content
			{
				get => (UIElement)GetValue(ContentProperty);
				set => SetValue(ContentProperty, value);
			}

			/// <summary>Handles changes to the <see cref="Content"/> property.</summary>
			private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((FloatingWindowContentHost)d).OnContentChanged((UIElement)e.OldValue, (UIElement)e.NewValue);

			/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Content"/> property.</summary>
			protected virtual void OnContentChanged(UIElement oldValue, UIElement newValue)
			{
				if (_rootPresenter != null) _rootPresenter.Child = Content;
				if (oldValue is FrameworkElement oldContent) oldContent.SizeChanged -= Content_SizeChanged;
				if (newValue is FrameworkElement newContent) newContent.SizeChanged += Content_SizeChanged;
			}

			#endregion Content

			#region SizeToContent

			/// <summary><see cref="SizeToContent"/> dependency property.</summary>
			public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register(nameof(SizeToContent), typeof(SizeToContent), typeof(FloatingWindowContentHost),
					new FrameworkPropertyMetadata(SizeToContent.Manual, OnSizeToContentChanged));

			/// <summary>Gets or sets the <see cref="SizeToContent"/> property.</summary>
			public SizeToContent SizeToContent
			{
				get => (SizeToContent)GetValue(SizeToContentProperty);
				set => SetValue(SizeToContentProperty, value);
			}

			/// <summary>Handles changes to the <see cref="SizeToContent"/> property.</summary>
			private static void OnSizeToContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((FloatingWindowContentHost)d).OnSizeToContentChanged((SizeToContent)e.OldValue, (SizeToContent)e.NewValue);

			/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="SizeToContent"/> property.</summary>
			protected virtual void OnSizeToContentChanged(SizeToContent oldValue, SizeToContent newValue)
			{
				if (_wpfContentHost != null) _wpfContentHost.SizeToContent = newValue;
			}

			#endregion SizeToContent

			#endregion Properties

			#region Overrides

			/// <inheritdoc />
			protected override HandleRef BuildWindowCore(HandleRef hwndParent)
			{
				_wpfContentHost = new HwndSource(new HwndSourceParameters
				{
					ParentWindow = hwndParent.Handle,
					WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN,
					Width = 1,
					Height = 1
				});

				_rootPresenter = new Border { Child = new AdornerDecorator { Child = Content }, Focusable = true };
				_rootPresenter.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background)) { Source = _owner });
				_wpfContentHost.RootVisual = _rootPresenter;
				_manager = _owner.Model.Root.Manager;
				_manager.InternalAddLogicalChild(_rootPresenter);
				return new HandleRef(this, _wpfContentHost.Handle);
			}

			/// <inheritdoc />
			protected override void DestroyWindowCore(HandleRef hwnd)
			{
				_manager.InternalRemoveLogicalChild(_rootPresenter);
				if (_wpfContentHost == null) return;
				_wpfContentHost.Dispose();
				_wpfContentHost = null;
			}

			/// <inheritdoc />
			protected override Size MeasureOverride(Size constraint)
			{
				if (Content == null) return base.MeasureOverride(constraint);
				Content.Measure(constraint);
				return Content.DesiredSize;
			}

			#endregion Overrides

			#region Methods

			/// <summary>
			/// Content_SizeChanged event handler.
			/// </summary>
			private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
			{
				InvalidateMeasure();
				InvalidateArrange();
			}

			#endregion Methods
		}

		#endregion Internal Classes
	}
}
