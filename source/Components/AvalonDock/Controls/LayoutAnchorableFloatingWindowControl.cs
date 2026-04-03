/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

using AvalonDock.Commands;
using AvalonDock.Converters;
using AvalonDock.Layout;

using Microsoft.Windows.Shell;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="LayoutFloatingWindowControl"/>
	/// <inheritdoc cref="IOverlayWindowHost"/>
	/// <summary>
	/// Class visualizes floating <see cref="LayoutAnchorable"/> (toolwindows) in AvalonDock.
	/// </summary>
	/// <seealso cref="LayoutFloatingWindowControl"/>
	/// <seealso cref="IOverlayWindowHost"/>
	public class LayoutAnchorableFloatingWindowControl : LayoutFloatingWindowControl, IOverlayWindowHost
	{
		#region fields

		private readonly LayoutAnchorableFloatingWindow _model;
		private OverlayWindow _overlayWindow = null;
		private List<IDropArea> _dropAreas = null;

		#endregion fields

		#region Constructors

		static LayoutAnchorableFloatingWindowControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorableFloatingWindowControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorableFloatingWindowControl)));
		}

		internal LayoutAnchorableFloatingWindowControl(LayoutAnchorableFloatingWindow model, bool isContentImmutable)
		   : base(model, isContentImmutable)
		{
			_model = model;
			HideWindowCommand = new RelayCommand<object>((p) => OnExecuteHideWindowCommand(p), (p) => CanExecuteHideWindowCommand(p));
			CloseWindowCommand = new RelayCommand<object>((p) => OnExecuteCloseWindowCommand(p), (p) => CanExecuteCloseWindowCommand(p));
			Activated += LayoutAnchorableFloatingWindowControl_Activated;
			UpdateThemeResources();
			MinWidth = _model.RootPanel.CalculatedDockMinWidth();
			MinHeight = _model.RootPanel.CalculatedDockMinHeight();
			if (_model.Root is LayoutRoot root) root.Updated += OnRootUpdated;
			_model.IsVisibleChanged += _model_IsVisibleChanged;
		}

		private void OnRootUpdated(object sender, EventArgs e)
		{
			if (_model?.RootPanel == null) return;
			MinWidth = _model.RootPanel.CalculatedDockMinWidth();
			MinHeight = _model.RootPanel.CalculatedDockMinHeight();
		}

		private void LayoutAnchorableFloatingWindowControl_Activated(object sender, EventArgs e)
		{
			// Issue similar to: http://avalondock.codeplex.com/workitem/15036
			var visibilityBinding = GetBindingExpression(VisibilityProperty);
			if (visibilityBinding == null && Visibility == Visibility.Visible) SetVisibilityBinding();
		}

		internal LayoutAnchorableFloatingWindowControl(LayoutAnchorableFloatingWindow model)
			: this(model, false)
		{
		}

		#endregion Constructors

		#region Properties

		/// <inheritdoc />
		public override ILayoutElement Model => _model;

		#region SingleContentLayoutItem

		/// <summary><see cref="SingleContentLayoutItem"/> dependency property.</summary>
		public static readonly DependencyProperty SingleContentLayoutItemProperty = DependencyProperty.Register(nameof(SingleContentLayoutItem), typeof(LayoutItem), typeof(LayoutAnchorableFloatingWindowControl),
				new FrameworkPropertyMetadata(null, OnSingleContentLayoutItemChanged));

		/// <summary>Gets/sets the layout item of the selected content when shown in a single anchorable pane.</summary>
		[Bindable(true), Description("Gets/sets the layout item of the selected content when shown in a single anchorable pane."), Category("Anchorable")]
		public LayoutItem SingleContentLayoutItem
		{
			get => (LayoutItem)GetValue(SingleContentLayoutItemProperty);
			set => SetValue(SingleContentLayoutItemProperty, value);
		}

		/// <summary>Handles changes to the <see cref="SingleContentLayoutItem"/> property.</summary>
		private static void OnSingleContentLayoutItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableFloatingWindowControl)d).OnSingleContentLayoutItemChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="SingleContentLayoutItem"/> property.</summary>
		protected virtual void OnSingleContentLayoutItemChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion SingleContentLayoutItem

		public ICommand HideWindowCommand { get; }

		public ICommand CloseWindowCommand { get; }

		DockingManager IOverlayWindowHost.Manager => _model.Root.Manager;

		#endregion Properties

		#region Public Methods

		/// <inheritdoc />
		public override void EnableBindings()
		{
			_model.PropertyChanged += _model_PropertyChanged;
			SetVisibilityBinding();
			if (Model.Root is LayoutRoot layoutRoot) layoutRoot.Updated += OnRootUpdated;
			base.EnableBindings();
		}

		/// <inheritdoc />
		public override void DisableBindings()
		{
			if (Model.Root is LayoutRoot layoutRoot) layoutRoot.Updated -= OnRootUpdated;
			BindingOperations.ClearBinding(_model, VisibilityProperty);
			_model.PropertyChanged -= _model_PropertyChanged;
			base.DisableBindings();
		}

		#region IOverlayWindowHost

		bool IOverlayWindowHost.HitTestScreen(Point dragPoint)
		{
			return HitTest(this.TransformToDeviceDPI(dragPoint));
		}

		bool HitTest(Point dragPoint)
		{
			if (dragPoint == default(Point)) 
				return false;
			var detectionRect = new Rect(this.PointToScreenDPIWithoutFlowDirection(new Point()), this.TransformActualSizeToAncestor());
			return detectionRect.Contains(dragPoint);
		}

		void IOverlayWindowHost.HideOverlayWindow()
		{
			_dropAreas = null;
			_overlayWindow.Owner = null;
			_overlayWindow.HideDropTargets();
			_overlayWindow.Close();
			_overlayWindow = null;
		}

		IOverlayWindow IOverlayWindowHost.ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow)
		{
			CreateOverlayWindow(draggingWindow);
			_overlayWindow.EnableDropTargets();
			_overlayWindow.Show();
			return _overlayWindow;
		}

		IEnumerable<IDropArea> IOverlayWindowHost.GetDropAreas(LayoutFloatingWindowControl draggingWindow)
		{
			if (_dropAreas != null) return _dropAreas;
			_dropAreas = new List<IDropArea>();
			if (draggingWindow.Model is LayoutDocumentFloatingWindow) return _dropAreas;
			var rootVisual = (Content as FloatingWindowContentHost).RootVisual;
			foreach (var areaHost in rootVisual.FindVisualChildren<LayoutAnchorablePaneControl>())
				_dropAreas.Add(new DropArea<LayoutAnchorablePaneControl>(areaHost, DropAreaType.AnchorablePane));
			foreach (var areaHost in rootVisual.FindVisualChildren<LayoutDocumentPaneControl>())
				_dropAreas.Add(new DropArea<LayoutDocumentPaneControl>(areaHost, DropAreaType.DocumentPane));
			return _dropAreas;
		}

		#endregion IOverlayWindowHost

		#endregion Public Methods

		#region Overrides

		/// <inheritdoc />
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			var manager = _model.Root.Manager;
			Content = manager.CreateUIElementForModel(_model.RootPanel);
			//SetBinding(VisibilityProperty, new Binding("IsVisible") { Source = _model, Converter = new BoolToVisibilityConverter(), Mode = BindingMode.OneWay, ConverterParameter = Visibility.Hidden });

			//Issue: http://avalondock.codeplex.com/workitem/15036
			IsVisibleChanged += LayoutAnchorableFloatingWindowControl_IsVisibleChanged;
			SetBinding(SingleContentLayoutItemProperty, new Binding("Model.SinglePane.SelectedContent") { Source = this, Converter = new LayoutItemFromLayoutModelConverter() });
			_model.PropertyChanged += _model_PropertyChanged;
		}

		/// <inheritdoc />
		protected override void OnClosed(EventArgs e)
		{
			if (Model.Root is LayoutRoot layoutRoot) layoutRoot.Updated -= OnRootUpdated;
			
			if (_overlayWindow != null)
			{
				_overlayWindow.Close();
				_overlayWindow = null;
			}
			base.OnClosed(e);

			// We have to clear binding instead of creating a new empty binding.
			BindingOperations.ClearBinding(_model, VisibilityProperty);

			_model.PropertyChanged -= _model_PropertyChanged;
			_model.IsVisibleChanged -= _model_IsVisibleChanged;
			Activated -= LayoutAnchorableFloatingWindowControl_Activated;
			IsVisibleChanged -= LayoutAnchorableFloatingWindowControl_IsVisibleChanged;
			BindingOperations.ClearBinding(this, VisibilityProperty);
			BindingOperations.ClearBinding(this, SingleContentLayoutItemProperty);
		}

		/// <inheritdoc />
		protected override void OnClosing(CancelEventArgs e)
		{
			// Allow base Window class and attached Closing event handlers to potentially cancel first.
		    base.OnClosing(e);

		    if (e.Cancel) // If already cancelled by base or others, do nothing.
		        return;

		    // If closed programmatically by AvalonDock (e.g., dragging last anchorable out), skip user checks.
		    if (!CloseInitiatedByUser)
			    return;

		    // Handle user-initiated close (Taskbar, Alt+F4, Window's 'X' button).
		    var manager = Model?.Root?.Manager;
		    if (manager == null)
		        return;

		    var anchorablesToProcess = Model.Descendents().OfType<LayoutAnchorable>().ToArray();
		    
			// Phase 1: Validate if ALL anchorables can be processed (closed or hidden)
		    // This checks properties, internal events, manager events, and command CanExecute
		    // before deciding if the window closing can proceed.
		    // Priority: Try Close Path
		    // - Check internal Closing 
		    // - Check manager AnchorableClosing event
		    // - Check command CanExecute
		    // Fallback: Try Hide Path
		    // - Check internal Hiding
		    // - Check manager AnchorableHiding event
		    // - Check command CanExecute
		    // Fallback 2: Cannot Close AND Cannot Hide
		    var cancelAll = anchorablesToProcess.Any(anch =>
		    {
			    var closeCommand = manager.GetLayoutItemFromModel(anch)?.CloseCommand;
			    var hideCommand = (manager.GetLayoutItemFromModel(anch) as LayoutAnchorableItem)?.HideCommand;
			    return anch.CanClose && (!anch.TestCanClose() || !ManagerTestCanClose(anch) || closeCommand?.CanExecute(null) is false) ||
			           anch.CanHide && (!anch.TestCanHide() || !ManagerTestCanHide(anch) || hideCommand?.CanExecute(null) is false) ||
			           !anch.CanClose && !anch.CanHide;
		    });

		    if (cancelAll)
		    {
			    e.Cancel = true;
			    return;
		    }

			// Phase 2: Execute actions based on the validated priority (Close > Hide) 
            // We use the compromise: execute user command if provided, otherwise execute default logic directly.
            var wasAnyContentHidden = false;
            foreach (var anch in anchorablesToProcess.ToList()) // Use ToList() as actions might modify the underlying collection.
            {
                var layoutItem = manager.GetLayoutItemFromModel(anch) as LayoutAnchorableItem;
                bool useDefaultLogic = layoutItem == null; // Should not happen, but safe default

                if (anch.CanClose) // Priority Action: Close
                {
                    if (!useDefaultLogic) useDefaultLogic = layoutItem.IsDefaultCloseCommand;

                    if (!useDefaultLogic)
                    {
                        // User Custom Command Path
                        layoutItem.CloseCommand?.Execute(null);
                    }
                    else
                    {
                        // Default AvalonDock Logic Path
                        anch.CloseInternal(); // Does NOT raise manager's Closing/Closed events again
                        if (layoutItem?.IsViewExists() == true)
	                        manager.InternalRemoveLogicalChild(layoutItem.View);
                        manager.RaiseAnchorableClosed(anch); // Raise final event
                    }
                }
                else if (anch.CanHide) // Fallback Action: Hide
                {
                     if (!useDefaultLogic) useDefaultLogic = layoutItem.IsDefaultHideCommand;

                    if (!useDefaultLogic)
                    {
                        // User Custom Command Path
                        layoutItem.HideCommand?.Execute(null);
                    }
                    else
                    {
                        // Default AvalonDock Logic Path
                        // Use 'false' to bypass internal cancel checks already done in Phase 1.
                        if (anch.HideAnchorable(false)) // Does NOT raise manager's Hiding/Hidden events again
                        {
                            // View removal for Hide is typically handled by DockingManager logic or CollectGarbage.
                            manager.RaiseAnchorableHidden(anch); // Raise final event
                        }
                    }
                    
                    wasAnyContentHidden = true;
                }
			    // If neither CanClose nor CanHide, do nothing (already validated in Phase 1).
		    }
            
			if (wasAnyContentHidden)
			{
				// Close the window only if all anchorables were closed
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Helper method to check DockingManager's AnchorableClosing event for cancellation.
		/// </summary>
		private bool ManagerTestCanClose(LayoutAnchorable anch)
		{
			var ancClosingArgs = new AnchorableClosingEventArgs(anch);
			Model?.Root?.Manager.RaiseAnchorableClosing(ancClosingArgs);
			return !ancClosingArgs.Cancel;
		}

		/// <summary>
		/// Helper method to check DockingManager's AnchorableHiding event for cancellation,
		/// including the CloseInsteadOfHide request when CanClose is false.
		/// </summary>
		private bool ManagerTestCanHide(LayoutAnchorable anch)
		{
			var hidingArgs = new AnchorableHidingEventArgs(anch);
			Model?.Root?.Manager.RaiseAnchorableHiding(hidingArgs);
			
			// If the Hiding event itself was cancelled, prevent the action.
			if (hidingArgs.Cancel)
				return false; 
			
			// If Hiding requests Close instead, but CanClose is false (which it must be
			// to reach this point), then the requested action cannot be performed, so cancel.
			if (hidingArgs.CloseInsteadOfHide)
			{
				// Log warning maybe? "CloseInsteadOfHide requested for an anchorable where CanClose=false."
				return false;
			}
			
			// Hiding was not cancelled and not replaced by an impossible Close action.
			return true;
		}

		/// <inheritdoc />
		protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case Win32Helper.WM_ACTIVATE:
					var isInactive = ((int)wParam & 0xFFFF) == Win32Helper.WA_INACTIVE;
					if (_model.IsSinglePane)
					{
						LayoutFloatingWindowControlHelper.ActiveTheContentOfSinglePane(this, !isInactive);
					}
					else
					{
						LayoutFloatingWindowControlHelper.ActiveTheContentOfMultiPane(this, !isInactive);
					}

					handled = true;
					break;

				case Win32Helper.WM_NCRBUTTONUP:
					if (wParam.ToInt32() == Win32Helper.HT_CAPTION)
					{
						var windowChrome = WindowChrome.GetWindowChrome(this);
						if (windowChrome != null)
						{
							if (OpenContextMenu()) handled = true;
							windowChrome.ShowSystemMenu = _model.Root.Manager.ShowSystemMenu && !handled;
						}
					}
					break;
			}
			return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
		}

		/// <inheritdoc />
		internal override void UpdateThemeResources(Themes.Theme oldTheme = null)
		{
			base.UpdateThemeResources(oldTheme);
			_overlayWindow?.UpdateThemeResources(oldTheme);
		}

		#endregion Overrides

		#region Private Methods

		private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(LayoutAnchorableFloatingWindow.RootPanel):
					if (_model.RootPanel == null) InternalClose();
					break;

				case nameof(LayoutAnchorableFloatingWindow.IsVisible):
					if (_model.IsVisible != IsVisible)
						Visibility = _model.IsVisible ? Visibility.Visible : Visibility.Hidden;
					break;
			}
		}

		private void _model_IsVisibleChanged(object sender, EventArgs e)
		{
			if (!IsVisible && _model.IsVisible) Show();
		}

		private void CreateOverlayWindow(LayoutFloatingWindowControl draggingWindow)
		{
			if (_overlayWindow == null) _overlayWindow = new OverlayWindow(this);

			// Usually, the overlay window is made a child of the main window. However, if the floating
			// window being dragged isn't also a child of the main window (because OwnedByDockingManagerWindow
			// is set to false to allow the parent window to be minimized independently of floating windows)
			if (draggingWindow?.OwnedByDockingManagerWindow ?? true)
				_overlayWindow.Owner = Window.GetWindow(_model.Root.Manager);
			else
				_overlayWindow.Owner = null;

			var rectWindow = new Rect(this.PointToScreenDPIWithoutFlowDirection(new Point()), this.TransformActualSizeToAncestor());
			_overlayWindow.Left = rectWindow.Left;
			_overlayWindow.Top = rectWindow.Top;
			_overlayWindow.Width = rectWindow.Width;
			_overlayWindow.Height = rectWindow.Height;
		}

		private bool OpenContextMenu()
		{
			var ctxMenu = _model.Root.Manager.AnchorableContextMenu;
			if (ctxMenu == null || SingleContentLayoutItem == null) return false;
			ctxMenu.PlacementTarget = null;
			ctxMenu.Placement = PlacementMode.MousePoint;
			ctxMenu.DataContext = SingleContentLayoutItem;
			ctxMenu.IsOpen = true;
			return true;
		}

		private void SetVisibilityBinding()
		{
			SetBinding(
			  VisibilityProperty,
			  new Binding(nameof(IsVisible))
			  {
				  Source = _model,
				  Converter = new BoolToVisibilityConverter(),
				  Mode = BindingMode.OneWay,
				  ConverterParameter = Visibility.Hidden
			  }
			);
		}

		/// <summary>IsVisibleChanged Event Handler.</summary>
		private void LayoutAnchorableFloatingWindowControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var visibilityBinding = GetBindingExpression(VisibilityProperty);
			if (IsVisible && visibilityBinding == null)
				SetBinding(VisibilityProperty, new Binding(nameof(IsVisible))
				{ Source = _model, Converter = new BoolToVisibilityConverter(), Mode = BindingMode.OneWay, ConverterParameter = Visibility.Hidden });
		}

		#region HideWindowCommand

		private bool CanExecuteHideWindowCommand(object parameter)
		{
			var manager = Model?.Root?.Manager;
			if (manager == null) return false;
			var canExecute = false;
			foreach (var anchorable in Model.Descendents().OfType<LayoutAnchorable>().ToArray())
			{
				if (!anchorable.CanHide)
				{
					canExecute = false;
					break;
				}
				var anchorableLayoutItem = manager.GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
				if (anchorableLayoutItem?.HideCommand == null || !anchorableLayoutItem.HideCommand.CanExecute(parameter))
				{
					canExecute = false;
					break;
				}
				canExecute = true;
			}
			return canExecute;
		}

		private void OnExecuteHideWindowCommand(object parameter)
		{
			var manager = Model.Root.Manager;
			foreach (var anchorable in Model.Descendents().OfType<LayoutAnchorable>().ToArray())
			{
				var anchorableLayoutItem = manager.GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
				anchorableLayoutItem.HideCommand.Execute(parameter);
			}
			Hide(); // Bring toolwindows inside hidden FloatingWindow back requires restart of app
		}

		#endregion HideWindowCommand

		#region CloseWindowCommand

		private bool CanExecuteCloseWindowCommand(object parameter)
		{
			var manager = Model?.Root?.Manager;
			if (manager == null) return false;
			var canExecute = false;
			foreach (var anchorable in Model.Descendents().OfType<LayoutAnchorable>().ToArray())
			{
				if (!anchorable.CanClose)
				{
					canExecute = false;
					break;
				}
				var anchorableLayoutItem = manager.GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
				if (anchorableLayoutItem?.CloseCommand == null || !anchorableLayoutItem.CloseCommand.CanExecute(parameter))
				{
					canExecute = false;
					break;
				}
				canExecute = true;
			}
			return canExecute;
		}

		private void OnExecuteCloseWindowCommand(object parameter)
		{
			var manager = Model.Root.Manager;
			foreach (var anchorable in Model.Descendents().OfType<LayoutAnchorable>().ToArray())
			{
				var anchorableLayoutItem = manager.GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
				anchorableLayoutItem.CloseCommand.Execute(parameter);
			}
		}

		#endregion CloseWindowCommand

		#endregion Private Methods
	}
}
