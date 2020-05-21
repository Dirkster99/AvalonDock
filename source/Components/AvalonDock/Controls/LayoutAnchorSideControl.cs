/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="Control"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Implements a side panel dependency property in the <see cref="DockingManager"/> control.
	/// See also <see cref="DockingManagerDropTarget"/>.
	/// </summary>
	/// <seealso cref="Control"/>
	/// <seealso cref="ILayoutControl"/>
	public class LayoutAnchorSideControl : Control, ILayoutControl
	{
		#region fields
		private readonly LayoutAnchorSide _model = null;
		private readonly ObservableCollection<LayoutAnchorGroupControl> _childViews = new ObservableCollection<LayoutAnchorGroupControl>();
		#endregion fields

		#region Constructors
		/// <summary>Static class constructor</summary>
		static LayoutAnchorSideControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorSideControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorSideControl)));
		}

		/// <summary>Class constructor from <see cref="LayoutAnchorSide"/> model.</summary>
		internal LayoutAnchorSideControl(LayoutAnchorSide model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			CreateChildrenViews();
			_model.Children.CollectionChanged += OnModelChildrenCollectionChanged;
			UpdateSide();

			Unloaded += LayoutAnchorSideControl_Unloaded;
		}
		#endregion Constructors

		#region Properties

		public ILayoutElement Model => _model;

		public ObservableCollection<LayoutAnchorGroupControl> Children => _childViews;

		#region IsLeftSide

		/// <summary><see cref="IsLeftSide"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey IsLeftSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsLeftSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsLeftSideProperty = IsLeftSidePropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="IsLeftSide"/> property. This dependency property indicates this control is anchored to left side.</summary>
		public bool IsLeftSide => (bool)GetValue(IsLeftSideProperty);

		/// <summary>Provides a secure method for setting the <see cref="IsLeftSide"/> property. This dependency property indicates this control is anchored to left side.</summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetIsLeftSide(bool value) => SetValue(IsLeftSidePropertyKey, value);

		#endregion IsLeftSide

		#region IsTopSide

		/// <summary><see cref="IsTopSide"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey IsTopSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsTopSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsTopSideProperty = IsTopSidePropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="IsTopSide"/> property. This dependency property indicates this control is anchored to top side.</summary>
		public bool IsTopSide => (bool)GetValue(IsTopSideProperty);

		/// <summary>Provides a secure method for setting the <see cref="IsTopSide"/> property. This dependency property indicates this control is anchored to top side.</summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetIsTopSide(bool value) => SetValue(IsTopSidePropertyKey, value);

		#endregion IsTopSide

		#region IsRightSide

		/// <summary><see cref="IsRightSide"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey IsRightSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsRightSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsRightSideProperty = IsRightSidePropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="IsRightSide"/> property. This dependency property indicates this control is anchored to right side.</summary>
		public bool IsRightSide => (bool)GetValue(IsRightSideProperty);

		/// <summary>Provides a secure method for setting the <see cref="IsRightSide"/> property. This dependency property indicates this control is anchored to right side.</summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetIsRightSide(bool value) => SetValue(IsRightSidePropertyKey, value);

		#endregion IsRightSide

		#region IsBottomSide

		/// <summary><see cref="IsBottomSide"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey IsBottomSidePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsBottomSide), typeof(bool), typeof(LayoutAnchorSideControl),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsBottomSideProperty = IsBottomSidePropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="IsBottomSide"/> property. This dependency property indicates if this panel is anchored to bottom side.</summary>
		public bool IsBottomSide => (bool)GetValue(IsBottomSideProperty);

		/// <summary>Provides a secure method for setting the <see cref="IsBottomSide"/> property. This dependency property indicates if this panel is anchored to bottom side.</summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetIsBottomSide(bool value) => SetValue(IsBottomSidePropertyKey, value);

		#endregion IsBottomSide

		#endregion Properties

		#region Private Methods
		/// <summary>
		/// Executes when the element is removed from within an element tree of loaded elements.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		private void OnModelChildrenCollectionChanged(object sender,
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

		#endregion Private Methods
	}
}
