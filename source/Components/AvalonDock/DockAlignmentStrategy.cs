using System.Collections.Generic;
using System.Linq;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Layout strategy for the base <see cref="DockingManager"/> that places anchorables
	/// into the correct anchor side based on the content-to-side map built by
	/// <see cref="LayoutSyncBridge"/> from the MVVM <see cref="Core.IToolDock.Alignment"/>.
	/// </summary>
	public sealed class DockAlignmentStrategy : ILayoutUpdateStrategy
	{
		private readonly IReadOnlyDictionary<object, AnchorSide> _contentToSide;

		/// <summary>
		/// Initializes a new instance of the <see cref="DockAlignmentStrategy"/> class.
		/// </summary>
		/// <param name="contentToSide">Map from anchorable content to target side.</param>
		public DockAlignmentStrategy(IReadOnlyDictionary<object, AnchorSide> contentToSide)
		{
			_contentToSide = contentToSide;
		}

		/// <inheritdoc/>
		public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
		{
			if (anchorableToShow?.Content == null)
				return false;

			if (!_contentToSide.TryGetValue(anchorableToShow.Content, out var side))
				return false;

			var anchorSide = GetLayoutAnchorSide(layout, side);
			if (anchorSide == null)
				return false;

			var group = anchorSide.Children.OfType<LayoutAnchorGroup>().FirstOrDefault();
			if (group == null)
			{
				group = new LayoutAnchorGroup();
				anchorSide.Children.Add(group);
			}

			group.Children.Add(anchorableToShow);
			return true;
		}

		/// <inheritdoc/>
		public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
		{
		}

		/// <inheritdoc/>
		public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
		{
			return false;
		}

		/// <inheritdoc/>
		public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
		{
		}

		private static LayoutAnchorSide GetLayoutAnchorSide(LayoutRoot layout, AnchorSide side)
		{
			switch (side)
			{
				case AnchorSide.Left:
					if (layout.LeftSide == null)
						layout.LeftSide = new LayoutAnchorSide();
					return layout.LeftSide;
				case AnchorSide.Right:
					if (layout.RightSide == null)
						layout.RightSide = new LayoutAnchorSide();
					return layout.RightSide;
				case AnchorSide.Top:
					if (layout.TopSide == null)
						layout.TopSide = new LayoutAnchorSide();
					return layout.TopSide;
				case AnchorSide.Bottom:
					if (layout.BottomSide == null)
						layout.BottomSide = new LayoutAnchorSide();
					return layout.BottomSide;
				default:
					return null;
			}
		}
	}
}