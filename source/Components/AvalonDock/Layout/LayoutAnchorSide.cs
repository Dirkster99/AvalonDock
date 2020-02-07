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
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorSide : LayoutGroup<LayoutAnchorGroup>
	{
		#region Properties

		#region Side

		private AnchorSide _side;
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
