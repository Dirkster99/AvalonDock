using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout positionable group.
	/// </summary>
	/// <typeparam name="T">The type of the related layout element.</typeparam>
	[Serializable]
	public abstract class LayoutPositionableGroup<T> : LayoutGroup<T>, ILayoutPositionableElementWithActualSize
		where T : class, ILayoutElement
	{
		// DockWidth fields
		private GridLength _dockWidth = new GridLength(1.0, GridUnitType.Star);

		private double? _resizableAbsoluteDockWidth;

		// DockHeight fields
		private GridLength _dockHeight = new GridLength(1.0, GridUnitType.Star);

		private double? _resizableAbsoluteDockHeight;

		private bool _allowDuplicateContent = true;
		private bool _canRepositionItems = true;

		private double _dockMinWidth = 25.0;
		private double _dockMinHeight = 25.0;
		private double _floatingWidth = 0.0;
		private double _floatingHeight = 0.0;
		private double _floatingLeft = 0.0;
		private double _floatingTop = 0.0;

		private bool _isMaximized = false;

		[NonSerialized]
		private double _actualWidth;

		[NonSerialized]
		private double _actualHeight;

		/// <summary>
		/// Occurs when the floating properties updated event is raised.
		/// </summary>
		public event EventHandler FloatingPropertiesUpdated;

		/// <summary>
		/// Gets or sets the dock width.
		/// </summary>
		public GridLength DockWidth
		{
			get => _dockWidth.IsAbsolute && _resizableAbsoluteDockWidth < _dockWidth.Value && _resizableAbsoluteDockWidth.HasValue ?
						new GridLength(_resizableAbsoluteDockWidth.Value) : _dockWidth;
			set
			{
				if (value == _dockWidth || !(value.Value > 0)) return;
				if (value.IsAbsolute) _resizableAbsoluteDockWidth = value.Value;
				RaisePropertyChanging(nameof(DockWidth));
				_dockWidth = value;
				RaisePropertyChanged(nameof(DockWidth));
				OnDockWidthChanged();
			}
		}

		/// <summary>
		/// Gets the fixed dock width.
		/// </summary>
		public double FixedDockWidth => _dockWidth.IsAbsolute && _dockWidth.Value >= _dockMinWidth ? _dockWidth.Value : _dockMinWidth;

		/// <summary>
		/// Gets or sets the resizable absolute dock width.
		/// </summary>
		public double ResizableAbsoluteDockWidth
		{
			get => _resizableAbsoluteDockWidth ?? 0;
			set
			{
				if (!_dockWidth.IsAbsolute) return;
				if (value <= _dockWidth.Value && value > 0)
				{
					RaisePropertyChanging(nameof(DockWidth));
					_resizableAbsoluteDockWidth = value;
					RaisePropertyChanged(nameof(DockWidth));
					OnDockWidthChanged();
				}
				else if (value > _dockWidth.Value && _resizableAbsoluteDockWidth < _dockWidth.Value)
				{
					_resizableAbsoluteDockWidth = _dockWidth.Value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the dock height.
		/// </summary>
		public GridLength DockHeight
		{
			get => _dockHeight.IsAbsolute && _resizableAbsoluteDockHeight < _dockHeight.Value && _resizableAbsoluteDockHeight.HasValue ?
						new GridLength(_resizableAbsoluteDockHeight.Value) : _dockHeight;
			set
			{
				if (_dockHeight == value || !(value.Value > 0)) return;
				if (value.IsAbsolute) _resizableAbsoluteDockHeight = value.Value;
				RaisePropertyChanging(nameof(DockHeight));
				_dockHeight = value;
				RaisePropertyChanged(nameof(DockHeight));
				OnDockHeightChanged();
			}
		}

		/// <summary>
		/// Gets the fixed dock height.
		/// </summary>
		public double FixedDockHeight => _dockHeight.IsAbsolute && _dockHeight.Value >= _dockMinHeight ? _dockHeight.Value : _dockMinHeight;

		/// <summary>
		/// Gets or sets the resizable absolute dock height.
		/// </summary>
		public double ResizableAbsoluteDockHeight
		{
			get => _resizableAbsoluteDockHeight ?? 0;
			set
			{
				if (!_dockHeight.IsAbsolute) return;
				if (value < _dockHeight.Value && value > 0)
				{
					RaisePropertyChanging(nameof(DockHeight));
					_resizableAbsoluteDockHeight = value;
					RaisePropertyChanged(nameof(DockHeight));
					OnDockHeightChanged();
				}
				else if (value > _dockHeight.Value && _resizableAbsoluteDockHeight < _dockHeight.Value)
				{
					_resizableAbsoluteDockHeight = _dockHeight.Value;
				}
				else if (value == 0)
				{
					_resizableAbsoluteDockHeight = DockMinHeight;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether duplicate content is allowed.
		/// </summary>
		public bool AllowDuplicateContent
		{
			get => _allowDuplicateContent;
			set
			{
				if (value == _allowDuplicateContent) return;
				RaisePropertyChanging(nameof(AllowDuplicateContent));
				_allowDuplicateContent = value;
				RaisePropertyChanged(nameof(AllowDuplicateContent));
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can reposition items.
		/// </summary>
		public bool CanRepositionItems
		{
			get => _canRepositionItems;
			set
			{
				if (value == _canRepositionItems) return;
				RaisePropertyChanging(nameof(CanRepositionItems));
				_canRepositionItems = value;
				RaisePropertyChanged(nameof(CanRepositionItems));
			}
		}

		/// <summary>
		/// Executes the calculated dock min width operation.
		/// </summary>
		/// <returns>The resulting value.</returns>
		public double CalculatedDockMinWidth()
		{
			var childrenDockMinWidth = 0.0;
			var visibleChildren = Children.OfType<ILayoutPositionableElement>().Where(child => child.IsVisible).ToList();
			if (this is ILayoutOrientableGroup orientableGroup && visibleChildren.Any())
			{
				childrenDockMinWidth = orientableGroup.Orientation == Orientation.Vertical ?
					visibleChildren.Max(child => child.CalculatedDockMinWidth())
				  : visibleChildren.Sum(child => child.CalculatedDockMinWidth() + (Root?.Manager?.GridSplitterWidth ?? 0) * (visibleChildren.Count - 1));
			}

			return Math.Max(this._dockMinWidth, childrenDockMinWidth);
		}

		/// <summary>
		/// Gets or sets the dock min width.
		/// </summary>
		public double DockMinWidth
		{
			get => _dockMinWidth;
			set
			{
				if (value == _dockMinWidth) return;
				MathHelper.AssertIsPositiveOrZero(value);
				RaisePropertyChanging(nameof(DockMinWidth));
				_dockMinWidth = value;
				RaisePropertyChanged(nameof(DockMinWidth));
			}
		}

		/// <summary>
		/// Executes the calculated dock min height operation.
		/// </summary>
		/// <returns>The resulting value.</returns>
		public double CalculatedDockMinHeight()
		{
			var childrenDockMinHeight = 0.0;
			var visibleChildren = Children.OfType<ILayoutPositionableElement>().Where(child => child.IsVisible).ToList();
			if (this is ILayoutOrientableGroup orientableGroup && visibleChildren.Any())
			{
				childrenDockMinHeight = orientableGroup.Orientation == Orientation.Vertical ?
					visibleChildren.Sum(child => child.CalculatedDockMinHeight() + (Root?.Manager?.GridSplitterHeight ?? 0) * (visibleChildren.Count - 1))
				  : visibleChildren.Max(child => child.CalculatedDockMinHeight());
			}

			return Math.Max(this._dockMinHeight, childrenDockMinHeight);
		}

		/// <summary>
		/// Gets or sets the dock min height.
		/// </summary>
		public double DockMinHeight
		{
			get => _dockMinHeight;
			set
			{
				if (value == _dockMinHeight) return;
				MathHelper.AssertIsPositiveOrZero(value);
				RaisePropertyChanging(nameof(DockMinHeight));
				_dockMinHeight = value;
				RaisePropertyChanged(nameof(DockMinHeight));
			}
		}

		/// <summary>
		/// Gets or sets the floating width.
		/// </summary>
		public double FloatingWidth
		{
			get => _floatingWidth;
			set
			{
				if (value == _floatingWidth) return;
				RaisePropertyChanging(nameof(FloatingWidth));
				_floatingWidth = value;
				RaisePropertyChanged(nameof(FloatingWidth));
			}
		}

		/// <summary>
		/// Gets or sets the floating height.
		/// </summary>
		public double FloatingHeight
		{
			get => _floatingHeight;
			set
			{
				if (_floatingHeight == value) return;
				RaisePropertyChanging(nameof(FloatingHeight));
				_floatingHeight = value;
				RaisePropertyChanged(nameof(FloatingHeight));
			}
		}

		/// <summary>
		/// Gets or sets the floating left.
		/// </summary>
		public double FloatingLeft
		{
			get => _floatingLeft;
			set
			{
				if (value == _floatingLeft) return;
				RaisePropertyChanging(nameof(FloatingLeft));
				_floatingLeft = value;
				RaisePropertyChanged(nameof(FloatingLeft));
			}
		}

		/// <summary>
		/// Gets or sets the floating top.
		/// </summary>
		public double FloatingTop
		{
			get => _floatingTop;
			set
			{
				if (value == _floatingTop) return;
				RaisePropertyChanging(nameof(FloatingTop));
				_floatingTop = value;
				RaisePropertyChanged(nameof(FloatingTop));
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is maximized.
		/// </summary>
		public bool IsMaximized
		{
			get => _isMaximized;
			set
			{
				if (value == _isMaximized) return;
				_isMaximized = value;
				RaisePropertyChanged(nameof(IsMaximized));
			}
		}

		/// <inheritdoc/>
		double ILayoutPositionableElementWithActualSize.ActualWidth
		{
			get => _actualWidth;
			set => _actualWidth = value;
		}

		/// <inheritdoc/>
		double ILayoutPositionableElementWithActualSize.ActualHeight
		{
			get => _actualHeight;
			set => _actualHeight = value;
		}

		/// <inheritdoc/>
		void ILayoutElementForFloatingWindow.RaiseFloatingPropertiesUpdated() => FloatingPropertiesUpdated?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Executes the on dock width changed operation.
		/// </summary>
		protected virtual void OnDockWidthChanged()
		{
		}

		/// <summary>
		/// Executes the on dock height changed operation.
		/// </summary>
		protected virtual void OnDockHeightChanged()
		{
		}
	}
}