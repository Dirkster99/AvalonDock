using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout anchor side control.
	/// </summary>
	public class LayoutAnchorSideControl : Control, ILayoutControl
	{
		private readonly LayoutAnchorSide _model = null;
		private readonly ObservableCollection<LayoutAnchorGroupControl> _childViews = new ObservableCollection<LayoutAnchorGroupControl>();

		/// <summary>
		/// Initializes static members of the <see cref="LayoutAnchorSideControl"/> class.
		/// </summary>
		static LayoutAnchorSideControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorSideControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorSideControl)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorSideControl"/> class.
		/// </summary>
		/// <param name="model">The layout model.</param>
		internal LayoutAnchorSideControl(LayoutAnchorSide model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
		}

		/// <summary>
		/// Gets the model.
		/// </summary>
		public ILayoutElement Model => _model;

		/// <summary>
		/// Gets the children.
		/// </summary>
		public ObservableCollection<LayoutAnchorGroupControl> Children => _childViews;

		/// <summary><see cref="IsLeftSide"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey IsLeftSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsLeftSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		/// <summary>
		/// <see cref="IsLeftSide"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsLeftSideProperty = IsLeftSidePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a value indicating whether this instance is left side.
		/// </summary>
		[Bindable(true)]
		[Description("Gets wether the control is anchored to left side.")]
		[Category("Anchor")]
		public bool IsLeftSide => (bool)GetValue(IsLeftSideProperty);

		/// <summary>
		/// Sets the is left side.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetIsLeftSide(bool value) => SetValue(IsLeftSidePropertyKey, value);

		/// <summary><see cref="IsTopSide"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey IsTopSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsTopSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		/// <summary>
		/// <see cref="IsTopSide"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsTopSideProperty = IsTopSidePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a value indicating whether this instance is top side.
		/// </summary>
		[Bindable(true)]
		[Description("Gets wether the control is anchored to top side.")]
		[Category("Anchor")]
		public bool IsTopSide => (bool)GetValue(IsTopSideProperty);

		/// <summary>
		/// Sets the is top side.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetIsTopSide(bool value) => SetValue(IsTopSidePropertyKey, value);

		/// <summary><see cref="IsRightSide"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey IsRightSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsRightSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		/// <summary>
		/// <see cref="IsRightSide"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsRightSideProperty = IsRightSidePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a value indicating whether this instance is right side.
		/// </summary>
		[Bindable(true)]
		[Description("Gets wether the control is anchored to right side.")]
		[Category("Anchor")]
		public bool IsRightSide => (bool)GetValue(IsRightSideProperty);

		/// <summary>
		/// Sets the is right side.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetIsRightSide(bool value) => SetValue(IsRightSidePropertyKey, value);

		/// <summary><see cref="IsBottomSide"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey IsBottomSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsBottomSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		/// <summary>
		/// <see cref="IsBottomSide"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsBottomSideProperty = IsBottomSidePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a value indicating whether this instance is bottom side.
		/// </summary>
		[Bindable(true)]
		[Description("Gets whether the control is anchored to bottom side.")]
		[Category("Anchor")]
		public bool IsBottomSide => (bool)GetValue(IsBottomSideProperty);

		/// <summary>
		/// Sets the is bottom side.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetIsBottomSide(bool value) => SetValue(IsBottomSidePropertyKey, value);

		/// <inheritdoc/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Loaded += LayoutAnchorSideControl_Loaded;
		}

		/// <inheritdoc/>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			CreateChildrenViews();
			UpdateSide();
		}

		private void LayoutAnchorSideControl_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= LayoutAnchorSideControl_Loaded;
			Unloaded += LayoutAnchorSideControl_Unloaded;
			_model.Children.CollectionChanged += OnModelChildrenCollectionChanged;
		}

		private void LayoutAnchorSideControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_model.Children.CollectionChanged -= OnModelChildrenCollectionChanged;
			Unloaded -= LayoutAnchorSideControl_Unloaded;
		}

		private void CreateChildrenViews()
		{
			var manager = _model.Root.Manager;
			foreach (var childModel in _model.Children) _childViews.Add(manager.CreateUIElementForModel(childModel) as LayoutAnchorGroupControl);
		}

		private void OnModelChildrenCollectionChanged(
			object sender,
			System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null &&
				(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
			{
				foreach (var childModel in e.OldItems)
					_childViews.Remove(_childViews.First(cv => cv.Model == childModel));
			}

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				_childViews.Clear();

			if (e.NewItems != null &&
				(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
			{
				var manager = _model.Root.Manager;
				var insertIndex = e.NewStartingIndex;
				foreach (LayoutAnchorGroup childModel in e.NewItems) _childViews.Insert(insertIndex++, manager.CreateUIElementForModel(childModel) as LayoutAnchorGroupControl);
			}
		}

		private void UpdateSide()
		{
			switch (_model.Side)
			{
				case AnchorSide.Left: SetIsLeftSide(true); break;
				case AnchorSide.Top: SetIsTopSide(true); break;
				case AnchorSide.Right: SetIsRightSide(true); break;
				case AnchorSide.Bottom: SetIsBottomSide(true); break;
			}
		}
	}
}