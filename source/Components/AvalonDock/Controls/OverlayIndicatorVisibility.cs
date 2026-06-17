using System.Windows.Controls;

namespace AvalonDock.Controls
{
	internal sealed class OverlayIndicatorVisibility
	{
		public bool InnerLeft { get; set; } = true;
		public bool InnerTop { get; set; } = true;
		public bool InnerRight { get; set; } = true;
		public bool InnerBottom { get; set; } = true;
		public bool AsLeft { get; set; } = true;
		public bool AsTop { get; set; } = true;
		public bool AsRight { get; set; } = true;
		public bool AsBottom { get; set; } = true;
		public bool CenterVisible { get; set; } = true;
	}

	internal static class OverlayIndicatorVisibilityRules
	{
		internal static OverlayIndicatorVisibility ForAnchorablePane(bool canDropInto)
		{
			return new OverlayIndicatorVisibility
			{
				CenterVisible = canDropInto,
				AsLeft = false,
				AsTop = false,
				AsRight = false,
				AsBottom = false,
			};
		}

		internal static OverlayIndicatorVisibility ForDocumentPaneGroup(bool canDropInto)
		{
			return new OverlayIndicatorVisibility
			{
				CenterVisible = canDropInto,
				InnerLeft = false,
				InnerTop = false,
				InnerRight = false,
				InnerBottom = false,
				AsLeft = false,
				AsTop = false,
				AsRight = false,
				AsBottom = false,
			};
		}

		internal static OverlayIndicatorVisibility ForDocumentPane(
			bool canDropInto,
			bool isAnchorableDrag,
			bool paneHostedInFloatingWindow,
			bool allowMixedOrientation,
			Orientation? parentOrientation,
			int visibleSiblingCount,
			bool isFirstVisible,
			bool isLastVisible,
			int paneChildrenCount)
		{
			var result = new OverlayIndicatorVisibility
			{
				CenterVisible = canDropInto,
			};

			if (parentOrientation != null && visibleSiblingCount > 1)
			{
				if (!allowMixedOrientation)
				{
					if (parentOrientation == Orientation.Horizontal)
					{
						result.InnerTop = false;
						result.InnerBottom = false;
						result.AsLeft = isFirstVisible;
						result.AsRight = isLastVisible;
						result.AsTop = false;
						result.AsBottom = false;
					}
					else
					{
						result.InnerLeft = false;
						result.InnerRight = false;
						result.AsTop = isFirstVisible;
						result.AsBottom = isLastVisible;
						result.AsLeft = false;
						result.AsRight = false;
					}
				}
				else
				{
					result.AsLeft = true;
					result.AsTop = true;
					result.AsRight = true;
					result.AsBottom = true;
				}
			}
			else if (parentOrientation == null && paneChildrenCount == 0)
			{
				result.InnerLeft = false;
				result.InnerTop = false;
				result.InnerRight = false;
				result.InnerBottom = false;
			}

			if (!isAnchorableDrag || paneHostedInFloatingWindow)
			{
				result.AsLeft = false;
				result.AsTop = false;
				result.AsRight = false;
				result.AsBottom = false;
			}

			if (isAnchorableDrag)
			{
				// An anchorable dragged over a document pane can only dock AS AN ANCHORABLE around
				// the document area (the outer "as-anchorable" ring). It cannot split or tab the
				// document, so the inner document-split diamond and the center "dock into" target do
				// not apply and stay hidden — matching WPF AvalonDock/ILSpy, which shows only the
				// four outer edge arrows in this case.
				result.InnerLeft = false;
				result.InnerTop = false;
				result.InnerRight = false;
				result.InnerBottom = false;
				result.CenterVisible = false;
			}

			return result;
		}

		internal static OverlayIndicatorVisibility ForManagerOrNoPane(bool centerVisible)
		{
			return new OverlayIndicatorVisibility
			{
				CenterVisible = centerVisible,
				AsLeft = false,
				AsTop = false,
				AsRight = false,
				AsBottom = false,
			};
		}
	}
}
