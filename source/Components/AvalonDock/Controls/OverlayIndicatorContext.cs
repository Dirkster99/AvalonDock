using System.Linq;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	internal static class OverlayIndicatorContextRules
	{
		internal static bool CanDropInto(ILayoutPositionableElement targetPane, LayoutFloatingWindow floatingModel)
		{
			return OverlayDropRules.ShouldShowDropTargetInto(targetPane, floatingModel);
		}

		internal static OverlayIndicatorVisibility ForDocumentPane(LayoutDocumentPane pane, bool isAnchorableDrag, bool allowMixedOrientation, LayoutFloatingWindow floatingModel)
		{
			var parentGroup = pane?.Parent as LayoutDocumentPaneGroup;
			var visibleChildren = parentGroup?.Children.Where(c => c.IsVisible).ToList();
			var visibleIndex = visibleChildren?.IndexOf(pane) ?? -1;

			return OverlayIndicatorVisibilityRules.ForDocumentPane(
				canDropInto: CanDropInto(pane, floatingModel),
				isAnchorableDrag: isAnchorableDrag,
				paneHostedInFloatingWindow: pane?.IsHostedInFloatingWindow ?? false,
				allowMixedOrientation: allowMixedOrientation,
				parentOrientation: parentGroup?.Orientation,
				visibleSiblingCount: visibleChildren?.Count ?? 0,
				isFirstVisible: visibleIndex == 0,
				isLastVisible: visibleChildren != null && visibleIndex == visibleChildren.Count - 1,
				paneChildrenCount: pane?.ChildrenCount ?? 0);
		}

		internal static OverlayIndicatorVisibility ForDocumentPaneGroup(LayoutDocumentPaneGroup group, LayoutFloatingWindow floatingModel, out LayoutDocumentPane representativePane)
		{
			representativePane = group?.Children?.OfType<LayoutDocumentPane>().FirstOrDefault();
			return OverlayIndicatorVisibilityRules.ForDocumentPaneGroup(CanDropInto(representativePane, floatingModel));
		}
	}
}
