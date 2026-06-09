/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

// Single shared file compiled into BOTH the WPF build and the Uno port.
// Platform-specific code is guarded by #if HAS_UNO / #if !HAS_UNO.
// WPF-only ghost-resizer overlay lives in LayoutGridControl.wpf.cs (partial).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;
#if HAS_UNO
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls.Primitives;
// Grid API types — identical names in WPF (System.Windows.Controls) and WinUI
// (Microsoft.UI.Xaml.Controls), but the shim doesn't cover Grid/Column/RowDefinition.
using ColumnDefinition = Microsoft.UI.Xaml.Controls.ColumnDefinition;
using RowDefinition     = Microsoft.UI.Xaml.Controls.RowDefinition;
using WinUIGrid         = Microsoft.UI.Xaml.Controls.Grid;
#else
// In WPF, alias WinUIGrid → System.Windows.Controls.Grid so Grid.SetColumn/SetRow
// calls compile identically in both platform builds.
using WinUIGrid = System.Windows.Controls.Grid;
#endif

namespace AvalonDock.Controls
{
	/// <summary>
	/// Abstract base for Grid-based layout panels. Handles column/row definitions,
	/// child wiring, splitter creation, and fixed-panel size adjustment.
	/// Platform differences (async dispatch, cursor, drag events) are handled
	/// via #if HAS_UNO guards; WPF ghost-resizer overlay is in LayoutGridControl.wpf.cs.
	/// </summary>
#if HAS_UNO
	public abstract partial class LayoutGridControl<T> : Microsoft.UI.Xaml.Controls.Grid, ILayoutControl, IAdjustableSizeLayout
#else
	public abstract partial class LayoutGridControl<T> : Grid, ILayoutControl, IAdjustableSizeLayout
#endif
		where T : class, ILayoutPanelElement
	{
		private readonly LayoutPositionableGroup<T> _model;
		private readonly Orientation _orientation;
		private bool _initialized;
		private ChildrenTreeChange? _asyncRefreshCalled;
		private readonly ReentrantFlag _fixingChildrenDockLengths = new ReentrantFlag();
#if HAS_UNO
		// Uno live-resize state (no ghost overlay).
		private LayoutGridResizerControl _activeSplitter;
#endif

		internal LayoutGridControl(LayoutPositionableGroup<T> model, Orientation orientation)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_orientation = orientation;
#if HAS_UNO
			FlowDirection = Microsoft.UI.Xaml.FlowDirection.LeftToRight;
			// Wire async refresh and initial UpdateChildren via DispatcherQueue + Loaded.
			_model.ChildrenTreeChanged += (s, args) =>
			{
				if (args.Change != ChildrenTreeChange.DirectChildrenChanged) return;
				if (_asyncRefreshCalled.HasValue && _asyncRefreshCalled.Value == args.Change) return;
				_asyncRefreshCalled = args.Change;
				DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
				{
					_asyncRefreshCalled = null;
					UpdateChildren();
				});
			};
			Loaded += (_, _) =>
			{
				SizeChanged += OnSizeChanged;
				if (!_initialized && _model.Root?.Manager != null) { _initialized = true; UpdateChildren(); }
			};
#else
			FlowDirection = System.Windows.FlowDirection.LeftToRight;
			Unloaded += OnUnloaded;
#endif
		}

		public ILayoutElement Model => _model;
		public Orientation Orientation => (_model as ILayoutOrientableGroup).Orientation;
		private bool AsyncRefreshCalled => _asyncRefreshCalled != null;

		// ── Platform-neutral resize math (called by platform drag handlers) ────────

		/// <summary>
		/// Apply a splitter drag delta to the adjacent panel models.
		/// Pass the actual pre-drag sizes of the two neighbouring children.
		/// </summary>
		protected void ApplyResizeDelta(
			LayoutGridResizerControl splitter,
			double delta,
			Size prevActualSize,
			Size nextActualSize)
		{
			var idx  = Children.IndexOf(splitter);
			var prev = Children[idx - 1] as FrameworkElement;
			var next = GetNextVisibleChild(idx);
			var prevModel = (ILayoutPositionableElement)(prev as ILayoutControl).Model;
			var nextModel = (ILayoutPositionableElement)(next as ILayoutControl).Model;

			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				prevModel.DockWidth = prevModel.DockWidth.IsStar
					? new GridLength(prevModel.DockWidth.Value * (prevActualSize.Width + delta) / prevActualSize.Width, GridUnitType.Star)
					: new GridLength(Math.Max(0, (prevModel.DockWidth.IsAuto ? prevActualSize.Width : prevModel.DockWidth.Value) + delta), GridUnitType.Pixel);
				nextModel.DockWidth = nextModel.DockWidth.IsStar
					? new GridLength(nextModel.DockWidth.Value * (nextActualSize.Width - delta) / nextActualSize.Width, GridUnitType.Star)
					: new GridLength(Math.Max(0, (nextModel.DockWidth.IsAuto ? nextActualSize.Width : nextModel.DockWidth.Value) - delta), GridUnitType.Pixel);
			}
			else
			{
				prevModel.DockHeight = prevModel.DockHeight.IsStar
					? new GridLength(prevModel.DockHeight.Value * (prevActualSize.Height + delta) / prevActualSize.Height, GridUnitType.Star)
					: new GridLength(Math.Max(0, (prevModel.DockHeight.IsAuto ? prevActualSize.Height : prevModel.DockHeight.Value) + delta), GridUnitType.Pixel);
				nextModel.DockHeight = nextModel.DockHeight.IsStar
					? new GridLength(nextModel.DockHeight.Value * (nextActualSize.Height - delta) / nextActualSize.Height, GridUnitType.Star)
					: new GridLength(Math.Max(0, (nextModel.DockHeight.IsAuto ? nextActualSize.Height : nextModel.DockHeight.Value) - delta), GridUnitType.Pixel);
			}
		}

		protected (FrameworkElement Prev, FrameworkElement Next) GetResizerNeighbours(LayoutGridResizerControl splitter)
		{
			var idx = Children.IndexOf(splitter);
			return (Children[idx - 1] as FrameworkElement, GetNextVisibleChild(idx));
		}

		// ── Grid lifecycle ────────────────────────────────────────────────────────

#if !HAS_UNO
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			_model.ChildrenTreeChanged += (s, args) =>
			{
				if (args.Change != ChildrenTreeChange.DirectChildrenChanged) return;
				if (_asyncRefreshCalled.HasValue && _asyncRefreshCalled.Value == args.Change) return;
				_asyncRefreshCalled = args.Change;
				Dispatcher.BeginInvoke(new Action(() =>
				{
					_asyncRefreshCalled = null;
					UpdateChildren();
				}), System.Windows.Threading.DispatcherPriority.Normal, null);
			};
			SizeChanged += OnSizeChanged;
		}
#endif

		protected void FixChildrenDockLengths()
		{
			using (_fixingChildrenDockLengths.Enter())
				OnFixChildrenDockLengths();
		}

		protected abstract void OnFixChildrenDockLengths();

		private void OnSizeChanged(object sender,
#if HAS_UNO
			Microsoft.UI.Xaml.SizeChangedEventArgs e)
#else
			SizeChangedEventArgs e)
#endif
		{
			var m = _model as ILayoutPositionableElementWithActualSize;
			m.ActualWidth  = ActualWidth;
			m.ActualHeight = ActualHeight;
			if (!_initialized) { _initialized = true; UpdateChildren(); }
			AdjustFixedChildrenPanelSizes();
		}

#if !HAS_UNO
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			SizeChanged -= OnSizeChanged;
			Unloaded    -= OnUnloaded;
		}
#endif

		// ── Children management ───────────────────────────────────────────────────

		private void UpdateChildren()
		{
			var existing = Children.OfType<ILayoutControl>().ToArray();
			DetachOldSplitters();
			DetachPropertyChangeHandler();
			Children.Clear();
			ColumnDefinitions.Clear();
			RowDefinitions.Clear();
			var manager = _model?.Root?.Manager;
			if (manager == null) return;
			foreach (var child in _model.Children)
			{
				var found = existing.FirstOrDefault(c => c.Model == child);
				var elem  = found != null ? found as UIElement : manager.CreateUIElementForModel(child);
				if (elem != null) Children.Add(elem);
			}
			CreateSplitters();
			UpdateRowColDefinitions();
			AttachNewSplitters();
			AttachPropertyChangeHandler();
		}

		private void AttachPropertyChangeHandler()
		{
			foreach (var c in Children.OfType<ILayoutControl>())
				c.Model.PropertyChanged += OnChildModelPropertyChanged;
		}

		private void DetachPropertyChangeHandler()
		{
			foreach (var c in Children.OfType<ILayoutControl>())
				c.Model.PropertyChanged -= OnChildModelPropertyChanged;
		}

		private void OnChildModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (AsyncRefreshCalled) return;
			if (_fixingChildrenDockLengths.CanEnter && e.PropertyName == nameof(ILayoutPositionableElement.DockWidth) && Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				if (ColumnDefinitions.Count != Children.Count) return;
				var changed = sender as ILayoutPositionableElement;
				var ctrl    = Children.OfType<ILayoutControl>().First(c => c.Model == changed) as UIElement;
				ColumnDefinitions[Children.IndexOf(ctrl)].Width = changed.DockWidth;
			}
			else if (_fixingChildrenDockLengths.CanEnter && e.PropertyName == nameof(ILayoutPositionableElement.DockHeight) && Orientation == System.Windows.Controls.Orientation.Vertical)
			{
				if (RowDefinitions.Count != Children.Count) return;
				var changed = sender as ILayoutPositionableElement;
				var ctrl    = Children.OfType<ILayoutControl>().First(c => c.Model == changed) as UIElement;
				RowDefinitions[Children.IndexOf(ctrl)].Height = changed.DockHeight;
			}
			else if (e.PropertyName == nameof(ILayoutPositionableElement.IsVisible))
				UpdateRowColDefinitions();
		}

		private void UpdateRowColDefinitions()
		{
			var manager = _model.Root?.Manager;
			if (manager == null) return;
			FixChildrenDockLengths();
			RowDefinitions.Clear();
			ColumnDefinitions.Clear();
			if (Orientation == Orientation.Horizontal)
			{
				int iCol = 0, iChild = 0;
				for (var iModel = 0; iModel < _model.Children.Count && iChild < Children.Count; iModel++, iCol++, iChild++)
				{
					var cm = _model.Children[iModel] as ILayoutPositionableElement;
					ColumnDefinitions.Add(new ColumnDefinition { Width = cm.IsVisible ? cm.DockWidth : new GridLength(0, GridUnitType.Pixel), MinWidth = cm.IsVisible ? cm.CalculatedDockMinWidth() : 0 });
#if HAS_UNO
					WinUIGrid.SetColumn((Microsoft.UI.Xaml.FrameworkElement)Children[iChild], iCol);
#else
					WinUIGrid.SetColumn(Children[iChild], iCol);
#endif
					if (iChild >= Children.Count - 1) continue;
					iChild++; iCol++;
					bool nv = false;
					for (var i = iModel + 1; i < _model.Children.Count; i++) if ((_model.Children[i] as ILayoutPositionableElement).IsVisible) { nv = true; break; }
					ColumnDefinitions.Add(new ColumnDefinition { Width = cm.IsVisible && nv ? new GridLength(manager.GridSplitterWidth) : new GridLength(0, GridUnitType.Pixel) });
#if HAS_UNO
					WinUIGrid.SetColumn((Microsoft.UI.Xaml.FrameworkElement)Children[iChild], iCol);
#else
					WinUIGrid.SetColumn(Children[iChild], iCol);
#endif
				}
			}
			else
			{
				int iRow = 0, iChild = 0;
				for (var iModel = 0; iModel < _model.Children.Count && iChild < Children.Count; iModel++, iRow++, iChild++)
				{
					var cm = _model.Children[iModel] as ILayoutPositionableElement;
					RowDefinitions.Add(new RowDefinition { Height = cm.IsVisible ? cm.DockHeight : new GridLength(0, GridUnitType.Pixel), MinHeight = cm.IsVisible ? cm.CalculatedDockMinHeight() : 0 });
#if HAS_UNO
					WinUIGrid.SetRow((Microsoft.UI.Xaml.FrameworkElement)Children[iChild], iRow);
#else
					WinUIGrid.SetRow(Children[iChild], iRow);
#endif
					if (iChild >= Children.Count - 1) continue;
					iChild++; iRow++;
					bool nv = false;
					for (var i = iModel + 1; i < _model.Children.Count; i++) if ((_model.Children[i] as ILayoutPositionableElement).IsVisible) { nv = true; break; }
					RowDefinitions.Add(new RowDefinition { Height = cm.IsVisible && nv ? new GridLength(manager.GridSplitterHeight) : new GridLength(0, GridUnitType.Pixel) });
#if HAS_UNO
					WinUIGrid.SetRow((Microsoft.UI.Xaml.FrameworkElement)Children[iChild], iRow);
#else
					WinUIGrid.SetRow(Children[iChild], iRow);
#endif
				}
			}
		}

		// ── Splitter management ───────────────────────────────────────────────────

		private void CreateSplitters()
		{
			for (var i = 1; i < Children.Count; i++)
			{
				var splitter = new LayoutGridResizerControl();
#if HAS_UNO
				splitter.IsHorizontalResizer = Orientation == System.Windows.Controls.Orientation.Horizontal;
				splitter.RefreshResizeCursor();
				splitter.Style = splitter.IsHorizontalResizer
					? _model.Root?.Manager?.GridSplitterVerticalStyle
					: _model.Root?.Manager?.GridSplitterHorizontalStyle;
#else
				splitter.Cursor = Orientation == Orientation.Horizontal ? Cursors.SizeWE : Cursors.SizeNS;
				splitter.Style  = Orientation == Orientation.Horizontal
					? _model.Root?.Manager?.GridSplitterVerticalStyle
					: _model.Root?.Manager?.GridSplitterHorizontalStyle;
#endif
				Children.Insert(i, splitter);
				i++;
			}
		}

		private void DetachOldSplitters()
		{
			foreach (var s in Children.OfType<LayoutGridResizerControl>())
			{
				s.DragStarted   -= OnSplitterDragStarted;
				s.DragDelta     -= OnSplitterDragDelta;
				s.DragCompleted -= OnSplitterDragCompleted;
			}
		}

		private void AttachNewSplitters()
		{
			foreach (var s in Children.OfType<LayoutGridResizerControl>())
			{
				s.DragStarted   += OnSplitterDragStarted;
				s.DragDelta     += OnSplitterDragDelta;
				s.DragCompleted += OnSplitterDragCompleted;
			}
		}

#if HAS_UNO
		// ── Uno drag handlers: live resize, no ghost overlay ──────────────────────

		private void OnSplitterDragStarted(object sender, DragStartedEventArgs e)
			=> _activeSplitter = sender as LayoutGridResizerControl;

		private void OnSplitterDragDelta(object sender, DragDeltaEventArgs e)
		{
			if (_activeSplitter == null) return;
			double delta = Orientation == System.Windows.Controls.Orientation.Horizontal
				? e.HorizontalChange : e.VerticalChange;
			if (delta == 0) return;
			var (prev, next) = GetResizerNeighbours(_activeSplitter);
			if (prev == null || next == null) return;
			var prevModel = (prev as ILayoutControl)?.Model as ILayoutPositionableElement;
			var nextModel = (next as ILayoutControl)?.Model as ILayoutPositionableElement;
			if (prevModel == null || nextModel == null) return;
			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				var pW = prev.ActualWidth; var nW = next.ActualWidth;
				var np = Math.Max(prevModel.CalculatedDockMinWidth(), pW + delta);
				var nn = Math.Max(nextModel.CalculatedDockMinWidth(), nW - delta);
				var t = pW + nW; if (np + nn > t) nn = t - np;
				prevModel.DockWidth = new GridLength(np, GridUnitType.Pixel);
				nextModel.DockWidth = new GridLength(nn, GridUnitType.Pixel);
			}
			else
			{
				var pH = prev.ActualHeight; var nH = next.ActualHeight;
				var np = Math.Max(prevModel.CalculatedDockMinHeight(), pH + delta);
				var nn = Math.Max(nextModel.CalculatedDockMinHeight(), nH - delta);
				var t = pH + nH; if (np + nn > t) nn = t - np;
				prevModel.DockHeight = new GridLength(np, GridUnitType.Pixel);
				nextModel.DockHeight = new GridLength(nn, GridUnitType.Pixel);
			}
		}

		private void OnSplitterDragCompleted(object sender, DragCompletedEventArgs e)
			=> _activeSplitter = null;

#else
		// ── WPF drag handlers delegated to LayoutGridControl.wpf.cs ─────────────
		private void OnSplitterDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
			=> ShowResizerOverlayWindow(sender as LayoutGridResizerControl);

		private void OnSplitterDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
			=> OnSplitterDragDeltaWpf(e);

		private void OnSplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
			=> OnSplitterDragCompletedWpf(sender as LayoutGridResizerControl, e);
#endif

		// ── Size adjustment ───────────────────────────────────────────────────────

		public virtual void AdjustFixedChildrenPanelSizes(Size? parentSize = null)
		{
			var visible = GetVisibleChildren();
			if (visible.Count == 0) return;
			var models    = visible.OfType<ILayoutControl>().Select(c => c.Model).OfType<ILayoutPositionableElementWithActualSize>().ToList();
			var splitters = visible.OfType<LayoutGridResizerControl>().ToList();
			var avail     = parentSize ?? new Size(ActualWidth, ActualHeight);
			List<ILayoutPositionableElementWithActualSize> fixedP, relP;
			double minH = 0, curH = 0, prefMinH = 0, minW = 0, curW = 0, prefMinW = 0;
			if (Orientation == Orientation.Vertical)
			{
				fixedP = models.Where(c => c.DockHeight.IsAbsolute).ToList();
				relP   = models.Where(c => !c.DockHeight.IsAbsolute).ToList();
				minH  += models.Sum(c => c.CalculatedDockMinHeight()) + splitters.Sum(c => c.ActualHeight);
				curH  += models.Sum(c => c.ActualHeight) + splitters.Sum(c => c.ActualHeight);
				prefMinH += minH + fixedP.Sum(c => c.FixedDockHeight) - fixedP.Sum(c => c.CalculatedDockMinHeight());
				var delta = avail.Height - curH + relP.Sum(c => c.ActualHeight - c.CalculatedDockMinHeight());
				foreach (var f in fixedP)
				{
					if (minH >= avail.Height)       { f.ResizableAbsoluteDockHeight = f.CalculatedDockMinHeight(); }
					else if (prefMinH <= avail.Height) { f.ResizableAbsoluteDockHeight = f.FixedDockHeight; }
					else if (relP.All(c => Math.Abs(c.ActualHeight - c.CalculatedDockMinHeight()) <= 1))
					{
						var idx = fixedP.IndexOf(f);
						var frac = delta < 0
							? (f.ActualHeight - f.CalculatedDockMinHeight()) / Math.Max(1, fixedP.Where(c => fixedP.IndexOf(c) >= idx).Sum(c => c.ActualHeight - c.CalculatedDockMinHeight()))
							: f.FixedDockHeight / Math.Max(1, fixedP.Where(c => fixedP.IndexOf(c) >= idx).Sum(c => c.FixedDockHeight));
						var h = Math.Max(Math.Round(delta * frac + f.ActualHeight), f.CalculatedDockMinHeight());
						f.ResizableAbsoluteDockHeight = h; delta -= h - f.ActualHeight;
					}
				}
			}
			else
			{
				fixedP = models.Where(c => c.DockWidth.IsAbsolute).ToList();
				relP   = models.Where(c => !c.DockWidth.IsAbsolute).ToList();
				minW  += models.Sum(c => c.CalculatedDockMinWidth()) + splitters.Sum(c => c.ActualWidth);
				curW  += models.Sum(c => c.ActualWidth) + splitters.Sum(c => c.ActualWidth);
				prefMinW += minW + fixedP.Sum(c => c.FixedDockWidth) - fixedP.Sum(c => c.CalculatedDockMinWidth());
				var delta = avail.Width - curW + relP.Sum(c => c.ActualWidth - c.CalculatedDockMinWidth());
				foreach (var f in fixedP)
				{
					if (minW >= avail.Width)       { f.ResizableAbsoluteDockWidth = f.CalculatedDockMinWidth(); }
					else if (prefMinW <= avail.Width) { f.ResizableAbsoluteDockWidth = f.FixedDockWidth; }
					else
					{
						var idx = fixedP.IndexOf(f);
						var frac = delta < 0
							? (f.ActualWidth - f.CalculatedDockMinWidth()) / Math.Max(1, fixedP.Where(c => fixedP.IndexOf(c) >= idx).Sum(c => c.ActualWidth - c.CalculatedDockMinWidth()))
							: f.FixedDockWidth / Math.Max(1, fixedP.Where(c => fixedP.IndexOf(c) >= idx).Sum(c => c.FixedDockWidth));
						var w = Math.Max(Math.Round(delta * frac + f.ActualWidth), f.CalculatedDockMinWidth());
						f.ResizableAbsoluteDockWidth = w; delta -= w - f.ActualWidth;
					}
				}
			}
			foreach (var c in Children.OfType<IAdjustableSizeLayout>())
				c.AdjustFixedChildrenPanelSizes(avail);
		}

		// ── Helpers ───────────────────────────────────────────────────────────────

		protected FrameworkElement GetNextVisibleChild(int index)
		{
			for (var i = index + 1; i < Children.Count; i++)
			{
				if (Children[i] is LayoutGridResizerControl) continue;
				if (IsChildVisible(i)) return Children[i] as FrameworkElement;
			}
			return null;
		}

		private List<FrameworkElement> GetVisibleChildren()
		{
			var list = new List<FrameworkElement>();
			for (var i = 0; i < Children.Count; i++)
				if (IsChildVisible(i) && Children[i] is FrameworkElement fe) list.Add(fe);
			return list;
		}

		private bool IsChildVisible(int index)
		{
			if (Orientation == Orientation.Horizontal)
			{
				if (index < ColumnDefinitions.Count)
					return ColumnDefinitions[index].Width.IsStar || ColumnDefinitions[index].Width.Value > 0;
			}
			else if (index < RowDefinitions.Count)
				return RowDefinitions[index].Height.IsStar || RowDefinitions[index].Height.Value > 0;
			return false;
		}
	}
}
