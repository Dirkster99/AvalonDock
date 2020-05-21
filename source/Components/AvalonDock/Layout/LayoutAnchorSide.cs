/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Markup;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Implements the viewmodel for a a side element (left, right, top, bottom) in AvalonDock's
	/// visual root of the <see cref="DockingManager"/>.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorSide : LayoutGroup<LayoutAnchorGroup>
	{
		#region fields
		private AnchorSide _side;
		#endregion fields

		#region Properties
		/// <summary>Gets the side (top, bottom, left, right) that this layout is anchored in the layout.</summary>
		public AnchorSide Side
		{
			get => _side;
			private set
			{
				if (value == _side) return;
				RaisePropertyChanging(nameof(Side));
				_side = value;
				RaisePropertyChanged(nameof(Side));
			}
		}
		#endregion

		#region Overrides

		/// <inheritdoc />
		protected override bool GetVisibility() => Children.Count > 0;

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			base.OnParentChanged(oldValue, newValue);
			UpdateSide();
		}

		#endregion

		#region Private Methods

		private void UpdateSide()
		{
			if (this == Root.LeftSide) Side = AnchorSide.Left;
			else if (this == Root.TopSide) Side = AnchorSide.Top;
			else if (this == Root.RightSide) Side = AnchorSide.Right;
			else if (this == Root.BottomSide) Side = AnchorSide.Bottom;
		}

		#endregion
	}
}
