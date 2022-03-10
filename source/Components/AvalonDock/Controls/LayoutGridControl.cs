/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="Grid"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <inheritdoc cref="IAdjustableSizeLayout"/>
	/// <summary>
	/// The abstract LayoutGridControl<T> class (and its inheriting classes) are used to layout non-floating
	/// windows and documents in AvalonDock. This contains a definition of size proportion per item and
	/// includes user interactions with Grid Splitter elements to resize UI items in an interactive way.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="Grid"/>
	/// <seealso cref="ILayoutControl"/>
	/// <seealso cref="IAdjustableSizeLayout"/>
	public abstract class LayoutGridControl<T> : Grid, ILayoutControl, IAdjustableSizeLayout where T : class, ILayoutPanelElement
	{
		#region fields

		private readonly LayoutPositionableGroup<T> _model;
		private readonly Orientation _orientation;
		private bool _initialized;
		private ChildrenTreeChange? _asyncRefreshCalled;
		private readonly ReentrantFlag _fixingChildrenDockLengths = new ReentrantFlag();
		private Border _resizerGhost = null;
		private Window _resizerWindowHost = null;
		private Vector _initialStartPoint;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="model"></param>
		/// <param name="orientation"></param>
		internal LayoutGridControl(LayoutPositionableGroup<T> model, Orientation orientation)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_orientation = orientation;
			FlowDirection = System.Windows.FlowDirection.LeftToRight;
			Unloaded += OnUnloaded;
		}

		#endregion Constructors

		#region Properties

		public ILayoutElement Model => _model;

		public Orientation Orientation => (_model as ILayoutOrientableGroup).Orientation;

		private bool AsyncRefreshCalled => _asyncRefreshCalled != null;

		#endregion Properties

		#region Overrides

		/// <inheritdoc/>
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
				}), DispatcherPriority.Normal, null);
			};
			this.SizeChanged += OnSizeChanged;
		}

		#endregion Overrides

		#region Internal Methods

		protected void FixChildrenDockLengths()
		{
			using (_fixingChildrenDockLengths.Enter())
				OnFixChildrenDockLengths();
		}

		protected abstract void OnFixChildrenDockLengths();

		#endregion Internal Methods

		#region Private Methods

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithAtcualSize.ActualWidth = ActualWidth;
			modelWithAtcualSize.ActualHeight = ActualHeight;
			if (!_initialized)
			{
				_initialized = true;
				UpdateChildren();
			}
			AdjustFixedChildrenPanelSizes();
		}

		/// <summary>
		/// Method executes when the element is removed from within an element tree of loaded elements.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// In order to prevent resource leaks, unsubscribe from SizeChanged events.
			SizeChanged -= OnSizeChanged;
			Unloaded -= OnUnloaded;
		}

		private void UpdateChildren()
		{
			var alreadyContainedChildren = Children.OfType<ILayoutControl>().ToArray();
			DetachOldSplitters();
			DetachPropertyChangeHandler();
			Children.Clear();
			ColumnDefinitions.Clear();
			RowDefinitions.Clear();
			var manager = _model?.Root?.Manager;
			if (manager == null) return;
			foreach (var child in _model.Children)
			{
				var foundContainedChild = alreadyContainedChildren.FirstOrDefault(chVM => chVM.Model == child);
				if (foundContainedChild != null)
					Children.Add(foundContainedChild as UIElement);
				else
					Children.Add(manager.CreateUIElementForModel(child));
			}
			CreateSplitters();
			UpdateRowColDefinitions();
			AttachNewSplitters();
			AttachPropertyChangeHandler();
		}

		private void AttachPropertyChangeHandler()
		{
			foreach (var child in InternalChildren.OfType<ILayoutControl>())
				child.Model.PropertyChanged += this.OnChildModelPropertyChanged;
		}

		private void DetachPropertyChangeHandler()
		{
			foreach (var child in InternalChildren.OfType<ILayoutControl>())
				child.Model.PropertyChanged -= this.OnChildModelPropertyChanged;
		}

		private void OnChildModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (AsyncRefreshCalled) return;
			if (_fixingChildrenDockLengths.CanEnter && e.PropertyName == nameof(ILayoutPositionableElement.DockWidth) && Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				if (ColumnDefinitions.Count != InternalChildren.Count) return;
				var changedElement = sender as ILayoutPositionableElement;
				var childFromModel = InternalChildren.OfType<ILayoutControl>().First(ch => ch.Model == changedElement) as UIElement;
				var indexOfChild = InternalChildren.IndexOf(childFromModel);
				ColumnDefinitions[indexOfChild].Width = changedElement.DockWidth;
			}
			else if (_fixingChildrenDockLengths.CanEnter && e.PropertyName == nameof(ILayoutPositionableElement.DockHeight) && Orientation == System.Windows.Controls.Orientation.Vertical)
			{
				if (RowDefinitions.Count != InternalChildren.Count) return;
				var changedElement = sender as ILayoutPositionableElement;
				var childFromModel = InternalChildren.OfType<ILayoutControl>().First(ch => ch.Model == changedElement) as UIElement;
				var indexOfChild = InternalChildren.IndexOf(childFromModel);
				RowDefinitions[indexOfChild].Height = changedElement.DockHeight;
			}
			else if (e.PropertyName == nameof(ILayoutPositionableElement.IsVisible))
				UpdateRowColDefinitions();
		}

		private void UpdateRowColDefinitions()
		{
			var root = _model.Root;
			var manager = root?.Manager;
			if (manager == null) return;
			FixChildrenDockLengths();
			//Debug.Assert(InternalChildren.Count == _model.ChildrenCount + (_model.ChildrenCount - 1));

			#region Setup GridRows/Cols

			RowDefinitions.Clear();
			ColumnDefinitions.Clear();
			if (Orientation == Orientation.Horizontal)
			{
				var iColumn = 0;
				var iChild = 0;
				// BD: 24.08.2020 added check for iChild against InternalChildren.Count
				for (var iChildModel = 0; iChildModel < _model.Children.Count && iChild < InternalChildren.Count; iChildModel++, iColumn++, iChild++)
				{
					var childModel = _model.Children[iChildModel] as ILayoutPositionableElement;
					ColumnDefinitions.Add(new ColumnDefinition
					{
						Width = childModel.IsVisible ? childModel.DockWidth : new GridLength(0.0, GridUnitType.Pixel),
						MinWidth = childModel.IsVisible ? childModel.CalculatedDockMinWidth() : 0.0
					});
					Grid.SetColumn(InternalChildren[iChild], iColumn);

					//append column for splitter
					if (iChild >= InternalChildren.Count - 1) continue;
					iChild++;
					iColumn++;

					var nextChildModelVisibleExist = false;
					for (var i = iChildModel + 1; i < _model.Children.Count; i++)
					{
						var nextChildModel = _model.Children[i] as ILayoutPositionableElement;
						if (!nextChildModel.IsVisible) continue;
						nextChildModelVisibleExist = true;
						break;
					}

					ColumnDefinitions.Add(new ColumnDefinition
					{
						Width = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength(manager.GridSplitterWidth) : new GridLength(0.0, GridUnitType.Pixel)
					});
					Grid.SetColumn(InternalChildren[iChild], iColumn);
				}
			}
			else //if (_model.Orientation == Orientation.Vertical)
			{
				var iRow = 0;
				var iChild = 0;
				// BD: 24.08.2020 added check for iChild against InternalChildren.Count
				for (var iChildModel = 0; iChildModel < _model.Children.Count && iChild < InternalChildren.Count; iChildModel++, iRow++, iChild++)
				{
					var childModel = _model.Children[iChildModel] as ILayoutPositionableElement;
					RowDefinitions.Add(new RowDefinition
					{
						Height = childModel.IsVisible ? childModel.DockHeight : new GridLength(0.0, GridUnitType.Pixel),
						MinHeight = childModel.IsVisible ? childModel.CalculatedDockMinHeight() : 0.0
					});
					Grid.SetRow(InternalChildren[iChild], iRow);

					//if (RowDefinitions.Last().Height.Value == 0.0)
					//    System.Diagnostics.Debugger.Break();

					//append row for splitter (if necessary)
					if (iChild >= InternalChildren.Count - 1) continue;
					iChild++;
					iRow++;

					var nextChildModelVisibleExist = false;
					for (var i = iChildModel + 1; i < _model.Children.Count; i++)
					{
						var nextChildModel = _model.Children[i] as ILayoutPositionableElement;
						if (!nextChildModel.IsVisible) continue;
						nextChildModelVisibleExist = true;
						break;
					}

					RowDefinitions.Add(new RowDefinition
					{
						Height = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength(manager.GridSplitterHeight) : new GridLength(0.0, GridUnitType.Pixel)
					});
					//if (RowDefinitions.Last().Height.Value == 0.0)
					//    System.Diagnostics.Debugger.Break();
					Grid.SetRow(InternalChildren[iChild], iRow);
				}
			}

			#endregion Setup GridRows/Cols
		}

		/// <summary>Apply Horizontal/Vertical cursor style
		/// (and splitter style if optional splitter style is set) to Grid Resizer Control.</summary>
		private void CreateSplitters()
		{
			for (var iChild = 1; iChild < Children.Count; iChild++)
			{
				var splitter = new LayoutGridResizerControl();

				if (Orientation == Orientation.Horizontal)
				{
					splitter.Cursor = Cursors.SizeWE;
					splitter.Style = _model.Root?.Manager?.GridSplitterVerticalStyle;
				}
				else
				{
					splitter.Cursor = Cursors.SizeNS;
					splitter.Style = _model.Root?.Manager?.GridSplitterHorizontalStyle;
				}


				Children.Insert(iChild, splitter);
				// TODO: MK Is this a bug????
				iChild++;
			}
		}

		private void DetachOldSplitters()
		{
			foreach (var splitter in Children.OfType<LayoutGridResizerControl>())
			{
				splitter.DragStarted -= OnSplitterDragStarted;
				splitter.DragDelta -= OnSplitterDragDelta;
				splitter.DragCompleted -= OnSplitterDragCompleted;
			}
		}

		private void AttachNewSplitters()
		{
			foreach (var splitter in Children.OfType<LayoutGridResizerControl>())
			{
				splitter.DragStarted += OnSplitterDragStarted;
				splitter.DragDelta += OnSplitterDragDelta;
				splitter.DragCompleted += OnSplitterDragCompleted;
			}
		}

		private void OnSplitterDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) => ShowResizerOverlayWindow(sender as LayoutGridResizerControl);

		private void OnSplitterDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			var rootVisual = this.FindVisualTreeRoot() as Visual;
			var trToWnd = TransformToAncestor(rootVisual);
			var transformedDelta = trToWnd.Transform(new Point(e.HorizontalChange, e.VerticalChange)) - trToWnd.Transform(new Point());
			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
				Canvas.SetLeft(_resizerGhost, MathHelper.MinMax(_initialStartPoint.X + transformedDelta.X, 0.0, _resizerWindowHost.Width - _resizerGhost.Width));
			else
				Canvas.SetTop(_resizerGhost, MathHelper.MinMax(_initialStartPoint.Y + transformedDelta.Y, 0.0, _resizerWindowHost.Height - _resizerGhost.Height));
		}

		private void OnSplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			var splitter = sender as LayoutGridResizerControl;
			var rootVisual = this.FindVisualTreeRoot() as Visual;

			var trToWnd = TransformToAncestor(rootVisual);
			var transformedDelta = trToWnd.Transform(new Point(e.HorizontalChange, e.VerticalChange)) - trToWnd.Transform(new Point());

			double delta;
			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
				delta = Canvas.GetLeft(_resizerGhost) - _initialStartPoint.X;
			else
				delta = Canvas.GetTop(_resizerGhost) - _initialStartPoint.Y;

			var indexOfResizer = InternalChildren.IndexOf(splitter);

			var prevChild = InternalChildren[indexOfResizer - 1] as FrameworkElement;
			var nextChild = GetNextVisibleChild(indexOfResizer);

			var prevChildActualSize = prevChild.TransformActualSizeToAncestor();
			var nextChildActualSize = nextChild.TransformActualSizeToAncestor();

			var prevChildModel = (ILayoutPositionableElement)(prevChild as ILayoutControl).Model;
			var nextChildModel = (ILayoutPositionableElement)(nextChild as ILayoutControl).Model;

			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				if (prevChildModel.DockWidth.IsStar)
					prevChildModel.DockWidth = new GridLength(prevChildModel.DockWidth.Value * (prevChildActualSize.Width + delta) / prevChildActualSize.Width, GridUnitType.Star);
				else
				{
					var width = (prevChildModel.DockWidth.IsAuto) ? prevChildActualSize.Width : prevChildModel.DockWidth.Value;
					var resizedWidth = width + delta;
					prevChildModel.DockWidth = new GridLength(double.IsNaN(resizedWidth) ? width : resizedWidth, GridUnitType.Pixel);
				}

				if (nextChildModel.DockWidth.IsStar)
					nextChildModel.DockWidth = new GridLength(nextChildModel.DockWidth.Value * (nextChildActualSize.Width - delta) / nextChildActualSize.Width, GridUnitType.Star);
				else
				{
					var width = (nextChildModel.DockWidth.IsAuto) ? nextChildActualSize.Width : nextChildModel.DockWidth.Value;
					var resizedWidth = width - delta;
					nextChildModel.DockWidth = new GridLength(double.IsNaN(resizedWidth) ? width : resizedWidth, GridUnitType.Pixel);
				}
			}
			else
			{
				if (prevChildModel.DockHeight.IsStar)
					prevChildModel.DockHeight = new GridLength(prevChildModel.DockHeight.Value * (prevChildActualSize.Height + delta) / prevChildActualSize.Height, GridUnitType.Star);
				else
				{
					var height = (prevChildModel.DockHeight.IsAuto) ? prevChildActualSize.Height : prevChildModel.DockHeight.Value;
					var resizedHeight = height + delta;
					prevChildModel.DockHeight = new GridLength(double.IsNaN(resizedHeight) ? height : resizedHeight, GridUnitType.Pixel);
				}

				if (nextChildModel.DockHeight.IsStar)
					nextChildModel.DockHeight = new GridLength(nextChildModel.DockHeight.Value * (nextChildActualSize.Height - delta) / nextChildActualSize.Height, GridUnitType.Star);
				else
				{
					var height = (nextChildModel.DockHeight.IsAuto) ? nextChildActualSize.Height : nextChildModel.DockHeight.Value;
					var resizedHeight = height - delta;
					nextChildModel.DockHeight = new GridLength(double.IsNaN(resizedHeight) ? height : resizedHeight, GridUnitType.Pixel);
				}
			}

			HideResizerOverlayWindow();
		}

		public virtual void AdjustFixedChildrenPanelSizes(Size? parentSize = null)
		{
			var visibleChildren = GetVisibleChildren();
			if (visibleChildren.Count == 0) return;

			var layoutChildrenModels = visibleChildren.OfType<ILayoutControl>()
			  .Select(child => child.Model)
			  .OfType<ILayoutPositionableElementWithActualSize>()
			  .ToList();

			var splitterChildren = visibleChildren.OfType<LayoutGridResizerControl>().ToList();
			List<ILayoutPositionableElementWithActualSize> fixedPanels;
			List<ILayoutPositionableElementWithActualSize> relativePanels;

			// Get current available size of panel.
			var availableSize = parentSize ?? new Size(ActualWidth, ActualHeight);

			// Calculate minimum required size and current size of children.
			var minimumSize = new Size(0, 0);
			var currentSize = new Size(0, 0);
			var preferredMinimumSize = new Size(0, 0);
			if (Orientation == Orientation.Vertical)
			{
				fixedPanels = layoutChildrenModels.Where(child => child.DockHeight.IsAbsolute).ToList();
				relativePanels = layoutChildrenModels.Where(child => !child.DockHeight.IsAbsolute).ToList();
				minimumSize.Width += layoutChildrenModels.Max(child => child.CalculatedDockMinWidth());
				minimumSize.Height += layoutChildrenModels.Sum(child => child.CalculatedDockMinHeight());
				minimumSize.Height += splitterChildren.Sum(child => child.ActualHeight);
				currentSize.Width += layoutChildrenModels.Max(child => child.ActualWidth);
				currentSize.Height += layoutChildrenModels.Sum(child => child.ActualHeight);
				currentSize.Height += splitterChildren.Sum(child => child.ActualHeight);
				preferredMinimumSize.Width += layoutChildrenModels.Max(child => child.CalculatedDockMinWidth());
				preferredMinimumSize.Height += minimumSize.Height + fixedPanels.Sum(child => child.FixedDockHeight) - fixedPanels.Sum(child => child.CalculatedDockMinHeight());
			}
			else
			{
				fixedPanels = layoutChildrenModels.Where(child => child.DockWidth.IsAbsolute).ToList();
				relativePanels = layoutChildrenModels.Where(child => !child.DockWidth.IsAbsolute).ToList();
				minimumSize.Width += layoutChildrenModels.Sum(child => child.CalculatedDockMinWidth());
				minimumSize.Height += layoutChildrenModels.Max(child => child.CalculatedDockMinHeight());
				minimumSize.Width += splitterChildren.Sum(child => child.ActualWidth);
				currentSize.Width += layoutChildrenModels.Sum(child => child.ActualWidth);
				currentSize.Height += layoutChildrenModels.Max(child => child.ActualHeight);
				currentSize.Width += splitterChildren.Sum(child => child.ActualWidth);
				preferredMinimumSize.Height += layoutChildrenModels.Max(child => child.CalculatedDockMinHeight());
				preferredMinimumSize.Width += minimumSize.Width + fixedPanels.Sum(child => child.FixedDockWidth) - fixedPanels.Sum(child => child.CalculatedDockMinWidth());
			}

			// Apply corrected sizes for fixed panels.
			if (Orientation == Orientation.Vertical)
			{
				var delta = availableSize.Height - currentSize.Height;
				var relativeDelta = relativePanels.Sum(child => child.ActualHeight - child.CalculatedDockMinHeight());
				delta += relativeDelta;
				foreach (var fixedChild in fixedPanels)
				{
					if (minimumSize.Height >= availableSize.Height)
						fixedChild.ResizableAbsoluteDockHeight = fixedChild.CalculatedDockMinHeight();
					else if (preferredMinimumSize.Height <= availableSize.Height)
						fixedChild.ResizableAbsoluteDockHeight = fixedChild.FixedDockHeight;
					else if (relativePanels.All(child => Math.Abs(child.ActualHeight - child.CalculatedDockMinHeight()) <= 1))
					{
						double panelFraction;
						var indexOfChild = fixedPanels.IndexOf(fixedChild);
						if (delta < 0)
						{
							var availableHeightLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
							  .Sum(child => child.ActualHeight - child.CalculatedDockMinHeight());
							panelFraction = (fixedChild.ActualHeight - fixedChild.CalculatedDockMinHeight()) / (availableHeightLeft > 0 ? availableHeightLeft : 1);
						}
						else
						{
							var fixedHeightLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
							  .Sum(child => child.FixedDockHeight);
							panelFraction = fixedChild.FixedDockHeight / (fixedHeightLeft > 0 ? fixedHeightLeft : 1);
						}

						var childActualHeight = fixedChild.ActualHeight;
						var heightToSet = Math.Max(Math.Round(delta * panelFraction + fixedChild.ActualHeight), fixedChild.CalculatedDockMinHeight());
						fixedChild.ResizableAbsoluteDockHeight = heightToSet;
						delta -= heightToSet - childActualHeight;
					}
				}
			}
			else
			{
				var delta = availableSize.Width - currentSize.Width;
				var relativeDelta = relativePanels.Sum(child => child.ActualWidth - child.CalculatedDockMinWidth());
				delta += relativeDelta;
				foreach (var fixedChild in fixedPanels)
				{
					if (minimumSize.Width >= availableSize.Width)
						fixedChild.ResizableAbsoluteDockWidth = fixedChild.CalculatedDockMinWidth();
					else if (preferredMinimumSize.Width <= availableSize.Width)
						fixedChild.ResizableAbsoluteDockWidth = fixedChild.FixedDockWidth;
					else
					{
						double panelFraction;
						var indexOfChild = fixedPanels.IndexOf(fixedChild);
						if (delta < 0)
						{
							var availableWidthLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
							  .Sum(child => child.ActualWidth - child.CalculatedDockMinWidth());
							panelFraction = (fixedChild.ActualWidth - fixedChild.CalculatedDockMinWidth()) / (availableWidthLeft > 0 ? availableWidthLeft : 1);
						}
						else
						{
							var fixedWidthLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
							  .Sum(child => child.FixedDockWidth);
							panelFraction = fixedChild.FixedDockWidth / (fixedWidthLeft > 0 ? fixedWidthLeft : 1);
						}

						var childActualWidth = fixedChild.ActualWidth;
						var widthToSet = Math.Max(Math.Round(delta * panelFraction + fixedChild.ActualWidth), fixedChild.CalculatedDockMinWidth());
						fixedChild.ResizableAbsoluteDockWidth = widthToSet;
						delta -= widthToSet - childActualWidth;
					}
				}
			}

			foreach (var child in InternalChildren.OfType<IAdjustableSizeLayout>())
				child.AdjustFixedChildrenPanelSizes(availableSize);
		}

		private FrameworkElement GetNextVisibleChild(int index)
		{
			for (var i = index + 1; i < InternalChildren.Count; i++)
			{
				if (InternalChildren[i] is LayoutGridResizerControl) continue;
				if (IsChildVisible(i)) return InternalChildren[i] as FrameworkElement;
			}
			return null;
		}

		private List<FrameworkElement> GetVisibleChildren()
		{
			var visibleChildren = new List<FrameworkElement>();
			for (var i = 0; i < InternalChildren.Count; i++)
			{
				if (IsChildVisible(i) && InternalChildren[i] is FrameworkElement)
					visibleChildren.Add(InternalChildren[i] as FrameworkElement);
			}
			return visibleChildren;
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

		private void ShowResizerOverlayWindow(LayoutGridResizerControl splitter)
		{
			_resizerGhost = new Border { Background = splitter.BackgroundWhileDragging, Opacity = splitter.OpacityWhileDragging };

			var indexOfResizer = InternalChildren.IndexOf(splitter);

			var prevChild = InternalChildren[indexOfResizer - 1] as FrameworkElement;
			var nextChild = GetNextVisibleChild(indexOfResizer);

			var prevChildActualSize = prevChild.TransformActualSizeToAncestor();
			var nextChildActualSize = nextChild.TransformActualSizeToAncestor();

			var prevChildModel = (ILayoutPositionableElement)(prevChild as ILayoutControl).Model;
			var nextChildModel = (ILayoutPositionableElement)(nextChild as ILayoutControl).Model;

			var ptTopLeftScreen = prevChild.PointToScreenDPIWithoutFlowDirection(new Point());

			Size actualSize;

			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				actualSize = new Size(
					prevChildActualSize.Width - prevChildModel.CalculatedDockMinWidth() + splitter.ActualWidth + nextChildActualSize.Width - nextChildModel.CalculatedDockMinWidth(),
					nextChildActualSize.Height);

				_resizerGhost.Width = splitter.ActualWidth;
				_resizerGhost.Height = actualSize.Height;
				ptTopLeftScreen.Offset(prevChildModel.CalculatedDockMinWidth(), 0.0);
			}
			else
			{
				actualSize = new Size(
					prevChildActualSize.Width,
					prevChildActualSize.Height - prevChildModel.CalculatedDockMinHeight() + splitter.ActualHeight + nextChildActualSize.Height - nextChildModel.CalculatedDockMinHeight());

				_resizerGhost.Height = splitter.ActualHeight;
				_resizerGhost.Width = actualSize.Width;

				ptTopLeftScreen.Offset(0.0, prevChildModel.CalculatedDockMinHeight());
			}

			_initialStartPoint = splitter.PointToScreenDPIWithoutFlowDirection(new Point()) - ptTopLeftScreen;

			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
				Canvas.SetLeft(_resizerGhost, _initialStartPoint.X);
			else
				Canvas.SetTop(_resizerGhost, _initialStartPoint.Y);

			var panelHostResizer = new Canvas { HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch, VerticalAlignment = System.Windows.VerticalAlignment.Stretch };
			panelHostResizer.Children.Add(_resizerGhost);

			_resizerWindowHost = new Window
			{
				Style = new Style(typeof(Window), null),
				SizeToContent = System.Windows.SizeToContent.Manual,
				ResizeMode = ResizeMode.NoResize,
				WindowStyle = System.Windows.WindowStyle.None,
				ShowInTaskbar = false,
				AllowsTransparency = true,
				Background = null,
				Width = actualSize.Width,
				Height = actualSize.Height,
				Left = ptTopLeftScreen.X,
				Top = ptTopLeftScreen.Y,
				ShowActivated = false,
				Owner = Window.GetWindow(this),
				Content = panelHostResizer
			};
			_resizerWindowHost.Loaded += (s, e) => _resizerWindowHost.SetParentToMainWindowOf(this);
			_resizerWindowHost.Show();
		}

		private void HideResizerOverlayWindow()
		{
			if (_resizerWindowHost == null) return;
			_resizerWindowHost.Close();
			_resizerWindowHost = null;
		}

		#endregion Private Methods
	}
}