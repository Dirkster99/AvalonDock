/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using AvalonDock.Commands;
using AvalonDock.Layout;

using Microsoft.Windows.Shell;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a floating window control that can host other controls
	/// (<see cref="LayoutAnchorableControl"/>, <see cref="LayoutDocumentControl"/>)
	/// and be dragged (independently of the <see cref="DockingManager"/>) around the screen.
	/// </summary>
	public class LayoutDocumentFloatingWindowControl : LayoutFloatingWindowControl, IOverlayWindowHost
	{
		#region fields

		private readonly LayoutDocumentFloatingWindow _model;
		private List<IDropArea> _dropAreas = null;

		#endregion fields

		#region Constructors

		/// <summary>Static class constructor</summary>
		static LayoutDocumentFloatingWindowControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentFloatingWindowControl), new FrameworkPropertyMetadata(typeof(LayoutDocumentFloatingWindowControl)));
		}

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="model"></param>
		/// <param name="isContentImmutable"></param>
		internal LayoutDocumentFloatingWindowControl(LayoutDocumentFloatingWindow model, bool isContentImmutable)
			: base(model, isContentImmutable)
		{
			_model = model;
			HideWindowCommand = new RelayCommand<object>(OnExecuteHideWindowCommand, CanExecuteHideWindowCommand);
			CloseWindowCommand = new RelayCommand<object>(OnExecuteCloseWindowCommand, CanExecuteCloseWindowCommand);
			Closed += (sender, args) => { Owner?.Focus(); };
			UpdateThemeResources();
		}

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="model"></param>
		internal LayoutDocumentFloatingWindowControl(LayoutDocumentFloatingWindow model)
			: this(model, false)
		{
		}

		#endregion Constructors

		#region Public Methods

		/// <inheritdoc />
		public override void EnableBindings()
		{
			_model.PropertyChanged += Model_PropertyChanged;
			_model.IsVisibleChanged += _model_IsVisibleChanged;
			
			base.EnableBindings();
		}

		/// <inheritdoc />
		public override void DisableBindings()
		{
			_model.PropertyChanged -= Model_PropertyChanged;
			_model.IsVisibleChanged -= _model_IsVisibleChanged;

			base.DisableBindings();
		}

		#endregion
		
		#region Overrides

		/// <inheritdoc />
		public override ILayoutElement Model => _model;

		#region SingleContentLayoutItem

		/// <summary><see cref="SingleContentLayoutItem"/> dependency property.</summary>
		public static readonly DependencyProperty SingleContentLayoutItemProperty = DependencyProperty.Register(nameof(SingleContentLayoutItem), typeof(LayoutItem), typeof(LayoutDocumentFloatingWindowControl),
				new FrameworkPropertyMetadata(null, OnSingleContentLayoutItemChanged));

		/// <summary>
		/// Gets or sets the <see cref="SingleContentLayoutItem"/> property.  This dependency property
		/// indicates the layout item of the selected content when is shown a single document pane.
		/// </summary>
		public LayoutItem SingleContentLayoutItem
		{
			get => (LayoutItem)GetValue(SingleContentLayoutItemProperty);
			set => SetValue(SingleContentLayoutItemProperty, value);
		}

		/// <summary>Handles changes to the <see cref="SingleContentLayoutItem"/> property.</summary>
		private static void OnSingleContentLayoutItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentFloatingWindowControl)d).OnSingleContentLayoutItemChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="SingleContentLayoutItem"/> property.</summary>
		protected virtual void OnSingleContentLayoutItemChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion SingleContentLayoutItem

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			var manager = _model.Root.Manager;
			Content = manager.CreateUIElementForModel(_model.RootPanel);
			EnableBindings();
			//SetBinding(SingleContentLayoutItemProperty, new Binding("Model.SinglePane.SelectedContent") { Source = this, Converter = new LayoutItemFromLayoutModelConverter() });
			_model.RootPanel.ChildrenCollectionChanged += RootPanelOnChildrenCollectionChanged;
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
							if (OpenContextMenu())
								handled = true;

							if (_model.Root.Manager.ShowSystemMenu)
								windowChrome.ShowSystemMenu = !handled;
							else
								windowChrome.ShowSystemMenu = false;
						}
					}
					break;
			}
			return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
		}

		/// <inheritdoc />
		protected override void OnClosed(EventArgs e)
		{
			if (_overlayWindow != null)
			{
				_overlayWindow.Close();
				_overlayWindow = null;
			}
			base.OnClosed(e);
			_model.PropertyChanged -= Model_PropertyChanged;
		}

		#endregion Overrides

		#region Private Methods

		private void _model_IsVisibleChanged(object sender, EventArgs e)
		{
			if (!IsVisible && _model.IsVisible) Show();
		}
		
		private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(LayoutDocumentFloatingWindow.RootPanel):
					if (_model.RootPanel == null) InternalClose();
					break;

				case nameof(LayoutDocumentFloatingWindow.IsVisible):
					if (_model.IsVisible != IsVisible)
					{
						Visibility = _model.IsVisible ? Visibility.Visible : Visibility.Hidden;
					}
					break;
			}
		}
		
		private void RootPanelOnChildrenCollectionChanged(object sender, EventArgs e)
		{
			if (_model.RootPanel == null || _model.RootPanel.Children.Count == 0) InternalClose();
		}

		private bool OpenContextMenu()
		{
			var ctxMenu = _model.Root.Manager.DocumentContextMenu;
			if (ctxMenu == null || SingleContentLayoutItem == null) return false;
			ctxMenu.PlacementTarget = null;
			ctxMenu.Placement = PlacementMode.MousePoint;
			ctxMenu.DataContext = SingleContentLayoutItem;
			ctxMenu.IsOpen = true;
			return true;
		}

		/// <inheritdoc />
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			// Allow base Window class and attached Closing event handlers to potentially cancel first.
			base.OnClosing(e);

			if (e.Cancel) // If already cancelled by base or others, do nothing.
				return;

			// If closed programmatically by AvalonDock (e.g., dragging last doc out), skip user checks.
			if (!CloseInitiatedByUser)
				return;

			// Handle user-initiated close (Taskbar, Alt+F4, Window's 'X' button).
			var manager = Model?.Root?.Manager;
			if (manager == null)
				return;

			var documentsToClose = this.Model.Descendents().OfType<LayoutDocument>().ToArray();
			var anchorablesToProcess = Model.Descendents().OfType<LayoutAnchorable>().ToArray();

			// Phase 1.1: Validate if ALL documents can be closed
			// This checks properties and fires Closing events without actually closing yet.
			// - Check explicit property first.
			// - Check internal LayoutContent.Closing event subscribers.
			// - Check external DockingManager.DocumentClosing event subscribers.
			// - Check command CanExecute.
			var cancelAllFromDocuments = documentsToClose.Any(
				doc => !doc.CanClose 
				       || !doc.TestCanClose() 
				       || !ManagerTestCanClose(doc)
				       || !(manager.GetLayoutItemFromModel(doc)?.CloseCommand?.CanExecute(null) ?? false));
			
			// Phase 1.2: Validate if ALL anchorables can be processed (closed or hidden)
			// Priority: Try Close Path
			// Fallback: Try Hide Path
			// Fallback 2: Cannot Close AND Cannot Hide
			var cancelAllFromAnchorables = anchorablesToProcess.Any(anch =>
			{
				var closeCommand = manager.GetLayoutItemFromModel(anch)?.CloseCommand;
				var hideCommand = (manager.GetLayoutItemFromModel(anch) as LayoutAnchorableItem)?.HideCommand;
				return anch.CanClose && (!anch.TestCanClose() || !ManagerTestCanClose(anch) || closeCommand?.CanExecute(null) is false) ||
				       anch.CanHide && (!anch.TestCanHide() || !ManagerTestCanHide(anch) || hideCommand?.CanExecute(null) is false) ||
				       !anch.CanClose && !anch.CanHide;
			});
			var cancelAll = cancelAllFromDocuments || cancelAllFromAnchorables;
			
			// If any document prevents closing, cancel the window closing.
			if (cancelAll)
			{
				e.Cancel = true;
				return;
			}

			// Phase 2: Execute actions
			foreach (var doc in documentsToClose.ToList())
			{
				var layoutItem = manager.GetLayoutItemFromModel(doc) as LayoutDocumentItem;

				if (layoutItem != null && !layoutItem.IsDefaultCloseCommand)
				{
					// User Custom Command Path
					// Execute the user's command. Assume user handles risks.
					layoutItem.CloseCommand?.Execute(null); // CanExecute already checked in Phase 1
				}
				else
				{
					// Default AvalonDock Logic Path
					// 1. Perform internal close logic (removes from parent, etc.)
					doc.CloseInternal(); // Does NOT raise manager's DocumentClosed event

					// 2. Clean up view/logical tree elements
					if (layoutItem?.IsViewExists() == true)
						manager.InternalRemoveLogicalChild(layoutItem.View);
					
					if (doc.Content is UIElement uiElement)
						manager.InternalRemoveLogicalChild(uiElement);
					
					doc.Content = null; // Final content cleanup

					// 3. Raise the manager's final event
					manager.RaiseDocumentClosed(doc);
				}
			}
			
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
			
			// Window will close naturally as e.Cancel was not set to true.
		}

		/// <summary>
		/// Helper method to check DockingManager's DocumentClosing event for cancellation.
		/// </summary>
		private bool ManagerTestCanClose(LayoutDocument doc)
		{
			var docClosingArgs = new DocumentClosingEventArgs(doc);
			Model?.Root?.Manager.RaiseDocumentClosing(docClosingArgs);
			return !docClosingArgs.Cancel;
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

		DockingManager IOverlayWindowHost.Manager => _model.Root.Manager;

		private OverlayWindow _overlayWindow = null;

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

		IOverlayWindow IOverlayWindowHost.ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow)
		{
			CreateOverlayWindow(draggingWindow);
			_overlayWindow.EnableDropTargets();
			_overlayWindow.Show();
			return _overlayWindow;
		}

		public void HideOverlayWindow()
		{
			_dropAreas = null;
			_overlayWindow.Owner = null;
			_overlayWindow.HideDropTargets();
			_overlayWindow.Close();
			_overlayWindow = null;
		}

		public IEnumerable<IDropArea> GetDropAreas(LayoutFloatingWindowControl draggingWindow)
		{
			if (_dropAreas != null) return _dropAreas;
			_dropAreas = new List<IDropArea>();
			var isDraggingDocuments = draggingWindow.Model is LayoutDocumentFloatingWindow;

			// Determine if floatingWindow is configured to dock as document or not
			var dockAsDocument = true;
			if (!isDraggingDocuments)
			{
				if (draggingWindow.Model is LayoutAnchorableFloatingWindow)
				{
					foreach (var item in GetAnchorableInFloatingWindow(draggingWindow))
					{
						if (item.CanDockAsTabbedDocument != false) continue;
						dockAsDocument = false;
						break;
					}
				}
			}

			var rootVisual = ((FloatingWindowContentHost)Content).RootVisual;

			foreach (var areaHost in rootVisual.FindVisualChildren<LayoutAnchorablePaneControl>())
				_dropAreas.Add(new DropArea<LayoutAnchorablePaneControl>(areaHost, DropAreaType.AnchorablePane));

			if (dockAsDocument)
			{
				foreach (var areaHost in rootVisual.FindVisualChildren<LayoutDocumentPaneControl>())
				{
					if (areaHost is LayoutDocumentPaneControl == true)
						_dropAreas.Add(new DropArea<LayoutDocumentPaneControl>(areaHost, DropAreaType.DocumentPane));
				}
			}

			return _dropAreas;
		}

		/// <summary>
		/// Finds all <see cref="LayoutAnchorable"/> objects (tool windows) within a
		/// <see cref="LayoutFloatingWindow"/> (if any) and return them.
		/// </summary>
		/// <param name="draggingWindow"></param>
		/// <returns></returns>
		private IEnumerable<LayoutAnchorable> GetAnchorableInFloatingWindow(LayoutFloatingWindowControl draggingWindow)
		{
			if (!(draggingWindow.Model is LayoutAnchorableFloatingWindow layoutAnchorableFloatingWindow)) yield break;
			//big part of code for getting type

			if (layoutAnchorableFloatingWindow.SinglePane is LayoutAnchorablePane layoutAnchorablePane && (layoutAnchorableFloatingWindow.IsSinglePane && layoutAnchorablePane.SelectedContent != null))
			{
				var layoutAnchorable = ((LayoutAnchorablePane)layoutAnchorableFloatingWindow.SinglePane).SelectedContent as LayoutAnchorable;
				yield return layoutAnchorable;
			}
			else
				foreach (var item in GetLayoutAnchorable(layoutAnchorableFloatingWindow.RootPanel))
					yield return item;
		}

		/// <summary>
		/// Finds all <see cref="LayoutAnchorable"/> objects (toolwindows) within a
		/// <see cref="LayoutAnchorablePaneGroup"/> (if any) and return them.
		/// </summary>
		/// <param name="layoutAnchPaneGroup"></param>
		/// <returns>All the anchorable items found.</returns>
		/// <seealso cref="LayoutAnchorable"/>
		/// <seealso cref="LayoutAnchorablePaneGroup"/>
		internal IEnumerable<LayoutAnchorable> GetLayoutAnchorable(LayoutAnchorablePaneGroup layoutAnchPaneGroup)
		{
			if (layoutAnchPaneGroup == null) yield break;
			foreach (var anchorable in layoutAnchPaneGroup.Descendents().OfType<LayoutAnchorable>())
				yield return anchorable;
		}

		#region HideWindowCommand

		public ICommand HideWindowCommand { get; }

		private bool CanExecuteHideWindowCommand(object parameter)
		{
			var root = Model?.Root;
			var manager = root?.Manager;
			if (manager == null) return false;

			// TODO check CanHide of anchorables
			var canExecute = false;
			foreach (var content in this.Model.Descendents().OfType<LayoutContent>().ToArray())
			{
				if (content is LayoutAnchorable anchorable && !anchorable.CanHide || !content.CanClose)
				{
					canExecute = false;
					break;
				}

				//if (!(manager.GetLayoutItemFromModel(content) is LayoutAnchorableItem layoutAnchorableItem) ||
				//	 layoutAnchorableItem.HideCommand == null ||
				//	 !layoutAnchorableItem.HideCommand.CanExecute(parameter))
				//{
				//	canExecute = false;
				//	break;
				//}
				if (!(manager.GetLayoutItemFromModel(content) is LayoutItem layoutItem) || layoutItem.CloseCommand == null || !layoutItem.CloseCommand.CanExecute(parameter))
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
			foreach (var anchorable in this.Model.Descendents().OfType<LayoutContent>().ToArray())
			{
				//if (manager.GetLayoutItemFromModel(anchorable) is LayoutAnchorableItem layoutAnchorableItem) layoutAnchorableItem.HideCommand.Execute(parameter);
				//else
				if (manager.GetLayoutItemFromModel(anchorable) is LayoutItem layoutItem) layoutItem.CloseCommand.Execute(parameter);
			}
		}

		#endregion HideWindowCommand

		#region CloseWindowCommand

		public ICommand CloseWindowCommand { get; }

		private bool CanExecuteCloseWindowCommand(object parameter)
		{
			var manager = Model?.Root?.Manager;
			if (manager == null) return false;

			var canExecute = false;
			foreach (var document in this.Model.Descendents().OfType<LayoutDocument>().ToArray())
			{
				if (!document.CanClose)
				{
					canExecute = false;
					break;
				}

				if (!(manager.GetLayoutItemFromModel(document) is LayoutDocumentItem documentLayoutItem) || documentLayoutItem.CloseCommand == null || !documentLayoutItem.CloseCommand.CanExecute(parameter))
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
			foreach (var document in this.Model.Descendents().OfType<LayoutDocument>().ToArray())
			{
				var documentLayoutItem = manager.GetLayoutItemFromModel(document) as LayoutDocumentItem;
				documentLayoutItem?.CloseCommand.Execute(parameter);
			}
		}

		#endregion CloseWindowCommand

		#endregion Private Methods
	}
}
