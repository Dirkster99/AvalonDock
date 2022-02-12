/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Commands;
using AvalonDock.Layout;
using Microsoft.Windows.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

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
			// TODO IsVisibleChanged
			//SetBinding(SingleContentLayoutItemProperty, new Binding("Model.SinglePane.SelectedContent") { Source = this, Converter = new LayoutItemFromLayoutModelConverter() });
			_model.RootPanel.ChildrenCollectionChanged += RootPanelOnChildrenCollectionChanged;
		}

		private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LayoutDocumentFloatingWindow.RootPanel) && _model.RootPanel == null) InternalClose();
		}

		/// <inheritdoc />
		protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case Win32Helper.WM_NCLBUTTONDOWN: //Left button down on title -> start dragging over docking manager
					if (wParam.ToInt32() == Win32Helper.HT_CAPTION)
					{
						LayoutDocumentPane layoutDocumentPane = _model.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault(p => p.ChildrenCount > 0 && p.SelectedContent != null);
						if (layoutDocumentPane != null)
						{
							layoutDocumentPane.SelectedContent.IsActive = true;
						}

						handled = true;
					}
					break;

				case Win32Helper.WM_NCRBUTTONUP:
					if (wParam.ToInt32() == Win32Helper.HT_CAPTION)
					{
						if (OpenContextMenu())
							handled = true;
						if (_model.Root.Manager.ShowSystemMenu)
							WindowChrome.GetWindowChrome(this).ShowSystemMenu = !handled;
						else
							WindowChrome.GetWindowChrome(this).ShowSystemMenu = false;
					}
					break;

				case Win32Helper.WM_CLOSE:
					if (CloseInitiatedByUser)
					{
						// We want to force the window to go through our standard logic for closing.
						// So, if the window close is initiated outside of our code (such as from the taskbar),
						// we cancel that close and trigger our close logic instead.
						this.CloseWindowCommand.Execute(null);
						handled = true;
					}
					break;
			}
			return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
		}

		/// <inheritdoc />
		protected override void OnClosed(EventArgs e)
		{
			var root = Model.Root;
			// MK sometimes root is null, prevent crash, or should it always be set??
			if (root != null)
			{
				root.Manager.RemoveFloatingWindow(this);
				root.CollectGarbage();
			}
			if (_overlayWindow != null)
			{
				_overlayWindow.Close();
				_overlayWindow = null;
			}
			base.OnClosed(e);
			if (!CloseInitiatedByUser) root?.FloatingWindows.Remove(_model);
			_model.PropertyChanged -= Model_PropertyChanged;
		}

		#endregion Overrides

		#region Private Methods

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
			// TODO
			if (CloseInitiatedByUser && !KeepContentVisibleOnClose)
			{
				e.Cancel = true;
				//_model.Descendents().OfType<LayoutDocument>().ToArray().ForEach<LayoutDocument>((a) => a.Hide());
			}
			base.OnClosing(e);
		}

		bool IOverlayWindowHost.HitTestScreen(Point dragPoint)
		{
			return HitTest(this.TransformToDeviceDPI(dragPoint));
		}

		bool HitTest(Point dragPoint)
		{
			var detectionRect = new Rect(this.PointToScreenDPIWithoutFlowDirection(new Point()), this.TransformActualSizeToAncestor());
			return detectionRect.Contains(dragPoint);
		}

		DockingManager IOverlayWindowHost.Manager => _model.Root.Manager;

		private OverlayWindow _overlayWindow = null;

		private void CreateOverlayWindow()
		{
			if (_overlayWindow == null) _overlayWindow = new OverlayWindow(this);
			_overlayWindow.Owner = Window.GetWindow(this);
			var rectWindow = new Rect(this.PointToScreenDPIWithoutFlowDirection(new Point()), this.TransformActualSizeToAncestor());
			_overlayWindow.Left = rectWindow.Left;
			_overlayWindow.Top = rectWindow.Top;
			_overlayWindow.Width = rectWindow.Width;
			_overlayWindow.Height = rectWindow.Height;
		}

		IOverlayWindow IOverlayWindowHost.ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow)
		{
			CreateOverlayWindow();
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